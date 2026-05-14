using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public interface ICoinbasePortfoliosClient
{
    /// <summary>
    /// List all portfolios visible to the authenticated user.
    /// <br/>CZ: VrĂˇtĂ­ vĹˇechna portfolia viditelnĂˇ pro pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele.
    /// </summary>
    Task<ListPortfoliosResponse> ListPortfoliosAsync(string? portfolioType = null, CancellationToken ct = default);

    /// <summary>
    /// Get a detailed breakdown of a portfolio: balances, spot/perp/futures positions.
    /// <br/>CZ: VrĂˇtĂ­ detailnĂ­ rozpad portfolia: zĹŻstatky, spot/perp/futures pozice.
    /// </summary>
    Task<GetPortfolioBreakdownResponse> GetPortfolioBreakdownAsync(
        string portfolioUuid,
        string? currency = null,
        CancellationToken ct = default);

    /// <summary>
    /// Request-object overload for <see cref="GetPortfolioBreakdownAsync(string,string?,CancellationToken)"/>.
    /// <br/>CZ: Varianta s request objektem pro <see cref="GetPortfolioBreakdownAsync(string,string?,CancellationToken)"/>.
    /// </summary>
    Task<GetPortfolioBreakdownResponse> GetPortfolioBreakdownAsync(
        GetPortfolioBreakdownRequest request,
        CancellationToken ct = default);
}
