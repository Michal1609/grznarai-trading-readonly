using GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    public async Task<ListAccountsResponse> ListAccountsAsync(
        int? limit = null, string? cursor = null, CancellationToken ct = default)
    {
        if (limit.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit.Value);

        var qs = new QueryStringBuilder()
            .AddIfHasValue("limit", limit)
            .Add("cursor", cursor);

        return await GetFromJsonAsync<ListAccountsResponse>(
            $"/api/v3/brokerage/accounts{qs}",
            "Empty list-accounts response.",
            ct).ConfigureAwait(false);
    }

    public async Task<GetAccountResponse> GetAccountAsync(string accountUuid, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountUuid);

        return await GetFromJsonAsync<GetAccountResponse>(
            $"/api/v3/brokerage/accounts/{QueryStringBuilder.EscapePathSegment(accountUuid)}",
            "Empty get-account response.",
            ct).ConfigureAwait(false);
    }
}
