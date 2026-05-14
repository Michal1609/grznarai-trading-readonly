using GrznarAi.Trading.ReadOnly.Etoro.Models.Social;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

public interface IEToroSocialClient
{
    /// <summary>
    /// Comprehensive search and analytics engine for user discovery â€” returns popular investors with performance metrics.
    /// <br/>CZ: VyhledĂˇvĂˇnĂ­ a analĂ˝za uĹľivatelĹŻ â€” vrĂˇtĂ­ populĂˇrnĂ­ investory s vĂ˝konnostnĂ­mi metrikami.
    /// </summary>
    Task<PopularInvestorsResponse> GetPopularInvestorsAsync(
        PopularInvestorsRequest request,
        CancellationToken ct = default);
}
