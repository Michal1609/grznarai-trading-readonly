using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientFeesTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientFeesTests()
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

    // ─── GetTransactionSummaryAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetTransactionSummary_no_params_calls_bare_path()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/transaction_summary")
            .Respond("application/json", MinimalSummaryJson());

        var result = await _client.GetTransactionSummaryAsync();

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetTransactionSummary_product_type_adds_query_param()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/transaction_summary?product_type=SPOT")
            .Respond("application/json", MinimalSummaryJson());

        await _client.GetTransactionSummaryAsync(productType: ProductType.Spot);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetTransactionSummary_future_with_expiry_type_builds_full_query()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/transaction_summary" +
                "?product_type=FUTURE&contract_expiry_type=EXPIRING")
            .Respond("application/json", MinimalSummaryJson());

        await _client.GetTransactionSummaryAsync(
            productType: ProductType.Future,
            contractExpiryType: ContractExpiryType.Expiring);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetTransactionSummary_product_venue_adds_query_param()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/transaction_summary?product_venue=FCM")
            .Respond("application/json", MinimalSummaryJson());

        await _client.GetTransactionSummaryAsync(productVenue: ProductVenue.Fcm);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetTransactionSummary_all_params_builds_full_query()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/transaction_summary" +
                "?product_type=FUTURE&contract_expiry_type=PERPETUAL&product_venue=INTX")
            .Respond("application/json", MinimalSummaryJson());

        await _client.GetTransactionSummaryAsync(
            productType: ProductType.Future,
            contractExpiryType: ContractExpiryType.Perpetual,
            productVenue: ProductVenue.Intx);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetTransactionSummary_request_overload_delegates_correctly()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/transaction_summary?product_type=SPOT")
            .Respond("application/json", MinimalSummaryJson());

        await _client.GetTransactionSummaryAsync(new GetTransactionSummaryRequest
        {
            ProductType = ProductType.Spot
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetTransactionSummary_request_overload_throws_on_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.GetTransactionSummaryAsync((GetTransactionSummaryRequest)null!));
    }

    [Fact]
    public async Task GetTransactionSummary_deserializes_full_response()
    {
        const string json = """
            {
              "total_fees": 25.5,
              "fee_tier": {
                "pricing_tier": "<$10k",
                "taker_fee_rate": "0.0060",
                "maker_fee_rate": "0.0040",
                "aop_from": "0",
                "aop_to": "10000"
              },
              "advanced_trade_only_volume": 1000.0,
              "advanced_trade_only_fees": 6.0,
              "coinbase_pro_volume": 500.0,
              "coinbase_pro_fees": 2.0,
              "total_balance": "5000",
              "has_cost_plus_commission": false
            }
            """;

        _mock.When("*/transaction_summary").Respond("application/json", json);

        var result = await _client.GetTransactionSummaryAsync();

        Assert.Equal(25.5m, result.TotalFees);
        Assert.Equal("<$10k", result.FeeTier!.PricingTier);
        Assert.Equal("0.0060", result.FeeTier.TakerFeeRate);
        Assert.Equal("0.0040", result.FeeTier.MakerFeeRate);
        Assert.Equal("0", result.FeeTier.AopFrom);
        Assert.Equal("10000", result.FeeTier.AopTo);
        Assert.Equal(1000.0m, result.AdvancedTradeOnlyVolume);
        Assert.Equal(6.0m, result.AdvancedTradeOnlyFees);
        Assert.Equal(500.0m, result.CoinbaseProVolume);
        Assert.Equal(2.0m, result.CoinbaseProFees);
        Assert.Equal("5000", result.TotalBalance);
        Assert.False(result.HasCostPlusCommission);
    }

    [Fact]
    public async Task GetTransactionSummary_deserializes_margin_rate()
    {
        const string json = """
            {
              "total_fees": 0,
              "fee_tier": { "pricing_tier": "<$10k" },
              "margin_rate": { "value": "0.05" }
            }
            """;

        _mock.When("*/transaction_summary").Respond("application/json", json);

        var result = await _client.GetTransactionSummaryAsync();

        Assert.Equal(0.05m, result.MarginRate!.Value);
    }

    [Fact]
    public async Task GetTransactionSummary_deserializes_gst()
    {
        const string json = """
            {
              "total_fees": 0,
              "fee_tier": {},
              "goods_and_services_tax": { "rate": "0.10", "type": "INCLUSIVE" }
            }
            """;

        _mock.When("*/transaction_summary").Respond("application/json", json);

        var result = await _client.GetTransactionSummaryAsync();

        Assert.Equal("0.10", result.GoodsAndServicesTax!.Rate);
        Assert.Equal(GstType.Inclusive, result.GoodsAndServicesTax.Type);
    }

    [Fact]
    public async Task GetTransactionSummary_deserializes_volume_breakdown()
    {
        const string json = """
            {
              "total_fees": 8.0,
              "fee_tier": {},
              "volume_breakdown": [
                { "volume_type": "VOLUME_TYPE_SPOT", "volume": 1000.0 },
                { "volume_type": "VOLUME_TYPE_US_DERIVATIVES", "volume": 500.0 }
              ]
            }
            """;

        _mock.When("*/transaction_summary").Respond("application/json", json);

        var result = await _client.GetTransactionSummaryAsync();

        Assert.Equal(2, result.VolumeBreakdown!.Count);
        Assert.Equal(VolumeType.Spot, result.VolumeBreakdown[0].VolumeType);
        Assert.Equal(1000.0m, result.VolumeBreakdown[0].Volume);
        Assert.Equal(VolumeType.UsDerivatives, result.VolumeBreakdown[1].VolumeType);
    }

    [Fact]
    public async Task GetTransactionSummary_deserializes_fee_tier_volume_types_and_range()
    {
        const string json = """
            {
              "total_fees": 0,
              "fee_tier": {
                "pricing_tier": "<$50k",
                "volume_types_and_range": [
                  {
                    "volume_types": ["VOLUME_TYPE_SPOT", "VOLUME_TYPE_US_DERIVATIVES"],
                    "vol_from": "0",
                    "vol_to": "50000"
                  }
                ]
              }
            }
            """;

        _mock.When("*/transaction_summary").Respond("application/json", json);

        var result = await _client.GetTransactionSummaryAsync();

        var rule = Assert.Single(result.FeeTier!.VolumeTypesAndRange!);
        Assert.Equal(2, rule.VolumeTypes!.Count);
        Assert.Contains(VolumeType.Spot, rule.VolumeTypes);
        Assert.Equal("0", rule.VolFrom);
        Assert.Equal("50000", rule.VolTo);
    }

    [Fact]
    public async Task GetTransactionSummary_throws_on_401()
    {
        _mock.When("*/transaction_summary")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetTransactionSummaryAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string MinimalSummaryJson() =>
        """{"total_fees":0,"fee_tier":{"pricing_tier":"<$10k"}}""";
}
