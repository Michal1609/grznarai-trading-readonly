using GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Read-only client for Coinbase Advanced Trade Fees endpoints.
/// </summary>
public interface ICoinbaseFeesClient
{
    /// <summary>
    /// Get a summary of transaction fees, volumes, and rates for the current user.
    /// <para>
    /// Endpoint: <c>GET /api/v3/brokerage/transaction_summary</c>
    /// </para>
    /// </summary>
    /// <param name="productType">
    /// Filter by product type. See <c>ProductType</c> constants.
    /// Defaults to all product types when <see langword="null"/>.
    /// </param>
    /// <param name="contractExpiryType">
    /// Filter by contract expiry type. Only applicable when <paramref name="productType"/> is <c>FUTURE</c>.
    /// </param>
    /// <param name="productVenue">
    /// Filter by product venue. See <c>ProductVenue</c> constants.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetTransactionSummaryResponse> GetTransactionSummaryAsync(
        string? productType = null,
        string? contractExpiryType = null,
        string? productVenue = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get a summary of transaction fees, volumes, and rates using a request object.
    /// <para>
    /// Endpoint: <c>GET /api/v3/brokerage/transaction_summary</c>
    /// </para>
    /// </summary>
    Task<GetTransactionSummaryResponse> GetTransactionSummaryAsync(
        GetTransactionSummaryRequest request,
        CancellationToken ct = default);
}
