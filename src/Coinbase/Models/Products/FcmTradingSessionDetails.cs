using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>FCM (Futures Commission Merchant) trading session details nested inside <see cref="Product"/>.</summary>
public sealed class FcmTradingSessionDetails
{
    [JsonPropertyName("is_session_open")] public bool? IsSessionOpen { get; set; }
    [JsonPropertyName("open_time")] public DateTimeOffset? OpenTime { get; set; }
    [JsonPropertyName("close_time")] public DateTimeOffset? CloseTime { get; set; }
    [JsonPropertyName("session_state")] public string? SessionState { get; set; }
    [JsonPropertyName("after_hours_order_entry_disabled")] public bool? AfterHoursOrderEntryDisabled { get; set; }
    [JsonPropertyName("closed_reason")] public string? ClosedReason { get; set; }
    [JsonPropertyName("maintenance")] public string? Maintenance { get; set; }
}
