using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Tax line item attached to a convert trade.
/// </summary>
public sealed class ConvertTaxInfo
{
    /// <summary>Tax type name (e.g. "VAT", "GST").</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Tax amount.</summary>
    [JsonPropertyName("amount")]
    public ConvertAmount? Amount { get; set; }
}
