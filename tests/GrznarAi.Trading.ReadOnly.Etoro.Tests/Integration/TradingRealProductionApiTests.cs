using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// ProdukÄŤnĂ­ / prĹŻzkumnĂ© testy Trading Real API.
/// CĂ­l: odhalit chovĂˇnĂ­ eToro nedokumentovanĂ© nebo odliĹˇnĂ© od dokumentace â€”
/// pageSize/page chovĂˇnĂ­, filtry minDate, konzistence PnL dat, chovĂˇnĂ­ GetOrderAsync.
/// Rate limit: 60 req/min â€” zajiĹˇĹĄuje RateLimitHandler automaticky.
///
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "FullyQualifiedName~TradingRealProductionApiTests"
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class TradingRealProductionApiTests
{
    private IEToroClient _client = null!;

    // Seed data z OneTimeSetUp
    private bool   _hasHistory;
    private int    _defaultPageCount;  // kolik zĂˇznamĹŻ vrĂˇtĂ­ strĂˇnka 0 bez pageSize

    // OrderId z PnL (OrdersForOpen + Orders) â€” nejvÄ›tĹˇĂ­ = nejnovÄ›jĹˇĂ­ otevĹ™enĂˇ objednĂˇvka
    private long   _pnlMaxOrderId;
    private bool   _hasPnlOrders;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();

        // Seed: default page count z historie
        var history = await _client.GetTradeHistoryAsync(
            DateTimeOffset.UtcNow.AddYears(-5), page: 0);
        _hasHistory       = history.Trades.Count > 0;
        _defaultPageCount = history.Trades.Count;

        // Seed: nejvÄ›tĹˇĂ­ OrderId z PnL â€” Positions, OrdersForOpen, Orders
        // Historie obsahuje jen uzavĹ™enĂ© pozice â€” pro GetOrderAsync je PnL lepĹˇĂ­ zdroj
        var pnl = await _client.GetPnlAsync(EToroEnvironment.Real);
        var pnlOrderIds = pnl.Positions.Select(p => p.OrderId)
            .Concat(pnl.OrdersForOpen.Select(o => o.OrderId))
            .Concat(pnl.Orders.Select(o => o.OrderId))
            .Where(id => id > 0)
            .ToList();
        _hasPnlOrders  = pnlOrderIds.Count > 0;
        _pnlMaxOrderId = _hasPnlOrders ? pnlOrderIds.Max() : 0;

        Debug.WriteLine($"[SetUp] history={_hasHistory} defaultPageCount={_defaultPageCount}");
        Debug.WriteLine($"[SetUp] PnL OrdersForOpen={pnl.OrdersForOpen.Count} Orders={pnl.Orders.Count} " +
                        $"â†’ hasPnlOrders={_hasPnlOrders} pnlMaxOrderId={_pnlMaxOrderId}");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetPortfolioAsync â€” trading/info/portfolio
    // Dokumentace: ĹľĂˇdnĂ© query parametry, vrĂˇtĂ­ positions + credit
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetPortfolioAsync_Real_CreditIsNonNegative()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Real);

        Debug.WriteLine($"[Portfolio] Credit={result.Credit:F2} BonusCredit={result.BonusCredit:F2} Positions={result.Positions.Count}");

        Assert.That(result.Credit, Is.GreaterThanOrEqualTo(0),
            "Credit nesmĂ­ bĂ˝t zĂˇpornĂ˝.");
        Assert.That(result.BonusCredit, Is.GreaterThanOrEqualTo(0),
            "BonusCredit nesmĂ­ bĂ˝t zĂˇpornĂ˝.");
    }

    [Test]
    public async Task GetPortfolioAsync_Real_AllPositionsHaveValidIds()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Real);

        Debug.WriteLine($"[Portfolio] Validuji {result.Positions.Count} pozic.");
        foreach (var pos in result.Positions)
        {
            Debug.WriteLine($"  PositionId={pos.PositionId} InstrumentId={pos.InstrumentId} " +
                            $"Invested={pos.InvestedAmount:F2} IsBuy={pos.IsBuy} Leverage={pos.Leverage}");
            Assert.Multiple(() =>
            {
                Assert.That(pos.PositionId, Is.GreaterThan(0),
                    $"PositionId musĂ­ bĂ˝t kladnĂ©.");
                Assert.That(pos.InstrumentId, Is.GreaterThan(0),
                    $"InstrumentId musĂ­ bĂ˝t kladnĂ© (PositionId={pos.PositionId}).");
                Assert.That(pos.InvestedAmount, Is.GreaterThanOrEqualTo(0),
                    $"InvestedAmount nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={pos.PositionId}).");
                Assert.That(pos.OpenRate, Is.GreaterThanOrEqualTo(0),
                    $"OpenRate nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={pos.PositionId}).");
                Assert.That(pos.Units, Is.GreaterThan(0),
                    $"Units musĂ­ bĂ˝t kladnĂ˝ (PositionId={pos.PositionId}).");
            });
        }
    }

    [Test]
    public async Task GetPortfolioAsync_Real_LeverageValuesAreSensible()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Real);

        var validLeverages = new[] { 1, 2, 5, 10, 25, 50, 100, 200, 400 };

        foreach (var pos in result.Positions)
        {
            Debug.WriteLine($"  PositionId={pos.PositionId} Leverage={pos.Leverage}");

            if (!validLeverages.Contains(pos.Leverage))
                Debug.WriteLine($"  âš  NeznĂˇmĂ˝ leverage={pos.Leverage} (PositionId={pos.PositionId}).");

            Assert.That(pos.Leverage, Is.GreaterThanOrEqualTo(1),
                $"Leverage musĂ­ bĂ˝t â‰Ą 1 (PositionId={pos.PositionId}).");
        }
    }

    [Test]
    public async Task GetPortfolioAsync_Real_OpenDateTimeIsInPast()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Real);

        var now = DateTimeOffset.UtcNow;
        foreach (var pos in result.Positions)
        {
            Debug.WriteLine($"  PositionId={pos.PositionId} OpenDateTime={pos.OpenDateTime:u}");

            if (pos.OpenDateTime > now)
                Debug.WriteLine($"  âš  OpenDateTime={pos.OpenDateTime:u} je v budoucnosti!");

            Assert.That(pos.OpenDateTime, Is.LessThanOrEqualTo(now.AddMinutes(5)),
                $"OpenDateTime nesmĂ­ bĂ˝t v budoucnosti (PositionId={pos.PositionId}).");
        }
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetPnlAsync â€” trading/info/real/pnl
    // Dokumentace: ĹľĂˇdnĂ© query parametry, vrĂˇtĂ­ kompletnĂ­ portfolio s PnL
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetPnlAsync_Real_BasicFieldsAreValid()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Real);

        Debug.WriteLine($"[PnL] Credit={result.Credit:F2} BonusCredit={result.BonusCredit:F2} " +
                        $"UnrealizedPnL={result.UnrealizedPnL:+0.00;-0.00}");
        Debug.WriteLine($"  Positions={result.Positions.Count} Mirrors={result.MirrorPortfolios.Count} " +
                        $"OrdersForOpen={result.OrdersForOpen.Count} Orders={result.Orders.Count}");

        Assert.Multiple(() =>
        {
            Assert.That(result.Credit, Is.GreaterThanOrEqualTo(0), "Credit nesmĂ­ bĂ˝t zĂˇpornĂ˝.");
            Assert.That(result.BonusCredit, Is.GreaterThanOrEqualTo(0), "BonusCredit nesmĂ­ bĂ˝t zĂˇpornĂ˝.");
        });
    }

    [Test]
    public async Task GetPnlAsync_Real_UnrealizedPnL_ConsistencyWithPositions()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Real);

        if (result.Positions.Count == 0 && result.MirrorPortfolios.Count == 0)
        {
            Assert.Pass("Ĺ˝ĂˇdnĂ© otevĹ™enĂ© pozice â€” PnL konzistenci nelze ovÄ›Ĺ™it.");
            return;
        }

        // SouÄŤet PnL z pĹ™Ă­mĂ˝ch pozic
        var directPnL = result.Positions.Sum(p => p.PnL);
        // SouÄŤet PnL z mirror pozic
        var mirrorPnL = result.MirrorPortfolios.Sum(m => m.Positions.Sum(p => p.PnL));
        var summedPnL = directPnL + mirrorPnL;

        Debug.WriteLine($"[PnL] UnrealizedPnL z API={result.UnrealizedPnL:+0.00;-0.00}");
        Debug.WriteLine($"  SouÄŤet pĹ™Ă­mĂ˝ch pozic: {directPnL:+0.00;-0.00} ({result.Positions.Count} pos)");
        Debug.WriteLine($"  SouÄŤet mirror pozic:  {mirrorPnL:+0.00;-0.00} ({result.MirrorPortfolios.Sum(m => m.Positions.Count)} pos)");
        Debug.WriteLine($"  CelkovĂ˝ souÄŤet:       {summedPnL:+0.00;-0.00}");

        var diff = Math.Abs(result.UnrealizedPnL - summedPnL);
        if (diff > 1m)
            Debug.WriteLine($"  âš  RozdĂ­l {diff:F4} USD mezi UnrealizedPnL a souÄŤtem pozic â€” moĹľnĂˇ rounding nebo ÄŤasovĂˇ neshoda.");

        // Tolerujeme malĂ© rozdĂ­ly (FX konverznĂ­ rounding, ÄŤasovĂˇ prodleva)
        Assert.That(diff, Is.LessThanOrEqualTo(50m),
            $"UnrealizedPnL={result.UnrealizedPnL:F2} se vĂ˝raznÄ› liĹˇĂ­ od souÄŤtu pozic {summedPnL:F2} (diff={diff:F2}).");
    }

    [Test]
    public async Task GetPnlAsync_Real_PositionsHaveValidData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Real);

        Debug.WriteLine($"[PnL] PĹ™Ă­mĂ˝ch pozic: {result.Positions.Count}");
        foreach (var pos in result.Positions)
        {
            Debug.WriteLine($"  PositionId={pos.PositionId} InstrumentId={pos.InstrumentId} " +
                            $"Amount={pos.Amount:F2} PnL={pos.PnL:+0.00;-0.00} IsBuy={pos.IsBuy}");
            Assert.Multiple(() =>
            {
                Assert.That(pos.PositionId, Is.GreaterThan(0),
                    "PositionId musĂ­ bĂ˝t kladnĂ©.");
                Assert.That(pos.InstrumentId, Is.GreaterThan(0),
                    $"InstrumentId musĂ­ bĂ˝t kladnĂ© (PositionId={pos.PositionId}).");
                Assert.That(pos.Amount, Is.GreaterThanOrEqualTo(0),
                    $"Amount nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={pos.PositionId}).");
                Assert.That(pos.OpenRate, Is.GreaterThanOrEqualTo(0),
                    $"OpenRate nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={pos.PositionId}).");
            });
        }
    }

    [Test]
    public async Task GetPnlAsync_Real_MirrorPortfoliosHaveValidData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Real);

        Debug.WriteLine($"[PnL] Mirror portfolios: {result.MirrorPortfolios.Count}");
        foreach (var mirror in result.MirrorPortfolios)
        {
            Debug.WriteLine($"  MirrorId={mirror.MirrorId} AvailableAmount={mirror.AvailableAmount:F2} " +
                            $"IsPaused={mirror.IsPaused} Positions={mirror.Positions.Count} " +
                            $"ClosedProfit={mirror.ClosedPositionsNetProfit:+0.00;-0.00}");

            Assert.That(mirror.MirrorId, Is.GreaterThan(0),
                "MirrorId musĂ­ bĂ˝t kladnĂ©.");
            Assert.That(mirror.AvailableAmount, Is.GreaterThanOrEqualTo(0),
                $"AvailableAmount nesmĂ­ bĂ˝t zĂˇpornĂ˝ (MirrorId={mirror.MirrorId}).");
        }
    }

    [Test]
    public async Task GetPnlAsync_Real_vs_GetPortfolioAsync_PositionsCountMatch()
    {
        var pnl       = await _client.GetPnlAsync(EToroEnvironment.Real);
        var portfolio  = await _client.GetPortfolioAsync(EToroEnvironment.Real);

        var pnlDirectCount   = pnl.Positions.Count;
        var portfolioCount    = portfolio.Positions.Count;

        Debug.WriteLine($"[Consistency] PnL.Positions={pnlDirectCount} Portfolio.Positions={portfolioCount}");

        if (pnlDirectCount != portfolioCount)
            Debug.WriteLine($"  âš  PoÄŤty se liĹˇĂ­ â€” GetPnlAsync a GetPortfolioAsync vrĂˇtily rĹŻznĂ˝ poÄŤet pĹ™Ă­mĂ˝ch pozic!");

        Assert.That(pnlDirectCount, Is.EqualTo(portfolioCount),
            "GetPnlAsync a GetPortfolioAsync by mÄ›ly vracet stejnĂ˝ poÄŤet pĹ™Ă­mĂ˝ch pozic.");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetOrderAsync â€” trading/info/real/orders/{orderId}
    // Dokumentace: orderId path param (int64), 404 pro neexistujĂ­cĂ­
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetOrderAsync_Real_ValidOrderId_ReturnsConsistentData()
    {
        if (!_hasPnlOrders)
        {
            Assert.Ignore("PnL neobsahuje ĹľĂˇdnĂ© otevĹ™enĂ©/ÄŤekajĂ­cĂ­ objednĂˇvky â†’ orderId nedostupnĂ˝.");
            return;
        }

        var result = await _client.GetOrderAsync(EToroEnvironment.Real, _pnlMaxOrderId);

        Debug.WriteLine($"[Order] OrderId={result.OrderId} StatusId={result.StatusId} " +
                        $"OrderType={result.OrderType} InstrumentId={result.InstrumentId}");
        Debug.WriteLine($"  Amount={result.Amount:F2} Units={result.Units} RequestOccurred={result.RequestOccurred:u}");
        Debug.WriteLine($"  Positions={result.Positions?.Count ?? 0}");

        if (result.Positions is not null)
            foreach (var p in result.Positions)
                Debug.WriteLine($"    PositionId={p.PositionId} Rate={p.Rate} Units={p.Units} IsOpen={p.IsOpen}");

        Assert.Multiple(() =>
        {
            Assert.That(result.OrderId, Is.EqualTo(_pnlMaxOrderId),
                "VrĂˇcenĂ© OrderId musĂ­ odpovĂ­dat poĹľadovanĂ©mu.");
            Assert.That(result.InstrumentId, Is.GreaterThan(0),
                "InstrumentId musĂ­ bĂ˝t kladnĂ©.");
            Assert.That(result.RequestOccurred, Is.LessThanOrEqualTo(DateTimeOffset.UtcNow.AddMinutes(5)),
                "RequestOccurred nesmĂ­ bĂ˝t v budoucnosti.");
        });
    }

    [Test]
    public async Task GetOrderAsync_Real_StatusId_IsKnownValue()
    {
        if (!_hasPnlOrders)
        {
            Assert.Ignore("PnL neobsahuje ĹľĂˇdnĂ© otevĹ™enĂ©/ÄŤekajĂ­cĂ­ objednĂˇvky â†’ orderId nedostupnĂ˝.");
            return;
        }

        // Dokumentace: 0=Pending, 1=Executed, 2=Cancelled, 3=Rejected, 4=PartiallyExecuted
        // Zdroj: nejvÄ›tĹˇĂ­ OrderId z PnL.OrdersForOpen + PnL.Orders (otevĹ™enĂ©, ne uzavĹ™enĂ©)
        var result = await _client.GetOrderAsync(EToroEnvironment.Real, _pnlMaxOrderId);

        Debug.WriteLine($"[Order] pnlMaxOrderId={_pnlMaxOrderId} â†’ StatusId={result.StatusId} OrderType={result.OrderType}");

        if (!new[] { 0, 1, 2, 3, 4 }.Contains(result.StatusId))
            Debug.WriteLine($"  âš  NeznĂˇmĂ˝ StatusId={result.StatusId} â€” dokumentace neodpovĂ­dĂˇ!");

        Assert.That(result.StatusId, Is.InRange(0, 4),
            $"StatusId={result.StatusId} nenĂ­ v rozsahu dokumentovanĂ˝ch hodnot 0â€“4.");
    }

    [Test]
    public async Task GetOrderAsync_Real_AllPnlOrderIds_ReturnConsistentData()
    {
        if (!_hasPnlOrders)
        {
            Assert.Ignore("PnL neobsahuje ĹľĂˇdnĂ© otevĹ™enĂ©/ÄŤekajĂ­cĂ­ objednĂˇvky.");
            return;
        }

        // NaÄŤteme PnL znovu pro kompletnĂ­ seznam orderIds (max 5 abychom ĹˇetĹ™ili rate limit)
        var pnl = await _client.GetPnlAsync(EToroEnvironment.Real);
        var orderIds = pnl.OrdersForOpen.Select(o => o.OrderId)
            .Concat(pnl.Orders.Select(o => o.OrderId))
            .Where(id => id > 0)
            .Distinct()
            .OrderByDescending(id => id)
            .Take(5)
            .ToList();

        Debug.WriteLine($"[Order] Testuji {orderIds.Count} orderIds z PnL: {string.Join(", ", orderIds)}");

        foreach (var orderId in orderIds)
        {
            var result = await _client.GetOrderAsync(EToroEnvironment.Real, orderId);
            Debug.WriteLine($"  OrderId={orderId} â†’ StatusId={result.StatusId} InstrumentId={result.InstrumentId} " +
                            $"Positions={result.Positions?.Count ?? 0}");

            Assert.That(result.OrderId, Is.EqualTo(orderId),
                $"VrĂˇcenĂ© OrderId={result.OrderId} â‰  poĹľadovanĂ©={orderId}.");
        }
    }

    [Test]
    public void GetOrderAsync_Real_ZeroOrderId_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetOrderAsync(EToroEnvironment.Real, 0L),
            "Client by mÄ›l odmĂ­tnout orderId=0.");
    }

    [Test]
    public void GetOrderAsync_Real_NegativeOrderId_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetOrderAsync(EToroEnvironment.Real, -1L),
            "Client by mÄ›l odmĂ­tnout zĂˇpornĂ© orderId.");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetTradeHistoryAsync â€” trading/info/trade/history
    // Dokumentace: minDate (required), page (optional), pageSize (optional)
    // HLAVNĂŤ cĂ­l: odhalit zda eToro respektuje pageSize
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    // â”€â”€â”€ pageSize kombinace â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [TestCase(1,   TestName = "TradeHistory_PageSize_1")]
    [TestCase(4,   TestName = "TradeHistory_PageSize_4")]
    [TestCase(5,   TestName = "TradeHistory_PageSize_5")]
    [TestCase(10,  TestName = "TradeHistory_PageSize_10")]
    [TestCase(20,  TestName = "TradeHistory_PageSize_20")]
    [TestCase(50,  TestName = "TradeHistory_PageSize_50")]
    [TestCase(100, TestName = "TradeHistory_PageSize_100")]
    [TestCase(200, TestName = "TradeHistory_PageSize_200_MaxLimit")]
    public async Task GetTradeHistoryAsync_PageSize_ActualCountMatchesRequested(int pageSize)
    {
        var minDate = DateTimeOffset.UtcNow.AddYears(-5);
        var result = await _client.GetTradeHistoryAsync(minDate, page: 0, pageSize: pageSize);

        var returned = result.Trades.Count;
        Debug.WriteLine($"[History] pageSize={pageSize} â†’ returned={returned} defaultPageCount={_defaultPageCount}");

        if (returned > pageSize)
            Debug.WriteLine($"  âš  API vrĂˇtilo {returned} > pageSize={pageSize} â€” parametr ignorovĂˇn!");
        else if (returned < pageSize && returned < _defaultPageCount)
            Debug.WriteLine($"  âš  API vrĂˇtilo {returned} < pageSize={pageSize} (defaultPageCount={_defaultPageCount}) â€” eToro bug?");
        else if (returned == _defaultPageCount && returned < pageSize)
            Debug.WriteLine($"  â„ą VrĂˇceno {returned} = defaultPageCount={_defaultPageCount} â‰¤ pageSize={pageSize} â€” ok, mĂ©nÄ› dat neĹľ strĂˇnka.");

        Assert.That(returned, Is.LessThanOrEqualTo(pageSize),
            $"API vrĂˇtilo {returned} trades, pĹ™estoĹľe pageSize={pageSize} â€” parametr ignorovĂˇn.");
    }

    // â”€â”€â”€ pageNumber indexovĂˇnĂ­ â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetTradeHistoryAsync_PageNumber_IndexingBehavior()
    {
        if (!_hasHistory)
        {
            Assert.Ignore("Ĺ˝ĂˇdnĂˇ obchodnĂ­ historie.");
            return;
        }

        var minDate  = DateTimeOffset.UtcNow.AddYears(-5);
        const int ps = 5;

        var page0 = await _client.GetTradeHistoryAsync(minDate, page: 0, pageSize: ps);
        var page1 = await _client.GetTradeHistoryAsync(minDate, page: 1, pageSize: ps);
        var page2 = await _client.GetTradeHistoryAsync(minDate, page: 2, pageSize: ps);

        var ids0 = page0.Trades.Select(t => t.PositionId).ToList();
        var ids1 = page1.Trades.Select(t => t.PositionId).ToList();
        var ids2 = page2.Trades.Select(t => t.PositionId).ToList();

        Debug.WriteLine($"[History] PageIndexing pageSize={ps}");
        Debug.WriteLine($"  page=0 ({page0.Trades.Count} trades): [{string.Join(",", ids0)}]");
        Debug.WriteLine($"  page=1 ({page1.Trades.Count} trades): [{string.Join(",", ids1)}]");
        Debug.WriteLine($"  page=2 ({page2.Trades.Count} trades): [{string.Join(",", ids2)}]");

        bool p0EqP1 = ids0.Count > 0 && ids1.Count > 0 && ids0.SequenceEqual(ids1);
        bool p1EqP2 = ids1.Count > 0 && ids2.Count > 0 && ids1.SequenceEqual(ids2);

        if (p0EqP1)
            Debug.WriteLine("  âš  page0 == page1 â€” eToro indexuje od 1 nebo strĂˇnkovĂˇnĂ­ nefunguje!");
        else if (ids0.Count > 0 && ids1.Count > 0)
            Debug.WriteLine("  âś“ page0 â‰  page1 â€” indexovĂˇnĂ­ od 0 funguje.");
        if (p1EqP2)
            Debug.WriteLine("  âš  page1 == page2 â€” strĂˇnkovĂˇnĂ­ nefunguje (stejnĂˇ data).");

        if (_defaultPageCount > ps)
            Assert.That(p0EqP1, Is.False,
                "page0 == page1 pĹ™estoĹľe existuje dost dat â€” strĂˇnkovĂˇnĂ­ nefunguje nebo indexovĂˇnĂ­ od 1.");
        else
            Assert.Pass($"Nedostatek dat pro ovÄ›Ĺ™enĂ­ strĂˇnkovĂˇnĂ­ (defaultPageCount={_defaultPageCount} â‰¤ pageSize={ps}).");
    }

    // â”€â”€â”€ minDate filtrovĂˇnĂ­ â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [TestCase(7,    TestName = "TradeHistory_MinDate_7DaysAgo")]
    [TestCase(30,   TestName = "TradeHistory_MinDate_30DaysAgo")]
    [TestCase(90,   TestName = "TradeHistory_MinDate_90DaysAgo")]
    [TestCase(365,  TestName = "TradeHistory_MinDate_1YearAgo")]
    [TestCase(1825, TestName = "TradeHistory_MinDate_5YearsAgo")]
    public async Task GetTradeHistoryAsync_MinDate_FilterWorks(int daysBack)
    {
        var minDate = DateTimeOffset.UtcNow.AddDays(-daysBack);
        var result  = await _client.GetTradeHistoryAsync(minDate, page: 0);

        Debug.WriteLine($"[History] minDate={minDate:yyyy-MM-dd} (daysBack={daysBack}) â†’ returned={result.Trades.Count}");

        if (result.Trades.Count == 0)
        {
            Debug.WriteLine("  â„ą Ĺ˝ĂˇdnĂ© obchody v danĂ©m obdobĂ­.");
            Assert.Pass($"Ĺ˝ĂˇdnĂ© obchody od {minDate:yyyy-MM-dd}.");
            return;
        }

        // OvÄ›Ĺ™enĂ­: closeTimestamp musĂ­ bĂ˝t >= minDate (eToro by mÄ›l filtrovat)
        var tooOld = result.Trades.Where(t => t.CloseTimestamp < minDate).ToList();
        if (tooOld.Count > 0)
        {
            Debug.WriteLine($"  âš  {tooOld.Count} obchodĹŻ mĂˇ CloseTimestamp pĹ™ed minDate={minDate:yyyy-MM-dd}:");
            foreach (var t in tooOld.Take(3))
                Debug.WriteLine($"    PositionId={t.PositionId} CloseTimestamp={t.CloseTimestamp:yyyy-MM-dd}");
        }

        // InformativnĂ­ â€” eToro mĹŻĹľe filtrovat dle openTimestamp nebo jinĂ©ho pole
        Assert.That(tooOld.Count, Is.EqualTo(0),
            $"{tooOld.Count} obchodĹŻ mĂˇ CloseTimestamp pĹ™ed minDate={minDate:yyyy-MM-dd} â€” filtr nefunguje?");
    }

    [Test]
    public async Task GetTradeHistoryAsync_ShortRange_vs_LongRange_ShortHasLessOrEqual()
    {
        var minDateShort = DateTimeOffset.UtcNow.AddDays(-30);
        var minDateLong  = DateTimeOffset.UtcNow.AddYears(-5);

        var shortResult = await _client.GetTradeHistoryAsync(minDateShort, page: 0);
        var longResult  = await _client.GetTradeHistoryAsync(minDateLong,  page: 0);

        Debug.WriteLine($"[History] 30d range = {shortResult.Trades.Count} trades");
        Debug.WriteLine($"[History] 5y range  = {longResult.Trades.Count} trades");

        if (shortResult.Trades.Count > longResult.Trades.Count)
            Debug.WriteLine("  âš  KratĹˇĂ­ rozsah vrĂˇtil vĂ­ce obchodĹŻ neĹľ delĹˇĂ­ â€” minDate filtr ignorovĂˇn?");

        Assert.That(shortResult.Trades.Count, Is.LessThanOrEqualTo(longResult.Trades.Count),
            "KratĹˇĂ­ ÄŤasovĂ˝ rozsah by mÄ›l vracet â‰¤ poÄŤet obchodĹŻ neĹľ delĹˇĂ­ rozsah.");
    }

    // â”€â”€â”€ data quality â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetTradeHistoryAsync_ClosedTrades_DataQuality()
    {
        if (!_hasHistory)
        {
            Assert.Ignore("Ĺ˝ĂˇdnĂˇ obchodnĂ­ historie.");
            return;
        }

        var result = await _client.GetTradeHistoryAsync(
            DateTimeOffset.UtcNow.AddYears(-1), page: 0, pageSize: 20);

        Debug.WriteLine($"[History] Kontrola kvality {result.Trades.Count} obchodĹŻ.");

        foreach (var t in result.Trades)
        {
            Debug.WriteLine($"  PositionId={t.PositionId} InstrumentId={t.InstrumentId} " +
                            $"OpenRate={t.OpenRate} CloseRate={t.CloseRate} " +
                            $"NetProfit={t.NetProfit:+0.00;-0.00} IsBuy={t.IsBuy} " +
                            $"Open={t.OpenTimestamp:yyyy-MM-dd} Close={t.CloseTimestamp:yyyy-MM-dd}");

            Assert.Multiple(() =>
            {
                Assert.That(t.PositionId, Is.GreaterThan(0),
                    "PositionId musĂ­ bĂ˝t kladnĂ©.");
                Assert.That(t.InstrumentId, Is.GreaterThan(0),
                    $"InstrumentId musĂ­ bĂ˝t kladnĂ© (PositionId={t.PositionId}).");
                Assert.That(t.OpenRate, Is.GreaterThan(0),
                    $"OpenRate musĂ­ bĂ˝t kladnĂ˝ (PositionId={t.PositionId}).");
                Assert.That(t.CloseRate, Is.GreaterThan(0),
                    $"CloseRate musĂ­ bĂ˝t kladnĂ˝ (PositionId={t.PositionId}).");
                Assert.That(t.Investment, Is.GreaterThanOrEqualTo(0),
                    $"Investment nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={t.PositionId}).");
                Assert.That(t.Units, Is.GreaterThan(0),
                    $"Units musĂ­ bĂ˝t kladnĂ˝ (PositionId={t.PositionId}).");
                Assert.That(t.CloseTimestamp, Is.GreaterThanOrEqualTo(t.OpenTimestamp),
                    $"CloseTimestamp musĂ­ bĂ˝t â‰Ą OpenTimestamp (PositionId={t.PositionId}).");
                Assert.That(t.Leverage, Is.GreaterThanOrEqualTo(1),
                    $"Leverage musĂ­ bĂ˝t â‰Ą 1 (PositionId={t.PositionId}).");
            });
        }
    }

    [Test]
    public async Task GetTradeHistoryAsync_Fees_AreNonNegative()
    {
        if (!_hasHistory)
        {
            Assert.Ignore("Ĺ˝ĂˇdnĂˇ obchodnĂ­ historie.");
            return;
        }

        var result = await _client.GetTradeHistoryAsync(
            DateTimeOffset.UtcNow.AddYears(-1), page: 0, pageSize: 50);

        var negFees = result.Trades.Where(t => t.Fees < 0).ToList();
        if (negFees.Count > 0)
        {
            Debug.WriteLine($"[History] âš  {negFees.Count} obchodĹŻ se zĂˇpornĂ˝mi fees:");
            foreach (var t in negFees.Take(5))
                Debug.WriteLine($"  PositionId={t.PositionId} Fees={t.Fees}");
        }

        Assert.That(negFees, Is.Empty, "Fees nesmĂ­ bĂ˝t zĂˇpornĂ©.");
    }

    [Test]
    public async Task GetTradeHistoryAsync_NoPageSize_vs_PageSize_DefaultBehavior()
    {
        var minDate = DateTimeOffset.UtcNow.AddYears(-2);

        var noSize = await _client.GetTradeHistoryAsync(minDate, page: 0);
        var size20 = await _client.GetTradeHistoryAsync(minDate, page: 0, pageSize: 20);

        Debug.WriteLine($"[History] Bez pageSize â†’ {noSize.Trades.Count} trades");
        Debug.WriteLine($"[History] pageSize=20 â†’ {size20.Trades.Count} trades");

        if (noSize.Trades.Count == size20.Trades.Count)
            Debug.WriteLine("  â„ą PoÄŤty se shodujĂ­ â€” moĹľnĂˇ defaultnĂ­ pageSize API je 20.");
        else if (noSize.Trades.Count > 20)
            Debug.WriteLine($"  â„ą DefaultnĂ­ pageSize API > 20 (vrĂˇceno {noSize.Trades.Count}).");

        Assert.That(size20.Trades.Count, Is.LessThanOrEqualTo(20),
            $"pageSize=20 ale vrĂˇceno {size20.Trades.Count} â€” parametr ignorovĂˇn.");
    }

    // â”€â”€â”€ client-side validace â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public void GetTradeHistoryAsync_NegativePage_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetTradeHistoryAsync(DateTimeOffset.UtcNow.AddYears(-1), page: -1),
            "Client by mÄ›l odmĂ­tnout zĂˇpornĂ© page.");
    }

    [Test]
    public void GetTradeHistoryAsync_ZeroPageSize_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetTradeHistoryAsync(DateTimeOffset.UtcNow.AddYears(-1), page: 0, pageSize: 0),
            "Client by mÄ›l odmĂ­tnout pageSize=0.");
    }

    [Test]
    public void GetTradeHistoryAsync_NegativePageSize_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetTradeHistoryAsync(DateTimeOffset.UtcNow.AddYears(-1), page: 0, pageSize: -1),
            "Client by mÄ›l odmĂ­tnout zĂˇpornĂ˝ pageSize.");
    }
}
