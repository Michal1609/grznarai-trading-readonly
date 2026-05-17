using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Response from <c>GET /api/v3/brokerage/cfm/balance_summary</c>.
/// </summary>
public sealed class GetFuturesBalanceSummaryResponse
{
    [JsonPropertyName("balance_summary")]
    public FuturesBalanceSummary? BalanceSummary { get; set; }
}
