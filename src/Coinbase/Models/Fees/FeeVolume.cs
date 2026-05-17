using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;

/// <summary>
/// Volume entry contributing to the fee tier calculation.
/// </summary>
public sealed class FeeVolume
{
    /// <summary>
    /// Category of this volume.
    /// See <see cref="VolumeType"/> for known values.
    /// </summary>
    [JsonPropertyName("volume_type")]
    public string? VolumeType { get; set; }

    /// <summary>Volume amount in USD.</summary>
    [JsonPropertyName("volume")]
    public decimal? Volume { get; set; }
}
