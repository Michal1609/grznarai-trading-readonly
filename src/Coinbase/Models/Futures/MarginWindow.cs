using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Describes the currently active margin window for CFM futures trading.
/// </summary>
public sealed class MarginWindow
{
    /// <summary>Type of margin window — see <see cref="MarginWindowType"/>.</summary>
    [JsonPropertyName("margin_window_type")]
    public string? MarginWindowType { get; set; }

    /// <summary>UTC timestamp when this margin window ends.</summary>
    [JsonPropertyName("end_time")]
    public DateTimeOffset? EndTime { get; set; }
}
