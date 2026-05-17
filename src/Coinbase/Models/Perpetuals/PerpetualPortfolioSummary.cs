using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Aggregate summary across all perpetuals portfolios.
/// </summary>
public sealed class PerpetualPortfolioSummary
{
    [JsonPropertyName("unrealized_pnl")]
    public PerpetualAmount? UnrealizedPnl { get; set; }

    [JsonPropertyName("buying_power")]
    public PerpetualAmount? BuyingPower { get; set; }

    [JsonPropertyName("total_balance")]
    public PerpetualAmount? TotalBalance { get; set; }

    [JsonPropertyName("max_withdrawal_amount")]
    public PerpetualAmount? MaxWithdrawalAmount { get; set; }
}
