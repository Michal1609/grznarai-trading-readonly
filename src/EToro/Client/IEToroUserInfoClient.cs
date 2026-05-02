using GrznarAi.Trading.ReadOnly.Models.Identity;
using GrznarAi.Trading.ReadOnly.Models.UserInfo;

namespace GrznarAi.Trading.ReadOnly.Client;

public interface IEToroUserInfoClient
{
    /// <summary>
    /// Retrieves the authenticated user's identity, including global, real, and demo account customer IDs.
    /// <br/>CZ: Vrátí identitu přihlášeného uživatele, včetně globálního, reálného a demo zákaznického ID.
    /// </summary>
    Task<UserIdentityResponse> GetIdentityAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns detailed user profile information including account status, verification levels, biographical data, and associated metadata.
    /// <br/>CZ: Vrati detailni profil uzivatele vcetne stavu uctu, urovni overeni, biografickych dat a souvisejicich metadat.
    /// </summary>
    Task<UserProfileResponse> GetUserProfilesAsync(
        IEnumerable<string>? usernames = null,
        IEnumerable<int>? cidList = null,
        CancellationToken ct = default);

    /// <summary>
    /// Powerful search platform that enables advanced user discovery with comprehensive filtering capabilities.
    /// <br/>CZ: Pokrocile vyhledavani uzivatelu s rozsahlymi filtry podle vykonnosti, rizika, investicnich vzorcu a vlastnosti uctu.
    /// </summary>
    Task<UserSearchResponse> SearchUsersAsync(
        UserSearchRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Provides detailed performance analytics including daily gains, cumulative returns, and period-specific metrics within a specified date range.
    /// <br/>CZ: Vrati detailni vykonnostni analytiku vcetne dennich zisku, kumulativnich vynosu a metrik pro zadane obdobi.
    /// </summary>
    Task<UserDailyGainResponse> GetUserDailyGainAsync(
        string username,
        DateOnly minDate,
        DateOnly maxDate,
        UserDailyGainType type = UserDailyGainType.Daily,
        CancellationToken ct = default);

    /// <summary>
    /// Returns comprehensive historical monthly and yearly performance data including gain percentages and detailed trading statistics.
    /// <br/>CZ: Vrati historickou mesicni a rocni vykonnost vcetne procentualnich zisku a detailnich obchodnich statistik.
    /// </summary>
    Task<UserGainResponse> GetUserGainAsync(
        string username,
        CancellationToken ct = default);

    /// <summary>
    /// Get the live portfolio of a user.
    /// <br/>CZ: Vrati aktualni zive portfolio uzivatele.
    /// </summary>
    Task<UserLivePortfolioResponse> GetUserLivePortfolioAsync(
        string username,
        CancellationToken ct = default);

    /// <summary>
    /// Get trade info for a specific user.
    /// <br/>CZ: Vrati obchodni informace pro konkretniho uzivatele.
    /// </summary>
    Task<UserTradeInfoResponse> GetUserTradeInfoAsync(
        string username,
        UserInfoPeriod period,
        CancellationToken ct = default);
}
