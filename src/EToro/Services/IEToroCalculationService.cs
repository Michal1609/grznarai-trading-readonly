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

    /// <summary>
    /// Returns invested open-position allocation grouped by instrument.
    /// </summary>
    /// <remarks>
    /// Fetches PnL for <paramref name="environment"/>, includes manual positions and mirror
    /// positions from copied people/portfolios, enriches instruments with market metadata,
    /// and returns rows sorted by invested amount descending. Percentual <c>Share</c> values
    /// are calculated from the total invested amount of returned open instrument positions.
    /// Cash, pending orders, realized PnL, unrealized PnL, fees, spreads, dividends, and
    /// closed positions are not included.
    /// </remarks>
    Task<IReadOnlyList<PortfolioInstrumentAllocation>> GetPortfolioInstrumentAllocationAsync(
        EToroEnvironment environment,
        CancellationToken ct = default);

    /// <summary>
    /// Aggregates instrument allocation rows by asset class.
    /// </summary>
    /// <remarks>
    /// The returned rows are sorted by invested amount descending. Percentual <c>Share</c>
    /// values are calculated from the total invested amount in <paramref name="instruments"/>.
    /// </remarks>
    IReadOnlyList<PortfolioGroupAllocation> CalculatePortfolioAssetClassAllocation(
        IEnumerable<PortfolioInstrumentAllocation> instruments);

    /// <summary>
    /// Aggregates instrument allocation rows by stock industry.
    /// </summary>
    /// <remarks>
    /// The returned rows are sorted by invested amount descending. Percentual <c>Share</c>
    /// values are calculated from the total invested amount in <paramref name="instruments"/>.
    /// Instruments without industry metadata are grouped under their Unknown fallback.
    /// </remarks>
    IReadOnlyList<PortfolioGroupAllocation> CalculatePortfolioIndustryAllocation(
        IEnumerable<PortfolioInstrumentAllocation> instruments);

    Task<AccountMetrics> GetAccountMetricsAsync(EToroEnvironment environment, CancellationToken ct);
    Task<AccountMetrics> GetAccountMetricsAsync(
        EToroEnvironment environment,
        DateOnly? fromDate = null,
        int pageSize = EToroCalculationService.DefaultTradeHistoryPageSize,
        int maxPages = EToroCalculationService.DefaultTradeHistoryMaxPages,
        CancellationToken ct = default);
}
