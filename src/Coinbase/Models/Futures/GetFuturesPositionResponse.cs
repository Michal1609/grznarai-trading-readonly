using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Response from <c>GET /api/v3/brokerage/cfm/positions/{product_id}</c>.
/// </summary>
public sealed class GetFuturesPositionResponse
{
    [JsonPropertyName("position")]
    public FcmPosition? Position { get; set; }
}
