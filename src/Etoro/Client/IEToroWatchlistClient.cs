using GrznarAi.Trading.ReadOnly.Etoro.Models.Watchlist;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

public interface IEToroWatchlistClient
{
    /// <summary>
    /// Retrieves curated investment lists available to the authenticated user.
    /// <br/>CZ: VrĂˇtĂ­ kurĂˇtorovanĂ© investiÄŤnĂ­ seznamy dostupnĂ© pĹ™ihlĂˇĹˇenĂ©mu uĹľivateli. VracĂ­ null pĹ™i prĂˇzdnĂ©m vĂ˝sledku (HTTP 204).
    /// </summary>
    Task<CuratedListsResponse?> GetCuratedListsAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves personalized market recommendations for the authenticated user.
    /// <br/>CZ: VrĂˇtĂ­ personalizovanĂˇ trĹľnĂ­ doporuÄŤenĂ­ pro pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele. VracĂ­ null pĹ™i prĂˇzdnĂ©m vĂ˝sledku (HTTP 204).
    /// </summary>
    Task<MarketRecommendationsResponse?> GetMarketRecommendationsAsync(
        int itemsCount = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all watchlists for the authenticated user with optional pagination and built-in watchlist management.
    /// <br/>CZ: VrĂˇtĂ­ vĹˇechny sledovanĂ© seznamy pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele s volitelnou strĂˇnkovacĂ­ a sprĂˇvou vestavÄ›nĂ˝ch seznamĹŻ.
    /// </summary>
    Task<WatchlistsResponse> GetUserWatchlistsAsync(
        int itemsPerPageForSingle = 100,
        bool ensureBuiltinWatchlists = true,
        bool addRelatedAssets = false,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves items from the user's default watchlists with an optional total-count limit.
    /// <br/>CZ: VrĂˇtĂ­ poloĹľky z vĂ˝chozĂ­ch sledovanĂ˝ch seznamĹŻ uĹľivatele s volitelnĂ˝m omezenĂ­m celkovĂ©ho poÄŤtu.
    /// </summary>
    Task<IReadOnlyList<WatchlistItemDto>> GetDefaultWatchlistItemsAsync(
        int? itemsLimit = null,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all public watchlists for a specific user.
    /// <br/>CZ: VrĂˇtĂ­ vĹˇechny veĹ™ejnĂ© sledovanĂ© seznamy konkrĂ©tnĂ­ho uĹľivatele.
    /// </summary>
    Task<WatchlistsResponse> GetUsersPublicWatchlistsAsync(
        int userId,
        int itemsPerPageForSingle = 100,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a specific public watchlist from a user.
    /// <br/>CZ: VrĂˇtĂ­ konkrĂ©tnĂ­ veĹ™ejnĂ˝ sledovanĂ˝ seznam danĂ©ho uĹľivatele.
    /// </summary>
    Task<WatchlistDto> GetSinglePublicWatchlistAsync(
        int userId,
        string watchlistId,
        int pageNumber = 0,
        int itemsPerPage = 100,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a specific watchlist with its items using pagination.
    /// <br/>CZ: VrĂˇtĂ­ konkrĂ©tnĂ­ sledovanĂ˝ seznam s jeho poloĹľkami a strĂˇnkovĂˇnĂ­m.
    /// </summary>
    Task<WatchlistsResponse> GetSingleWatchlistAsync(
        string watchlistId,
        int pageNumber = 0,
        int itemsPerPage = 100,
        CancellationToken ct = default);
}
