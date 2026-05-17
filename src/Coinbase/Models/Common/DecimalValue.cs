using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Json;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

/// <summary>
/// Coinbase common decimal shape: <c>{ "value": "1234.56" }</c>.
/// Used for rate/margin fields that carry no currency.
/// </summary>
public sealed class DecimalValue
{
    [JsonPropertyName("value")]
    [JsonConverter(typeof(DecimalStringConverter))]
    public decimal Value { get; set; }
}
