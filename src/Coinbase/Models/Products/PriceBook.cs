using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Order book snapshot for a single product.</summary>
public sealed class PriceBook
{
    /// <summary>Trading pair (e.g. 'BTC-USD').</summary>
    [JsonPropertyName("product_id")] public string? ProductId { get; set; }

    /// <summary>Bid-side price levels, best first.</summary>
    [JsonPropertyName("bids")] public List<L2Level>? Bids { get; set; }

    /// <summary>Ask-side price levels, best first.</summary>
    [JsonPropertyName("asks")] public List<L2Level>? Asks { get; set; }

    /// <summary>Timestamp of the snapshot (RFC 3339).</summary>
    [JsonPropertyName("time")] public DateTimeOffset? Time { get; set; }
}
