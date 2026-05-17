using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbaseAccountsIntegrationTests
{
    // Lazy – only created when a test method actually runs (skipped tests never access it).
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── ListAccountsAsync ───────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListAccounts_returns_accounts()
    {
        var result = await Client.ListAccountsAsync();

        Debug.WriteLine($"Total accounts returned: {result.Size}");
        Debug.WriteLine($"Has next page:           {result.HasNext}");

        foreach (var acc in (result.Accounts ?? []).Take(10))
            Debug.WriteLine($"  UUID={acc.Uuid} Name={acc.Name} Currency={acc.Currency} Type={acc.Type} Platform={acc.Platform} Balance={acc.AvailableBalance?.Value} {acc.AvailableBalance?.Currency}");

        Assert.NotNull(result.Accounts);
    }

    [CoinbaseIntegrationFact]
    public async Task ListAccounts_with_limit_1_returns_single_page()
    {
        var result = await Client.ListAccountsAsync(limit: 1);

        Debug.WriteLine($"Size: {result.Size}, HasNext: {result.HasNext}, Cursor: {result.Cursor}");

        Assert.NotNull(result.Accounts);
        Assert.True(result.Accounts!.Count <= 1);
    }

    [CoinbaseIntegrationFact]
    public async Task ListAccounts_request_overload_returns_same_result_as_params()
    {
        var byParams = await Client.ListAccountsAsync(limit: 5);
        var byRequest = await Client.ListAccountsAsync(new ListAccountsRequest { Limit = 5 });

        Assert.Equal(byParams.Size, byRequest.Size);
    }

    [CoinbaseIntegrationFact]
    public async Task ListAccounts_pagination_cursor_advances_page()
    {
        var page1 = await Client.ListAccountsAsync(limit: 1);

        if (page1.HasNext != true || string.IsNullOrWhiteSpace(page1.Cursor))
        {
            Debug.WriteLine("Only one page of accounts — pagination test skipped.");
            return;
        }

        var page2 = await Client.ListAccountsAsync(limit: 1, cursor: page1.Cursor);

        Debug.WriteLine($"Page 1 first UUID: {page1.Accounts?.FirstOrDefault()?.Uuid}");
        Debug.WriteLine($"Page 2 first UUID: {page2.Accounts?.FirstOrDefault()?.Uuid}");

        Assert.NotEqual(
            page1.Accounts?.FirstOrDefault()?.Uuid,
            page2.Accounts?.FirstOrDefault()?.Uuid);
    }

    // ─── GetAccountAsync ─────────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetAccount_returns_account_for_first_listed_uuid()
    {
        var list = await Client.ListAccountsAsync(limit: 1);
        var uuid = list.Accounts?.FirstOrDefault()?.Uuid;

        if (string.IsNullOrWhiteSpace(uuid))
        {
            Debug.WriteLine("No accounts available — GetAccount test skipped.");
            return;
        }

        var result = await Client.GetAccountAsync(uuid);

        Debug.WriteLine($"UUID:     {result.Account?.Uuid}");
        Debug.WriteLine($"Name:     {result.Account?.Name}");
        Debug.WriteLine($"Currency: {result.Account?.Currency}");
        Debug.WriteLine($"Type:     {result.Account?.Type}");
        Debug.WriteLine($"Platform: {result.Account?.Platform}");
        Debug.WriteLine($"Balance:  {result.Account?.AvailableBalance?.Value} {result.Account?.AvailableBalance?.Currency}");
        Debug.WriteLine($"Hold:     {result.Account?.Hold?.Value} {result.Account?.Hold?.Currency}");
        Debug.WriteLine($"Active:   {result.Account?.Active}");
        Debug.WriteLine($"Ready:    {result.Account?.Ready}");

        Assert.NotNull(result.Account);
        Assert.Equal(uuid, result.Account!.Uuid);
    }

    [CoinbaseIntegrationFact]
    public async Task GetAccount_throws_CoinbaseApiException_for_unknown_uuid()
    {
        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => Client.GetAccountAsync("00000000-0000-0000-0000-000000000000"));

        Debug.WriteLine($"StatusCode: {ex.StatusCode}");
        Debug.WriteLine($"Endpoint:   {ex.Endpoint}");

        Assert.True(
            ex.StatusCode == System.Net.HttpStatusCode.NotFound
            || ex.StatusCode == System.Net.HttpStatusCode.BadRequest
            || ex.StatusCode == System.Net.HttpStatusCode.Forbidden);
    }
}
