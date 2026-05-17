using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbaseConvertIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── GetConvertTradeAsync ────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetConvertTrade_returns_trade_for_known_id()
    {
        // NOTE: Replace the values below with a real trade ID and account currencies
        // obtained from a previously created convert quote via the Coinbase UI or API.
        const string tradeId = "replace-with-real-trade-id";
        const string fromAccount = "USD";
        const string toAccount = "USDC";

        var result = await Client.GetConvertTradeAsync(tradeId, fromAccount, toAccount);

        Debug.WriteLine($"Trade ID:        {result.Trade?.Id}");
        Debug.WriteLine($"Status:          {result.Trade?.Status}");
        Debug.WriteLine($"Source currency: {result.Trade?.SourceCurrency}");
        Debug.WriteLine($"Target currency: {result.Trade?.TargetCurrency}");
        Debug.WriteLine($"Amount:          {result.Trade?.Amount?.Value} {result.Trade?.Amount?.Currency}");
        Debug.WriteLine($"Total fee:       {result.Trade?.TotalFee?.Amount?.Value} {result.Trade?.TotalFee?.Amount?.Currency}");
        Debug.WriteLine($"Exchange rate:   {result.Trade?.ExchangeRate?.Value}");

        Assert.NotNull(result.Trade);
        Assert.Equal(tradeId, result.Trade!.Id);
    }

    [CoinbaseIntegrationFact]
    public async Task GetConvertTrade_request_overload_returns_same_result()
    {
        const string tradeId = "replace-with-real-trade-id";
        const string fromAccount = "USD";
        const string toAccount = "USDC";

        var byParams = await Client.GetConvertTradeAsync(tradeId, fromAccount, toAccount);
        var byRequest = await Client.GetConvertTradeAsync(new GetConvertTradeRequest
        {
            TradeId = tradeId,
            FromAccount = fromAccount,
            ToAccount = toAccount
        });

        Assert.Equal(byParams.Trade?.Id, byRequest.Trade?.Id);
        Assert.Equal(byParams.Trade?.Status, byRequest.Trade?.Status);
    }

    [CoinbaseIntegrationFact]
    public async Task GetConvertTrade_throws_CoinbaseApiException_for_unknown_trade_id()
    {
        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => Client.GetConvertTradeAsync(
                "00000000-0000-0000-0000-000000000000",
                "USD",
                "USDC"));

        Debug.WriteLine($"StatusCode: {ex.StatusCode}");
        Debug.WriteLine($"Endpoint:   {ex.Endpoint}");

        Assert.True(
            ex.StatusCode == System.Net.HttpStatusCode.NotFound
            || ex.StatusCode == System.Net.HttpStatusCode.BadRequest
            || ex.StatusCode == System.Net.HttpStatusCode.Forbidden
            || ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity);
    }
}
