using GrznarAi.Trading.ReadOnly.Coinbase.Models.PaymentMethods;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Read-only client for Coinbase Advanced Trade Payment Methods endpoints.
/// </summary>
public interface ICoinbasePaymentMethodsClient
{
    /// <summary>
    /// List all payment methods registered with the authenticated Coinbase account.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All registered payment methods.</returns>
    Task<ListPaymentMethodsResponse> ListPaymentMethodsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get a single payment method by its identifier.
    /// </summary>
    /// <param name="paymentMethodId">The unique identifier of the payment method.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The requested payment method details.</returns>
    Task<GetPaymentMethodResponse> GetPaymentMethodAsync(string paymentMethodId, CancellationToken ct = default);
}
