using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Json;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Monetary amount as returned by the Coinbase Convert API.
/// </summary>
public sealed class ConvertAmount
{
    /// <summary>Decimal value encoded as a string by the API.</summary>
    [JsonPropertyName("value")]
    [JsonConverter(typeof(DecimalStringConverter))]
    public decimal Value { get; set; }

    /// <summary>ISO 4217 currency code (e.g. "USD", "BTC").</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>CBRN resource identifier (version:type:network:sub-network:id:sub-id).</summary>
    [JsonPropertyName("cbrn")]
    public string? Cbrn { get; set; }
}
