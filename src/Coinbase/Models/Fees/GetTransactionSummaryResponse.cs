using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;

/// <summary>
/// Response from <c>GET /api/v3/brokerage/transaction_summary</c>.
/// </summary>
public sealed class GetTransactionSummaryResponse
{
    /// <summary>Total fees across all assets, in USD.</summary>
    [JsonPropertyName("total_fees")]
    public decimal? TotalFees { get; set; }

    /// <summary>Current maker/taker fee tier with rate details.</summary>
    [JsonPropertyName("fee_tier")]
    public FeeTier? FeeTier { get; set; }

    /// <summary>
    /// Margin rate. Only set when filtering by <c>product_type=FUTURE</c>.
    /// </summary>
    [JsonPropertyName("margin_rate")]
    public DecimalValue? MarginRate { get; set; }

    /// <summary>Goods and services tax details, if applicable.</summary>
    [JsonPropertyName("goods_and_services_tax")]
    public GoodsAndServicesTax? GoodsAndServicesTax { get; set; }

    /// <summary>Advanced Trade volume (excludes Coinbase Pro), in USD.</summary>
    [JsonPropertyName("advanced_trade_only_volume")]
    public decimal? AdvancedTradeOnlyVolume { get; set; }

    /// <summary>Advanced Trade fees (excludes Coinbase Pro), in USD.</summary>
    [JsonPropertyName("advanced_trade_only_fees")]
    public decimal? AdvancedTradeOnlyFees { get; set; }

    /// <summary>Coinbase Pro volume in USD.</summary>
    [JsonPropertyName("coinbase_pro_volume")]
    public decimal? CoinbaseProVolume { get; set; }

    /// <summary>Coinbase Pro fees in USD.</summary>
    [JsonPropertyName("coinbase_pro_fees")]
    public decimal? CoinbaseProFees { get; set; }

    /// <summary>Total balance across all assets and products, in USD.</summary>
    [JsonPropertyName("total_balance")]
    public string? TotalBalance { get; set; }

    /// <summary>Per-type volume breakdown that contributed to the fee tier calculation.</summary>
    [JsonPropertyName("volume_breakdown")]
    public List<FeeVolume>? VolumeBreakdown { get; set; }

    /// <summary>Whether the user is on a cost-plus commission pricing model.</summary>
    [JsonPropertyName("has_cost_plus_commission")]
    public bool? HasCostPlusCommission { get; set; }
}
