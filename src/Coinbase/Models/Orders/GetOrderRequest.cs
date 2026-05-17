namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Request parameters for <c>GET /api/v3/brokerage/orders/historical/{order_id}</c>.</summary>
public sealed class GetOrderRequest
{
    /// <summary>The unique order ID. Required.</summary>
    public required string OrderId { get; init; }

    /// <summary>Deprecated by Coinbase. Client-assigned order ID.</summary>
    public string? ClientOrderId { get; init; }

    /// <summary>Deprecated by Coinbase. Native currency for order values. Defaults to USD.</summary>
    public string? UserNativeCurrency { get; init; }
}
