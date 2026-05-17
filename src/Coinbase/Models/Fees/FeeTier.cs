using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;

/// <summary>
/// Rule defining which volume types and range determine a fee tier.
/// </summary>
public sealed class VolumeTypesAndRange
{
    /// <summary>
    /// Volume type(s) combined to evaluate this tier rule.
    /// See <see cref="VolumeType"/> for known values.
    /// </summary>
    [JsonPropertyName("volume_types")]
    public List<string>? VolumeTypes { get; set; }

    /// <summary>Lower bound (inclusive) of combined volume in USD.</summary>
    [JsonPropertyName("vol_from")]
    public string? VolFrom { get; set; }

    /// <summary>Upper bound (exclusive) of combined volume in USD.</summary>
    [JsonPropertyName("vol_to")]
    public string? VolTo { get; set; }
}

/// <summary>
/// Maker/taker fee tier applied to a user's trades.
/// </summary>
public sealed class FeeTier
{
    /// <summary>Human-readable pricing tier label (e.g. "&lt;$10k").</summary>
    [JsonPropertyName("pricing_tier")]
    public string? PricingTier { get; set; }

    /// <summary>Taker fee rate applied when the order takes liquidity (e.g. "0.0010").</summary>
    [JsonPropertyName("taker_fee_rate")]
    public string? TakerFeeRate { get; set; }

    /// <summary>Maker fee rate applied when the order adds liquidity (e.g. "0.0020").</summary>
    [JsonPropertyName("maker_fee_rate")]
    public string? MakerFeeRate { get; set; }

    /// <summary>Lower bound (inclusive) of this pricing tier in USD.</summary>
    [JsonPropertyName("aop_from")]
    public string? AopFrom { get; set; }

    /// <summary>Upper bound (exclusive) of this pricing tier in USD.</summary>
    [JsonPropertyName("aop_to")]
    public string? AopTo { get; set; }

    /// <summary>Rules defining how this tier evaluates 30-day volume.</summary>
    [JsonPropertyName("volume_types_and_range")]
    public List<VolumeTypesAndRange>? VolumeTypesAndRange { get; set; }
}
