using GrznarAi.Trading.ReadOnly.Models.Agent;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Models.Pnl;
using GrznarAi.Trading.ReadOnly.Models.Portfolio;
using GrznarAi.Trading.ReadOnly.Models.Trades;

namespace GrznarAi.Trading.ReadOnly.Client;

public interface IEToroTradingClient
{
    /// <summary>
    /// Retrieves all agent portfolios owned by the authenticated user, including associated user tokens and mirror IDs.
    /// <br/>CZ: Vrátí všechna agentní portfolia přihlášeného uživatele včetně přiřazených tokenů a mirror ID.
    /// </summary>
    Task<AgentPortfolioResponse> GetAgentPortfoliosAsync(CancellationToken ct = default);

    /// <summary>
    /// Get demo or real account PnL and portfolio details.
    /// <br/>CZ: Vrátí PnL a detaily portfolia demo nebo reálného účtu.
    /// </summary>
    Task<PnlResponse> GetPnlAsync(
        EToroEnvironment environment,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieve comprehensive portfolio information including positions, orders and account status.
    /// <br/>CZ: Vrátí kompletní informace o portfoliu včetně pozic, příkazů a stavu účtu.
    /// </summary>
    Task<PortfolioResponse> GetPortfolioAsync(
        EToroEnvironment environment,
        CancellationToken ct = default);

    /// <summary>
    /// List trading history of closed positions.
    /// <br/>CZ: Vrátí historii obchodů uzavřených pozic.
    /// </summary>
    Task<TradeHistoryResponse> GetTradeHistoryAsync(
        DateOnly minDate,
        int page = 0,
        int? pageSize = null,
        CancellationToken ct = default);

    /// <summary>
    /// List trading history of closed positions.
    /// <br/>CZ: Vrátí historii obchodů uzavřených pozic.
    /// </summary>
    Task<TradeHistoryResponse> GetTradeHistoryAsync(
        DateTimeOffset minDate,
        int page = 0,
        int? pageSize = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get order information and position details for a demo or real account order.
    /// Returns the order status, execution details, and all positions opened from the specified order.
    /// <br/>CZ: Vrátí informace o příkazu a detaily pozic pro demo nebo reálný účet.
    /// Vrací stav příkazu, detaily provedení a všechny pozice otevřené z daného příkazu.
    /// </summary>
    Task<OrderForOpenInfoResponse> GetOrderAsync(
        EToroEnvironment environment,
        long orderId,
        CancellationToken ct = default);
}
