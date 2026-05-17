using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Response from GET /api/v3/brokerage/intx/positions/{portfolio_uuid}/{symbol}.
/// </summary>
public sealed class GetPerpetualPositionResponse
{
    [JsonPropertyName("position")]
    public PerpetualPosition? Position { get; set; }
}
