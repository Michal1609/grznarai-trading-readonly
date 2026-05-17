using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Response from <c>GET /api/v3/brokerage/orders/historical/batch</c>.</summary>
public sealed class ListOrdersResponse
{
    [JsonPropertyName("orders")] public List<Order>? Orders { get; set; }
    [JsonPropertyName("has_next")] public bool? HasNext { get; set; }
    [JsonPropertyName("cursor")] public string? Cursor { get; set; }
    [JsonPropertyName("proof_token_required")] public bool? ProofTokenRequired { get; set; }

    /// <summary>Deprecated by Coinbase.</summary>
    [JsonPropertyName("sequence")] public string? Sequence { get; set; }
}
