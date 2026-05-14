using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Json;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;

public sealed class AccountBalance
{
    [JsonPropertyName("value")]
    [JsonConverter(typeof(DecimalStringConverter))]
    public decimal Value { get; set; }

    [JsonPropertyName("currency")] public string? Currency { get; set; }
}
