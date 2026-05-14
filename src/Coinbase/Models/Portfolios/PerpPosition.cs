using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;
using GrznarAi.Trading.ReadOnly.Json;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

public sealed class PerpPosition
{
    [JsonPropertyName("product_id")] public string? ProductId { get; set; }
    [JsonPropertyName("product_uuid")] public string? ProductUuid { get; set; }
    [JsonPropertyName("symbol")] public string? Symbol { get; set; }
    [JsonPropertyName("asset_image_url")] public string? AssetImageUrl { get; set; }
    [JsonPropertyName("vwap")] public MoneyValue? Vwap { get; set; }
    [JsonPropertyName("position_side")] public PositionSide? PositionSide { get; set; }

    [JsonPropertyName("net_size")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? NetSize { get; set; }

    [JsonPropertyName("buy_order_size")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? BuyOrderSize { get; set; }

    [JsonPropertyName("sell_order_size")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? SellOrderSize { get; set; }

    [JsonPropertyName("im_contribution")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? ImContribution { get; set; }

    [JsonPropertyName("unrealized_pnl")] public MoneyValue? UnrealizedPnl { get; set; }
    [JsonPropertyName("mark_price")] public MoneyValue? MarkPrice { get; set; }
    [JsonPropertyName("liquidation_price")] public MoneyValue? LiquidationPrice { get; set; }

    [JsonPropertyName("leverage")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? Leverage { get; set; }

    [JsonPropertyName("im_notional")] public MoneyValue? ImNotional { get; set; }
    [JsonPropertyName("mm_notional")] public MoneyValue? MmNotional { get; set; }
    [JsonPropertyName("position_notional")] public MoneyValue? PositionNotional { get; set; }

    [JsonPropertyName("margin_type")] public MarginType? MarginType { get; set; }

    [JsonPropertyName("liquidation_buffer")] public string? LiquidationBuffer { get; set; }
    [JsonPropertyName("liquidation_percentage")] public string? LiquidationPercentage { get; set; }
}
