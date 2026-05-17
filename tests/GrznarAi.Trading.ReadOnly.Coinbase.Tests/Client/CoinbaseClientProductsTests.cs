using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientProductsTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientProductsTests()
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

    // ─── GetBestBidAskAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetBestBidAsk_without_filter_calls_correct_url()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/best_bid_ask")
            .Respond("application/json", """{"pricebooks":[{"product_id":"BTC-USD","bids":[{"price":"50000","size":"1"}],"asks":[{"price":"50001","size":"2"}]}]}""");

        var result = await _client.GetBestBidAskAsync();

        Assert.NotNull(result.Pricebooks);
        Assert.Single(result.Pricebooks!);
        Assert.Equal("BTC-USD", result.Pricebooks![0].ProductId);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetBestBidAsk_with_product_ids_appends_repeated_params()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/best_bid_ask?product_ids=BTC-USD&product_ids=ETH-USD")
            .Respond("application/json", """{"pricebooks":[]}""");

        var result = await _client.GetBestBidAskAsync(["BTC-USD", "ETH-USD"]);

        Assert.NotNull(result.Pricebooks);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetBestBidAsk_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/best_bid_ask?product_ids=BTC-USD")
            .Respond("application/json", """{"pricebooks":[]}""");

        await _client.GetBestBidAskAsync(new GetBestBidAskRequest { ProductIds = ["BTC-USD"] });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetBestBidAsk_request_overload_rejects_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.GetBestBidAskAsync((GetBestBidAskRequest)null!));
    }

    [Fact]
    public async Task GetBestBidAsk_throws_on_401()
    {
        _mock.When("*/best_bid_ask*")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetBestBidAskAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    // ─── GetMarketTradesAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetMarketTrades_builds_correct_url()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products/BTC-USD/ticker?limit=10")
            .Respond("application/json", """{"trades":[],"best_bid":"49999","best_ask":"50001"}""");

        var result = await _client.GetMarketTradesAsync("BTC-USD", 10);

        Assert.Equal("49999", result.BestBid);
        Assert.Equal("50001", result.BestAsk);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetMarketTrades_with_time_range_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products/BTC-USD/ticker?limit=5&start=1700000000&end=1700001000")
            .Respond("application/json", """{"trades":[],"best_bid":"50000","best_ask":"50010"}""");

        await _client.GetMarketTradesAsync("BTC-USD", 5, "1700000000", "1700001000");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetMarketTrades_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products/ETH-USD/ticker?limit=3")
            .Respond("application/json", """{"trades":[],"best_bid":"2000","best_ask":"2001"}""");

        await _client.GetMarketTradesAsync(new GetMarketTradesRequest { ProductId = "ETH-USD", Limit = 3 });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetMarketTrades_rejects_empty_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetMarketTradesAsync("", 10));
    }

    [Fact]
    public async Task GetMarketTrades_rejects_zero_limit()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetMarketTradesAsync("BTC-USD", 0));
    }

    [Fact]
    public async Task GetMarketTrades_throws_on_404()
    {
        _mock.When("*/ticker*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetMarketTradesAsync("UNKNOWN-USD", 10));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("/products/{product_id}/ticker", ex.Endpoint);
    }

    // ─── GetProductAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetProduct_builds_correct_path()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products/BTC-USD")
            .Respond("application/json", """{"product_id":"BTC-USD","price":"50000","base_name":"Bitcoin","quote_name":"US Dollar"}""");

        var result = await _client.GetProductAsync("BTC-USD");

        Assert.Equal("BTC-USD", result.ProductId);
        Assert.Equal("50000", result.Price);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProduct_with_tradability_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products/BTC-USD?get_tradability_status=true")
            .Respond("application/json", """{"product_id":"BTC-USD"}""");

        await _client.GetProductAsync("BTC-USD", getTradabilityStatus: true);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProduct_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products/ETH-USD")
            .Respond("application/json", """{"product_id":"ETH-USD"}""");

        await _client.GetProductAsync(new GetProductRequest { ProductId = "ETH-USD" });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProduct_rejects_empty_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetProductAsync(""));
    }

    [Fact]
    public async Task GetProduct_throws_on_404()
    {
        _mock.When("*/products/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetProductAsync("MISSING-USD"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("/products/{product_id}", ex.Endpoint);
    }

    // ─── GetProductBookAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetProductBook_builds_correct_url()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/product_book?product_id=BTC-USD")
            .Respond("application/json", """{"pricebook":{"product_id":"BTC-USD","bids":[{"price":"49999","size":"1"}],"asks":[{"price":"50001","size":"1"}]}}""");

        var result = await _client.GetProductBookAsync("BTC-USD");

        Assert.Equal("BTC-USD", result.Pricebook?.ProductId);
        Assert.Single(result.Pricebook!.Bids!);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProductBook_with_limit_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/product_book?product_id=BTC-USD&limit=5")
            .Respond("application/json", """{"pricebook":{"product_id":"BTC-USD","bids":[],"asks":[]}}""");

        await _client.GetProductBookAsync("BTC-USD", limit: 5);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProductBook_with_aggregation_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/product_book?product_id=BTC-USD&aggregation_price_increment=10")
            .Respond("application/json", """{"pricebook":{"product_id":"BTC-USD","bids":[],"asks":[]}}""");

        await _client.GetProductBookAsync("BTC-USD", aggregationPriceIncrement: "10");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProductBook_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/product_book?product_id=ETH-USD&limit=10")
            .Respond("application/json", """{"pricebook":{"product_id":"ETH-USD","bids":[],"asks":[]}}""");

        await _client.GetProductBookAsync(new GetProductBookRequest { ProductId = "ETH-USD", Limit = 10 });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProductBook_rejects_empty_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetProductBookAsync(""));
    }

    // ─── GetProductCandlesAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetProductCandles_builds_correct_url()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products/BTC-USD/candles?start=1700000000&end=1700003600&granularity=ONE_HOUR")
            .Respond("application/json", """{"candles":[{"start":"1700000000","low":"49000","high":"51000","open":"49500","close":"50500","volume":"1000"}]}""");

        var result = await _client.GetProductCandlesAsync("BTC-USD", "1700000000", "1700003600", Granularity.OneHour);

        Assert.NotNull(result.Candles);
        Assert.Single(result.Candles!);
        Assert.Equal("49000", result.Candles![0].Low);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProductCandles_with_limit_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products/BTC-USD/candles?start=1700000000&end=1700003600&granularity=ONE_MINUTE&limit=100")
            .Respond("application/json", """{"candles":[]}""");

        await _client.GetProductCandlesAsync("BTC-USD", "1700000000", "1700003600", Granularity.OneMinute, limit: 100);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProductCandles_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products/ETH-USD/candles?start=1700000000&end=1700003600&granularity=ONE_DAY")
            .Respond("application/json", """{"candles":[]}""");

        await _client.GetProductCandlesAsync(new GetProductCandlesRequest
        {
            ProductId = "ETH-USD",
            Start = "1700000000",
            End = "1700003600",
            Granularity = Granularity.OneDay
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetProductCandles_rejects_empty_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetProductCandlesAsync("", "1700000000", "1700003600", Granularity.OneHour));
    }

    [Fact]
    public async Task GetProductCandles_throws_on_404()
    {
        _mock.When("*/candles*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetProductCandlesAsync("MISSING-USD", "1700000000", "1700003600", Granularity.OneHour));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("/products/{product_id}/candles", ex.Endpoint);
    }

    // ─── ListProductsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ListProducts_without_filter_calls_correct_url()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products")
            .Respond("application/json", """{"products":[{"product_id":"BTC-USD"}],"num_products":1}""");

        var result = await _client.ListProductsAsync();

        Assert.NotNull(result.Products);
        Assert.Single(result.Products!);
        Assert.Equal(1, result.NumProducts);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListProducts_with_product_type_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products?product_type=SPOT")
            .Respond("application/json", """{"products":[],"num_products":0}""");

        await _client.ListProductsAsync(productType: "SPOT");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListProducts_with_product_ids_uses_repeated_params()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products?product_ids=BTC-USD&product_ids=ETH-USD")
            .Respond("application/json", """{"products":[],"num_products":0}""");

        await _client.ListProductsAsync(productIds: ["BTC-USD", "ETH-USD"]);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListProducts_with_limit_and_offset_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products?limit=10&offset=20")
            .Respond("application/json", """{"products":[],"num_products":0}""");

        await _client.ListProductsAsync(limit: 10, offset: 20);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListProducts_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products?product_type=FUTURE")
            .Respond("application/json", """{"products":[],"num_products":0}""");

        await _client.ListProductsAsync(new ListProductsRequest { ProductType = "FUTURE" });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListProducts_request_overload_rejects_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.ListProductsAsync((ListProductsRequest)null!));
    }

    [Fact]
    public async Task ListProducts_deserializes_pagination()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/products")
            .Respond("application/json", """{"products":[],"num_products":0,"pagination":{"next_cursor":"abc","has_next":true,"has_prev":false}}""");

        var result = await _client.ListProductsAsync();

        Assert.NotNull(result.Pagination);
        Assert.Equal("abc", result.Pagination!.NextCursor);
        Assert.True(result.Pagination.HasNext);
        _mock.VerifyNoOutstandingExpectation();
    }
}
