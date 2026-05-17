using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Monetary amount returned by CFM futures endpoints, carrying an optional
/// Coinbase Resource Name (<c>cbrn</c>) in addition to <c>value</c> and <c>currency</c>.
/// </summary>
public sealed class FuturesAmount
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>Coinbase Resource Name — may be absent for older response shapes.</summary>
    [JsonPropertyName("cbrn")]
    public string? Cbrn { get; set; }
}
