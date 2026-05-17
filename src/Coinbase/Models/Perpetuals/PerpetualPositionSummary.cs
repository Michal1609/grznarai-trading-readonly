using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Aggregate summary returned alongside a list of perpetuals positions.
/// </summary>
public sealed class PerpetualPositionSummary
{
    [JsonPropertyName("aggregated_pnl")]
    public PerpetualAmount? AggregatedPnl { get; set; }
}
