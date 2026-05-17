using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    /// <inheritdoc cref="ICoinbasePortfoliosClient.ListPortfoliosAsync(string?,CancellationToken)"/>
    public async Task<ListPortfoliosResponse> ListPortfoliosAsync(
        string? portfolioType = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder().Add("portfolio_type", portfolioType);

        return await GetFromJsonAsync<ListPortfoliosResponse>(
            $"/api/v3/brokerage/portfolios{qs}",
            "Empty list-portfolios response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbasePortfoliosClient.ListPortfoliosAsync(ListPortfoliosRequest,CancellationToken)"/>
    public Task<ListPortfoliosResponse> ListPortfoliosAsync(
        ListPortfoliosRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ListPortfoliosAsync(request.PortfolioType, ct);
    }

    /// <inheritdoc cref="ICoinbasePortfoliosClient.GetPortfolioBreakdownAsync(string,string?,CancellationToken)"/>
    public async Task<GetPortfolioBreakdownResponse> GetPortfolioBreakdownAsync(
        string portfolioUuid, string? currency = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(portfolioUuid);

        var qs = new QueryStringBuilder().Add("currency", currency);
        var path = $"/api/v3/brokerage/portfolios/{QueryStringBuilder.EscapePathSegment(portfolioUuid)}{qs}";

        return await GetFromJsonAsync<GetPortfolioBreakdownResponse>(
            path,
            "Empty portfolio-breakdown response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbasePortfoliosClient.GetPortfolioBreakdownAsync(GetPortfolioBreakdownRequest,CancellationToken)"/>
    public Task<GetPortfolioBreakdownResponse> GetPortfolioBreakdownAsync(
        GetPortfolioBreakdownRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetPortfolioBreakdownAsync(request.PortfolioUuid, request.Currency, ct);
    }
}
