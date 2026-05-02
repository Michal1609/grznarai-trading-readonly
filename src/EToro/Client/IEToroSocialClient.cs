using GrznarAi.Trading.ReadOnly.Models.Social;

namespace GrznarAi.Trading.ReadOnly.Client;

public interface IEToroSocialClient
{
    /// <summary>
    /// Comprehensive search and analytics engine for user discovery — returns popular investors with performance metrics.
    /// <br/>CZ: Vyhledávání a analýza uživatelů — vrátí populární investory s výkonnostními metrikami.
    /// </summary>
    Task<PopularInvestorsResponse> GetPopularInvestorsAsync(
        PopularInvestorsRequest request,
        CancellationToken ct = default);
}
