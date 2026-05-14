using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Configuration;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// RuÄŤnĂ­ integraÄŤnĂ­ testy pro Trading REAL sekci eToro API.
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "Category=Integration"
/// NutnĂ©: nastavit EToroOptions__ApiKey + EToroOptions__UserKey nebo lokĂˇlnĂ­ appsettings.test.json
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class TradingRealIntegrationTests
{
    private IEToroClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();
    }

    // â”€â”€â”€ PnL â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetPnlAsync_Real_ReturnsPortfolioData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Real);

        Debug.WriteLine($"Credit:         {result.Credit:F2} USD");
        Debug.WriteLine($"BonusCredit:    {result.BonusCredit:F2} USD");
        Debug.WriteLine($"UnrealizedPnL:  {result.UnrealizedPnL:+0.00;-0.00} USD");
        Debug.WriteLine($"Positions:      {result.Positions.Count}");
        Debug.WriteLine($"Mirrors:        {result.MirrorPortfolios.Count}");
        Debug.WriteLine($"OrdersForOpen:  {result.OrdersForOpen.Count}");

        Assert.That(result.Credit, Is.GreaterThanOrEqualTo(0));
    }

    // â”€â”€â”€ Portfolio â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetPortfolioAsync_Real_ReturnsPortfolioData()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Real);

        Debug.WriteLine($"Credit:     {result.Credit:F2} USD");
        Debug.WriteLine($"Positions:  {result.Positions.Count}");

        foreach (var pos in result.Positions.Take(5))
            Debug.WriteLine($"  InstrumentID={pos.InstrumentId} Invested={pos.InvestedAmount:F2} NetProfit={pos.NetProfit:F2}");

        Assert.That(result, Is.Not.Null);
    }

    // â”€â”€â”€ Order â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetOrderAsync_Real_WithKnownOrderId_ReturnsOrderDetails()
    {
        // Prerequisite: fill in a valid real orderId from your account.
        // You can get orderIds from GetPnlAsync positions or GetPortfolioAsync.
        const long orderId = 0L; // Replace with actual orderId
        Assume.That(orderId, Is.GreaterThan(0), "Set a valid orderId to run this test.");

        var result = await _client.GetOrderAsync(EToroEnvironment.Real, orderId);

        Debug.WriteLine($"OrderID:         {result.OrderId}");
        Debug.WriteLine($"StatusID:        {result.StatusId}");
        Debug.WriteLine($"OrderType:       {result.OrderType}");
        Debug.WriteLine($"InstrumentID:    {result.InstrumentId}");
        Debug.WriteLine($"Amount:          {result.Amount:F2}");
        Debug.WriteLine($"Units:           {result.Units}");
        Debug.WriteLine($"RequestOccurred: {result.RequestOccurred:u}");
        Debug.WriteLine($"Positions:       {result.Positions?.Count ?? 0}");

        if (result.Positions is not null)
        {
            foreach (var p in result.Positions)
                Debug.WriteLine($"  PositionID={p.PositionId} Rate={p.Rate} Units={p.Units} IsOpen={p.IsOpen}");
        }

        Assert.That(result.OrderId, Is.EqualTo(orderId));
    }

    // â”€â”€â”€ Trade History â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetTradeHistoryAsync_Real_ReturnsClosedTrades()
    {
        var minDate = DateTimeOffset.UtcNow.AddYears(-1);
        var result = await _client.GetTradeHistoryAsync(minDate, page: 0);

        Debug.WriteLine($"Trades on page: {result.Trades.Count}");

        foreach (var t in result.Trades.Take(5))
            Debug.WriteLine($"  PositionID={t.PositionId} Instrument={t.InstrumentId} NetProfit={t.NetProfit:+0.00;-0.00} Close={t.CloseTimestamp:yyyy-MM-dd}");

        Assert.That(result.Trades, Is.Not.Null);
    }

    [Test]
    public async Task GetTradeHistoryAsync_Real_WithPagination_ReturnsPage()
    {
        var minDate = DateTimeOffset.UtcNow.AddYears(-6);

        var page0 = await _client.GetTradeHistoryAsync(minDate, page: 0);
        var page1 = await _client.GetTradeHistoryAsync(minDate, page: 1);

        Debug.WriteLine($"Page 0 trades: {page0.Trades.Count}");
        Debug.WriteLine($"Page 1 trades: {page1.Trades.Count}");

        Assert.That(page0.Trades, Is.Not.Null);
        Assert.That(page1.Trades, Is.Not.Null);
    }
}
