using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.DataApi;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbaseDataApiIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── GetApiKeyPermissionsAsync ────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetApiKeyPermissions_returns_permissions()
    {
        var result = await Client.GetApiKeyPermissionsAsync();

        Debug.WriteLine($"CanView:       {result.CanView}");
        Debug.WriteLine($"CanTrade:      {result.CanTrade}");
        Debug.WriteLine($"CanTransfer:   {result.CanTransfer}");
        Debug.WriteLine($"CanReceive:    {result.CanReceive}");
        Debug.WriteLine($"PortfolioUuid: {result.PortfolioUuid}");
        Debug.WriteLine($"PortfolioType: {result.PortfolioType}");

        Assert.NotNull(result);
        Assert.NotNull(result.CanView);
    }

    [CoinbaseIntegrationFact]
    public async Task GetApiKeyPermissions_portfolio_type_is_known_value()
    {
        var result = await Client.GetApiKeyPermissionsAsync();

        var knownTypes = new[]
        {
            PortfolioType.Undefined,
            PortfolioType.Default,
            PortfolioType.Consumer,
            PortfolioType.Intx
        };

        Debug.WriteLine($"PortfolioType: {result.PortfolioType}");

        if (result.PortfolioType is not null)
            Assert.Contains(result.PortfolioType, knownTypes);
    }

    [CoinbaseIntegrationFact]
    public async Task GetApiKeyPermissions_read_only_key_has_can_view_true()
    {
        var result = await Client.GetApiKeyPermissionsAsync();

        Debug.WriteLine($"CanView: {result.CanView}");

        Assert.True(result.CanView,
            "Expected CanView=true for a read-only API key. " +
            "If the key has no view permission, requests to other endpoints will also fail.");
    }
}
