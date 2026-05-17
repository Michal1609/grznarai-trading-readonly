using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.PaymentMethods;

/// <summary>
/// Response from GET /api/v3/brokerage/payment_methods.
/// </summary>
public sealed class ListPaymentMethodsResponse
{
    /// <summary>All payment methods registered with the authenticated account.</summary>
    [JsonPropertyName("payment_methods")]
    public List<PaymentMethod>? PaymentMethods { get; set; }
}
