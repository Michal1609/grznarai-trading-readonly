using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>A single OHLCV candle bucket.</summary>
public sealed class Candle
{
    /// <summary>UNIX timestamp marking the start of the interval.</summary>
    [JsonPropertyName("start")] public string? Start { get; set; }

    /// <summary>Lowest price during the interval.</summary>
    [JsonPropertyName("low")] public string? Low { get; set; }

    /// <summary>Highest price during the interval.</summary>
    [JsonPropertyName("high")] public string? High { get; set; }

    /// <summary>Price of the first trade in the interval.</summary>
    [JsonPropertyName("open")] public string? Open { get; set; }

    /// <summary>Price of the last trade in the interval.</summary>
    [JsonPropertyName("close")] public string? Close { get; set; }

    /// <summary>Trading volume during the interval.</summary>
    [JsonPropertyName("volume")] public string? Volume { get; set; }
}
