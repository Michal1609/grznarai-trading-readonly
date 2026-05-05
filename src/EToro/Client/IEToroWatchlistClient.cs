using GrznarAi.Trading.ReadOnly.Models.Watchlist;

namespace GrznarAi.Trading.ReadOnly.Client;

public interface IEToroWatchlistClient
{
    /// <summary>
    /// Retrieves curated investment lists available to the authenticated user.
    /// <br/>CZ: Vrátí kurátorované investiční seznamy dostupné přihlášenému uživateli. Vrací null při prázdném výsledku (HTTP 204).
    /// </summary>
    Task<CuratedListsResponse?> GetCuratedListsAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves personalized market recommendations for the authenticated user.
    /// <br/>CZ: Vrátí personalizovaná tržní doporučení pro přihlášeného uživatele. Vrací null při prázdném výsledku (HTTP 204).
    /// </summary>
    Task<MarketRecommendationsResponse?> GetMarketRecommendationsAsync(
        int itemsCount = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all watchlists for the authenticated user with optional pagination and built-in watchlist management.
    /// <br/>CZ: Vrátí všechny sledované seznamy přihlášeného uživatele s volitelnou stránkovací a správou vestavěných seznamů.
    /// </summary>
    Task<WatchlistsResponse> GetUserWatchlistsAsync(
        int itemsPerPageForSingle = 100,
        bool ensureBuiltinWatchlists = true,
        bool addRelatedAssets = false,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves items from the user's default watchlists with an optional total-count limit.
    /// <br/>CZ: Vrátí položky z výchozích sledovaných seznamů uživatele s volitelným omezením celkového počtu.
    /// </summary>
    Task<IReadOnlyList<WatchlistItemDto>> GetDefaultWatchlistItemsAsync(
        int? itemsLimit = null,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all public watchlists for a specific user.
    /// <br/>CZ: Vrátí všechny veřejné sledované seznamy konkrétního uživatele.
    /// </summary>
    Task<WatchlistsResponse> GetUsersPublicWatchlistsAsync(
        int userId,
        int itemsPerPageForSingle = 100,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a specific public watchlist from a user.
    /// <br/>CZ: Vrátí konkrétní veřejný sledovaný seznam daného uživatele.
    /// </summary>
    Task<WatchlistDto> GetSinglePublicWatchlistAsync(
        int userId,
        string watchlistId,
        int pageNumber = 0,
        int itemsPerPage = 100,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a specific watchlist with its items using pagination.
    /// <br/>CZ: Vrátí konkrétní sledovaný seznam s jeho položkami a stránkováním.
    /// </summary>
    Task<WatchlistsResponse> GetSingleWatchlistAsync(
        string watchlistId,
        int pageNumber = 0,
        int itemsPerPage = 100,
        CancellationToken ct = default);
}
