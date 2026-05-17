using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Asset balances for a single perpetuals portfolio.
/// </summary>
public sealed class PortfolioBalance
{
    [JsonPropertyName("portfolio_uuid")]
    public string? PortfolioUuid { get; set; }

    [JsonPropertyName("balances")]
    public List<PortfolioBalanceItem>? Balances { get; set; }

    [JsonPropertyName("is_margin_limit_reached")]
    public bool? IsMarginLimitReached { get; set; }
}
