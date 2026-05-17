using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientOrdersTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientOrdersTests()
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

    // ─── GetOrderAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrder_builds_correct_path()
    {
        const string orderId = "order-abc-123";
        _mock.Expect(HttpMethod.Get,
                $"https://api.coinbase.com/api/v3/brokerage/orders/historical/{orderId}")
            .Respond("application/json", """{"order":{"order_id":"order-abc-123","product_id":"BTC-USD"}}""");

        var result = await _client.GetOrderAsync(orderId);

        Assert.Equal("order-abc-123", result.Order!.OrderId);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetOrder_url_encodes_order_id()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/order%2Fwith%2Fslashes")
            .Respond("application/json", """{"order":{"order_id":"order/with/slashes"}}""");

        await _client.GetOrderAsync("order/with/slashes");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetOrder_sanitizes_order_id_in_exception_endpoint()
    {
        _mock.When("*/orders/historical/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetOrderAsync("missing-order-id"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("/orders/historical/{order_id}", ex.Endpoint);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetOrder_rejects_blank_order_id(string orderId)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetOrderAsync(orderId));
    }

    [Fact]
    public async Task GetOrder_request_overload_delegates_correctly()
    {
        const string orderId = "req-order-1";
        _mock.Expect(HttpMethod.Get,
                $"https://api.coinbase.com/api/v3/brokerage/orders/historical/{orderId}")
            .Respond("application/json", """{"order":{"order_id":"req-order-1"}}""");

        var result = await _client.GetOrderAsync(new GetOrderRequest { OrderId = orderId });

        Assert.Equal(orderId, result.Order!.OrderId);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetOrder_request_overload_appends_deprecated_params()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/oid-1?client_order_id=coid-1&user_native_currency=EUR")
            .Respond("application/json", """{"order":{"order_id":"oid-1"}}""");

        await _client.GetOrderAsync(new GetOrderRequest
        {
            OrderId = "oid-1",
            ClientOrderId = "coid-1",
            UserNativeCurrency = "EUR"
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetOrder_request_overload_throws_on_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.GetOrderAsync((GetOrderRequest)null!));
    }

    [Fact]
    public async Task GetOrder_deserializes_full_order()
    {
        const string json = """
            {
              "order": {
                "order_id": "full-order-1",
                "product_id": "BTC-USD",
                "user_id": "user-1",
                "side": "BUY",
                "client_order_id": "client-1",
                "status": "FILLED",
                "time_in_force": "GOOD_UNTIL_CANCELLED",
                "created_time": "2024-03-01T10:00:00Z",
                "completion_percentage": "100",
                "filled_size": "0.001",
                "average_filled_price": "50000.00",
                "number_of_fills": "1",
                "filled_value": "50.00",
                "pending_cancel": false,
                "size_in_quote": false,
                "total_fees": "0.25",
                "size_inclusive_of_fees": false,
                "total_value_after_fees": "49.75",
                "order_type": "LIMIT",
                "settled": true,
                "product_type": "SPOT",
                "order_configuration": {
                  "limit_limit_gtc": {
                    "base_size": "0.001",
                    "limit_price": "50000.00",
                    "post_only": false
                  }
                }
              }
            }
            """;

        _mock.When("*/orders/historical/*").Respond("application/json", json);

        var result = await _client.GetOrderAsync("full-order-1");
        var order = result.Order!;

        Assert.Equal("full-order-1", order.OrderId);
        Assert.Equal("BTC-USD", order.ProductId);
        Assert.Equal(OrderSide.Buy, order.Side);
        Assert.Equal(OrderStatus.Filled, order.Status);
        Assert.Equal(TimeInForce.GoodUntilCancelled, order.TimeInForce);
        Assert.Equal(new DateTimeOffset(2024, 3, 1, 10, 0, 0, TimeSpan.Zero), order.CreatedTime);
        Assert.Equal("100", order.CompletionPercentage);
        Assert.Equal("0.001", order.FilledSize);
        Assert.Equal(OrderType.Limit, order.OrderType);
        Assert.True(order.Settled);
        Assert.NotNull(order.OrderConfiguration);
        Assert.NotNull(order.OrderConfiguration.LimitGtc);
        Assert.Equal("0.001", order.OrderConfiguration.LimitGtc.BaseSize);
        Assert.Equal("50000.00", order.OrderConfiguration.LimitGtc.LimitPrice);
        Assert.False(order.OrderConfiguration.LimitGtc.PostOnly);
    }

    [Fact]
    public async Task GetOrder_throws_on_401()
    {
        _mock.When("*/orders/historical/*")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetOrderAsync("some-order"));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    // ─── ListOrdersAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ListOrders_no_params_builds_bare_path()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/batch")
            .Respond("application/json", """{"orders":[],"has_next":false,"cursor":""}""");

        var result = await _client.ListOrdersAsync();

        Assert.NotNull(result.Orders);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListOrders_request_overload_throws_on_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.ListOrdersAsync((ListOrdersRequest)null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ListOrders_rejects_non_positive_limit(int limit)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.ListOrdersAsync(new ListOrdersRequest { Limit = limit }));
    }

    [Fact]
    public async Task ListOrders_with_product_id_filter()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/batch?product_ids=BTC-USD")
            .Respond("application/json", """{"orders":[],"has_next":false}""");

        await _client.ListOrdersAsync(new ListOrdersRequest
        {
            ProductIds = ["BTC-USD"]
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListOrders_with_status_filter()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/batch?order_status=OPEN,PENDING")
            .Respond("application/json", """{"orders":[],"has_next":false}""");

        await _client.ListOrdersAsync(new ListOrdersRequest
        {
            OrderStatus = [OrderStatus.Open, OrderStatus.Pending]
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListOrders_with_order_type_and_side_filter()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/batch?order_types=LIMIT&order_side=BUY")
            .Respond("application/json", """{"orders":[],"has_next":false}""");

        await _client.ListOrdersAsync(new ListOrdersRequest
        {
            OrderTypes = [OrderType.Limit],
            OrderSide = OrderSide.Buy
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListOrders_with_date_range_appends_params()
    {
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 3, 31, 23, 59, 59, TimeSpan.Zero);

        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/batch?start_date=2024-01-01T00%3A00%3A00Z&end_date=2024-03-31T23%3A59%3A59Z")
            .Respond("application/json", """{"orders":[],"has_next":false}""");

        await _client.ListOrdersAsync(new ListOrdersRequest
        {
            StartDate = start,
            EndDate = end
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListOrders_with_limit_and_cursor()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/batch?limit=25&cursor=tok123")
            .Respond("application/json", """{"orders":[],"has_next":true,"cursor":"tok456"}""");

        var result = await _client.ListOrdersAsync(new ListOrdersRequest
        {
            Limit = 25,
            Cursor = "tok123"
        });

        Assert.True(result.HasNext);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListOrders_deserializes_orders()
    {
        const string json = """
            {
              "orders": [
                {
                  "order_id": "o-1",
                  "product_id": "ETH-USD",
                  "side": "SELL",
                  "status": "OPEN",
                  "order_type": "LIMIT",
                  "total_fees": "0.10",
                  "size_in_quote": false,
                  "pending_cancel": false,
                  "size_inclusive_of_fees": true
                }
              ],
              "has_next": false,
              "cursor": ""
            }
            """;

        _mock.When("*/orders/historical/batch").Respond("application/json", json);

        var result = await _client.ListOrdersAsync();
        var order = Assert.Single(result.Orders!);

        Assert.Equal("o-1", order.OrderId);
        Assert.Equal("ETH-USD", order.ProductId);
        Assert.Equal(OrderSide.Sell, order.Side);
        Assert.Equal(OrderStatus.Open, order.Status);
        Assert.Equal(OrderType.Limit, order.OrderType);
    }

    [Fact]
    public async Task ListOrders_throws_on_401()
    {
        _mock.When("*/orders/historical/batch")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.ListOrdersAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    // ─── ListFillsAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ListFills_no_params_builds_bare_path()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/fills")
            .Respond("application/json", """{"fills":[],"cursor":""}""");

        var result = await _client.ListFillsAsync();

        Assert.NotNull(result.Fills);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListFills_request_overload_throws_on_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.ListFillsAsync((ListFillsRequest)null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ListFills_rejects_non_positive_limit(int limit)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.ListFillsAsync(new ListFillsRequest { Limit = limit }));
    }

    [Fact]
    public async Task ListFills_with_order_id_filter()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/fills?order_ids=oid-1,oid-2")
            .Respond("application/json", """{"fills":[],"cursor":""}""");

        await _client.ListFillsAsync(new ListFillsRequest
        {
            OrderIds = ["oid-1", "oid-2"]
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListFills_with_date_range_appends_params()
    {
        var start = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 6, 30, 23, 59, 59, TimeSpan.Zero);

        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/orders/historical/fills?start_sequence_timestamp=2024-06-01T00%3A00%3A00Z&end_sequence_timestamp=2024-06-30T23%3A59%3A59Z")
            .Respond("application/json", """{"fills":[],"cursor":""}""");

        await _client.ListFillsAsync(new ListFillsRequest
        {
            StartSequenceTimestamp = start,
            EndSequenceTimestamp = end
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListFills_deserializes_fills()
    {
        const string json = """
            {
              "fills": [
                {
                  "entry_id": "entry-1",
                  "trade_id": "trade-1",
                  "order_id": "order-1",
                  "trade_time": "2024-04-01T12:00:00Z",
                  "trade_type": "FILL",
                  "price": "65000.00",
                  "size": "0.001",
                  "commission": "0.13",
                  "product_id": "BTC-USD",
                  "liquidity_indicator": "TAKER",
                  "size_in_quote": false,
                  "user_id": "user-1",
                  "side": "BUY",
                  "fillSource": "FILL_SOURCE_CLOB"
                }
              ],
              "cursor": ""
            }
            """;

        _mock.When("*/fills").Respond("application/json", json);

        var result = await _client.ListFillsAsync();
        var fill = Assert.Single(result.Fills!);

        Assert.Equal("entry-1", fill.EntryId);
        Assert.Equal("trade-1", fill.TradeId);
        Assert.Equal("order-1", fill.OrderId);
        Assert.Equal(new DateTimeOffset(2024, 4, 1, 12, 0, 0, TimeSpan.Zero), fill.TradeTime);
        Assert.Equal(TradeType.Fill, fill.TradeType);
        Assert.Equal("65000.00", fill.Price);
        Assert.Equal("0.001", fill.Size);
        Assert.Equal(LiquidityIndicator.Taker, fill.LiquidityIndicator);
        Assert.Equal(OrderSide.Buy, fill.Side);
        Assert.Equal(FillSource.Clob, fill.FillSource);
    }

    [Fact]
    public async Task ListFills_throws_on_401()
    {
        _mock.When("*/fills")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.ListFillsAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }
}
