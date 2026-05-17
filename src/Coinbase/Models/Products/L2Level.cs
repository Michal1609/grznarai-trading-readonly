using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>A single price level in an order book (bid or ask).</summary>
public sealed class L2Level
{
    /// <summary>Price at this level, in quote currency.</summary>
    [JsonPropertyName("price")] public string? Price { get; set; }

    /// <summary>Size available at this level, in base currency.</summary>
    [JsonPropertyName("size")] public string? Size { get; set; }
}
