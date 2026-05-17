using GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Read-only client for Coinbase Advanced Trade Accounts endpoints.
/// </summary>
public interface ICoinbaseAccountsClient
{
    /// <summary>
    /// List the authenticated user's brokerage accounts.
    /// </summary>
    /// <param name="limit">Max accounts per page (1–250). Default: 49.</param>
    /// <param name="cursor">Pagination cursor from a previous response.</param>
    /// <param name="retailPortfolioId">Filter by retail portfolio ID (deprecated by Coinbase).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ListAccountsResponse> ListAccountsAsync(
        int? limit = null,
        string? cursor = null,
        string? retailPortfolioId = null,
        CancellationToken ct = default);

    /// <summary>
    /// List the authenticated user's brokerage accounts using a request object.
    /// </summary>
    Task<ListAccountsResponse> ListAccountsAsync(ListAccountsRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get a single brokerage account by UUID.
    /// </summary>
    /// <param name="accountUuid">The account's UUID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetAccountResponse> GetAccountAsync(string accountUuid, CancellationToken ct = default);
}
