using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

public sealed class PortfolioBreakdown
{
    [JsonPropertyName("portfolio")] public Portfolio? Portfolio { get; set; }
    [JsonPropertyName("portfolio_balances")] public PortfolioBalances? PortfolioBalances { get; set; }
    [JsonPropertyName("spot_positions")] public List<SpotPosition>? SpotPositions { get; set; }
    [JsonPropertyName("perp_positions")] public List<PerpPosition>? PerpPositions { get; set; }
    [JsonPropertyName("futures_positions")] public List<FuturesPosition>? FuturesPositions { get; set; }
}
