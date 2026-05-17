using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Margin measurements for a single margin window (intraday or overnight) within the
/// futures balance summary. All monetary fields are string-encoded decimals.
/// </summary>
public sealed class FcmMarginWindowMeasure
{
    /// <summary>Window type — see <see cref="FcmMarginWindowType"/>.</summary>
    [JsonPropertyName("margin_window_type")]
    public string? MarginWindowType { get; set; }

    /// <summary>Current margin health level — see <see cref="MarginLevelType"/>.</summary>
    [JsonPropertyName("margin_level")]
    public string? MarginLevel { get; set; }

    [JsonPropertyName("initial_margin")]
    public string? InitialMargin { get; set; }

    [JsonPropertyName("maintenance_margin")]
    public string? MaintenanceMargin { get; set; }

    [JsonPropertyName("liquidation_buffer")]
    public string? LiquidationBuffer { get; set; }

    [JsonPropertyName("total_hold")]
    public string? TotalHold { get; set; }

    [JsonPropertyName("futures_buying_power")]
    public string? FuturesBuyingPower { get; set; }
}
