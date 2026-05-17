using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.PaymentMethods;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientPaymentMethodsTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientPaymentMethodsTests()
    {
        var http = _mock.ToHttpClient();
        http.BaseAddress = new Uri("https://api.coinbase.com");
        var opts = Options.Create(new CoinbaseOptions
        {
            KeyName = "test",
            PrivateKeyPem = "test",
            BaseUrl = "https://api.coinbase.com"
        });
        _client = new CoinbaseClient(http, opts);
    }

    public void Dispose()
    {
        _mock.Dispose();
        GC.SuppressFinalize(this);
    }

    // ─── ListPaymentMethodsAsync ─────────────────────────────────────────────

    [Fact]
    public async Task ListPaymentMethods_builds_correct_path()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/payment_methods")
            .Respond("application/json", """{"payment_methods":[]}""");

        var result = await _client.ListPaymentMethodsAsync();

        Assert.NotNull(result.PaymentMethods);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListPaymentMethods_deserializes_payment_method_correctly()
    {
        const string json = """
            {
              "payment_methods": [
                {
                  "id": "8bfc20d7-f7c6-4422-bf07-8243ca4169fe",
                  "type": "ACH",
                  "name": "ALLY BANK ******1234",
                  "currency": "USD",
                  "verified": true,
                  "allow_buy": true,
                  "allow_sell": true,
                  "allow_deposit": false,
                  "allow_withdraw": false,
                  "created_at": "2021-05-31T09:59:59.000Z",
                  "updated_at": "2021-05-31T09:59:59.000Z"
                }
              ]
            }
            """;

        _mock.When("*/payment_methods").Respond("application/json", json);

        var result = await _client.ListPaymentMethodsAsync();

        var pm = Assert.Single(result.PaymentMethods!);
        Assert.Equal("8bfc20d7-f7c6-4422-bf07-8243ca4169fe", pm.Id);
        Assert.Equal("ACH", pm.Type);
        Assert.Equal("ALLY BANK ******1234", pm.Name);
        Assert.Equal("USD", pm.Currency);
        Assert.True(pm.Verified);
        Assert.True(pm.AllowBuy);
        Assert.True(pm.AllowSell);
        Assert.False(pm.AllowDeposit);
        Assert.False(pm.AllowWithdraw);
        Assert.Equal(new DateTimeOffset(2021, 5, 31, 9, 59, 59, TimeSpan.Zero), pm.CreatedAt);
    }

    [Fact]
    public async Task ListPaymentMethods_returns_empty_list()
    {
        _mock.When("*/payment_methods").Respond("application/json", """{"payment_methods":[]}""");

        var result = await _client.ListPaymentMethodsAsync();

        Assert.Empty(result.PaymentMethods!);
    }

    [Fact]
    public async Task ListPaymentMethods_throws_on_401()
    {
        _mock.When("*/payment_methods")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.ListPaymentMethodsAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task ListPaymentMethods_deserializes_multiple_payment_methods()
    {
        const string json = """
            {
              "payment_methods": [
                {
                  "id": "id-1",
                  "type": "ACH",
                  "name": "Bank A ******0001",
                  "currency": "USD",
                  "verified": true,
                  "allow_buy": true,
                  "allow_sell": true,
                  "allow_deposit": true,
                  "allow_withdraw": true
                },
                {
                  "id": "id-2",
                  "type": "CREDIT_CARD",
                  "name": "Visa ****4242",
                  "currency": "USD",
                  "verified": false,
                  "allow_buy": true,
                  "allow_sell": false,
                  "allow_deposit": false,
                  "allow_withdraw": false
                }
              ]
            }
            """;

        _mock.When("*/payment_methods").Respond("application/json", json);

        var result = await _client.ListPaymentMethodsAsync();

        Assert.Equal(2, result.PaymentMethods!.Count);
        Assert.Equal("id-1", result.PaymentMethods[0].Id);
        Assert.Equal("id-2", result.PaymentMethods[1].Id);
        Assert.Equal("CREDIT_CARD", result.PaymentMethods[1].Type);
        Assert.False(result.PaymentMethods[1].Verified);
    }

    // ─── GetPaymentMethodAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetPaymentMethod_builds_correct_path()
    {
        const string pmId = "8bfc20d7-f7c6-4422-bf07-8243ca4169fe";
        _mock.Expect(HttpMethod.Get,
                $"https://api.coinbase.com/api/v3/brokerage/payment_methods/{pmId}")
            .Respond("application/json", $$$"""{"payment_method":{"id":"{{{pmId}}}","type":"ACH","name":"Test Bank"}}""");

        var result = await _client.GetPaymentMethodAsync(pmId);

        Assert.Equal(pmId, result.PaymentMethod!.Id);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPaymentMethod_url_encodes_id()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/payment_methods/id%2Fwith%2Fslashes")
            .Respond("application/json", """{"payment_method":{"id":"id/with/slashes"}}""");

        await _client.GetPaymentMethodAsync("id/with/slashes");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPaymentMethod_deserializes_full_response()
    {
        const string json = """
            {
              "payment_method": {
                "id": "full-pm-id",
                "type": "PAYPAL",
                "name": "PayPal user@example.com",
                "currency": "USD",
                "verified": true,
                "allow_buy": true,
                "allow_sell": true,
                "allow_deposit": true,
                "allow_withdraw": false,
                "created_at": "2022-03-10T14:00:00Z",
                "updated_at": "2023-07-01T08:15:00Z"
              }
            }
            """;

        _mock.When("*/payment_methods/*").Respond("application/json", json);

        var result = await _client.GetPaymentMethodAsync("full-pm-id");
        var pm = result.PaymentMethod!;

        Assert.Equal("full-pm-id", pm.Id);
        Assert.Equal("PAYPAL", pm.Type);
        Assert.Equal("PayPal user@example.com", pm.Name);
        Assert.Equal("USD", pm.Currency);
        Assert.True(pm.Verified);
        Assert.True(pm.AllowBuy);
        Assert.True(pm.AllowSell);
        Assert.True(pm.AllowDeposit);
        Assert.False(pm.AllowWithdraw);
        Assert.Equal(new DateTimeOffset(2022, 3, 10, 14, 0, 0, TimeSpan.Zero), pm.CreatedAt);
        Assert.Equal(new DateTimeOffset(2023, 7, 1, 8, 15, 0, TimeSpan.Zero), pm.UpdatedAt);
    }

    [Fact]
    public async Task GetPaymentMethod_sanitizes_id_in_exception_endpoint()
    {
        _mock.When("*/payment_methods/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetPaymentMethodAsync("missing-id"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("/payment_methods/{payment_method_id}", ex.Endpoint);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPaymentMethod_rejects_blank_id(string id)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPaymentMethodAsync(id));
    }

    [Fact]
    public async Task GetPaymentMethod_throws_on_403()
    {
        _mock.When("*/payment_methods/*")
            .Respond(HttpStatusCode.Forbidden, "application/json", """{"error":"FORBIDDEN"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetPaymentMethodAsync("some-id"));

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
    }
}
