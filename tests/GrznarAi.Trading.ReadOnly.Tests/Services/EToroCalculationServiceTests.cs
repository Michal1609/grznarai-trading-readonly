using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Models.Pnl;
using GrznarAi.Trading.ReadOnly.Models.Trades;
using GrznarAi.Trading.ReadOnly.Services;
using NUnit.Framework;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace GrznarAi.Trading.ReadOnly.Tests.Services;

[TestFixture]
public class EToroCalculationServiceTests
{
    private EToroCalculationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new EToroCalculationService(null!);
    }

    private static PnlResponse EmptyPnl(decimal credit = 0) =>
        new(credit, [], [], [], []);

    private static PositionUnrealizedPnL Upnl(decimal pnl) =>
        new(PnL: pnl, PnlAssetCurrency: pnl, ExposureInAccountCurrency: 0, ExposureInAssetCurrency: 0,
            MarginInAccountCurrency: 0, MarginInAssetCurrency: 0);

    private static Position Pos(int instrumentId, decimal amount, decimal pnl) =>
        new(PositionId: 0, InstrumentId: instrumentId, Amount: amount, UnrealizedPnL: Upnl(pnl));

    private static MirrorPosition MirrorPos(int instrumentId, decimal amount, decimal pnl) =>
        new(PositionId: 0, InstrumentId: instrumentId, Amount: amount, UnrealizedPnL: Upnl(pnl));

    // ─── AvailableCash ───────────────────────────────────────────────────────

    [Test]
    public void CalculateAvailableCash_NoPendingOrders_ReturnsCredit()
    {
        var pnl = EmptyPnl(1000m);

        var result = _service.CalculateAvailableCash(pnl);

        Assert.That(result, Is.EqualTo(1000m));
    }

    [Test]
    public void CalculateAvailableCash_NullPnl_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.CalculateAvailableCash(null!));
    }

    [Test]
    public void CalculateAvailableCash_ManualPendingOrder_SubtractsAmount()
    {
        var pnl = new PnlResponse(
            1000m,
            Positions: [],
            MirrorPortfolios: [],
            OrdersForOpen: [new OrderForOpen(MirrorId: 0, Amount: 200m, TotalExternalCosts: 0)],
            Orders: []);

        var result = _service.CalculateAvailableCash(pnl);

        Assert.That(result, Is.EqualTo(800m));
    }

    [Test]
    public void CalculateAvailableCash_MirroredPendingOrder_ExcludedFromSubtraction()
    {
        var pnl = new PnlResponse(
            1000m,
            Positions: [],
            MirrorPortfolios: [],
            OrdersForOpen: [new OrderForOpen(MirrorId: 5, Amount: 200m, TotalExternalCosts: 0)],
            Orders: []);

        var result = _service.CalculateAvailableCash(pnl);

        Assert.That(result, Is.EqualTo(1000m));
    }

    [Test]
    public void CalculateAvailableCash_MitOrders_SubtractsAll()
    {
        var pnl = new PnlResponse(
            1000m,
            Positions: [],
            MirrorPortfolios: [],
            OrdersForOpen: [],
            Orders: [new MitOrder(OrderId: 1, Amount: 150m), new MitOrder(OrderId: 2, Amount: 100m)]);

        var result = _service.CalculateAvailableCash(pnl);

        Assert.That(result, Is.EqualTo(750m));
    }

    [Test]
    public void CalculateAvailableCash_MixedOrders_CorrectResult()
    {
        // credit=1000, manual pending=200, mirrored pending=100 (excluded), MIT=150 → 650
        var pnl = new PnlResponse(
            1000m,
            Positions: [],
            MirrorPortfolios: [],
            OrdersForOpen: [
                new OrderForOpen(MirrorId: 0, Amount: 200m, TotalExternalCosts: 0),
                new OrderForOpen(MirrorId: 5, Amount: 100m, TotalExternalCosts: 0)],
            Orders: [new MitOrder(OrderId: 1, Amount: 150m)]);

        var result = _service.CalculateAvailableCash(pnl);

        Assert.That(result, Is.EqualTo(650m));
    }

    // ─── TotalInvested ───────────────────────────────────────────────────────

    [Test]
    public void CalculateTotalInvested_EmptyAccount_ReturnsZero()
    {
        var result = _service.CalculateTotalInvested(EmptyPnl());

        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void CalculateTotalInvested_OpenPositions_SumsAmounts()
    {
        var pnl = new PnlResponse(
            0m,
            Positions: [Pos(1001, amount: 500m, pnl: 0), Pos(1002, amount: 300m, pnl: 0)],
            MirrorPortfolios: [],
            OrdersForOpen: [],
            Orders: []);

        var result = _service.CalculateTotalInvested(pnl);

        Assert.That(result, Is.EqualTo(800m));
    }

    [Test]
    public void CalculateTotalInvested_MirrorPortfolio_IncludesMirrorPositionsAndAdjustedAvailability()
    {
        var pnl = new PnlResponse(
            0m,
            Positions: [],
            MirrorPortfolios: [
                new MirrorPortfolio(
                    MirrorId: 1,
                    AvailableAmount: 100m,
                    ClosedPositionsNetProfit: 50m,
                    Positions: [MirrorPos(2001, amount: 200m, pnl: 0)])],
            OrdersForOpen: [],
            Orders: []);

        // mirrorPositions=200, adjustedAvailability=100-50=50 → total=250
        var result = _service.CalculateTotalInvested(pnl);

        Assert.That(result, Is.EqualTo(250m));
    }

    [Test]
    public void CalculateTotalInvested_ManualPendingWithCosts_IncludesExternalCosts()
    {
        var pnl = new PnlResponse(
            0m,
            Positions: [],
            MirrorPortfolios: [],
            OrdersForOpen: [new OrderForOpen(MirrorId: 0, Amount: 200m, TotalExternalCosts: 10m)],
            Orders: []);

        var result = _service.CalculateTotalInvested(pnl);

        Assert.That(result, Is.EqualTo(210m));
    }

    [Test]
    public void CalculateTotalInvested_ComplexScenario_CorrectSum()
    {
        // positions=500+300=800, mirror positions=200+150=350, adjustedAvail=100-50=50
        // manual pending=200+10=210, MIT=150 → total=1560
        var pnl = new PnlResponse(
            0m,
            Positions: [Pos(1, 500m, 0), Pos(2, 300m, 0)],
            MirrorPortfolios: [
                new MirrorPortfolio(MirrorId: 1, AvailableAmount: 100m, ClosedPositionsNetProfit: 50m,
                    Positions: [MirrorPos(3, 200m, 0), MirrorPos(4, 150m, 0)])],
            OrdersForOpen: [new OrderForOpen(MirrorId: 0, Amount: 200m, TotalExternalCosts: 10m)],
            Orders: [new MitOrder(OrderId: 1, Amount: 150m)]);

        var result = _service.CalculateTotalInvested(pnl);

        Assert.That(result, Is.EqualTo(1560m));
    }

    // ─── ProfitLoss ──────────────────────────────────────────────────────────

    [Test]
    public void CalculateProfitLoss_NoPositions_ReturnsZero()
    {
        var result = _service.CalculateProfitLoss(EmptyPnl());

        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void CalculateProfitLoss_MixedPnl_SumsCorrectly()
    {
        // positions: 50 + (-20) = 30, mirror positions: 30 + 15 = 45
        // total = 75. Closed mirror profits are realized PnL, not unrealized PnL.
        var pnl = new PnlResponse(
            0m,
            Positions: [Pos(1, 0, pnl: 50m), Pos(2, 0, pnl: -20m)],
            MirrorPortfolios: [
                new MirrorPortfolio(MirrorId: 1, AvailableAmount: 0, ClosedPositionsNetProfit: 100m,
                    Positions: [MirrorPos(3, 0, pnl: 30m), MirrorPos(4, 0, pnl: 15m)])],
            OrdersForOpen: [],
            Orders: []);

        var result = _service.CalculateProfitLoss(pnl);

        Assert.That(result, Is.EqualTo(75m));
    }

    [Test]
    public void CalculateProfitLoss_NegativePnl_ReturnsNegative()
    {
        var pnl = new PnlResponse(
            0m,
            Positions: [Pos(1, 0, pnl: -500m)],
            MirrorPortfolios: [],
            OrdersForOpen: [],
            Orders: []);

        var result = _service.CalculateProfitLoss(pnl);

        Assert.That(result, Is.EqualTo(-500m));
    }

    // ─── Equity ──────────────────────────────────────────────────────────────

    [Test]
    public void CalculateEquity_SimpleCase_EqualsSumOfComponents()
    {
        // credit=1000, no orders → availableCash=1000
        // positions=500 → totalInvested=500
        // pnl=50 → pnl=50
        // equity = 1550
        var pnl = new PnlResponse(
            1000m,
            Positions: [Pos(1, amount: 500m, pnl: 50m)],
            MirrorPortfolios: [],
            OrdersForOpen: [],
            Orders: []);

        var result = _service.CalculateEquity(pnl);

        Assert.That(result, Is.EqualTo(1550m));
    }

    [Test]
    public void CalculateEquity_Formula_AvailableCashPlusTotalInvestedPlusPnl()
    {
        var pnl = new PnlResponse(
            2000m,
            Positions: [Pos(1, amount: 300m, pnl: 75m)],
            MirrorPortfolios: [],
            OrdersForOpen: [new OrderForOpen(MirrorId: 0, Amount: 200m, TotalExternalCosts: 0)],
            Orders: []);

        var availableCash = _service.CalculateAvailableCash(pnl);
        var totalInvested = _service.CalculateTotalInvested(pnl);
        var pnlValue = _service.CalculateProfitLoss(pnl);
        var equity = _service.CalculateEquity(pnl);

        Assert.That(equity, Is.EqualTo(availableCash + totalInvested + pnlValue));
    }

    // ─── RealizedProfit ──────────────────────────────────────────────────────

    [Test]
    public void CalculateRealizedProfit_NoTrades_ReturnsZero()
    {
        var result = _service.CalculateRealizedProfit([]);

        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void CalculateRealizedProfit_NullTrades_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.CalculateRealizedProfit(null!));
    }

    [Test]
    public void CalculateRealizedProfit_ProfitableTrades_SumsProfits()
    {
        var trades = new[]
        {
            CreateTrade(netProfit: 200m),
            CreateTrade(netProfit: 150m),
            CreateTrade(netProfit: -50m)
        };

        var result = _service.CalculateRealizedProfit(trades);

        Assert.That(result, Is.EqualTo(300m));
    }

    [Test]
    public void CalculateRealizedProfit_AllLosses_ReturnsNegativeSum()
    {
        var trades = new[]
        {
            CreateTrade(netProfit: -100m),
            CreateTrade(netProfit: -75m)
        };

        var result = _service.CalculateRealizedProfit(trades);

        Assert.That(result, Is.EqualTo(-175m));
    }

    [Test]
    public async Task GetAccountMetricsAsync_UsesPageSizeAndStopsOnShortPage()
    {
        var handler = new MetricsHttpMessageHandler([
            [CreateTrade(positionId: 1, netProfit: 10m), CreateTrade(positionId: 2, netProfit: -2m)],
            [CreateTrade(positionId: 3, netProfit: 5m)]
        ]);
        var client = new EToroClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://public-api.etoro.com/api/v1/")
        });
        var service = new EToroCalculationService(client);

        var metrics = await service.GetAccountMetricsAsync(
            EToroEnvironment.Real,
            fromDate: new DateOnly(2024, 1, 1),
            pageSize: 2,
            maxPages: 10);

        Assert.That(metrics.RealizedPnL, Is.EqualTo(13m));
        Assert.That(metrics.TotalReturn, Is.EqualTo(13m));
        Assert.That(handler.TradeHistoryUris, Has.Count.EqualTo(2));
        Assert.That(handler.TradeHistoryUris[0], Does.Contain("minDate=2024-01-01"));
        Assert.That(handler.TradeHistoryUris[0], Does.Contain("page=0"));
        Assert.That(handler.TradeHistoryUris[0], Does.Contain("pageSize=2"));
        Assert.That(handler.TradeHistoryUris[1], Does.Contain("page=1"));
    }

    [Test]
    public void GetAccountMetricsAsync_HitsMaxPagesWithFullLastPage_Throws()
    {
        // 3 pages available, maxPages=2, pageSize=1 — last fetched page is full → must throw
        var handler = new MetricsHttpMessageHandler([
            [CreateTrade(positionId: 1, netProfit: 1m)],
            [CreateTrade(positionId: 2, netProfit: 1m)],
            [CreateTrade(positionId: 3, netProfit: 1m)]
        ]);
        var client = new EToroClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://public-api.etoro.com/api/v1/")
        });
        var service = new EToroCalculationService(client);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetAccountMetricsAsync(EToroEnvironment.Real, pageSize: 1, maxPages: 2));
        Assert.That(handler.TradeHistoryUris, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAccountMetricsAsync_MaxPagesExactlyCoversAllData_DoesNotThrow()
    {
        // 2 full pages + 1 empty page → exits cleanly at empty page, no throw
        var handler = new MetricsHttpMessageHandler([
            [CreateTrade(positionId: 1, netProfit: 1m)],
            [CreateTrade(positionId: 2, netProfit: 1m)],
            []
        ]);
        var client = new EToroClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://public-api.etoro.com/api/v1/")
        });
        var service = new EToroCalculationService(client);

        var metrics = await service.GetAccountMetricsAsync(EToroEnvironment.Real, pageSize: 1, maxPages: 3);

        Assert.That(metrics.RealizedPnL, Is.EqualTo(2m));
    }

    [Test]
    public async Task GetAccountMetricsAsync_DeduplicatesPositionIds_CountsOnce()
    {
        // Page 0 returns trade 1 and 2. Page 1 returns trade 1 again (duplicate) and trade 3.
        // Trade 1 must be counted only once.
        var handler = new MetricsHttpMessageHandler([
            [CreateTrade(positionId: 1, netProfit: 10m), CreateTrade(positionId: 2, netProfit: 5m)],
            [CreateTrade(positionId: 1, netProfit: 10m), CreateTrade(positionId: 3, netProfit: 3m)]
            // page 2 returns empty (out of range) → terminates naturally
        ]);
        var client = new EToroClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://public-api.etoro.com/api/v1/")
        });
        var service = new EToroCalculationService(client);

        var metrics = await service.GetAccountMetricsAsync(EToroEnvironment.Real, pageSize: 2, maxPages: 10);

        Assert.That(metrics.RealizedPnL, Is.EqualTo(18m)); // 10 + 5 + 3
    }

    [Test]
    public async Task GetAccountMetricsAsync_AllDuplicatePage_StopsEarly()
    {
        // Page 0: trade 1. Page 1: trade 1 again (all duplicates) → stop.
        var handler = new MetricsHttpMessageHandler([
            [CreateTrade(positionId: 1, netProfit: 7m)],
            [CreateTrade(positionId: 1, netProfit: 7m)]
        ]);
        var client = new EToroClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://public-api.etoro.com/api/v1/")
        });
        var service = new EToroCalculationService(client);

        var metrics = await service.GetAccountMetricsAsync(EToroEnvironment.Real, pageSize: 1, maxPages: 10);

        Assert.That(metrics.RealizedPnL, Is.EqualTo(7m));
        Assert.That(handler.TradeHistoryUris, Has.Count.EqualTo(2));
    }

    private static ClosedTrade CreateTrade(decimal netProfit) =>
        CreateTrade(positionId: 9000, netProfit);

    private static ClosedTrade CreateTrade(long positionId, decimal netProfit) =>
        new(PositionId: positionId, InstrumentId: 1001, IsBuy: true, Leverage: 1,
            Investment: 500m, InitialInvestment: 500m,
            OpenRate: 100m, CloseRate: 120m,
            OpenTimestamp: DateTimeOffset.UtcNow.AddMonths(-6),
            CloseTimestamp: DateTimeOffset.UtcNow,
            NetProfit: netProfit,
            Fees: 0m, Units: 5m,
            StopLossRate: 0m, TakeProfitRate: 0m,
            TrailingStopLoss: false, OrderId: 0, SocialTradeId: 0, ParentPositionId: 0);

    private sealed class MetricsHttpMessageHandler(IReadOnlyList<IReadOnlyList<ClosedTrade>> pages) : HttpMessageHandler
    {
        public List<string> TradeHistoryUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var pathAndQuery = request.RequestUri!.PathAndQuery;
            if (pathAndQuery.Contains("trading/info/real/pnl", StringComparison.Ordinal))
            {
                return JsonResponse("""
                {
                  "clientPortfolio": {
                    "credit": 1000,
                    "positions": [],
                    "mirrors": [],
                    "ordersForOpen": [],
                    "orders": []
                  }
                }
                """);
            }

            TradeHistoryUris.Add(pathAndQuery);
            var page = ReadIntQueryValue(request.RequestUri!, "page");
            var trades = page < pages.Count ? pages[page] : [];
            var json = "[" + string.Join(",", trades.Select(ToJson)) + "]";

            return JsonResponse(json);
        }

        private static Task<HttpResponseMessage> JsonResponse(string json) =>
            new StringContent(json, Encoding.UTF8, "application/json").ReadAsHttpResponseMessageAsync();

        private static int ReadIntQueryValue(Uri uri, string name)
        {
            var pair = uri.Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2 && p[0] == name)
                .FirstOrDefault();

            return pair is not null ? int.Parse(Uri.UnescapeDataString(pair[1])) : 0;
        }

        private static string ToJson(ClosedTrade trade)
        {
            return $$"""
            {
              "positionId": {{trade.PositionId}},
              "instrumentId": {{trade.InstrumentId}},
              "isBuy": true,
              "leverage": 1,
              "investment": 500,
              "initialInvestment": 500,
              "openRate": 100,
              "closeRate": 120,
              "openTimestamp": "2024-01-01T00:00:00Z",
              "closeTimestamp": "2024-02-01T00:00:00Z",
              "netProfit": {{trade.NetProfit.ToString(CultureInfo.InvariantCulture)}},
              "fees": 0,
              "units": 5,
              "stopLossRate": 0,
              "takeProfitRate": 0,
              "trailingStopLoss": false,
              "orderId": 0,
              "socialTradeId": 0,
              "parentPositionId": 0
            }
            """;
        }
    }
}
