using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Fee entry as returned by the Coinbase Convert API.
/// </summary>
public sealed class ConvertFee
{
    /// <summary>Display name for the fee line item.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>Human-readable explanation of what the fee covers.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Monetary value of the fee.</summary>
    [JsonPropertyName("amount")]
    public ConvertAmount? Amount { get; set; }

    /// <summary>Machine-readable fee identifier.</summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
