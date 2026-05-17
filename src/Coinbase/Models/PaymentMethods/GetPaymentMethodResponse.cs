using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.PaymentMethods;

/// <summary>
/// Response from GET /api/v3/brokerage/payment_methods/{payment_method_id}.
/// </summary>
public sealed class GetPaymentMethodResponse
{
    /// <summary>The requested payment method.</summary>
    [JsonPropertyName("payment_method")]
    public PaymentMethod? PaymentMethod { get; set; }
}
