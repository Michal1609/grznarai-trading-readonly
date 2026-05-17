using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Monetary amount returned by International Derivatives (perpetuals) endpoints.
/// </summary>
public sealed class PerpetualAmount
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
