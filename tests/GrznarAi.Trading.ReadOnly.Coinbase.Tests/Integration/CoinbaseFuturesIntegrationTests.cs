using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API — US Derivatives (CFM Futures).
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// Note: some tests require a CFM futures-enabled account; they may return empty results
///       on standard spot accounts.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbaseFuturesIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── GetCurrentMarginWindowAsync ─────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetCurrentMarginWindow_returns_window()
    {
        var result = await Client.GetCurrentMarginWindowAsync();

        Debug.WriteLine($"MarginWindowType:                            {result.MarginWindow?.MarginWindowType}");
        Debug.WriteLine($"EndTime:                                     {result.MarginWindow?.EndTime}");
        Debug.WriteLine($"IsIntradayMarginKillswitchEnabled:           {result.IsIntradayMarginKillswitchEnabled}");
        Debug.WriteLine($"IsIntradayEnrollmentKillswitchEnabled:       {result.IsIntradayMarginEnrollmentKillswitchEnabled}");

        Assert.NotNull(result);
    }

    [CoinbaseIntegrationFact]
    public async Task GetCurrentMarginWindow_with_retail_regular_profile()
    {
        var result = await Client.GetCurrentMarginWindowAsync(
            marginProfileType: MarginProfileType.RetailRegular);

        Debug.WriteLine($"MarginWindowType (RetailRegular): {result.MarginWindow?.MarginWindowType}");

        Assert.NotNull(result);
    }

    [CoinbaseIntegrationFact]
    public async Task GetCurrentMarginWindow_request_overload_matches_params_overload()
    {
        var byParams = await Client.GetCurrentMarginWindowAsync();
        var byRequest = await Client.GetCurrentMarginWindowAsync(
            new GetCurrentMarginWindowRequest());

        Assert.Equal(
            byParams.MarginWindow?.MarginWindowType,
            byRequest.MarginWindow?.MarginWindowType);
        Assert.Equal<bool?>(
            byParams.IsIntradayMarginKillswitchEnabled,
            byRequest.IsIntradayMarginKillswitchEnabled);
    }

    // ─── GetFuturesBalanceSummaryAsync ────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetFuturesBalanceSummary_returns_summary()
    {
        var result = await Client.GetFuturesBalanceSummaryAsync();

        var s = result.BalanceSummary;
        Debug.WriteLine($"FuturesBuyingPower:             {s?.FuturesBuyingPower?.Value} {s?.FuturesBuyingPower?.Currency}");
        Debug.WriteLine($"TotalUsdBalance:                {s?.TotalUsdBalance?.Value} {s?.TotalUsdBalance?.Currency}");
        Debug.WriteLine($"UnrealizedPnl:                  {s?.UnrealizedPnl?.Value}");
        Debug.WriteLine($"DailyRealizedPnl:               {s?.DailyRealizedPnl?.Value}");
        Debug.WriteLine($"LiquidationBufferPercentage:    {s?.LiquidationBufferPercentage}");
        Debug.WriteLine($"IntradayWindowType:             {s?.IntradayMarginWindowMeasure?.MarginWindowType}");
        Debug.WriteLine($"IntradayMarginLevel:            {s?.IntradayMarginWindowMeasure?.MarginLevel}");

        Assert.NotNull(result);
    }

    // ─── GetIntradayMarginSettingAsync ────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetIntradayMarginSetting_returns_setting()
    {
        var result = await Client.GetIntradayMarginSettingAsync();

        Debug.WriteLine($"IntradayMarginSetting: {result.Setting}");

        Assert.NotNull(result.Setting);
    }

    // ─── ListFuturesPositionsAsync ────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListFuturesPositions_returns_positions()
    {
        var result = await Client.ListFuturesPositionsAsync();

        Debug.WriteLine($"Positions count: {result.Positions?.Count ?? 0}");
        foreach (var pos in result.Positions ?? [])
        {
            Debug.WriteLine($"  ProductId={pos.ProductId} Side={pos.Side} " +
                            $"Contracts={pos.NumberOfContracts} UnrealizedPnl={pos.UnrealizedPnl}");
        }

        Assert.NotNull(result);
    }

    // ─── GetFuturesPositionAsync ──────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetFuturesPosition_returns_position_for_first_listed()
    {
        var list = await Client.ListFuturesPositionsAsync();

        if (list.Positions is not { Count: > 0 })
        {
            Debug.WriteLine("No open futures positions — skipping GetFuturesPosition test.");
            return;
        }

        var productId = list.Positions[0].ProductId!;
        var result = await Client.GetFuturesPositionAsync(productId);

        Debug.WriteLine($"ProductId:        {result.Position?.ProductId}");
        Debug.WriteLine($"Side:             {result.Position?.Side}");
        Debug.WriteLine($"Contracts:        {result.Position?.NumberOfContracts}");
        Debug.WriteLine($"CurrentPrice:     {result.Position?.CurrentPrice}");
        Debug.WriteLine($"AvgEntryPrice:    {result.Position?.AvgEntryPrice}");
        Debug.WriteLine($"UnrealizedPnl:    {result.Position?.UnrealizedPnl}");

        Assert.Equal(productId, result.Position?.ProductId);
    }

    [CoinbaseIntegrationFact]
    public async Task GetFuturesPosition_throws_CoinbaseApiException_for_unknown_product()
    {
        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => Client.GetFuturesPositionAsync("UNKNOWN-PRODUCT-XYZ1"));

        Debug.WriteLine($"StatusCode: {ex.StatusCode}");
        Debug.WriteLine($"RequestId:  {ex.RequestId}");

        Assert.True(ex.StatusCode.HasValue && (int)ex.StatusCode.Value >= 400);
    }

    // ─── ListFuturesSweepsAsync ───────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListFuturesSweeps_returns_sweeps()
    {
        var result = await Client.ListFuturesSweepsAsync();

        Debug.WriteLine($"Sweeps count: {result.Sweeps?.Count ?? 0}");
        foreach (var sweep in result.Sweeps ?? [])
        {
            Debug.WriteLine($"  Id={sweep.Id} Status={sweep.Status} " +
                            $"Amount={sweep.RequestedAmount?.Value} {sweep.RequestedAmount?.Currency} " +
                            $"SweepAll={sweep.ShouldSweepAll}");
        }

        Assert.NotNull(result);
    }
}
