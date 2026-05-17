using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbasePaymentMethodsIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── ListPaymentMethodsAsync ─────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListPaymentMethods_returns_payment_methods()
    {
        var result = await Client.ListPaymentMethodsAsync();

        Debug.WriteLine($"Payment methods returned: {result.PaymentMethods?.Count ?? 0}");

        foreach (var pm in (result.PaymentMethods ?? []).Take(10))
        {
            Debug.WriteLine($"  ID={pm.Id} Type={pm.Type} Name={pm.Name} Currency={pm.Currency}");
            Debug.WriteLine($"    Verified={pm.Verified} AllowBuy={pm.AllowBuy} AllowSell={pm.AllowSell}");
            Debug.WriteLine($"    AllowDeposit={pm.AllowDeposit} AllowWithdraw={pm.AllowWithdraw}");
        }

        Assert.NotNull(result.PaymentMethods);
    }

    // ─── GetPaymentMethodAsync ───────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetPaymentMethod_returns_method_for_first_listed_id()
    {
        var list = await Client.ListPaymentMethodsAsync();
        var id = list.PaymentMethods?.FirstOrDefault()?.Id;

        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.WriteLine("No payment methods available — GetPaymentMethod test skipped.");
            return;
        }

        var result = await Client.GetPaymentMethodAsync(id);

        Debug.WriteLine($"ID:           {result.PaymentMethod?.Id}");
        Debug.WriteLine($"Type:         {result.PaymentMethod?.Type}");
        Debug.WriteLine($"Name:         {result.PaymentMethod?.Name}");
        Debug.WriteLine($"Currency:     {result.PaymentMethod?.Currency}");
        Debug.WriteLine($"Verified:     {result.PaymentMethod?.Verified}");
        Debug.WriteLine($"AllowBuy:     {result.PaymentMethod?.AllowBuy}");
        Debug.WriteLine($"AllowSell:    {result.PaymentMethod?.AllowSell}");
        Debug.WriteLine($"AllowDeposit: {result.PaymentMethod?.AllowDeposit}");
        Debug.WriteLine($"AllowWithdraw:{result.PaymentMethod?.AllowWithdraw}");
        Debug.WriteLine($"CreatedAt:    {result.PaymentMethod?.CreatedAt}");

        Assert.NotNull(result.PaymentMethod);
        Assert.Equal(id, result.PaymentMethod!.Id);
    }

    [CoinbaseIntegrationFact]
    public async Task GetPaymentMethod_throws_CoinbaseApiException_for_unknown_id()
    {
        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => Client.GetPaymentMethodAsync("00000000-0000-0000-0000-000000000000"));

        Debug.WriteLine($"StatusCode: {ex.StatusCode}");
        Debug.WriteLine($"Endpoint:   {ex.Endpoint}");

        Assert.True(
            ex.StatusCode == System.Net.HttpStatusCode.NotFound
            || ex.StatusCode == System.Net.HttpStatusCode.BadRequest
            || ex.StatusCode == System.Net.HttpStatusCode.Forbidden);
    }
}
