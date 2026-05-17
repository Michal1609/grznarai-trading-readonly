using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Payment method reference used as source or target in a convert trade.
/// </summary>
public sealed class ConvertPaymentMethod
{
    /// <summary>Payment method type classification.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Blockchain or payment network name.</summary>
    [JsonPropertyName("network")]
    public string? Network { get; set; }

    /// <summary>Unique identifier of the payment method.</summary>
    [JsonPropertyName("payment_method_id")]
    public string? PaymentMethodId { get; set; }

    /// <summary>Human-readable type description.</summary>
    [JsonPropertyName("payment_method_type_string")]
    public string? PaymentMethodTypeString { get; set; }
}
