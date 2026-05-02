using GrznarAi.Trading.ReadOnly.Models.Watchlist;

namespace GrznarAi.Trading.ReadOnly.Client;

public sealed partial class EToroClient
{
    public async Task<CuratedListsResponse?> GetCuratedListsAsync(CancellationToken ct = default)
    {
        return await GetFromJsonOrNoContentAsync<CuratedListsResponse>(
            "curated-lists",
            "Empty curated lists response.",
            ct).ConfigureAwait(false);
    }

    public async Task<MarketRecommendationsResponse?> GetMarketRecommendationsAsync(
        int itemsCount = 10,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(itemsCount, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(itemsCount, EToroRequestLimits.MaxItemsCount);

        return await GetFromJsonOrNoContentAsync<MarketRecommendationsResponse>(
            $"market-recommendations/{itemsCount}",
            "Empty market recommendations response.",
            ct).ConfigureAwait(false);
    }

    public async Task<WatchlistsResponse> GetUserWatchlistsAsync(
        int itemsPerPageForSingle = 100,
        bool ensureBuiltinWatchlists = true,
        bool addRelatedAssets = false,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(itemsPerPageForSingle, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(itemsPerPageForSingle, EToroRequestLimits.MaxItemsPerPage);

        var qs = new QueryStringBuilder()
            .Add("itemsPerPageForSingle", itemsPerPageForSingle)
            .Add("ensureBuiltinWatchlists", ensureBuiltinWatchlists)
            .Add("addRelatedAssets", addRelatedAssets);

        return await GetFromJsonAsync<WatchlistsResponse>(
            $"watchlists{qs}",
            "Empty user watchlists response.",
            ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<WatchlistItemDto>> GetDefaultWatchlistItemsAsync(
        int? itemsLimit = null,
        int itemsPerPage = 100,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(itemsPerPage, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(itemsPerPage, EToroRequestLimits.MaxItemsPerPage);
        if (itemsLimit.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(itemsLimit.Value, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(itemsLimit.Value, EToroRequestLimits.MaxItemsLimit);
        }

        var qs = new QueryStringBuilder()
            .Add("itemsPerPage", itemsPerPage)
            .AddIfHasValue("itemsLimit", itemsLimit);

        return await GetFromJsonAsync<IReadOnlyList<WatchlistItemDto>>(
            $"watchlists/default-watchlists/items{qs}",
            "Empty default watchlist items response.",
            ct).ConfigureAwait(false);
    }

    public async Task<WatchlistsResponse> GetUsersPublicWatchlistsAsync(
        int userId,
        int itemsPerPageForSingle = 100,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(itemsPerPageForSingle, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(itemsPerPageForSingle, EToroRequestLimits.MaxItemsPerPage);

        var qs = new QueryStringBuilder().Add("itemsPerPageForSingle", itemsPerPageForSingle);
        return await GetFromJsonAsync<WatchlistsResponse>(
            $"watchlists/public/{userId}{qs}",
            "Empty public watchlists response.",
            ct).ConfigureAwait(false);
    }

    public async Task<WatchlistDto> GetSinglePublicWatchlistAsync(
        int userId,
        string watchlistId,
        int pageNumber = 0,
        int itemsPerPage = 100,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(watchlistId);
        EToroInputValidator.ValidateRequiredString(watchlistId, nameof(watchlistId), EToroRequestLimits.MaxWatchlistIdLength);
        ArgumentOutOfRangeException.ThrowIfNegative(pageNumber);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(itemsPerPage, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(itemsPerPage, EToroRequestLimits.MaxItemsPerPage);

        var qs = new QueryStringBuilder()
            .Add("pageNumber", pageNumber)
            .Add("itemsPerPage", itemsPerPage);
        return await GetFromJsonAsync<WatchlistDto>(
            $"watchlists/public/{userId}/{QueryStringBuilder.EscapePathSegment(watchlistId)}{qs}",
            "Empty single public watchlist response.",
            ct).ConfigureAwait(false);
    }

    public async Task<WatchlistsResponse> GetSingleWatchlistAsync(
        string watchlistId,
        int pageNumber = 0,
        int itemsPerPage = 100,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(watchlistId);
        EToroInputValidator.ValidateRequiredString(watchlistId, nameof(watchlistId), EToroRequestLimits.MaxWatchlistIdLength);
        ArgumentOutOfRangeException.ThrowIfNegative(pageNumber);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(itemsPerPage, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(itemsPerPage, EToroRequestLimits.MaxItemsPerPage);

        var qs = new QueryStringBuilder()
            .Add("pageNumber", pageNumber)
            .Add("itemsPerPage", itemsPerPage);
        return await GetFromJsonAsync<WatchlistsResponse>(
            $"watchlists/{QueryStringBuilder.EscapePathSegment(watchlistId)}{qs}",
            "Empty single watchlist response.",
            ct).ConfigureAwait(false);
    }
}
