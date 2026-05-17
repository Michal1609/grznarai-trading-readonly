using GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    /// <inheritdoc cref="ICoinbaseAccountsClient.ListAccountsAsync(int?,string?,string?,CancellationToken)"/>
    public async Task<ListAccountsResponse> ListAccountsAsync(
        int? limit = null,
        string? cursor = null,
        string? retailPortfolioId = null,
        CancellationToken ct = default)
    {
        if (limit.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit.Value);

        var qs = new QueryStringBuilder()
            .AddIfHasValue("limit", limit)
            .Add("cursor", cursor)
            .Add("retail_portfolio_id", retailPortfolioId);

        return await GetFromJsonAsync<ListAccountsResponse>(
            $"/api/v3/brokerage/accounts{qs}",
            "Empty list-accounts response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseAccountsClient.ListAccountsAsync(ListAccountsRequest,CancellationToken)"/>
    public Task<ListAccountsResponse> ListAccountsAsync(ListAccountsRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ListAccountsAsync(request.Limit, request.Cursor, request.RetailPortfolioId, ct);
    }

    /// <inheritdoc cref="ICoinbaseAccountsClient.GetAccountAsync"/>
    public async Task<GetAccountResponse> GetAccountAsync(string accountUuid, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountUuid);

        return await GetFromJsonAsync<GetAccountResponse>(
            $"/api/v3/brokerage/accounts/{QueryStringBuilder.EscapePathSegment(accountUuid)}",
            "Empty get-account response.",
            ct).ConfigureAwait(false);
    }
}
