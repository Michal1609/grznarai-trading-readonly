using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;

/// <summary>
/// Goods and services tax applied to a transaction.
/// </summary>
public sealed class GoodsAndServicesTax
{
    /// <summary>GST rate (e.g. "0.10" for 10%).</summary>
    [JsonPropertyName("rate")]
    public string? Rate { get; set; }

    /// <summary>
    /// Whether the GST is inclusive or exclusive.
    /// See <see cref="GstType"/> for known values.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
