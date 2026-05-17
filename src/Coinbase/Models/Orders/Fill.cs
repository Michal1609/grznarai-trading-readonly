using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>
/// A single fill (trade execution) returned by the Coinbase Advanced Trade API.
/// Numeric fields are string-encoded to preserve precision.
/// </summary>
public sealed class Fill
{
    [JsonPropertyName("entry_id")] public string? EntryId { get; set; }
    [JsonPropertyName("trade_id")] public string? TradeId { get; set; }
    [JsonPropertyName("order_id")] public string? OrderId { get; set; }
    [JsonPropertyName("trade_time")] public DateTimeOffset? TradeTime { get; set; }

    /// <summary>Fill type — see <see cref="TradeType"/>.</summary>
    [JsonPropertyName("trade_type")] public string? TradeType { get; set; }

    [JsonPropertyName("price")] public string? Price { get; set; }
    [JsonPropertyName("size")] public string? Size { get; set; }
    [JsonPropertyName("commission")] public string? Commission { get; set; }
    [JsonPropertyName("product_id")] public string? ProductId { get; set; }
    [JsonPropertyName("sequence_timestamp")] public DateTimeOffset? SequenceTimestamp { get; set; }

    /// <summary>Whether this fill provided or removed liquidity — see <see cref="LiquidityIndicator"/>.</summary>
    [JsonPropertyName("liquidity_indicator")] public string? LiquidityIndicator { get; set; }

    [JsonPropertyName("size_in_quote")] public bool? SizeInQuote { get; set; }
    [JsonPropertyName("user_id")] public string? UserId { get; set; }

    /// <summary>Trade direction — see <see cref="OrderSide"/>.</summary>
    [JsonPropertyName("side")] public string? Side { get; set; }

    [JsonPropertyName("retail_portfolio_id")] public string? RetailPortfolioId { get; set; }

    /// <summary>Fill source — see <see cref="FillSource"/>.</summary>
    [JsonPropertyName("fillSource")] public string? FillSource { get; set; }

    [JsonPropertyName("commission_detail_total")] public CommissionDetailTotal? CommissionDetailTotal { get; set; }
}
