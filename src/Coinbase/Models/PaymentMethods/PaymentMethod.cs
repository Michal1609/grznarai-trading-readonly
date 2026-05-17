using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.PaymentMethods;

/// <summary>
/// A payment method registered with the authenticated Coinbase account.
/// </summary>
public sealed class PaymentMethod
{
    /// <summary>Unique payment method identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Payment method type (e.g. "ACH", "CREDIT_CARD", "PAYPAL").</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Display name with masked account details (e.g. "ALLY BANK ******1234").</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Currency symbol for this payment method (e.g. "USD").</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>Whether the payment method has been verified by Coinbase.</summary>
    [JsonPropertyName("verified")]
    public bool? Verified { get; set; }

    /// <summary>Whether this payment method can be used to buy crypto.</summary>
    [JsonPropertyName("allow_buy")]
    public bool? AllowBuy { get; set; }

    /// <summary>Whether this payment method can be used to sell crypto.</summary>
    [JsonPropertyName("allow_sell")]
    public bool? AllowSell { get; set; }

    /// <summary>Whether this payment method can be used for deposits.</summary>
    [JsonPropertyName("allow_deposit")]
    public bool? AllowDeposit { get; set; }

    /// <summary>Whether this payment method can be used for withdrawals.</summary>
    [JsonPropertyName("allow_withdraw")]
    public bool? AllowWithdraw { get; set; }

    /// <summary>Timestamp when the payment method was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>Timestamp when the payment method was last updated.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
