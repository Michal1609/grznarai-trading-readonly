using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Response from <c>GET /api/v3/brokerage/orders/historical/{order_id}</c>.</summary>
public sealed class GetOrderResponse
{
    [JsonPropertyName("order")] public Order? Order { get; set; }
}
