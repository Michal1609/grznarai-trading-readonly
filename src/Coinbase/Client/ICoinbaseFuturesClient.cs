using GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Coinbase Advanced Trade API client for US Derivatives (CFM Futures) endpoints.
/// All endpoints require credentials and a CFM futures-enabled account.
/// </summary>
public interface ICoinbaseFuturesClient
{
    /// <summary>
    /// Returns the current margin window and killswitch flags for CFM futures trading.
    /// </summary>
    /// <param name="marginProfileType">
    /// Optional filter. Use constants from <see cref="MarginProfileType"/>.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/cfm/intraday/current_margin_window</remarks>
    Task<GetCurrentMarginWindowResponse> GetCurrentMarginWindowAsync(
        string? marginProfileType = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the current margin window and killswitch flags for CFM futures trading.
    /// </summary>
    /// <param name="request">Request parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/cfm/intraday/current_margin_window</remarks>
    Task<GetCurrentMarginWindowResponse> GetCurrentMarginWindowAsync(
        GetCurrentMarginWindowRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a summary of balances, margins, and P&amp;L for the authenticated user's
    /// CFM futures account.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/cfm/balance_summary</remarks>
    Task<GetFuturesBalanceSummaryResponse> GetFuturesBalanceSummaryAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Returns a single open CFM futures position by product ID.
    /// </summary>
    /// <param name="productId">Ticker symbol, e.g. <c>BIT-28JUL23-CDE</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/cfm/positions/{product_id}</remarks>
    Task<GetFuturesPositionResponse> GetFuturesPositionAsync(
        string productId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the intraday margin enrollment setting for the authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/cfm/intraday/margin_setting</remarks>
    Task<GetIntradayMarginSettingResponse> GetIntradayMarginSettingAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Returns all open CFM futures positions for the authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/cfm/positions</remarks>
    Task<ListFuturesPositionsResponse> ListFuturesPositionsAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Returns pending and processing fund sweeps between the user's Coinbase spot
    /// account and their CFM futures account.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>GET /api/v3/brokerage/cfm/sweeps</remarks>
    Task<ListFuturesSweepsResponse> ListFuturesSweepsAsync(
        CancellationToken ct = default);
}
