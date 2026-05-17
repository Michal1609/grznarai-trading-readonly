using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientConvertTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientConvertTests()
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

    private const string TradeId = "trade-abc-123";
    private const string FromAccount = "USD";
    private const string ToAccount = "USDC";

    private static string ConvertTradeUrl(string id, string from, string to) =>
        $"https://api.coinbase.com/api/v3/brokerage/convert/trade/{id}?from_account={from}&to_account={to}";

    // ─── GetConvertTradeAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetConvertTrade_builds_correct_url()
    {
        _mock.Expect(HttpMethod.Get, ConvertTradeUrl(TradeId, FromAccount, ToAccount))
            .Respond("application/json", MinimalTradeJson(TradeId));

        var result = await _client.GetConvertTradeAsync(TradeId, FromAccount, ToAccount);

        Assert.NotNull(result.Trade);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetConvertTrade_request_overload_delegates_correctly()
    {
        _mock.Expect(HttpMethod.Get, ConvertTradeUrl(TradeId, FromAccount, ToAccount))
            .Respond("application/json", MinimalTradeJson(TradeId));

        await _client.GetConvertTradeAsync(new GetConvertTradeRequest
        {
            TradeId = TradeId,
            FromAccount = FromAccount,
            ToAccount = ToAccount
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetConvertTrade_url_encodes_trade_id()
    {
        const string rawId = "trade/with/slashes";
        _mock.Expect(HttpMethod.Get,
                $"https://api.coinbase.com/api/v3/brokerage/convert/trade/trade%2Fwith%2Fslashes?from_account={FromAccount}&to_account={ToAccount}")
            .Respond("application/json", MinimalTradeJson(rawId));

        await _client.GetConvertTradeAsync(rawId, FromAccount, ToAccount);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetConvertTrade_deserializes_trade_fields()
    {
        const string json = """
            {
              "trade": {
                "id": "trade-abc-123",
                "status": "TRADE_STATUS_COMPLETED",
                "user_entered_amount": { "value": "100.00", "currency": "USD" },
                "amount": { "value": "99.50", "currency": "USDC" },
                "subtotal": { "value": "100.00", "currency": "USD" },
                "total": { "value": "100.50", "currency": "USD" },
                "total_fee": {
                  "title": "Coinbase Fee",
                  "description": "Trading fee",
                  "amount": { "value": "0.50", "currency": "USD" },
                  "label": "coinbase_fee"
                },
                "source_currency": "USD",
                "target_currency": "USDC",
                "source_id": "src-001",
                "target_id": "tgt-002",
                "exchange_rate": { "value": "1.00", "currency": "USD" },
                "unit_price": {
                  "target_to_fiat": {
                    "amount": { "value": "1.00", "currency": "USD" },
                    "scale": 2
                  }
                }
              }
            }
            """;

        _mock.When("*/convert/trade/*").Respond("application/json", json);

        var result = await _client.GetConvertTradeAsync(TradeId, FromAccount, ToAccount);
        var trade = result.Trade!;

        Assert.Equal("trade-abc-123", trade.Id);
        Assert.Equal(TradeStatus.Completed, trade.Status);
        Assert.Equal(100.00m, trade.UserEnteredAmount!.Value);
        Assert.Equal("USD", trade.UserEnteredAmount.Currency);
        Assert.Equal(99.50m, trade.Amount!.Value);
        Assert.Equal("USDC", trade.Amount.Currency);
        Assert.Equal(100.50m, trade.Total!.Value);
        Assert.Equal("Coinbase Fee", trade.TotalFee!.Title);
        Assert.Equal(0.50m, trade.TotalFee.Amount!.Value);
        Assert.Equal("coinbase_fee", trade.TotalFee.Label);
        Assert.Equal("USD", trade.SourceCurrency);
        Assert.Equal("USDC", trade.TargetCurrency);
        Assert.Equal("src-001", trade.SourceId);
        Assert.Equal("tgt-002", trade.TargetId);
        Assert.Equal(1.00m, trade.ExchangeRate!.Value);
        Assert.Equal(1.00m, trade.UnitPrice!.TargetToFiat!.Amount!.Value);
        Assert.Equal(2, trade.UnitPrice.TargetToFiat.Scale);
    }

    [Fact]
    public async Task GetConvertTrade_deserializes_fees_list()
    {
        const string json = """
            {
              "trade": {
                "id": "t1",
                "fees": [
                  { "title": "Fee A", "amount": { "value": "0.10", "currency": "USD" }, "label": "fee_a" },
                  { "title": "Fee B", "amount": { "value": "0.40", "currency": "USD" }, "label": "fee_b" }
                ]
              }
            }
            """;

        _mock.When("*/convert/trade/*").Respond("application/json", json);

        var result = await _client.GetConvertTradeAsync(TradeId, FromAccount, ToAccount);

        Assert.Equal(2, result.Trade!.Fees!.Count);
        Assert.Equal("Fee A", result.Trade.Fees[0].Title);
        Assert.Equal(0.10m, result.Trade.Fees[0].Amount!.Value);
        Assert.Equal("Fee B", result.Trade.Fees[1].Title);
    }

    [Fact]
    public async Task GetConvertTrade_deserializes_subscription_info()
    {
        const string json = """
            {
              "trade": {
                "id": "t1",
                "subscription_info": {
                  "has_benefit_cap": true,
                  "applied_subscription_benefit": false,
                  "max_free_trading_volume": { "value": "10000.00", "currency": "USD" },
                  "remaining_free_trading_volume": { "value": "8500.00", "currency": "USD" }
                }
              }
            }
            """;

        _mock.When("*/convert/trade/*").Respond("application/json", json);

        var result = await _client.GetConvertTradeAsync(TradeId, FromAccount, ToAccount);
        var sub = result.Trade!.SubscriptionInfo!;

        Assert.True(sub.HasBenefitCap);
        Assert.False(sub.AppliedSubscriptionBenefit);
        Assert.Equal(10000.00m, sub.MaxFreeTradingVolume!.Value);
        Assert.Equal(8500.00m, sub.RemainingFreeTradingVolume!.Value);
    }

    [Fact]
    public async Task GetConvertTrade_deserializes_tax_details()
    {
        const string json = """
            {
              "trade": {
                "id": "t1",
                "tax_details": [
                  { "name": "VAT", "amount": { "value": "0.05", "currency": "USD" } }
                ]
              }
            }
            """;

        _mock.When("*/convert/trade/*").Respond("application/json", json);

        var result = await _client.GetConvertTradeAsync(TradeId, FromAccount, ToAccount);

        var tax = Assert.Single(result.Trade!.TaxDetails!);
        Assert.Equal("VAT", tax.Name);
        Assert.Equal(0.05m, tax.Amount!.Value);
    }

    [Fact]
    public async Task GetConvertTrade_deserializes_trade_incentive_info()
    {
        const string json = """
            {
              "trade": {
                "id": "t1",
                "trade_incentive_info": {
                  "applied_incentive": true,
                  "user_incentive_id": "inc-999",
                  "code_val": "PROMO10",
                  "redeemed": true,
                  "fee_without_incentive": { "value": "2.00", "currency": "USD" }
                }
              }
            }
            """;

        _mock.When("*/convert/trade/*").Respond("application/json", json);

        var result = await _client.GetConvertTradeAsync(TradeId, FromAccount, ToAccount);
        var info = result.Trade!.TradeIncentiveInfo!;

        Assert.True(info.AppliedIncentive);
        Assert.Equal("inc-999", info.UserIncentiveId);
        Assert.Equal("PROMO10", info.CodeVal);
        Assert.True(info.Redeemed);
        Assert.Equal(2.00m, info.FeeWithoutIncentive!.Value);
    }

    [Fact]
    public async Task GetConvertTrade_deserializes_user_warnings()
    {
        const string json = """
            {
              "trade": {
                "id": "t1",
                "user_warnings": [
                  { "id": "w1", "code": "HIGH_VOLATILITY", "message": "Market is volatile." }
                ]
              }
            }
            """;

        _mock.When("*/convert/trade/*").Respond("application/json", json);

        var result = await _client.GetConvertTradeAsync(TradeId, FromAccount, ToAccount);

        var warning = Assert.Single(result.Trade!.UserWarnings!);
        Assert.Equal("w1", warning.Id);
        Assert.Equal("HIGH_VOLATILITY", warning.Code);
        Assert.Equal("Market is volatile.", warning.Message);
    }

    [Theory]
    [InlineData("", FromAccount, ToAccount)]
    [InlineData("   ", FromAccount, ToAccount)]
    [InlineData(TradeId, "", ToAccount)]
    [InlineData(TradeId, "   ", ToAccount)]
    [InlineData(TradeId, FromAccount, "")]
    [InlineData(TradeId, FromAccount, "   ")]
    public async Task GetConvertTrade_rejects_blank_params(string tradeId, string fromAccount, string toAccount)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetConvertTradeAsync(tradeId, fromAccount, toAccount));
    }

    [Fact]
    public async Task GetConvertTrade_request_overload_throws_on_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.GetConvertTradeAsync((GetConvertTradeRequest)null!));
    }

    [Fact]
    public async Task GetConvertTrade_throws_on_401()
    {
        _mock.When("*/convert/trade/*")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetConvertTradeAsync(TradeId, FromAccount, ToAccount));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task GetConvertTrade_sanitizes_trade_id_in_exception_endpoint()
    {
        _mock.When("*/convert/trade/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetConvertTradeAsync("missing-trade", FromAccount, ToAccount));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("/convert/trade/{trade_id}", ex.Endpoint);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string MinimalTradeJson(string id) =>
        $$$"""{"trade":{"id":"{{{id}}}","status":"TRADE_STATUS_COMPLETED"}}""";
}
