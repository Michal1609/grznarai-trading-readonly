using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public interface ICoinbasePortfoliosClient
{
    /// <summary>
    /// List all portfolios visible to the authenticated user.
    /// </summary>
    /// <param name="portfolioType">
    /// Optional filter. Known values: <c>DEFAULT</c>, <c>CONSUMER</c>, <c>INTX</c>.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<ListPortfoliosResponse> ListPortfoliosAsync(string? portfolioType = null, CancellationToken ct = default);

    /// <summary>
    /// List all portfolios visible to the authenticated user using a request object.
    /// </summary>
    Task<ListPortfoliosResponse> ListPortfoliosAsync(ListPortfoliosRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get a detailed breakdown of a portfolio: balances, spot/perp/futures positions.
    /// </summary>
    /// <param name="portfolioUuid">UUID of the portfolio.</param>
    /// <param name="currency">Optional ISO 4217 currency code for valuation (e.g. <c>USD</c>).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetPortfolioBreakdownResponse> GetPortfolioBreakdownAsync(
        string portfolioUuid,
        string? currency = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get a detailed breakdown of a portfolio using a request object.
    /// </summary>
    Task<GetPortfolioBreakdownResponse> GetPortfolioBreakdownAsync(
        GetPortfolioBreakdownRequest request,
        CancellationToken ct = default);
}
