using GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    /// <inheritdoc cref="ICoinbaseFeesClient.GetTransactionSummaryAsync(string?,string?,string?,CancellationToken)"/>
    public async Task<GetTransactionSummaryResponse> GetTransactionSummaryAsync(
        string? productType = null,
        string? contractExpiryType = null,
        string? productVenue = null,
        CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("product_type", productType)
            .Add("contract_expiry_type", contractExpiryType)
            .Add("product_venue", productVenue);

        return await GetFromJsonAsync<GetTransactionSummaryResponse>(
            $"/api/v3/brokerage/transaction_summary{qs}",
            "Empty get-transaction-summary response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseFeesClient.GetTransactionSummaryAsync(GetTransactionSummaryRequest,CancellationToken)"/>
    public Task<GetTransactionSummaryResponse> GetTransactionSummaryAsync(
        GetTransactionSummaryRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetTransactionSummaryAsync(request.ProductType, request.ContractExpiryType, request.ProductVenue, ct);
    }
}
