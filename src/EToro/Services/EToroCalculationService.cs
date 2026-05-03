using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Calculations;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Models.Pnl;
using GrznarAi.Trading.ReadOnly.Models.Trades;

namespace GrznarAi.Trading.ReadOnly.Services;

public sealed class EToroCalculationService(IEToroClient client) : IEToroCalculationService
{
    public static readonly DateOnly DefaultTradeHistoryFromDate = new(2007, 1, 1);
    public const int DefaultTradeHistoryPageSize = 100;
    public const int DefaultTradeHistoryMaxPages = 1_000;

    public decimal CalculateAvailableCash(PnlResponse pnl)
    {
        ArgumentNullException.ThrowIfNull(pnl);
        var manualPending = pnl.OrdersForOpen
            .Where(o => o.MirrorId == 0)
            .Sum(o => o.Amount);

        var mitOrders = pnl.Orders.Sum(o => o.Amount);

        return pnl.Credit - manualPending - mitOrders;
    }

    public decimal CalculateTotalInvested(PnlResponse pnl)
    {
        return CalculateInvestedPrincipal(pnl);
    }

    public decimal CalculateInvestedPrincipal(PnlResponse pnl)
    {
        ArgumentNullException.ThrowIfNull(pnl);
        var openPositions = pnl.Positions.Sum(p => p.Amount);

        var mirrorPositions = pnl.MirrorPortfolios
            .SelectMany(m => m.Positions)
            .Sum(p => p.Amount);

        // adjustedAvailability = availableAmount - closedPositionsNetProfit per mirror
        var mirrorAdjusted = pnl.MirrorPortfolios
            .Sum(m => m.AvailableAmount - m.ClosedPositionsNetProfit);

        var manualPendingWithCosts = pnl.OrdersForOpen
            .Where(o => o.MirrorId == 0)
            .Sum(o => o.Amount + o.TotalExternalCosts);

        var mitOrders = pnl.Orders.Sum(o => o.Amount);

        return openPositions + mirrorPositions + mirrorAdjusted + manualPendingWithCosts + mitOrders;
    }

    public decimal CalculateProfitLoss(PnlResponse pnl)
    {
        return CalculateUnrealizedPnL(pnl);
    }

    /// <summary>
    /// Recomputes unrealized PnL by summing <c>PnL</c> across all open manual and copy positions.
    /// </summary>
    /// <remarks>
    /// This value is derived from position-level data and may differ from the API-reported
    /// <see cref="PnlResponse.UnrealizedPnL"/> field due to rounding or different grouping logic
    /// on the eToro server side. To use the API-reported value directly, read
    /// <c>pnl.UnrealizedPnL</c> instead.
    /// </remarks>
    public decimal CalculateUnrealizedPnL(PnlResponse pnl)
    {
        ArgumentNullException.ThrowIfNull(pnl);
        var positionsPnl = pnl.Positions.Sum(p => p.PnL);

        var mirrorPnl = pnl.MirrorPortfolios
            .SelectMany(m => m.Positions)
            .Sum(p => p.PnL);

        return positionsPnl + mirrorPnl;
    }

    public decimal CalculateEquity(PnlResponse pnl)
    {
        return CalculateAvailableCash(pnl) + CalculateInvestedPrincipal(pnl) + CalculateUnrealizedPnL(pnl);
    }

    public decimal CalculateRealizedProfit(IEnumerable<ClosedTrade> trades)
    {
        ArgumentNullException.ThrowIfNull(trades);
        return trades.Sum(t => t.NetProfit);
    }

    public Task<AccountMetrics> GetAccountMetricsAsync(EToroEnvironment environment, CancellationToken ct)
    {
        return GetAccountMetricsAsync(environment, fromDate: null, DefaultTradeHistoryPageSize, DefaultTradeHistoryMaxPages, ct);
    }

    public async Task<AccountMetrics> GetAccountMetricsAsync(
        EToroEnvironment environment,
        DateOnly? fromDate = null,
        int pageSize = DefaultTradeHistoryPageSize,
        int maxPages = DefaultTradeHistoryMaxPages,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxPages);

        var pnl = await client.GetPnlAsync(environment, ct).ConfigureAwait(false);

        var availableCash = CalculateAvailableCash(pnl);
        var investedPrincipal = CalculateInvestedPrincipal(pnl);
        var unrealizedPnl = CalculateUnrealizedPnL(pnl);
        var realizedPnl = await CalculateRealizedPnLAsync(fromDate ?? DefaultTradeHistoryFromDate, pageSize, maxPages, ct).ConfigureAwait(false);
        var equity = CalculateEquity(pnl);
        var totalReturn = realizedPnl + unrealizedPnl;
        var estimatedNetDeposits = equity - totalReturn;
        var totalReturnPct = estimatedNetDeposits == 0m ? 0m : totalReturn / estimatedNetDeposits * 100m;

        return new AccountMetrics(
            availableCash,
            investedPrincipal,
            unrealizedPnl,
            realizedPnl,
            equity,
            estimatedNetDeposits,
            totalReturn,
            totalReturnPct);
    }

    /// <summary>
    /// Paginates closed-trade history, deduplicating by <c>PositionId</c>.
    /// Throws <see cref="InvalidOperationException"/> when <paramref name="maxPages"/> is
    /// exhausted before a terminal page is reached, to prevent silently truncated PnL totals.
    /// </summary>
    private async Task<decimal> CalculateRealizedPnLAsync(
        DateOnly fromDate,
        int pageSize,
        int maxPages,
        CancellationToken ct)
    {
        var realizedPnl = 0m;
        var seenPositionIds = new HashSet<long>();

        for (var page = 0; page < maxPages; page++)
        {
            var response = await client.GetTradeHistoryAsync(fromDate, page, pageSize, ct).ConfigureAwait(false);
            if (response.Trades.Count == 0)
                return realizedPnl;

            var newTrades = false;
            foreach (var trade in response.Trades.Where(t => seenPositionIds.Add(t.PositionId)))
            {
                realizedPnl += trade.NetProfit;
                newTrades = true;
            }

            if (!newTrades || response.Trades.Count < pageSize)
                return realizedPnl;
        }

        throw new InvalidOperationException(
            $"Trade history scan reached the {maxPages}-page cap (pageSize={pageSize}) without " +
            "finding a terminal page. Realized PnL is incomplete. " +
            "Increase maxPages or narrow the fromDate range.");
    }
}
