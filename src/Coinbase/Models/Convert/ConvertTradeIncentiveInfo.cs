using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Promotional incentive applied to a convert trade.
/// </summary>
public sealed class ConvertTradeIncentiveInfo
{
    /// <summary>Whether an incentive was applied.</summary>
    [JsonPropertyName("applied_incentive")]
    public bool? AppliedIncentive { get; set; }

    /// <summary>Internal identifier of the user's incentive.</summary>
    [JsonPropertyName("user_incentive_id")]
    public string? UserIncentiveId { get; set; }

    /// <summary>Promotional code value redeemed.</summary>
    [JsonPropertyName("code_val")]
    public string? CodeVal { get; set; }

    /// <summary>When the incentive offer expires.</summary>
    [JsonPropertyName("ends_at")]
    public DateTimeOffset? EndsAt { get; set; }

    /// <summary>Fee that would have applied without the incentive.</summary>
    [JsonPropertyName("fee_without_incentive")]
    public ConvertAmount? FeeWithoutIncentive { get; set; }

    /// <summary>Whether the incentive has been redeemed.</summary>
    [JsonPropertyName("redeemed")]
    public bool? Redeemed { get; set; }
}
