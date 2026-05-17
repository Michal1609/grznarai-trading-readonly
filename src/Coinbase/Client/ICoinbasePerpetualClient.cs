using GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Coinbase Advanced Trade API client for International Derivatives (perpetuals/intx) endpoints.
/// All endpoints require credentials and an intx-enabled portfolio.
/// </summary>
public interface ICoinbasePerpetualClient
{
    /// <summary>
    /// Returns a summary of the perpetuals portfolio, including collateral, margins,
    /// liquidation status, and unrealized P&amp;L.
    /// </summary>
    /// <param name="portfolioUuid">Unique identifier of the perpetuals portfolio.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/intx/portfolio/{portfolio_uuid}</remarks>
    Task<GetPerpetualPortfolioSummaryResponse> GetPerpetualPortfolioSummaryAsync(
        string portfolioUuid,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a single open perpetuals position by portfolio and symbol.
    /// </summary>
    /// <param name="portfolioUuid">Unique identifier of the perpetuals portfolio.</param>
    /// <param name="symbol">Trading pair symbol, e.g. <c>BTC-PERP-INTX</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/intx/positions/{portfolio_uuid}/{symbol}</remarks>
    Task<GetPerpetualPositionResponse> GetPerpetualPositionAsync(
        string portfolioUuid,
        string symbol,
        CancellationToken ct = default);

    /// <summary>
    /// Returns asset balances for a perpetuals portfolio.
    /// </summary>
    /// <param name="portfolioUuid">Unique identifier of the perpetuals portfolio.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/intx/balances/{portfolio_uuid}</remarks>
    Task<GetPortfolioBalancesResponse> GetPortfolioBalancesAsync(
        string portfolioUuid,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all open perpetuals positions for a portfolio, plus an aggregate summary.
    /// </summary>
    /// <param name="portfolioUuid">Unique identifier of the perpetuals portfolio.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/intx/positions/{portfolio_uuid}</remarks>
    Task<ListPerpetualPositionsResponse> ListPerpetualPositionsAsync(
        string portfolioUuid,
        CancellationToken ct = default);
}
