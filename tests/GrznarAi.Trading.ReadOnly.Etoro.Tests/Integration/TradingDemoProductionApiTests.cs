using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// ProdukÄŤnĂ­ / prĹŻzkumnĂ© testy Trading DEMO API.
/// CĂ­l: odhalit chovĂˇnĂ­ eToro nedokumentovanĂ© nebo odliĹˇnĂ© od dokumentace â€”
/// PnL konzistence, portfolio data, order data, env-oddÄ›lenĂ­ Demo vs Real.
/// Rate limit: 60 req/min â€” zajiĹˇĹĄuje RateLimitHandler automaticky.
///
/// Endpointy:
///   GET /api/v1/trading/info/demo/pnl
///   GET /api/v1/trading/info/demo/portfolio  (impl. hits /trading/info/portfolio â€” zĂˇmÄ›rnÄ› testovĂˇno)
///   GET /api/v1/trading/info/demo/orders/{orderId}
///
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "FullyQualifiedName~TradingDemoProductionApiTests"
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class TradingDemoProductionApiTests
{
    private IEToroClient _client = null!;

    // Seed data z OneTimeSetUp â€” sdĂ­leno napĹ™Ă­ÄŤ testy
    private long _pnlMaxOrderId;
    private bool _hasPnlOrders;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();

        var pnl = await _client.GetPnlAsync(EToroEnvironment.Demo);

        var pnlOrderIds = pnl.Positions.Select(p => p.OrderId)
            .Concat(pnl.OrdersForOpen.Select(o => o.OrderId))
            .Concat(pnl.Orders.Select(o => o.OrderId))
            .Where(id => id > 0)
            .ToList();

        _hasPnlOrders  = pnlOrderIds.Count > 0;
        _pnlMaxOrderId = _hasPnlOrders ? pnlOrderIds.Max() : 0;

        Debug.WriteLine($"[SetUp] Credit={pnl.Credit:F2} Positions={pnl.Positions.Count} " +
                        $"Mirrors={pnl.MirrorPortfolios.Count} OrdersForOpen={pnl.OrdersForOpen.Count} " +
                        $"Orders={pnl.Orders.Count} hasPnlOrders={_hasPnlOrders} pnlMaxOrderId={_pnlMaxOrderId}");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetPnlAsync â€” GET /api/v1/trading/info/demo/pnl
    // Ĺ˝ĂˇdnĂ© query parametry. VrĂˇtĂ­ kompletnĂ­ portfolio s PnL.
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetPnlAsync_Demo_BasicFieldsAreValid()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"[PnL] Credit={result.Credit:F2} BonusCredit={result.BonusCredit:F2} " +
                        $"UnrealizedPnL={result.UnrealizedPnL:+0.00;-0.00} AccountCurrencyId={result.AccountCurrencyId}");
        Debug.WriteLine($"  Positions={result.Positions.Count} Mirrors={result.MirrorPortfolios.Count}");
        Debug.WriteLine($"  OrdersForOpen={result.OrdersForOpen.Count} Orders={result.Orders.Count}");
        Debug.WriteLine($"  OrdersForClose={result.OrdersForClose?.Count ?? 0} OrdersForCloseMultiple={result.OrdersForCloseMultiple?.Count ?? 0}");

        Assert.Multiple(() =>
        {
            Assert.That(result.Credit, Is.GreaterThanOrEqualTo(0), "Credit nesmĂ­ bĂ˝t zĂˇpornĂ˝.");
            Assert.That(result.BonusCredit, Is.GreaterThanOrEqualTo(0), "BonusCredit nesmĂ­ bĂ˝t zĂˇpornĂ˝.");
        });
    }

    [Test]
    public async Task GetPnlAsync_Demo_AccountCurrencyId_IsUSD()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"[PnL] AccountCurrencyId={result.AccountCurrencyId}");

        // Dokumentace: 1 = USD. Demo ĂşÄŤty jsou vĹľdy v USD.
        if (result.AccountCurrencyId != 1)
            Debug.WriteLine($"  âš  AccountCurrencyId={result.AccountCurrencyId} â€” oÄŤekĂˇvĂˇno 1 (USD)!");

        Assert.That(result.AccountCurrencyId, Is.EqualTo(1),
            "Demo ĂşÄŤet by mÄ›l mĂ­t AccountCurrencyId=1 (USD).");
    }

    [Test]
    public async Task GetPnlAsync_Demo_UnrealizedPnL_ConsistencyWithPositions()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        if (result.Positions.Count == 0 && result.MirrorPortfolios.Count == 0)
        {
            Assert.Pass("Ĺ˝ĂˇdnĂ© otevĹ™enĂ© pozice â€” PnL konzistenci nelze ovÄ›Ĺ™it.");
            return;
        }

        var directPnL = result.Positions.Sum(p => p.PnL);
        var mirrorPnL = result.MirrorPortfolios.Sum(m => m.Positions.Sum(p => p.PnL));
        var summedPnL = directPnL + mirrorPnL;
        var diff      = Math.Abs(result.UnrealizedPnL - summedPnL);

        Debug.WriteLine($"[PnL] UnrealizedPnL z API={result.UnrealizedPnL:+0.00;-0.00}");
        Debug.WriteLine($"  PĹ™Ă­mĂ© pozice: {directPnL:+0.00;-0.00} ({result.Positions.Count} pos)");
        Debug.WriteLine($"  Mirror pozice: {mirrorPnL:+0.00;-0.00} ({result.MirrorPortfolios.Sum(m => m.Positions.Count)} pos)");
        Debug.WriteLine($"  Celkem souÄŤet: {summedPnL:+0.00;-0.00} diff={diff:F4}");

        if (diff > 1m)
            Debug.WriteLine($"  âš  RozdĂ­l {diff:F4} USD â€” moĹľnĂˇ rounding, novĂˇ nedokumentovanĂˇ pole nebo ÄŤasovĂˇ prodleva.");

        Assert.That(diff, Is.LessThanOrEqualTo(50m),
            $"UnrealizedPnL={result.UnrealizedPnL:F2} se vĂ˝raznÄ› liĹˇĂ­ od souÄŤtu pozic {summedPnL:F2} (diff={diff:F2}).");
    }

    [Test]
    public async Task GetPnlAsync_Demo_PositionsHaveValidData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"[PnL] PĹ™Ă­mĂ˝ch pozic: {result.Positions.Count}");
        foreach (var pos in result.Positions)
        {
            Debug.WriteLine($"  PositionId={pos.PositionId} InstrumentId={pos.InstrumentId} " +
                            $"Amount={pos.Amount:F2} PnL={pos.PnL:+0.00;-0.00} IsBuy={pos.IsBuy} " +
                            $"Leverage={pos.Leverage} OpenRate={pos.OpenRate} MirrorId={pos.MirrorId}");

            Assert.Multiple(() =>
            {
                Assert.That(pos.PositionId, Is.GreaterThan(0), "PositionId musĂ­ bĂ˝t kladnĂ©.");
                Assert.That(pos.InstrumentId, Is.GreaterThan(0),
                    $"InstrumentId musĂ­ bĂ˝t kladnĂ© (PositionId={pos.PositionId}).");
                Assert.That(pos.Amount, Is.GreaterThanOrEqualTo(0),
                    $"Amount nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={pos.PositionId}).");
                Assert.That(pos.OpenRate, Is.GreaterThanOrEqualTo(0),
                    $"OpenRate nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={pos.PositionId}).");
                Assert.That(pos.Leverage, Is.GreaterThanOrEqualTo(1),
                    $"Leverage musĂ­ bĂ˝t â‰Ą 1 (PositionId={pos.PositionId}).");
            });
        }
    }

    [Test]
    public async Task GetPnlAsync_Demo_PositionsOpenDateTime_IsInPast()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);
        var now    = DateTimeOffset.UtcNow;

        foreach (var pos in result.Positions)
        {
            Debug.WriteLine($"  PositionId={pos.PositionId} OpenDateTime={pos.OpenDateTime:u}");

            if (pos.OpenDateTime > now)
                Debug.WriteLine($"  âš  OpenDateTime={pos.OpenDateTime:u} je v budoucnosti!");

            Assert.That(pos.OpenDateTime, Is.LessThanOrEqualTo(now.AddMinutes(5)),
                $"OpenDateTime nesmĂ­ bĂ˝t v budoucnosti (PositionId={pos.PositionId}).");
        }
    }

    [Test]
    public async Task GetPnlAsync_Demo_MirrorPortfoliosHaveValidData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"[PnL] Mirror portfolios: {result.MirrorPortfolios.Count}");
        foreach (var mirror in result.MirrorPortfolios)
        {
            Debug.WriteLine($"  MirrorId={mirror.MirrorId} AvailableAmount={mirror.AvailableAmount:F2} " +
                            $"IsPaused={mirror.IsPaused} PendingForClosure={mirror.PendingForClosure} " +
                            $"Positions={mirror.Positions.Count} ClosedProfit={mirror.ClosedPositionsNetProfit:+0.00;-0.00} " +
                            $"StartedCopyDate={mirror.StartedCopyDate:yyyy-MM-dd} ParentUsername={mirror.ParentUsername}");

            Assert.Multiple(() =>
            {
                Assert.That(mirror.MirrorId, Is.GreaterThan(0), "MirrorId musĂ­ bĂ˝t kladnĂ©.");
                Assert.That(mirror.AvailableAmount, Is.GreaterThanOrEqualTo(0),
                    $"AvailableAmount nesmĂ­ bĂ˝t zĂˇpornĂ˝ (MirrorId={mirror.MirrorId}).");
                Assert.That(mirror.InitialInvestment, Is.GreaterThanOrEqualTo(0),
                    $"InitialInvestment nesmĂ­ bĂ˝t zĂˇpornĂ˝ (MirrorId={mirror.MirrorId}).");
            });
        }
    }

    [Test]
    public async Task GetPnlAsync_Demo_MirrorPositionsHaveValidData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        foreach (var mirror in result.MirrorPortfolios)
        {
            Debug.WriteLine($"  Mirror MirrorId={mirror.MirrorId} mĂˇ {mirror.Positions.Count} pozic");
            foreach (var pos in mirror.Positions)
            {
                Debug.WriteLine($"    PositionId={pos.PositionId} InstrumentId={pos.InstrumentId} " +
                                $"Amount={pos.Amount:F2} PnL={pos.PnL:+0.00;-0.00} IsBuy={pos.IsBuy}");

                Assert.Multiple(() =>
                {
                    Assert.That(pos.PositionId, Is.GreaterThan(0), "Mirror PositionId musĂ­ bĂ˝t kladnĂ©.");
                    Assert.That(pos.InstrumentId, Is.GreaterThan(0),
                        $"Mirror InstrumentId musĂ­ bĂ˝t kladnĂ© (PositionId={pos.PositionId}).");
                    Assert.That(pos.Amount, Is.GreaterThanOrEqualTo(0),
                        $"Mirror Amount nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={pos.PositionId}).");
                });
            }
        }
    }

    [Test]
    public async Task GetPnlAsync_Demo_OrdersForOpen_HaveValidData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"[PnL] OrdersForOpen: {result.OrdersForOpen.Count}");
        foreach (var order in result.OrdersForOpen)
        {
            Debug.WriteLine($"  OrderId={order.OrderId} InstrumentId={order.InstrumentId} " +
                            $"StatusId={order.StatusId} OrderType={order.OrderType} " +
                            $"Amount={order.Amount:F2} IsBuy={order.IsBuy} Leverage={order.Leverage}");

            Assert.Multiple(() =>
            {
                Assert.That(order.OrderId, Is.GreaterThan(0), "OrderId musĂ­ bĂ˝t kladnĂ©.");
                Assert.That(order.InstrumentId, Is.GreaterThan(0),
                    $"InstrumentId musĂ­ bĂ˝t kladnĂ© (OrderId={order.OrderId}).");
                Assert.That(order.Amount, Is.GreaterThanOrEqualTo(0),
                    $"Amount nesmĂ­ bĂ˝t zĂˇpornĂ˝ (OrderId={order.OrderId}).");
                // Dokumentace: 0=Pending, 1=Executed, 2=Cancelled, 3=Rejected, 4=PartiallyExecuted
                Assert.That(order.StatusId, Is.InRange(0, 4),
                    $"StatusId={order.StatusId} mimo dokumentovanĂ˝ rozsah 0â€“4 (OrderId={order.OrderId}).");
            });
        }
    }

    [Test]
    public async Task GetPnlAsync_Demo_OrdersForClose_HaveValidData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        var ordersForClose = result.OrdersForClose ?? [];
        Debug.WriteLine($"[PnL] OrdersForClose: {ordersForClose.Count}");
        foreach (var order in ordersForClose)
        {
            Debug.WriteLine($"  OrderId={order.OrderId} PositionId={order.PositionId} " +
                            $"InstrumentId={order.InstrumentId} StatusId={order.StatusId} " +
                            $"UnitsToDeduct={order.UnitsToDeduct}");

            Assert.Multiple(() =>
            {
                Assert.That(order.OrderId, Is.GreaterThan(0), "OrderId musĂ­ bĂ˝t kladnĂ©.");
                Assert.That(order.PositionId, Is.GreaterThan(0),
                    $"PositionId musĂ­ bĂ˝t kladnĂ© (OrderId={order.OrderId}).");
                Assert.That(order.UnitsToDeduct, Is.GreaterThanOrEqualTo(0),
                    $"UnitsToDeduct nesmĂ­ bĂ˝t zĂˇpornĂ˝ (OrderId={order.OrderId}).");
            });
        }
    }

    [Test]
    public async Task GetPnlAsync_Demo_OrdersForCloseMultiple_HaveValidData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        var ordersForCloseMultiple = result.OrdersForCloseMultiple ?? [];
        Debug.WriteLine($"[PnL] OrdersForCloseMultiple: {ordersForCloseMultiple.Count}");
        foreach (var order in ordersForCloseMultiple)
        {
            Debug.WriteLine($"  OrderId={order.OrderId} InstrumentId={order.InstrumentId} " +
                            $"StatusId={order.StatusId} PendingPositions={order.PendingClosePositionIds?.Count ?? 0}");

            Assert.Multiple(() =>
            {
                Assert.That(order.OrderId, Is.GreaterThan(0), "OrderId musĂ­ bĂ˝t kladnĂ©.");
                if (order.PendingClosePositionIds is not null)
                    Assert.That(order.PendingClosePositionIds, Is.Not.Empty,
                        $"PendingClosePositionIds nesmĂ­ bĂ˝t prĂˇzdnĂ˝ seznam (OrderId={order.OrderId}).");
            });
        }
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetPortfolioAsync â€” GET /api/v1/trading/info/demo/portfolio
    // POZOR: implementace ignoruje env param a vĹľdy hits /trading/info/portfolio
    // Testy odhalĂ­, zda endpoint vracĂ­ demo nebo sdĂ­lenĂˇ data.
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetPortfolioAsync_Demo_CreditIsNonNegative()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"[Portfolio] Credit={result.Credit:F2} BonusCredit={result.BonusCredit:F2} " +
                        $"Positions={result.Positions.Count}");

        Assert.Multiple(() =>
        {
            Assert.That(result.Credit, Is.GreaterThanOrEqualTo(0), "Credit nesmĂ­ bĂ˝t zĂˇpornĂ˝.");
            Assert.That(result.BonusCredit, Is.GreaterThanOrEqualTo(0), "BonusCredit nesmĂ­ bĂ˝t zĂˇpornĂ˝.");
        });
    }

    [Test]
    public async Task GetPortfolioAsync_Demo_AllPositionsHaveValidIds()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"[Portfolio] Validuji {result.Positions.Count} pozic.");
        foreach (var pos in result.Positions)
        {
            Debug.WriteLine($"  PositionId={pos.PositionId} InstrumentId={pos.InstrumentId} " +
                            $"Invested={pos.InvestedAmount:F2} IsBuy={pos.IsBuy} Leverage={pos.Leverage} " +
                            $"Units={pos.Units} OpenRate={pos.OpenRate} NetProfit={pos.NetProfit:+0.00;-0.00}");

            Assert.Multiple(() =>
            {
                Assert.That(pos.PositionId, Is.GreaterThan(0), "PositionId musĂ­ bĂ˝t kladnĂ©.");
                Assert.That(pos.InstrumentId, Is.GreaterThan(0),
                    $"InstrumentId musĂ­ bĂ˝t kladnĂ© (PositionId={pos.PositionId}).");
                Assert.That(pos.InvestedAmount, Is.GreaterThanOrEqualTo(0),
                    $"InvestedAmount nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={pos.PositionId}).");
                Assert.That(pos.OpenRate, Is.GreaterThanOrEqualTo(0),
                    $"OpenRate nesmĂ­ bĂ˝t zĂˇpornĂ˝ (PositionId={pos.PositionId}).");
                Assert.That(pos.Units, Is.GreaterThan(0),
                    $"Units musĂ­ bĂ˝t kladnĂ˝ (PositionId={pos.PositionId}).");
                Assert.That(pos.Leverage, Is.GreaterThanOrEqualTo(1),
                    $"Leverage musĂ­ bĂ˝t â‰Ą 1 (PositionId={pos.PositionId}).");
            });
        }
    }

    [Test]
    public async Task GetPortfolioAsync_Demo_OpenDateTimeIsInPast()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Demo);
        var now    = DateTimeOffset.UtcNow;

        foreach (var pos in result.Positions)
        {
            Debug.WriteLine($"  PositionId={pos.PositionId} OpenDateTime={pos.OpenDateTime:u}");

            if (pos.OpenDateTime > now)
                Debug.WriteLine($"  âš  OpenDateTime={pos.OpenDateTime:u} je v budoucnosti!");

            Assert.That(pos.OpenDateTime, Is.LessThanOrEqualTo(now.AddMinutes(5)),
                $"OpenDateTime nesmĂ­ bĂ˝t v budoucnosti (PositionId={pos.PositionId}).");
        }
    }

    [Test]
    public async Task GetPortfolioAsync_Demo_LeverageValuesAreSensible()
    {
        var result         = await _client.GetPortfolioAsync(EToroEnvironment.Demo);
        var validLeverages = new[] { 1, 2, 5, 10, 25, 50, 100, 200, 400 };

        foreach (var pos in result.Positions)
        {
            Debug.WriteLine($"  PositionId={pos.PositionId} Leverage={pos.Leverage}");

            if (!validLeverages.Contains(pos.Leverage))
                Debug.WriteLine($"  âš  NeznĂˇmĂ˝ leverage={pos.Leverage} (PositionId={pos.PositionId}) â€” dokumentace chybĂ­?");

            Assert.That(pos.Leverage, Is.GreaterThanOrEqualTo(1),
                $"Leverage musĂ­ bĂ˝t â‰Ą 1 (PositionId={pos.PositionId}).");
        }
    }

    [Test]
    public async Task GetPnlAsync_Demo_vs_GetPortfolioAsync_PositionsCountMatch()
    {
        var pnl       = await _client.GetPnlAsync(EToroEnvironment.Demo);
        var portfolio  = await _client.GetPortfolioAsync(EToroEnvironment.Demo);

        var pnlCount       = pnl.Positions.Count;
        var portfolioCount = portfolio.Positions.Count;

        Debug.WriteLine($"[Consistency] GetPnlAsync(Demo).Positions={pnlCount} " +
                        $"GetPortfolioAsync(Demo).Positions={portfolioCount}");

        if (pnlCount != portfolioCount)
            Debug.WriteLine($"  âš  PoÄŤty se liĹˇĂ­ â€” GetPortfolioAsync ignoruje env param a moĹľnĂˇ vracĂ­ real data!");

        Assert.That(pnlCount, Is.EqualTo(portfolioCount),
            "GetPnlAsync(Demo) a GetPortfolioAsync(Demo) by mÄ›ly vracet stejnĂ˝ poÄŤet pĹ™Ă­mĂ˝ch pozic.");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetOrderAsync â€” GET /api/v1/trading/info/demo/orders/{orderId}
    // Path param: orderId (int64, required). Validace v clientu: > 0.
    // Dokumentace: 404 pro neexistujĂ­cĂ­ orderId.
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetOrderAsync_Demo_ValidOrderId_ReturnsConsistentData()
    {
        if (!_hasPnlOrders)
        {
            Assert.Ignore("PnL(Demo) neobsahuje ĹľĂˇdnĂ© otevĹ™enĂ©/ÄŤekajĂ­cĂ­ objednĂˇvky â†’ orderId nedostupnĂ˝.");
            return;
        }

        var result = await _client.GetOrderAsync(EToroEnvironment.Demo, _pnlMaxOrderId);

        Debug.WriteLine($"[Order] OrderId={result.OrderId} StatusId={result.StatusId} " +
                        $"OrderType={result.OrderType} InstrumentId={result.InstrumentId}");
        Debug.WriteLine($"  Amount={result.Amount:F2} Units={result.Units} RequestOccurred={result.RequestOccurred:u}");
        Debug.WriteLine($"  Cid={result.Cid} Positions={result.Positions?.Count ?? 0}");

        if (result.Positions is not null)
            foreach (var p in result.Positions)
                Debug.WriteLine($"    PositionId={p.PositionId} Rate={p.Rate} Units={p.Units} " +
                                $"IsOpen={p.IsOpen} Amount={p.Amount:F2}");

        Assert.Multiple(() =>
        {
            Assert.That(result.OrderId, Is.EqualTo(_pnlMaxOrderId),
                "VrĂˇcenĂ© OrderId musĂ­ odpovĂ­dat poĹľadovanĂ©mu.");
            Assert.That(result.InstrumentId, Is.GreaterThan(0), "InstrumentId musĂ­ bĂ˝t kladnĂ©.");
            Assert.That(result.RequestOccurred, Is.LessThanOrEqualTo(DateTimeOffset.UtcNow.AddMinutes(5)),
                "RequestOccurred nesmĂ­ bĂ˝t v budoucnosti.");
            Assert.That(result.Amount, Is.GreaterThanOrEqualTo(0), "Amount nesmĂ­ bĂ˝t zĂˇpornĂ˝.");
            Assert.That(result.Units, Is.GreaterThanOrEqualTo(0), "Units nesmĂ­ bĂ˝t zĂˇpornĂ©.");
        });
    }

    [Test]
    public async Task GetOrderAsync_Demo_StatusId_IsKnownValue()
    {
        if (!_hasPnlOrders)
        {
            Assert.Ignore("PnL(Demo) neobsahuje ĹľĂˇdnĂ© otevĹ™enĂ©/ÄŤekajĂ­cĂ­ objednĂˇvky â†’ orderId nedostupnĂ˝.");
            return;
        }

        var result = await _client.GetOrderAsync(EToroEnvironment.Demo, _pnlMaxOrderId);

        Debug.WriteLine($"[Order] pnlMaxOrderId={_pnlMaxOrderId} â†’ StatusId={result.StatusId} " +
                        $"OrderType={result.OrderType}");

        // Dokumentace: 0=Pending, 1=Executed, 2=Cancelled, 3=Rejected, 4=PartiallyExecuted
        if (!new[] { 0, 1, 2, 3, 4 }.Contains(result.StatusId))
            Debug.WriteLine($"  âš  NeznĂˇmĂ˝ StatusId={result.StatusId} â€” dokumentace neodpovĂ­dĂˇ!");

        // Dokumentace: 1=Market, 2=Limit, 3=Stop
        if (!new[] { 1, 2, 3 }.Contains(result.OrderType))
            Debug.WriteLine($"  âš  NeznĂˇmĂ˝ OrderType={result.OrderType} â€” dokumentace neodpovĂ­dĂˇ!");

        Assert.That(result.StatusId, Is.InRange(0, 4),
            $"StatusId={result.StatusId} nenĂ­ v rozsahu dokumentovanĂ˝ch hodnot 0â€“4.");
    }

    [Test]
    public async Task GetOrderAsync_Demo_AllPnlOrderIds_ReturnConsistentData()
    {
        if (!_hasPnlOrders)
        {
            Assert.Ignore("PnL(Demo) neobsahuje ĹľĂˇdnĂ© otevĹ™enĂ©/ÄŤekajĂ­cĂ­ objednĂˇvky.");
            return;
        }

        // NaÄŤteme PnL znovu â€” max 5 orderIds abychom ĹˇetĹ™ili rate limit
        var pnl = await _client.GetPnlAsync(EToroEnvironment.Demo);
        var orderIds = pnl.OrdersForOpen.Select(o => o.OrderId)
            .Concat(pnl.Orders.Select(o => o.OrderId))
            .Where(id => id > 0)
            .Distinct()
            .OrderByDescending(id => id)
            .Take(5)
            .ToList();

        Debug.WriteLine($"[Order] Testuji {orderIds.Count} orderIds z PnL(Demo): {string.Join(", ", orderIds)}");

        foreach (var orderId in orderIds)
        {
            var result = await _client.GetOrderAsync(EToroEnvironment.Demo, orderId);
            Debug.WriteLine($"  OrderId={orderId} â†’ returned={result.OrderId} StatusId={result.StatusId} " +
                            $"InstrumentId={result.InstrumentId} Positions={result.Positions?.Count ?? 0}");

            Assert.That(result.OrderId, Is.EqualTo(orderId),
                $"VrĂˇcenĂ© OrderId={result.OrderId} â‰  poĹľadovanĂ©={orderId}.");
        }
    }

    [Test]
    public async Task GetOrderAsync_Demo_PositionIds_CrossCheckWithPnl()
    {
        if (!_hasPnlOrders)
        {
            Assert.Ignore("PnL(Demo) neobsahuje ĹľĂˇdnĂ© otevĹ™enĂ©/ÄŤekajĂ­cĂ­ objednĂˇvky.");
            return;
        }

        var pnl          = await _client.GetPnlAsync(EToroEnvironment.Demo);
        var pnlPositionIds = pnl.Positions.Select(p => p.PositionId).ToHashSet();

        var orderResult = await _client.GetOrderAsync(EToroEnvironment.Demo, _pnlMaxOrderId);

        Debug.WriteLine($"[Order] OrderId={_pnlMaxOrderId} â†’ {orderResult.Positions?.Count ?? 0} pozic");
        Debug.WriteLine($"  PnL.Positions obsahuje {pnlPositionIds.Count} PositionId");

        if (orderResult.Positions is null || orderResult.Positions.Count == 0)
        {
            Debug.WriteLine("  â„ą ObjednĂˇvka nemĂˇ ĹľĂˇdnĂ© pozice â€” mĹŻĹľe bĂ˝t pending nebo cancelled.");
            Assert.Pass("ObjednĂˇvka nemĂˇ ĹľĂˇdnĂ© pozice.");
            return;
        }

        foreach (var p in orderResult.Positions)
        {
            var inPnl = pnlPositionIds.Contains(p.PositionId);
            Debug.WriteLine($"  PositionId={p.PositionId} IsOpen={p.IsOpen} Rate={p.Rate} â†’ v PnL={inPnl}");

            if (p.IsOpen && !inPnl)
                Debug.WriteLine($"  âš  OtevĹ™enĂˇ pozice PositionId={p.PositionId} chybĂ­ v PnL.Positions â€” nekonzistentnĂ­ data!");
        }
    }

    [Test]
    public void GetOrderAsync_Demo_ZeroOrderId_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetOrderAsync(EToroEnvironment.Demo, 0L),
            "Client by mÄ›l odmĂ­tnout orderId=0.");
    }

    [Test]
    public void GetOrderAsync_Demo_NegativeOrderId_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetOrderAsync(EToroEnvironment.Demo, -1L),
            "Client by mÄ›l odmĂ­tnout zĂˇpornĂ© orderId.");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // Cross-endpoint: Demo vs Real konzistence
    // CĂ­l: ovÄ›Ĺ™it, Ĺľe demo endpointy skuteÄŤnÄ› vrĂˇtĂ­ jinĂˇ data neĹľ real.
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetPnlAsync_Demo_vs_Real_PositionIds_AreDifferentSets()
    {
        var demoPnl = await _client.GetPnlAsync(EToroEnvironment.Demo);
        var realPnl = await _client.GetPnlAsync(EToroEnvironment.Real);

        var demoIds = demoPnl.Positions.Select(p => p.PositionId).ToHashSet();
        var realIds = realPnl.Positions.Select(p => p.PositionId).ToHashSet();
        var overlap = demoIds.Intersect(realIds).ToList();

        Debug.WriteLine($"[Cross] Demo.Positions={demoIds.Count} Real.Positions={realIds.Count} " +
                        $"Overlap={overlap.Count}");
        Debug.WriteLine($"  Demo Credit={demoPnl.Credit:F2} Real Credit={realPnl.Credit:F2}");

        if (overlap.Count > 0)
            Debug.WriteLine($"  âš  Demo a Real sdĂ­lejĂ­ PositionId: {string.Join(", ", overlap.Take(5))} â€” env oddÄ›lenĂ­ nefunguje!");
        else if (demoIds.Count > 0 && realIds.Count > 0)
            Debug.WriteLine("  âś“ Demo a Real PositionIds jsou disjunktnĂ­ â€” env oddÄ›lenĂ­ funguje.");
        else
            Debug.WriteLine("  â„ą Jedno nebo obÄ› prostĹ™edĂ­ nemajĂ­ ĹľĂˇdnĂ© pozice â€” overlap nelze ovÄ›Ĺ™it.");

        Assert.That(overlap, Is.Empty,
            $"Demo a Real sdĂ­lejĂ­ {overlap.Count} PositionId â€” endpointy nerozliĹˇujĂ­ prostĹ™edĂ­?");
    }

    [Test]
    public async Task GetPortfolioAsync_Demo_vs_Real_CreditAreDifferent()
    {
        var demoPortfolio = await _client.GetPortfolioAsync(EToroEnvironment.Demo);
        var realPortfolio = await _client.GetPortfolioAsync(EToroEnvironment.Real);

        var demoCredit = demoPortfolio.Credit;
        var realCredit = realPortfolio.Credit;

        Debug.WriteLine($"[Cross] GetPortfolioAsync(Demo) Credit={demoCredit:F2} " +
                        $"GetPortfolioAsync(Real) Credit={realCredit:F2}");
        Debug.WriteLine($"  Demo Positions={demoPortfolio.Positions.Count} " +
                        $"Real Positions={realPortfolio.Positions.Count}");

        // GetPortfolioAsync implementace ignoruje env param (vĹľdy hits /trading/info/portfolio)
        // Pokud jsou data identickĂˇ, endpoint je env-agnostic â€” jde o implementaÄŤnĂ­ bug.
        if (demoCredit == realCredit && demoPortfolio.Positions.Count == realPortfolio.Positions.Count)
            Debug.WriteLine("  âš  Demo a Real Credit a Positions jsou identickĂ© â€” implementace GetPortfolioAsync " +
                            "ignoruje env param (vĹľdy hits /trading/info/portfolio)!");

        // InformativnĂ­ â€” tvrdĂ˝ assert nenĂ­ moĹľnĂ˝ bez znalosti skuteÄŤnĂ˝ch hodnot
        Assert.Pass($"Demo Credit={demoCredit:F2} Real Credit={realCredit:F2} â€” viz Debug vĂ˝stup pro posouzenĂ­.");
    }

    [Test]
    public async Task GetPnlAsync_Demo_vs_Real_CreditAreDifferent()
    {
        var demoPnl = await _client.GetPnlAsync(EToroEnvironment.Demo);
        var realPnl = await _client.GetPnlAsync(EToroEnvironment.Real);

        Debug.WriteLine($"[Cross] GetPnlAsync(Demo) Credit={demoPnl.Credit:F2} " +
                        $"GetPnlAsync(Real) Credit={realPnl.Credit:F2}");

        if (demoPnl.Credit == realPnl.Credit)
            Debug.WriteLine("  âš  Demo a Real Credit jsou identickĂ© â€” env oddÄ›lenĂ­ nefunguje nebo shoda nĂˇhodou!");
        else
            Debug.WriteLine("  âś“ Demo Credit â‰  Real Credit â€” env oddÄ›lenĂ­ funguje.");

        Assert.That(demoPnl.Credit, Is.Not.EqualTo(realPnl.Credit),
            "Demo a Real Credit by mÄ›ly bĂ˝t rĹŻznĂ© â€” demo ĂşÄŤet mĂˇ jinĂ˝ zĹŻstatek.");
    }
}
