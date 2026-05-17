using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientPublicTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientPublicTests()
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

    // ─── GetPublicMarketTradesAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetPublicMarketTrades_calls_market_endpoint()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products/BTC-USD/ticker?limit=5")
            .Respond("application/json", """{"trades":[{"trade_id":"1","product_id":"BTC-USD","price":"50000","size":"0.1","side":"BUY"}],"best_bid":"49999","best_ask":"50001"}""");

        var result = await _client.GetPublicMarketTradesAsync("BTC-USD", 5);

        Assert.NotNull(result.Trades);
        Assert.Single(result.Trades!);
        Assert.Equal("1", result.Trades![0].TradeId);
        Assert.Equal("49999", result.BestBid);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicMarketTrades_with_start_end_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products/ETH-USD/ticker?limit=10&start=1000&end=2000")
            .Respond("application/json", """{"trades":[],"best_bid":"2000","best_ask":"2001"}""");

        await _client.GetPublicMarketTradesAsync("ETH-USD", 10, "1000", "2000");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicMarketTrades_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products/BTC-USD/ticker?limit=3")
            .Respond("application/json", """{"trades":[],"best_bid":"50000","best_ask":"50001"}""");

        await _client.GetPublicMarketTradesAsync(new GetPublicMarketTradesRequest { ProductId = "BTC-USD", Limit = 3 });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicMarketTrades_request_overload_rejects_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.GetPublicMarketTradesAsync((GetPublicMarketTradesRequest)null!));
    }

    [Fact]
    public async Task GetPublicMarketTrades_rejects_empty_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPublicMarketTradesAsync("", 5));
    }

    [Fact]
    public async Task GetPublicMarketTrades_rejects_zero_limit()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetPublicMarketTradesAsync("BTC-USD", 0));
    }

    // ─── GetPublicProductAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetPublicProduct_calls_market_endpoint()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products/BTC-USD")
            .Respond("application/json", """{"product_id":"BTC-USD","price":"50000","product_type":"SPOT"}""");

        var result = await _client.GetPublicProductAsync("BTC-USD");

        Assert.Equal("BTC-USD", result.ProductId);
        Assert.Equal("50000", result.Price);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProduct_escapes_product_id_in_path()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products/BTC-USD")
            .Respond("application/json", """{"product_id":"BTC-USD"}""");

        await _client.GetPublicProductAsync("BTC-USD");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProduct_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products/ETH-USD")
            .Respond("application/json", """{"product_id":"ETH-USD"}""");

        await _client.GetPublicProductAsync(new GetPublicProductRequest { ProductId = "ETH-USD" });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProduct_request_overload_rejects_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.GetPublicProductAsync((GetPublicProductRequest)null!));
    }

    [Fact]
    public async Task GetPublicProduct_rejects_empty_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPublicProductAsync(""));
    }

    [Fact]
    public async Task GetPublicProduct_throws_on_404()
    {
        _mock.When("*/market/products/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetPublicProductAsync("UNKNOWN-PRODUCT"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    // ─── GetPublicProductBookAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetPublicProductBook_calls_market_product_book_endpoint()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/product_book?product_id=BTC-USD")
            .Respond("application/json", """{"pricebook":{"product_id":"BTC-USD","bids":[{"price":"49999","size":"1"}],"asks":[{"price":"50001","size":"2"}]}}""");

        var result = await _client.GetPublicProductBookAsync("BTC-USD");

        Assert.NotNull(result.Pricebook);
        Assert.Equal("BTC-USD", result.Pricebook!.ProductId);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProductBook_with_limit_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/product_book?product_id=BTC-USD&limit=10")
            .Respond("application/json", """{"pricebook":{"product_id":"BTC-USD"}}""");

        await _client.GetPublicProductBookAsync("BTC-USD", limit: 10);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProductBook_with_aggregation_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/product_book?product_id=BTC-USD&aggregation_price_increment=0.01")
            .Respond("application/json", """{"pricebook":{"product_id":"BTC-USD"}}""");

        await _client.GetPublicProductBookAsync("BTC-USD", aggregationPriceIncrement: "0.01");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProductBook_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/product_book?product_id=ETH-USD&limit=5")
            .Respond("application/json", """{"pricebook":{"product_id":"ETH-USD"}}""");

        await _client.GetPublicProductBookAsync(new GetPublicProductBookRequest { ProductId = "ETH-USD", Limit = 5 });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProductBook_request_overload_rejects_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.GetPublicProductBookAsync((GetPublicProductBookRequest)null!));
    }

    [Fact]
    public async Task GetPublicProductBook_rejects_empty_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPublicProductBookAsync(""));
    }

    // ─── GetPublicProductCandlesAsync ────────────────────────────────────────

    [Fact]
    public async Task GetPublicProductCandles_calls_market_candles_endpoint()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products/BTC-USD/candles?start=1000&end=2000&granularity=ONE_HOUR")
            .Respond("application/json", """{"candles":[{"start":"1000","low":"49000","high":"51000","open":"50000","close":"50500","volume":"100"}]}""");

        var result = await _client.GetPublicProductCandlesAsync("BTC-USD", "1000", "2000", "ONE_HOUR");

        Assert.NotNull(result.Candles);
        Assert.Single(result.Candles!);
        Assert.Equal("50500", result.Candles![0].Close);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProductCandles_with_limit_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products/BTC-USD/candles?start=1000&end=2000&granularity=ONE_DAY&limit=10")
            .Respond("application/json", """{"candles":[]}""");

        await _client.GetPublicProductCandlesAsync("BTC-USD", "1000", "2000", "ONE_DAY", limit: 10);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProductCandles_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products/BTC-USD/candles?start=100&end=200&granularity=FIVE_MINUTE")
            .Respond("application/json", """{"candles":[]}""");

        await _client.GetPublicProductCandlesAsync(new GetPublicProductCandlesRequest
        {
            ProductId = "BTC-USD",
            Start = "100",
            End = "200",
            Granularity = "FIVE_MINUTE"
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPublicProductCandles_request_overload_rejects_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.GetPublicProductCandlesAsync((GetPublicProductCandlesRequest)null!));
    }

    [Fact]
    public async Task GetPublicProductCandles_rejects_empty_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPublicProductCandlesAsync("", "1000", "2000", "ONE_HOUR"));
    }

    [Fact]
    public async Task GetPublicProductCandles_rejects_empty_granularity()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPublicProductCandlesAsync("BTC-USD", "1000", "2000", ""));
    }

    // ─── GetServerTimeAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetServerTime_returns_parsed_response()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/time")
            .Respond("application/json", """{"iso":"2024-01-15T12:00:00Z","epochSeconds":1705320000,"epochMillis":1705320000000}""");

        var result = await _client.GetServerTimeAsync();

        Assert.Equal("2024-01-15T12:00:00Z", result.Iso);
        Assert.Equal(1705320000L, result.EpochSeconds);
        Assert.Equal(1705320000000L, result.EpochMillis);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetServerTime_throws_on_500()
    {
        _mock.When("*/time")
            .Respond(HttpStatusCode.InternalServerError, "application/json", """{"error":"server_error"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetServerTimeAsync());

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    // ─── ListPublicProductsAsync ─────────────────────────────────────────────

    [Fact]
    public async Task ListPublicProducts_without_filter_calls_market_endpoint()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products")
            .Respond("application/json", """{"products":[{"product_id":"BTC-USD","price":"50000","product_type":"SPOT"}],"num_products":1}""");

        var result = await _client.ListPublicProductsAsync();

        Assert.NotNull(result.Products);
        Assert.Single(result.Products!);
        Assert.Equal("BTC-USD", result.Products![0].ProductId);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListPublicProducts_with_limit_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products?limit=10")
            .Respond("application/json", """{"products":[],"num_products":0}""");

        await _client.ListPublicProductsAsync(limit: 10);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListPublicProducts_with_product_type_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products?product_type=SPOT")
            .Respond("application/json", """{"products":[],"num_products":0}""");

        await _client.ListPublicProductsAsync(productType: "SPOT");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListPublicProducts_with_product_ids_appends_repeated_params()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products?product_ids=BTC-USD&product_ids=ETH-USD")
            .Respond("application/json", """{"products":[],"num_products":0}""");

        await _client.ListPublicProductsAsync(productIds: ["BTC-USD", "ETH-USD"]);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListPublicProducts_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/market/products?limit=5&product_type=FUTURE")
            .Respond("application/json", """{"products":[],"num_products":0}""");

        await _client.ListPublicProductsAsync(new ListPublicProductsRequest { Limit = 5, ProductType = "FUTURE" });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListPublicProducts_request_overload_rejects_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.ListPublicProductsAsync((ListPublicProductsRequest)null!));
    }
}
