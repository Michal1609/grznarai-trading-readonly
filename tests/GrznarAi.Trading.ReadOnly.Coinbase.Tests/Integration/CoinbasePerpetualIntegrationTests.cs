using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API — International Derivatives (perpetuals/intx).
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// Note: some tests require an intx-enabled portfolio; they may return empty results
///       on standard spot accounts.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbasePerpetualIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── GetPerpetualPortfolioSummaryAsync ───────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetPerpetualPortfolioSummary_returns_summary_for_first_portfolio()
    {
        var portfolios = await GetFirstPerpetualPortfolioUuidAsync();
        if (portfolios is null)
        {
            Debug.WriteLine("No perpetuals portfolios found — skipping.");
            return;
        }

        var result = await Client.GetPerpetualPortfolioSummaryAsync(portfolios);

        Debug.WriteLine($"Portfolios count:      {result.Portfolios?.Count ?? 0}");
        foreach (var p in result.Portfolios ?? [])
        {
            Debug.WriteLine($"  PortfolioUuid:       {p.PortfolioUuid}");
            Debug.WriteLine($"  Collateral:          {p.Collateral}");
            Debug.WriteLine($"  PositionNotional:    {p.PositionNotional}");
            Debug.WriteLine($"  MarginType:          {p.MarginType}");
            Debug.WriteLine($"  LiquidationStatus:   {p.LiquidationStatus}");
            Debug.WriteLine($"  UnrealizedPnl:       {p.UnrealizedPnl?.Value} {p.UnrealizedPnl?.Currency}");
        }
        Debug.WriteLine($"Summary.BuyingPower:  {result.Summary?.BuyingPower?.Value} {result.Summary?.BuyingPower?.Currency}");
        Debug.WriteLine($"Summary.TotalBalance: {result.Summary?.TotalBalance?.Value} {result.Summary?.TotalBalance?.Currency}");

        Assert.NotNull(result);
    }

    // ─── ListPerpetualPositionsAsync ─────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListPerpetualPositions_returns_positions()
    {
        var portfolioUuid = await GetFirstPerpetualPortfolioUuidAsync();
        if (portfolioUuid is null)
        {
            Debug.WriteLine("No perpetuals portfolios found — skipping.");
            return;
        }

        var result = await Client.ListPerpetualPositionsAsync(portfolioUuid);

        Debug.WriteLine($"Positions count:         {result.Positions?.Count ?? 0}");
        foreach (var pos in result.Positions ?? [])
        {
            Debug.WriteLine($"  Symbol={pos.Symbol} Side={pos.PositionSide} " +
                            $"NetSize={pos.NetSize} Leverage={pos.Leverage} " +
                            $"UnrealizedPnl={pos.UnrealizedPnl?.Value}");
        }
        Debug.WriteLine($"Summary.AggregatedPnl: {result.Summary?.AggregatedPnl?.Value} {result.Summary?.AggregatedPnl?.Currency}");

        Assert.NotNull(result);
    }

    // ─── GetPerpetualPositionAsync ───────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetPerpetualPosition_returns_position_for_first_open()
    {
        var portfolioUuid = await GetFirstPerpetualPortfolioUuidAsync();
        if (portfolioUuid is null)
        {
            Debug.WriteLine("No perpetuals portfolios — skipping.");
            return;
        }

        var list = await Client.ListPerpetualPositionsAsync(portfolioUuid);
        if (list.Positions is not { Count: > 0 })
        {
            Debug.WriteLine("No open perpetuals positions — skipping GetPerpetualPosition test.");
            return;
        }

        var symbol = list.Positions[0].Symbol!;
        var result = await Client.GetPerpetualPositionAsync(portfolioUuid, symbol);

        Debug.WriteLine($"Symbol:           {result.Position?.Symbol}");
        Debug.WriteLine($"PositionSide:     {result.Position?.PositionSide}");
        Debug.WriteLine($"NetSize:          {result.Position?.NetSize}");
        Debug.WriteLine($"Leverage:         {result.Position?.Leverage}");
        Debug.WriteLine($"MarkPrice:        {result.Position?.MarkPrice?.Value}");
        Debug.WriteLine($"LiquidationPrice: {result.Position?.LiquidationPrice?.Value}");
        Debug.WriteLine($"UnrealizedPnl:    {result.Position?.UnrealizedPnl?.Value}");
        Debug.WriteLine($"ImNotional:       {result.Position?.ImNotional?.Value}");
        Debug.WriteLine($"MmNotional:       {result.Position?.MmNotional?.Value}");

        Assert.Equal(symbol, result.Position?.Symbol);
    }

    [CoinbaseIntegrationFact]
    public async Task GetPerpetualPosition_throws_CoinbaseApiException_for_unknown_symbol()
    {
        var portfolioUuid = await GetFirstPerpetualPortfolioUuidAsync();
        if (portfolioUuid is null)
        {
            Debug.WriteLine("No perpetuals portfolios — skipping.");
            return;
        }

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => Client.GetPerpetualPositionAsync(portfolioUuid, "UNKNOWN-SYMBOL-XYZ"));

        Debug.WriteLine($"StatusCode: {ex.StatusCode}");
        Debug.WriteLine($"RequestId:  {ex.RequestId}");

        Assert.True(ex.StatusCode.HasValue && (int)ex.StatusCode.Value >= 400);
    }

    // ─── GetPortfolioBalancesAsync ───────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetPortfolioBalances_returns_balances()
    {
        var portfolioUuid = await GetFirstPerpetualPortfolioUuidAsync();
        if (portfolioUuid is null)
        {
            Debug.WriteLine("No perpetuals portfolios — skipping.");
            return;
        }

        var result = await Client.GetPortfolioBalancesAsync(portfolioUuid);

        Debug.WriteLine($"PortfolioBalances count: {result.PortfolioBalances?.Count ?? 0}");
        foreach (var pb in result.PortfolioBalances ?? [])
        {
            Debug.WriteLine($"  PortfolioUuid:        {pb.PortfolioUuid}");
            Debug.WriteLine($"  IsMarginLimitReached: {pb.IsMarginLimitReached}");
            Debug.WriteLine($"  Balances count:       {pb.Balances?.Count ?? 0}");
            foreach (var b in pb.Balances ?? [])
            {
                Debug.WriteLine($"    Asset={b.Asset?.AssetName} Qty={b.Quantity} " +
                                $"Hold={b.Hold} CollateralValue={b.CollateralValue}");
            }
        }

        Assert.NotNull(result);
    }

    // ─── Helper ──────────────────────────────────────────────────────────────

    private async Task<string?> GetFirstPerpetualPortfolioUuidAsync()
    {
        // Use the Portfolios endpoint to find an intx-type portfolio UUID.
        var portfolios = await Client.ListPortfoliosAsync(portfolioType: "INTX");
        return portfolios.Portfolios?.FirstOrDefault()?.Uuid;
    }
}
