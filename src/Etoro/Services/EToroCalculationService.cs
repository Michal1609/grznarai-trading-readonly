using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Calculations;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Pnl;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Trades;

namespace GrznarAi.Trading.ReadOnly.Etoro.Services;

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

    public async Task<IReadOnlyList<PortfolioInstrumentAllocation>> GetPortfolioInstrumentAllocationAsync(
        EToroEnvironment environment,
        CancellationToken ct = default)
    {
        var pnl = await client.GetPnlAsync(environment, ct).ConfigureAwait(false);
        var buckets = BuildInstrumentAllocationBuckets(pnl);
        var totalInvested = buckets.Values.Sum(b => b.InvestedAmount);
        if (totalInvested <= 0m)
            return [];

        var metadataByInstrumentId = await ResolveInstrumentMetadataAsync(buckets.Keys, ct).ConfigureAwait(false);

        return buckets.Values
            .Select(bucket =>
            {
                metadataByInstrumentId.TryGetValue(bucket.InstrumentId, out var metadata);

                return new PortfolioInstrumentAllocation(
                    bucket.InstrumentId,
                    metadata?.Symbol ?? $"#{bucket.InstrumentId}",
                    metadata?.AssetClassId,
                    metadata?.AssetClass ?? "Unknown asset class",
                    metadata?.IndustryId,
                    metadata?.Industry ?? "Unknown industry",
                    bucket.InvestedAmount,
                    bucket.InvestedAmount / totalInvested,
                    bucket.ManualAmount,
                    bucket.MirrorAmount,
                    bucket.PositionCount);
            })
            .OrderByDescending(row => row.InvestedAmount)
            .ThenBy(row => row.InstrumentId)
            .ToList();
    }

    public IReadOnlyList<PortfolioGroupAllocation> CalculatePortfolioAssetClassAllocation(
        IEnumerable<PortfolioInstrumentAllocation> instruments)
    {
        ArgumentNullException.ThrowIfNull(instruments);
        return CalculateGroupAllocation(
            instruments,
            instrument => instrument.AssetClassId,
            instrument => instrument.AssetClass);
    }

    public IReadOnlyList<PortfolioGroupAllocation> CalculatePortfolioIndustryAllocation(
        IEnumerable<PortfolioInstrumentAllocation> instruments)
    {
        ArgumentNullException.ThrowIfNull(instruments);
        return CalculateGroupAllocation(
            instruments,
            instrument => instrument.IndustryId,
            instrument => instrument.Industry);
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

    private static Dictionary<int, AllocationBucket> BuildInstrumentAllocationBuckets(PnlResponse pnl)
    {
        ArgumentNullException.ThrowIfNull(pnl);

        var buckets = new Dictionary<int, AllocationBucket>();

        foreach (var position in pnl.Positions)
            AddPosition(buckets, position.InstrumentId, position.Amount, AllocationSource.Manual);

        foreach (var position in pnl.MirrorPortfolios.SelectMany(m => m.Positions))
            AddPosition(buckets, position.InstrumentId, position.Amount, AllocationSource.Mirror);

        return buckets;
    }

    private static void AddPosition(
        IDictionary<int, AllocationBucket> buckets,
        int instrumentId,
        decimal amount,
        AllocationSource source)
    {
        if (instrumentId <= 0 || amount <= 0m)
            return;

        buckets.TryGetValue(instrumentId, out var existing);
        existing ??= new AllocationBucket(instrumentId, 0m, 0m, 0);

        buckets[instrumentId] = source == AllocationSource.Manual
            ? existing with
            {
                ManualAmount = existing.ManualAmount + amount,
                PositionCount = existing.PositionCount + 1
            }
            : existing with
            {
                MirrorAmount = existing.MirrorAmount + amount,
                PositionCount = existing.PositionCount + 1
            };
    }

    private async Task<IReadOnlyDictionary<int, InstrumentAllocationMetadata>> ResolveInstrumentMetadataAsync(
        IEnumerable<int> instrumentIds,
        CancellationToken ct)
    {
        var metadata = new Dictionary<int, InstrumentMetadataDraft>();
        var ids = instrumentIds.Distinct().OrderBy(id => id).ToList();

        foreach (var chunk in ids.Chunk(EToroRequestLimits.MaxCsvIds))
        {
            var response = await client.GetInstrumentMetadataAsync(instrumentIds: chunk, ct: ct).ConfigureAwait(false);
            foreach (var instrument in response.InstrumentDisplayDatas)
            {
                metadata[instrument.InstrumentId] = new InstrumentMetadataDraft(
                    FormatInstrument(instrument.InstrumentId, instrument.SymbolFull, instrument.InstrumentDisplayName),
                    instrument.InstrumentTypeId,
                    instrument.StocksIndustryId);
            }
        }

        var assetClassById = await ResolveAssetClassesAsync(metadata.Values.Select(m => m.AssetClassId), ct).ConfigureAwait(false);
        var industryById = await ResolveIndustriesAsync(metadata.Values.Select(m => m.IndustryId), ct).ConfigureAwait(false);

        return metadata.ToDictionary(
            item => item.Key,
            item => new InstrumentAllocationMetadata(
                item.Value.Symbol,
                item.Value.AssetClassId,
                ResolveGroupName(item.Value.AssetClassId, assetClassById, "Unknown asset class"),
                item.Value.IndustryId,
                ResolveGroupName(item.Value.IndustryId, industryById, "Unknown industry")));
    }

    private async Task<IReadOnlyDictionary<int, string>> ResolveAssetClassesAsync(
        IEnumerable<int?> assetClassIds,
        CancellationToken ct)
    {
        var ids = assetClassIds
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        if (ids.Count == 0)
            return new Dictionary<int, string>();

        var response = await client.GetInstrumentTypesAsync(ids, ct).ConfigureAwait(false);
        return response.InstrumentTypes.ToDictionary(
            item => item.InstrumentTypeId,
            item => string.IsNullOrWhiteSpace(item.InstrumentTypeDescription)
                ? $"Asset class {item.InstrumentTypeId}"
                : item.InstrumentTypeDescription);
    }

    private async Task<IReadOnlyDictionary<int, string>> ResolveIndustriesAsync(
        IEnumerable<int?> industryIds,
        CancellationToken ct)
    {
        var ids = industryIds
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        if (ids.Count == 0)
            return new Dictionary<int, string>();

        var response = await client.GetStocksIndustriesAsync(ids, ct).ConfigureAwait(false);
        return response.StocksIndustries.ToDictionary(
            item => item.IndustryId,
            item => string.IsNullOrWhiteSpace(item.IndustryName)
                ? $"Industry {item.IndustryId}"
                : item.IndustryName);
    }

    private static List<PortfolioGroupAllocation> CalculateGroupAllocation(
        IEnumerable<PortfolioInstrumentAllocation> instruments,
        Func<PortfolioInstrumentAllocation, int?> idSelector,
        Func<PortfolioInstrumentAllocation, string> nameSelector)
    {
        var instrumentList = instruments.ToList();
        var totalInvested = instrumentList.Sum(i => i.InvestedAmount);
        if (totalInvested <= 0m)
            return [];

        return instrumentList
            .GroupBy(
                instrument => new AllocationGroupKey(idSelector(instrument), nameSelector(instrument)),
                AllocationGroupKeyComparer.Instance)
            .Select(group =>
            {
                var investedAmount = group.Sum(i => i.InvestedAmount);
                return new PortfolioGroupAllocation(
                    group.Key.Id,
                    group.Key.Name,
                    investedAmount,
                    investedAmount / totalInvested,
                    group.Count(),
                    group.Sum(i => i.PositionCount));
            })
            .OrderByDescending(group => group.InvestedAmount)
            .ThenBy(group => group.GroupName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string FormatInstrument(
        int instrumentId,
        string? symbol,
        string? displayName)
    {
        if (!string.IsNullOrWhiteSpace(symbol))
            return symbol;

        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName;

        return $"#{instrumentId}";
    }

    private static string ResolveGroupName(
        int? id,
        IReadOnlyDictionary<int, string> names,
        string fallback)
    {
        if (!id.HasValue)
            return fallback;

        return names.TryGetValue(id.Value, out var name) ? name : $"{fallback} #{id.Value}";
    }

    private enum AllocationSource
    {
        Manual,
        Mirror,
    }

    private sealed record AllocationBucket(
        int InstrumentId,
        decimal ManualAmount,
        decimal MirrorAmount,
        int PositionCount)
    {
        public decimal InvestedAmount => ManualAmount + MirrorAmount;
    }

    private sealed record InstrumentMetadataDraft(
        string Symbol,
        int? AssetClassId,
        int? IndustryId);

    private sealed record InstrumentAllocationMetadata(
        string Symbol,
        int? AssetClassId,
        string AssetClass,
        int? IndustryId,
        string Industry);

    private sealed record AllocationGroupKey(int? Id, string Name);

    private sealed class AllocationGroupKeyComparer : IEqualityComparer<AllocationGroupKey>
    {
        public static readonly AllocationGroupKeyComparer Instance = new();

        public bool Equals(AllocationGroupKey? x, AllocationGroupKey? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Id == y.Id && string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(AllocationGroupKey obj)
        {
            return HashCode.Combine(obj.Id, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name));
        }
    }
}
