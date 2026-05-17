using GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Read-only client for Coinbase Advanced Trade Orders endpoints.
/// </summary>
public interface ICoinbaseOrdersClient
{
    /// <summary>
    /// Get a single historical order by its unique order ID.
    /// </summary>
    /// <param name="orderId">The unique order ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/orders/historical/{order_id}</remarks>
    Task<GetOrderResponse> GetOrderAsync(string orderId, CancellationToken ct = default);

    /// <summary>
    /// Get a single historical order using a request object.
    /// Allows passing deprecated <c>client_order_id</c> and <c>user_native_currency</c> parameters.
    /// </summary>
    /// <param name="request">Request parameters including required <see cref="GetOrderRequest.OrderId"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/orders/historical/{order_id}</remarks>
    Task<GetOrderResponse> GetOrderAsync(GetOrderRequest request, CancellationToken ct = default);

    /// <summary>
    /// List all historical orders for the authenticated user. Returns up to one page of results.
    /// Use <see cref="ListOrdersResponse.HasNext"/> and <see cref="ListOrdersResponse.Cursor"/> to paginate.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/orders/historical/batch</remarks>
    Task<ListOrdersResponse> ListOrdersAsync(CancellationToken ct = default);

    /// <summary>
    /// List historical orders with filtering, sorting, and pagination via a request object.
    /// </summary>
    /// <param name="request">
    /// Filter options including status, product, type, date range, and pagination.
    /// Use string constants from <see cref="OrderStatus"/>, <see cref="OrderType"/>, and related classes.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/orders/historical/batch</remarks>
    Task<ListOrdersResponse> ListOrdersAsync(ListOrdersRequest request, CancellationToken ct = default);

    /// <summary>
    /// List all fills (trade executions) for the authenticated user. Returns up to one page.
    /// Use <see cref="ListFillsResponse.Cursor"/> to paginate.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/orders/historical/fills</remarks>
    Task<ListFillsResponse> ListFillsAsync(CancellationToken ct = default);

    /// <summary>
    /// List fills with filtering and pagination via a request object.
    /// </summary>
    /// <param name="request">
    /// Filter options including order IDs, product IDs, date range, and pagination.
    /// Use string constants from <see cref="FillSortBy"/> and <see cref="OrderSide"/>.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/orders/historical/fills</remarks>
    Task<ListFillsResponse> ListFillsAsync(ListFillsRequest request, CancellationToken ct = default);
}
