using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Amount scaled by a display precision factor.
/// </summary>
public sealed class ScaledAmount
{
    /// <summary>The monetary value.</summary>
    [JsonPropertyName("amount")]
    public ConvertAmount? Amount { get; set; }

    /// <summary>Number of decimal places used to display the amount.</summary>
    [JsonPropertyName("scale")]
    public int? Scale { get; set; }
}

/// <summary>
/// Per-unit pricing information for a convert trade.
/// </summary>
public sealed class ConvertUnitPrice
{
    /// <summary>Target currency price expressed in fiat (e.g. $3,000 / ETH).</summary>
    [JsonPropertyName("target_to_fiat")]
    public ScaledAmount? TargetToFiat { get; set; }

    /// <summary>
    /// Target currency price in source currency units (e.g. 25 ETH / BTC).
    /// Only set for crypto-to-crypto trades.
    /// </summary>
    [JsonPropertyName("target_to_source")]
    public ScaledAmount? TargetToSource { get; set; }

    /// <summary>
    /// Source currency price in fiat (e.g. $6,000 / BTC).
    /// Only set for crypto-to-crypto trades.
    /// </summary>
    [JsonPropertyName("source_to_fiat")]
    public ScaledAmount? SourceToFiat { get; set; }
}
