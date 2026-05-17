using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Response from GET /api/v3/brokerage/intx/portfolio/{portfolio_uuid}.
/// </summary>
public sealed class GetPerpetualPortfolioSummaryResponse
{
    [JsonPropertyName("portfolios")]
    public List<PerpetualPortfolio>? Portfolios { get; set; }

    [JsonPropertyName("summary")]
    public PerpetualPortfolioSummary? Summary { get; set; }
}
