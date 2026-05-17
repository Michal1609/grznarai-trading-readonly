using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Response from <c>GET /api/v3/brokerage/cfm/intraday/current_margin_window</c>.
/// </summary>
public sealed class GetCurrentMarginWindowResponse
{
    /// <summary>Details of the currently active margin window.</summary>
    [JsonPropertyName("margin_window")]
    public MarginWindow? MarginWindow { get; set; }

    /// <summary>
    /// When <c>true</c>, intraday margin trading is globally disabled
    /// and all positions revert to overnight margin requirements.
    /// </summary>
    [JsonPropertyName("is_intraday_margin_killswitch_enabled")]
    public bool? IsIntradayMarginKillswitchEnabled { get; set; }

    /// <summary>
    /// When <c>true</c>, new enrollment in intraday margin is blocked.
    /// </summary>
    [JsonPropertyName("is_intraday_margin_enrollment_killswitch_enabled")]
    public bool? IsIntradayMarginEnrollmentKillswitchEnabled { get; set; }
}
