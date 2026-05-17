using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Response from <c>GET /api/v3/brokerage/cfm/positions</c>.
/// </summary>
public sealed class ListFuturesPositionsResponse
{
    [JsonPropertyName("positions")]
    public List<FcmPosition>? Positions { get; set; }
}
