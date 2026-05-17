using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Response from <c>GET /api/v3/brokerage/cfm/intraday/margin_setting</c>.
/// </summary>
public sealed class GetIntradayMarginSettingResponse
{
    /// <summary>
    /// Current intraday margin enrollment setting for the authenticated user.
    /// Use constants from <see cref="IntradayMarginSetting"/> to interpret.
    /// </summary>
    [JsonPropertyName("setting")]
    public string? Setting { get; set; }
}
