using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbaseOrdersIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── ListOrdersAsync ─────────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListOrders_returns_orders()
    {
        var result = await Client.ListOrdersAsync();

        Debug.WriteLine($"Total orders returned: {result.Orders?.Count}");
        Debug.WriteLine($"Has next page:         {result.HasNext}");

        foreach (var o in (result.Orders ?? []).Take(5))
            Debug.WriteLine($"  Id={o.OrderId} Product={o.ProductId} Side={o.Side} Status={o.Status} Type={o.OrderType} Created={o.CreatedTime}");

        Assert.NotNull(result.Orders);
    }

    [CoinbaseIntegrationFact]
    public async Task ListOrders_with_filled_status_filter()
    {
        var result = await Client.ListOrdersAsync(new ListOrdersRequest
        {
            OrderStatus = [OrderStatus.Filled],
            Limit = 5
        });

        Debug.WriteLine($"Filled orders (up to 5): {result.Orders?.Count}");

        Assert.NotNull(result.Orders);

        foreach (var o in result.Orders!)
            Assert.Equal(OrderStatus.Filled, o.Status);
    }

    [CoinbaseIntegrationFact]
    public async Task ListOrders_pagination_advances_cursor()
    {
        var page1 = await Client.ListOrdersAsync(new ListOrdersRequest { Limit = 1 });

        if (page1.HasNext != true || string.IsNullOrWhiteSpace(page1.Cursor))
        {
            Debug.WriteLine("Only one page of orders — pagination test skipped.");
            return;
        }

        var page2 = await Client.ListOrdersAsync(new ListOrdersRequest
        {
            Limit = 1,
            Cursor = page1.Cursor
        });

        Debug.WriteLine($"Page 1 order id: {page1.Orders?.FirstOrDefault()?.OrderId}");
        Debug.WriteLine($"Page 2 order id: {page2.Orders?.FirstOrDefault()?.OrderId}");

        Assert.NotEqual(
            page1.Orders?.FirstOrDefault()?.OrderId,
            page2.Orders?.FirstOrDefault()?.OrderId);
    }

    [CoinbaseIntegrationFact]
    public async Task ListOrders_with_product_type_spot_filter()
    {
        var result = await Client.ListOrdersAsync(new ListOrdersRequest
        {
            ProductType = GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductType.Spot,
            Limit = 5
        });

        Debug.WriteLine($"Spot orders returned: {result.Orders?.Count}");

        Assert.NotNull(result.Orders);
    }

    // ─── GetOrderAsync ───────────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetOrder_returns_order_for_first_listed()
    {
        var list = await Client.ListOrdersAsync(new ListOrdersRequest { Limit = 1 });
        var orderId = list.Orders?.FirstOrDefault()?.OrderId;

        if (string.IsNullOrWhiteSpace(orderId))
        {
            Debug.WriteLine("No orders available — GetOrder test skipped.");
            return;
        }

        var result = await Client.GetOrderAsync(orderId);

        Debug.WriteLine($"OrderId:    {result.Order?.OrderId}");
        Debug.WriteLine($"Product:    {result.Order?.ProductId}");
        Debug.WriteLine($"Side:       {result.Order?.Side}");
        Debug.WriteLine($"Status:     {result.Order?.Status}");
        Debug.WriteLine($"Type:       {result.Order?.OrderType}");
        Debug.WriteLine($"TotalFees:  {result.Order?.TotalFees}");

        Assert.NotNull(result.Order);
        Assert.Equal(orderId, result.Order!.OrderId);
    }

    [CoinbaseIntegrationFact]
    public async Task GetOrder_throws_CoinbaseApiException_for_unknown_order_id()
    {
        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => Client.GetOrderAsync("00000000-0000-0000-0000-000000000000"));

        Debug.WriteLine($"StatusCode: {ex.StatusCode}");
        Debug.WriteLine($"Endpoint:   {ex.Endpoint}");

        Assert.True(
            ex.StatusCode == System.Net.HttpStatusCode.NotFound
            || ex.StatusCode == System.Net.HttpStatusCode.BadRequest
            || ex.StatusCode == System.Net.HttpStatusCode.Forbidden);
    }

    // ─── ListFillsAsync ──────────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListFills_returns_fills()
    {
        var result = await Client.ListFillsAsync(new ListFillsRequest { Limit = 10 });

        Debug.WriteLine($"Fills returned: {result.Fills?.Count}");
        Debug.WriteLine($"Cursor:         {result.Cursor}");

        foreach (var f in (result.Fills ?? []).Take(5))
            Debug.WriteLine($"  TradeId={f.TradeId} Product={f.ProductId} Side={f.Side} Price={f.Price} Size={f.Size} Commission={f.Commission}");

        Assert.NotNull(result.Fills);
    }

    [CoinbaseIntegrationFact]
    public async Task ListFills_for_first_filled_order()
    {
        var orders = await Client.ListOrdersAsync(new ListOrdersRequest
        {
            OrderStatus = [OrderStatus.Filled],
            Limit = 1
        });

        var orderId = orders.Orders?.FirstOrDefault()?.OrderId;

        if (string.IsNullOrWhiteSpace(orderId))
        {
            Debug.WriteLine("No filled orders — ListFills by order ID test skipped.");
            return;
        }

        var result = await Client.ListFillsAsync(new ListFillsRequest
        {
            OrderIds = [orderId]
        });

        Debug.WriteLine($"Fills for order {orderId}: {result.Fills?.Count}");

        Assert.NotNull(result.Fills);
    }
}
