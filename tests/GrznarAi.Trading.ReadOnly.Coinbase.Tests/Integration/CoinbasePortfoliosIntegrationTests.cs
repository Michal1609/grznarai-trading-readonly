using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API — Portfolios section.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbasePortfoliosIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── ListPortfoliosAsync ─────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListPortfolios_returns_portfolios()
    {
        var result = await Client.ListPortfoliosAsync();

        Debug.WriteLine($"Portfolio count: {result.Portfolios?.Count ?? 0}");
        foreach (var p in result.Portfolios ?? [])
            Debug.WriteLine($"  UUID={p.Uuid}  Name={p.Name}  Type={p.Type}  Deleted={p.Deleted}");

        Assert.NotNull(result.Portfolios);
    }

    [CoinbaseIntegrationFact]
    public async Task ListPortfolios_with_default_type_filter_returns_subset()
    {
        var all = await Client.ListPortfoliosAsync();
        var defaultOnly = await Client.ListPortfoliosAsync(portfolioType: "DEFAULT");

        Debug.WriteLine($"All: {all.Portfolios?.Count ?? 0}  DEFAULT: {defaultOnly.Portfolios?.Count ?? 0}");

        Assert.NotNull(defaultOnly.Portfolios);
        Assert.True(
            (defaultOnly.Portfolios?.Count ?? 0) <= (all.Portfolios?.Count ?? 0),
            "Filtered result should not exceed unfiltered.");
    }

    [CoinbaseIntegrationFact]
    public async Task ListPortfolios_request_overload_matches_params_overload()
    {
        var byParams = await Client.ListPortfoliosAsync();
        var byRequest = await Client.ListPortfoliosAsync(new ListPortfoliosRequest());

        Assert.Equal(byParams.Portfolios?.Count ?? 0, byRequest.Portfolios?.Count ?? 0);
    }

    // ─── GetPortfolioBreakdownAsync ──────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetPortfolioBreakdown_returns_breakdown_for_first_portfolio()
    {
        var list = await Client.ListPortfoliosAsync();
        var uuid = list.Portfolios?.FirstOrDefault()?.Uuid;

        if (string.IsNullOrWhiteSpace(uuid))
        {
            Debug.WriteLine("No portfolios available — GetPortfolioBreakdown test skipped.");
            return;
        }

        var result = await Client.GetPortfolioBreakdownAsync(uuid);

        Debug.WriteLine($"Portfolio UUID:        {result.Breakdown?.Portfolio?.Uuid}");
        Debug.WriteLine($"Portfolio Name:        {result.Breakdown?.Portfolio?.Name}");
        Debug.WriteLine($"Portfolio Type:        {result.Breakdown?.Portfolio?.Type}");
        Debug.WriteLine($"Total Balance:         {result.Breakdown?.PortfolioBalances?.TotalBalance?.Value} {result.Breakdown?.PortfolioBalances?.TotalBalance?.Currency}");
        Debug.WriteLine($"Total Crypto Balance:  {result.Breakdown?.PortfolioBalances?.TotalCryptoBalance?.Value} {result.Breakdown?.PortfolioBalances?.TotalCryptoBalance?.Currency}");
        Debug.WriteLine($"Spot positions:        {result.Breakdown?.SpotPositions?.Count ?? 0}");
        Debug.WriteLine($"Perp positions:        {result.Breakdown?.PerpPositions?.Count ?? 0}");
        Debug.WriteLine($"Futures positions:     {result.Breakdown?.FuturesPositions?.Count ?? 0}");

        Assert.NotNull(result.Breakdown);
        Assert.Equal(uuid, result.Breakdown!.Portfolio?.Uuid);
    }

    [CoinbaseIntegrationFact]
    public async Task GetPortfolioBreakdown_with_currency_returns_usd_valuation()
    {
        var list = await Client.ListPortfoliosAsync();
        var uuid = list.Portfolios?.FirstOrDefault()?.Uuid;

        if (string.IsNullOrWhiteSpace(uuid))
        {
            Debug.WriteLine("No portfolios available — skipping.");
            return;
        }

        var result = await Client.GetPortfolioBreakdownAsync(uuid, "USD");

        Debug.WriteLine($"Total Balance (USD): {result.Breakdown?.PortfolioBalances?.TotalBalance?.Value} {result.Breakdown?.PortfolioBalances?.TotalBalance?.Currency}");

        Assert.NotNull(result.Breakdown);
    }

    [CoinbaseIntegrationFact]
    public async Task GetPortfolioBreakdown_request_overload_matches_params_overload()
    {
        var list = await Client.ListPortfoliosAsync();
        var uuid = list.Portfolios?.FirstOrDefault()?.Uuid;

        if (string.IsNullOrWhiteSpace(uuid))
        {
            Debug.WriteLine("No portfolios available — skipping.");
            return;
        }

        var byParams = await Client.GetPortfolioBreakdownAsync(uuid);
        var byRequest = await Client.GetPortfolioBreakdownAsync(new GetPortfolioBreakdownRequest { PortfolioUuid = uuid });

        Assert.Equal(byParams.Breakdown?.Portfolio?.Uuid, byRequest.Breakdown?.Portfolio?.Uuid);
    }

    [CoinbaseIntegrationFact]
    public async Task GetPortfolioBreakdown_throws_CoinbaseApiException_for_unknown_uuid()
    {
        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => Client.GetPortfolioBreakdownAsync("00000000-0000-0000-0000-000000000000"));

        Debug.WriteLine($"StatusCode: {ex.StatusCode}");
        Debug.WriteLine($"Endpoint:   {ex.Endpoint}");

        Assert.True(
            ex.StatusCode == System.Net.HttpStatusCode.NotFound
            || ex.StatusCode == System.Net.HttpStatusCode.BadRequest
            || ex.StatusCode == System.Net.HttpStatusCode.Forbidden);
    }
}
