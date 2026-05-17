using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Response from GET /api/v3/brokerage/intx/positions/{portfolio_uuid}.
/// </summary>
public sealed class ListPerpetualPositionsResponse
{
    [JsonPropertyName("positions")]
    public List<PerpetualPosition>? Positions { get; set; }

    [JsonPropertyName("summary")]
    public PerpetualPositionSummary? Summary { get; set; }
}
