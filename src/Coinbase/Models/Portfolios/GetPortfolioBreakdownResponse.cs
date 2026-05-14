using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

public sealed class GetPortfolioBreakdownResponse
{
    [JsonPropertyName("breakdown")] public PortfolioBreakdown? Breakdown { get; set; }
}
