using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>A single historical market trade.</summary>
public sealed class MarketTrade
{
    [JsonPropertyName("trade_id")] public string? TradeId { get; set; }
    [JsonPropertyName("product_id")] public string? ProductId { get; set; }

    /// <summary>Trade price in quote currency.</summary>
    [JsonPropertyName("price")] public string? Price { get; set; }

    /// <summary>Trade size in base currency.</summary>
    [JsonPropertyName("size")] public string? Size { get; set; }

    [JsonPropertyName("time")] public DateTimeOffset? Time { get; set; }

    /// <summary>Maker side of the trade — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders.OrderSide"/>.</summary>
    [JsonPropertyName("side")] public string? Side { get; set; }

    [JsonPropertyName("exchange")] public string? Exchange { get; set; }
}
