using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Common;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Tests.Integration;

/// <summary>
/// Manual integration tests for the Trading DEMO section of the eToro API.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires EToroOptions__ApiKey + EToroOptions__UserKey or local appsettings.test.json.
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Manual run only - requires real API credentials outside git")]
public class TradingDemoIntegrationTests
{
    private IEToroClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();
    }

    [Test]
    public async Task GetPnlAsync_Demo_ReturnsPortfolioData()
    {
        var result = await _client.GetPnlAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"Credit:         {result.Credit:F2} USD");
        Debug.WriteLine($"BonusCredit:    {result.BonusCredit:F2} USD");
        Debug.WriteLine($"UnrealizedPnL:  {result.UnrealizedPnL:+0.00;-0.00} USD");
        Debug.WriteLine($"Positions:      {result.Positions.Count}");
        Debug.WriteLine($"Mirrors:        {result.MirrorPortfolios.Count}");
        Debug.WriteLine($"OrdersForOpen:  {result.OrdersForOpen.Count}");

        Assert.That(result.Credit, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task GetPortfolioAsync_Demo_UsesSharedPortfolioEndpoint()
    {
        var result = await _client.GetPortfolioAsync(EToroEnvironment.Demo);

        Debug.WriteLine($"Credit:     {result.Credit:F2} USD");
        Debug.WriteLine($"Positions:  {result.Positions.Count}");

        foreach (var position in result.Positions.Take(5))
        {
            Debug.WriteLine(
                $"  InstrumentID={position.InstrumentId} " +
                $"Invested={position.InvestedAmount:F2} " +
                $"NetProfit={position.NetProfit:F2}");
        }

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetOrderAsync_Demo_WithKnownOrderId_ReturnsOrderDetails()
    {
        // Prerequisite: fill in a valid demo orderId from your account.
        const long orderId = 0L;
        Assume.That(orderId, Is.GreaterThan(0), "Set a valid demo orderId to run this test.");

        var result = await _client.GetOrderAsync(EToroEnvironment.Demo, orderId);

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
            foreach (var position in result.Positions)
            {
                Debug.WriteLine(
                    $"  PositionID={position.PositionId} " +
                    $"Rate={position.Rate} " +
                    $"Units={position.Units} " +
                    $"IsOpen={position.IsOpen}");
            }
        }

        Assert.That(result.OrderId, Is.EqualTo(orderId));
    }
}
