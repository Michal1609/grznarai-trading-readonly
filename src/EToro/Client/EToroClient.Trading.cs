using System.Globalization;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Models.Pnl;
using GrznarAi.Trading.ReadOnly.Models.Portfolio;
using GrznarAi.Trading.ReadOnly.Models.Trades;


namespace GrznarAi.Trading.ReadOnly.Client;

public sealed partial class EToroClient
{
    public async Task<PnlResponse> GetPnlAsync(EToroEnvironment environment, CancellationToken ct = default)
    {
        var env = ToEnvSegment(environment);
        var wrapper = await GetFromJsonAsync<PnlApiResponse>(
            $"trading/info/{env}/pnl",
            "Empty PnL response.",
            ct).ConfigureAwait(false);
        return wrapper.ClientPortfolio;
    }

    public async Task<PortfolioResponse> GetPortfolioAsync(EToroEnvironment environment, CancellationToken ct = default)
    {
        var wrapper = await GetFromJsonAsync<PortfolioApiResponse>(
            "trading/info/portfolio",
            "Empty portfolio response.",
            ct).ConfigureAwait(false);
        return wrapper.ClientPortfolio;
    }

    public async Task<TradeHistoryResponse> GetTradeHistoryAsync(
        DateOnly minDate, int page = 0, int? pageSize = null, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        if (pageSize.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize.Value);

        var qs = new QueryStringBuilder()
            .Add("minDate", minDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Add("page", page)
            .AddIfHasValue("pageSize", pageSize);
        var trades = await GetFromJsonAsync<List<ClosedTrade>>(
            $"trading/info/trade/history{qs}",
            "Empty trade history response.",
            ct).ConfigureAwait(false);
        return new TradeHistoryResponse(trades);
    }

    /// <summary>
    /// Convenience overload that converts <paramref name="minDate"/> to a <see cref="DateOnly"/>
    /// by taking <see cref="DateTimeOffset.UtcDateTime"/>. Callers in non-UTC time zones may
    /// receive trades from a different local calendar day than expected; prefer the
    /// <see cref="GetTradeHistoryAsync(DateOnly,int,int?,CancellationToken)"/> overload when
    /// exact date boundaries matter.
    /// </summary>
    public Task<TradeHistoryResponse> GetTradeHistoryAsync(
        DateTimeOffset minDate, int page = 0, int? pageSize = null, CancellationToken ct = default)
    {
        return GetTradeHistoryAsync(DateOnly.FromDateTime(minDate.UtcDateTime), page, pageSize, ct);
    }

    public async Task<OrderForOpenInfoResponse> GetOrderAsync(
        EToroEnvironment environment, long orderId, CancellationToken ct = default)
    {
        var env = ToEnvSegment(environment);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(orderId);

        return await GetFromJsonAsync<OrderForOpenInfoResponse>(
            $"trading/info/{env}/orders/{orderId}",
            "Empty order response.",
            ct).ConfigureAwait(false);
    }
}
