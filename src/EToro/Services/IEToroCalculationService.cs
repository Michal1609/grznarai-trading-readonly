using GrznarAi.Trading.ReadOnly.Models.Calculations;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Models.Pnl;
using GrznarAi.Trading.ReadOnly.Models.Trades;

namespace GrznarAi.Trading.ReadOnly.Services;

public interface IEToroCalculationService
{
    decimal CalculateAvailableCash(PnlResponse pnl);
    decimal CalculateTotalInvested(PnlResponse pnl);
    decimal CalculateProfitLoss(PnlResponse pnl);
    decimal CalculateInvestedPrincipal(PnlResponse pnl);
    decimal CalculateUnrealizedPnL(PnlResponse pnl);
    decimal CalculateEquity(PnlResponse pnl);
    decimal CalculateRealizedProfit(IEnumerable<ClosedTrade> trades);
    Task<AccountMetrics> GetAccountMetricsAsync(EToroEnvironment environment, CancellationToken ct);
    Task<AccountMetrics> GetAccountMetricsAsync(
        EToroEnvironment environment,
        DateOnly? fromDate = null,
        int pageSize = EToroCalculationService.DefaultTradeHistoryPageSize,
        int maxPages = EToroCalculationService.DefaultTradeHistoryMaxPages,
        CancellationToken ct = default);
}
