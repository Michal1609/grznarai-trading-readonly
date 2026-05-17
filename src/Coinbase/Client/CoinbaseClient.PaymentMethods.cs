using GrznarAi.Trading.ReadOnly.Coinbase.Models.PaymentMethods;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    /// <inheritdoc cref="ICoinbasePaymentMethodsClient.ListPaymentMethodsAsync"/>
    public async Task<ListPaymentMethodsResponse> ListPaymentMethodsAsync(CancellationToken ct = default)
    {
        return await GetFromJsonAsync<ListPaymentMethodsResponse>(
            "/api/v3/brokerage/payment_methods",
            "Empty list-payment-methods response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbasePaymentMethodsClient.GetPaymentMethodAsync"/>
    public async Task<GetPaymentMethodResponse> GetPaymentMethodAsync(
        string paymentMethodId,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentMethodId);

        return await GetFromJsonAsync<GetPaymentMethodResponse>(
            $"/api/v3/brokerage/payment_methods/{QueryStringBuilder.EscapePathSegment(paymentMethodId)}",
            "Empty get-payment-method response.",
            ct).ConfigureAwait(false);
    }
}
