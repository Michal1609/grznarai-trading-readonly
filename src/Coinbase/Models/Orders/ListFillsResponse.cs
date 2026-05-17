using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Response from <c>GET /api/v3/brokerage/orders/historical/fills</c>.</summary>
public sealed class ListFillsResponse
{
    [JsonPropertyName("fills")] public List<Fill>? Fills { get; set; }
    [JsonPropertyName("cursor")] public string? Cursor { get; set; }
    [JsonPropertyName("proof_token_required")] public bool? ProofTokenRequired { get; set; }
}
