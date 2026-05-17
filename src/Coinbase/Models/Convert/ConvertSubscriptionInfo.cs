using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Coinbase One / subscription benefit details applied to a convert trade.
/// </summary>
public sealed class ConvertSubscriptionInfo
{
    /// <summary>Date and time when the zero-fee trading allowance resets.</summary>
    [JsonPropertyName("free_trading_reset_date")]
    public DateTimeOffset? FreeTradingResetDate { get; set; }

    /// <summary>Amount of zero-fee trading volume already consumed in the current period.</summary>
    [JsonPropertyName("used_zero_fee_trading")]
    public ConvertAmount? UsedZeroFeeTrading { get; set; }

    /// <summary>Remaining zero-fee trading volume available in the current period.</summary>
    [JsonPropertyName("remaining_free_trading_volume")]
    public ConvertAmount? RemainingFreeTradingVolume { get; set; }

    /// <summary>Maximum zero-fee trading volume allowed by the subscription.</summary>
    [JsonPropertyName("max_free_trading_volume")]
    public ConvertAmount? MaxFreeTradingVolume { get; set; }

    /// <summary>Whether a benefit cap is in effect.</summary>
    [JsonPropertyName("has_benefit_cap")]
    public bool? HasBenefitCap { get; set; }

    /// <summary>Whether a subscription benefit was applied to this trade.</summary>
    [JsonPropertyName("applied_subscription_benefit")]
    public bool? AppliedSubscriptionBenefit { get; set; }

    /// <summary>Fee that would have applied without the subscription benefit.</summary>
    [JsonPropertyName("fee_without_subscription_benefit")]
    public ConvertAmount? FeeWithoutSubscriptionBenefit { get; set; }

    /// <summary>Payment-method fee baseline without the subscription benefit.</summary>
    [JsonPropertyName("payment_method_fee_without_subscription_benefit")]
    public ConvertAmount? PaymentMethodFeeWithoutSubscriptionBenefit { get; set; }
}
