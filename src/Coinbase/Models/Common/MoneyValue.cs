using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Json;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

/// <summary>
/// Coinbase common money shape: <c>{ "value": "1234.56", "currency": "USD" }</c>.
/// </summary>
public sealed class MoneyValue
{
    [JsonPropertyName("value")]
    [JsonConverter(typeof(DecimalStringConverter))]
    public decimal Value { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
