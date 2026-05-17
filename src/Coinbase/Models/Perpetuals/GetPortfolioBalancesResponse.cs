using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Response from GET /api/v3/brokerage/intx/balances/{portfolio_uuid}.
/// </summary>
public sealed class GetPortfolioBalancesResponse
{
    [JsonPropertyName("portfolio_balances")]
    public List<PortfolioBalance>? PortfolioBalances { get; set; }
}
