using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Full representation of a convert trade as returned by the Coinbase Advanced Trade API.
/// Use <see cref="TradeStatus"/> constants to compare <see cref="Status"/>.
/// </summary>
public sealed class ConvertTrade
{
    /// <summary>Unique trade identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Current trade status. See <see cref="TradeStatus"/> for known values.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Amount as entered by the user.</summary>
    [JsonPropertyName("user_entered_amount")]
    public ConvertAmount? UserEnteredAmount { get; set; }

    /// <summary>Converted amount after all adjustments.</summary>
    [JsonPropertyName("amount")]
    public ConvertAmount? Amount { get; set; }

    /// <summary>Pre-fee subtotal.</summary>
    [JsonPropertyName("subtotal")]
    public ConvertAmount? Subtotal { get; set; }

    /// <summary>Total charged to the user including all fees.</summary>
    [JsonPropertyName("total")]
    public ConvertAmount? Total { get; set; }

    /// <summary>Individual fee line items.</summary>
    [JsonPropertyName("fees")]
    public List<ConvertFee>? Fees { get; set; }

    /// <summary>Aggregate fee for the trade.</summary>
    [JsonPropertyName("total_fee")]
    public ConvertFee? TotalFee { get; set; }

    /// <summary>Source payment method.</summary>
    [JsonPropertyName("source")]
    public ConvertPaymentMethod? Source { get; set; }

    /// <summary>Target payment method.</summary>
    [JsonPropertyName("target")]
    public ConvertPaymentMethod? Target { get; set; }

    /// <summary>Per-unit exchange rate breakdown.</summary>
    [JsonPropertyName("unit_price")]
    public ConvertUnitPrice? UnitPrice { get; set; }

    /// <summary>Warnings shown to the user for this trade.</summary>
    [JsonPropertyName("user_warnings")]
    public List<ConvertUserWarning>? UserWarnings { get; set; }

    /// <summary>User-supplied reference string.</summary>
    [JsonPropertyName("user_reference")]
    public string? UserReference { get; set; }

    /// <summary>Source currency code.</summary>
    [JsonPropertyName("source_currency")]
    public string? SourceCurrency { get; set; }

    /// <summary>Target currency code.</summary>
    [JsonPropertyName("target_currency")]
    public string? TargetCurrency { get; set; }

    /// <summary>Reason the trade was cancelled, if applicable.</summary>
    [JsonPropertyName("cancellation_reason")]
    public ConvertCancellationReason? CancellationReason { get; set; }

    /// <summary>Source account identifier.</summary>
    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }

    /// <summary>Target account identifier.</summary>
    [JsonPropertyName("target_id")]
    public string? TargetId { get; set; }

    /// <summary>Coinbase One subscription benefit details.</summary>
    [JsonPropertyName("subscription_info")]
    public ConvertSubscriptionInfo? SubscriptionInfo { get; set; }

    /// <summary>Exchange rate applied for this trade.</summary>
    [JsonPropertyName("exchange_rate")]
    public ConvertAmount? ExchangeRate { get; set; }

    /// <summary>Tax line items applied to the trade.</summary>
    [JsonPropertyName("tax_details")]
    public List<ConvertTaxInfo>? TaxDetails { get; set; }

    /// <summary>Promotional incentive details.</summary>
    [JsonPropertyName("trade_incentive_info")]
    public ConvertTradeIncentiveInfo? TradeIncentiveInfo { get; set; }

    /// <summary>Total fee excluding tax.</summary>
    [JsonPropertyName("total_fee_without_tax")]
    public ConvertFee? TotalFeeWithoutTax { get; set; }

    /// <summary>Total expressed in fiat currency.</summary>
    [JsonPropertyName("fiat_denoted_total")]
    public ConvertAmount? FiatDenotedTotal { get; set; }
}
