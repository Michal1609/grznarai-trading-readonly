using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Models.Market;
using GrznarAi.Trading.ReadOnly.Models.Social;
using GrznarAi.Trading.ReadOnly.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Tests.Integration;

/// <summary>
/// Ruční integrační testy proti reálnému eToro API.
/// Spuštění: dotnet test --filter "Category=Integration"
/// Nutné: nastavit EToroOptions__ApiKey + EToroOptions__UserKey nebo lokální appsettings.test.json
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruční spuštění — vyžaduje reálné API klíče mimo git")]
public class EToroIntegrationTests
{
    private IEToroClient _client = null!;
    private IEToroCalculationService _service = null!;
    private EToroOptions _options = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _options = IntegrationTestSupport.LoadOptionsOrIgnore();
        _client = IntegrationTestSupport.CreateClient(_options);
        _service = new EToroCalculationService(_client);
    }

    // ─── Client — PnL ────────────────────────────────────────────────────────

    [Test]
    public async Task Client_GetPnlAsync_Real_ReturnsData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Real);

        Debug.WriteLine($"Credit:       {result.Credit:F2}");
        Debug.WriteLine($"BonusCredit:  {result.BonusCredit:F2}");
        Debug.WriteLine($"Positions:    {result.Positions.Count}");
        Debug.WriteLine($"Mirrors:      {result.MirrorPortfolios.Count}");
        Debug.WriteLine($"OrdersForOpen:{result.OrdersForOpen.Count}");
        Debug.WriteLine($"MIT Orders:   {result.Orders.Count}");

        Assert.That(result.Credit, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task Client_GetPnlAsync_Demo_ReturnsData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"Demo Credit:  {result.Credit:F2}");

        Assert.That(result.Credit, Is.GreaterThanOrEqualTo(0));
    }

    // ─── Client — Portfolio ───────────────────────────────────────────────────

    [Test]
    public async Task Client_GetPortfolioAsync_Real_ReturnsData()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Real);

        Debug.WriteLine($"Portfolio positions: {result.Positions.Count}");
        Debug.WriteLine($"Credit:             {result.Credit:F2}");

        foreach (var pos in result.Positions.Take(5))
            Debug.WriteLine($"  InstrumentID={pos.InstrumentId} Invested={pos.InvestedAmount:F2} NetProfit={pos.NetProfit:F2}");

        Assert.That(result, Is.Not.Null);
    }

    // ─── Client — Trade History ───────────────────────────────────────────────

    [Test]
    public async Task Client_GetTradeHistoryAsync_ReturnsData()
    {
        var minDate = DateTimeOffset.UtcNow.AddYears(-6);
        var result = await _client.GetTradeHistoryAsync(minDate, page: 0);

        Debug.WriteLine($"Trades on page: {result.Trades.Count}");

        foreach (var t in result.Trades.Take(5))
            Debug.WriteLine($"  PositionID={t.PositionId} NetProfit={t.NetProfit:F2} CloseDate={t.CloseTimestamp:yyyy-MM-dd}");

        Assert.That(result.Trades, Is.Not.Null);
    }

    // ─── Client — Instruments ─────────────────────────────────────────────────

    [Test]
    public async Task Client_SearchInstrumentsAsync_ReturnsResults()
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields =
            [
                InstrumentFields.InstrumentId,
                InstrumentFields.InternalInstrumentDisplayName,
                InstrumentFields.InternalSymbolFull
            ],
            PageSize = 100
        });

        Debug.WriteLine($"Total instruments found: {result.TotalItems}");

        foreach (var i in result.Instruments)
            Debug.WriteLine($"  ID={i.InstrumentId} Name={i.InternalInstrumentDisplayName} Symbol={i.InternalSymbolFull}");

        Assert.That(result.Instruments, Is.Not.Empty);
    }

    [Test]
    public async Task Client_SearchInstrumentsAsync_NoPaging_ReturnsFirstPage()
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = [InstrumentFields.InstrumentId, InstrumentFields.DisplayName],
            PageSize = 20,
        });

        Debug.WriteLine($"First page instruments: {result.Instruments.Count} / {result.TotalItems}");

        Assert.That(result.Instruments, Is.Not.Empty);
    }

    // ─── Client — Rates ───────────────────────────────────────────────────────

    [Test]
    public async Task Client_GetRatesAsync_ReturnsRates()
    {
        // Apple (1001) + Microsoft (1002) — adjust IDs if needed
        var result = await _client.GetRatesAsync([1001, 1002]);

        Debug.WriteLine($"Rates returned: {result.Rates.Count}");

        foreach (var r in result.Rates)
            Debug.WriteLine($"  ID={r.InstrumentId} Bid={r.Bid} Ask={r.Ask} LastExec={r.LastExecution} Date={r.Date:u}");

        Assert.That(result.Rates, Is.Not.Empty);
    }

    // ─── Client — Candles ─────────────────────────────────────────────────────

    [Test]
    public async Task Client_GetCandlesAsync_ReturnsCandles()
    {
        // InstrumentID 1001 = Apple (adjust if needed)
        var result = await _client.GetCandlesAsync(
            instrumentId: 1001,
            interval: CandleInterval.OneDay,
            direction: CandleDirection.Desc,
            candlesCount: 10);

        Debug.WriteLine($"Interval: {result.Interval}");
        Debug.WriteLine($"Candles returned: {result.Candles.Count}");

        foreach (var c in result.Candles.Take(5))
            Debug.WriteLine($"  {c.FromDate:yyyy-MM-dd} O={c.Open} H={c.High} L={c.Low} C={c.Close} V={c.Volume}");

        Assert.That(result.Candles, Is.Not.Empty);
    }

    // ─── Client — Watchlists ──────────────────────────────────────────────────

    [Test]
    public async Task Client_GetUserWatchlistsAsync_ReturnsWatchlists()
    {
        var result = await _client.GetUserWatchlistsAsync(
            itemsPerPageForSingle: 10,
            ensureBuiltinWatchlists: true,
            addRelatedAssets: false);

        Debug.WriteLine($"Watchlists: {result.Watchlists.Count}");

        foreach (var w in result.Watchlists)
            Debug.WriteLine($"  ID={w.WatchlistId} Name='{w.Name}' Type={w.WatchlistType} Items={w.TotalItems}");

        Assert.That(result.Watchlists, Is.Not.Null);
    }

    // ─── Client — Popular Investors ───────────────────────────────────────────

    [Test]
    public async Task Client_GetPopularInvestorsAsync_ReturnsInvestors()
    {
        var result = await _client.GetPopularInvestorsAsync(new PopularInvestorsRequest
        {
            Period = PopularInvestorPeriod.CurrYear,
            PopularInvestor = true,
            PageSize = 5
        });

        Debug.WriteLine($"Total popular investors: {result.TotalItems}");

        foreach (var inv in result.Items)
            Debug.WriteLine($"  {inv.Username} Gain={inv.Gain:F1}% Risk={inv.RiskScore} Copiers={inv.Copiers}");

        Assert.That(result.Items, Is.Not.Null);
    }

    // ─── Service — Calculations ───────────────────────────────────────────────

    [Test]
    public async Task Service_CalculateAvailableCash_Real()
    {
        var pnl = await _client.GetPnlAsync(EToroEnvironment.Real);

        var result = _service.CalculateAvailableCash(pnl);

        Debug.WriteLine($"Available Cash: {result:F2} USD");

        Assert.That(result, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task Service_CalculateTotalInvested_Real()
    {
        var pnl = await _client.GetPnlAsync(EToroEnvironment.Real);

        var result = _service.CalculateTotalInvested(pnl);

        Debug.WriteLine($"Total Invested: {result:F2} USD");

        Assert.That(result, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task Service_CalculateProfitLoss_Real_RecomputesFromOpenPositions()
    {
        var pnl = await _client.GetPnlAsync(EToroEnvironment.Real);

        var result = _service.CalculateProfitLoss(pnl);
        var expected = pnl.Positions.Sum(p => p.PnL)
                       + pnl.MirrorPortfolios.SelectMany(m => m.Positions).Sum(p => p.PnL);

        Debug.WriteLine($"Unrealized P/L: {result:+0.00;-0.00} USD");
        Debug.WriteLine($"API-reported unrealized P/L: {pnl.UnrealizedPnL:+0.00;-0.00} USD");

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test] // Celkov8 hodnota
    public async Task Service_CalculateEquity_Real()
    {
        var pnl = await _client.GetPnlAsync(EToroEnvironment.Real);

        var result = _service.CalculateEquity(pnl);

        Debug.WriteLine($"Equity: {result:F2} USD");

        Assert.That(result, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task Service_GetAccountMetricsAsync_PrintsFullSummary()
    {
        var metrics = await _service.GetAccountMetricsAsync(EToroEnvironment.Real);

        Debug.WriteLine("═══════════════════════════════════");
        Debug.WriteLine("       ACCOUNT METRICS SUMMARY     ");
        Debug.WriteLine("═══════════════════════════════════");
        Debug.WriteLine($"Available Cash:         {metrics.AvailableCash,12:F2} USD");
        Debug.WriteLine($"Total Invested:         {metrics.TotalInvested,12:F2} USD");
        Debug.WriteLine($"Unrealized P/L:         {metrics.UnrealizedPnL,12:+0.00;-0.00} USD");
        Debug.WriteLine($"Realized Profit:        {metrics.RealizedProfit,12:+0.00;-0.00} USD");
        Debug.WriteLine($"Equity:                 {metrics.Equity,12:F2} USD");
        Debug.WriteLine($"Total Performance:      {metrics.TotalAccountPerformance,12:+0.00;-0.00} USD");
        Debug.WriteLine("═══════════════════════════════════");

        Assert.That(metrics.Equity, Is.GreaterThanOrEqualTo(0));
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    private static HttpMessageHandler BuildPipeline(
        EToroAuthHandler auth, RateLimitHandler rateLimit)
    {
        rateLimit.InnerHandler = new HttpClientHandler();
        auth.InnerHandler = rateLimit;
        return auth;
    }
}
