using GrznarAi.Trading.ReadOnly.Etoro.Models.Agent;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Pnl;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Portfolio;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Trades;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

public interface IEToroTradingClient
{
    /// <summary>
    /// Retrieves all agent portfolios owned by the authenticated user, including associated user tokens and mirror IDs.
    /// <br/>CZ: VrĂˇtĂ­ vĹˇechna agentnĂ­ portfolia pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele vÄŤetnÄ› pĹ™iĹ™azenĂ˝ch tokenĹŻ a mirror ID.
    /// </summary>
    Task<AgentPortfolioResponse> GetAgentPortfoliosAsync(CancellationToken ct = default);

    /// <summary>
    /// Get demo or real account PnL and portfolio details.
    /// <br/>CZ: VrĂˇtĂ­ PnL a detaily portfolia demo nebo reĂˇlnĂ©ho ĂşÄŤtu.
    /// </summary>
    Task<PnlResponse> GetPnlAsync(
        EToroEnvironment environment,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieve comprehensive portfolio information including positions, orders and account status.
    /// <br/>CZ: VrĂˇtĂ­ kompletnĂ­ informace o portfoliu vÄŤetnÄ› pozic, pĹ™Ă­kazĹŻ a stavu ĂşÄŤtu.
    /// </summary>
    Task<PortfolioResponse> GetPortfolioAsync(
        EToroEnvironment environment,
        CancellationToken ct = default);

    /// <summary>
    /// List trading history of closed positions.
    /// <br/>CZ: VrĂˇtĂ­ historii obchodĹŻ uzavĹ™enĂ˝ch pozic.
    /// </summary>
    Task<TradeHistoryResponse> GetTradeHistoryAsync(
        DateOnly minDate,
        int page = 0,
        int? pageSize = null,
        CancellationToken ct = default);

    /// <summary>
    /// List trading history of closed positions.
    /// <br/>CZ: VrĂˇtĂ­ historii obchodĹŻ uzavĹ™enĂ˝ch pozic.
    /// </summary>
    Task<TradeHistoryResponse> GetTradeHistoryAsync(
        DateTimeOffset minDate,
        int page = 0,
        int? pageSize = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get order information and position details for a demo or real account order.
    /// Returns the order status, execution details, and all positions opened from the specified order.
    /// <br/>CZ: VrĂˇtĂ­ informace o pĹ™Ă­kazu a detaily pozic pro demo nebo reĂˇlnĂ˝ ĂşÄŤet.
    /// VracĂ­ stav pĹ™Ă­kazu, detaily provedenĂ­ a vĹˇechny pozice otevĹ™enĂ© z danĂ©ho pĹ™Ă­kazu.
    /// </summary>
    Task<OrderForOpenInfoResponse> GetOrderAsync(
        EToroEnvironment environment,
        long orderId,
        CancellationToken ct = default);
}
