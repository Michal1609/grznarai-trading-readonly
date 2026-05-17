using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// A single International Derivatives (perpetuals) position.
/// </summary>
public sealed class PerpetualPosition
{
    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    [JsonPropertyName("product_uuid")]
    public string? ProductUuid { get; set; }

    [JsonPropertyName("portfolio_uuid")]
    public string? PortfolioUuid { get; set; }

    /// <summary>Trading pair symbol, e.g. <c>BTC-PERP-INTX</c>.</summary>
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    /// <summary>Volume-weighted average price.</summary>
    [JsonPropertyName("vwap")]
    public PerpetualAmount? Vwap { get; set; }

    /// <summary>Entry volume-weighted average price.</summary>
    [JsonPropertyName("entry_vwap")]
    public PerpetualAmount? EntryVwap { get; set; }

    [JsonPropertyName("position_side")]
    public PositionSide PositionSide { get; set; }

    [JsonPropertyName("margin_type")]
    public MarginType MarginType { get; set; }

    /// <summary>Positive = long, negative = short.</summary>
    [JsonPropertyName("net_size")]
    public string? NetSize { get; set; }

    [JsonPropertyName("buy_order_size")]
    public string? BuyOrderSize { get; set; }

    [JsonPropertyName("sell_order_size")]
    public string? SellOrderSize { get; set; }

    [JsonPropertyName("im_contribution")]
    public string? ImContribution { get; set; }

    [JsonPropertyName("unrealized_pnl")]
    public PerpetualAmount? UnrealizedPnl { get; set; }

    [JsonPropertyName("mark_price")]
    public PerpetualAmount? MarkPrice { get; set; }

    [JsonPropertyName("liquidation_price")]
    public PerpetualAmount? LiquidationPrice { get; set; }

    [JsonPropertyName("leverage")]
    public string? Leverage { get; set; }

    [JsonPropertyName("im_notional")]
    public PerpetualAmount? ImNotional { get; set; }

    [JsonPropertyName("mm_notional")]
    public PerpetualAmount? MmNotional { get; set; }

    [JsonPropertyName("position_notional")]
    public PerpetualAmount? PositionNotional { get; set; }

    [JsonPropertyName("aggregated_pnl")]
    public PerpetualAmount? AggregatedPnl { get; set; }
}
