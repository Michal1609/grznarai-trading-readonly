using GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    /// <inheritdoc cref="ICoinbasePerpetualClient.GetPerpetualPortfolioSummaryAsync"/>
    public Task<GetPerpetualPortfolioSummaryResponse> GetPerpetualPortfolioSummaryAsync(
        string portfolioUuid,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(portfolioUuid);
        var id = QueryStringBuilder.EscapePathSegment(portfolioUuid);
        return GetFromJsonAsync<GetPerpetualPortfolioSummaryResponse>(
            $"/api/v3/brokerage/intx/portfolio/{id}",
            "Empty get-perpetual-portfolio-summary response.",
            ct);
    }

    /// <inheritdoc cref="ICoinbasePerpetualClient.GetPerpetualPositionAsync"/>
    public Task<GetPerpetualPositionResponse> GetPerpetualPositionAsync(
        string portfolioUuid,
        string symbol,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(portfolioUuid);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        var id = QueryStringBuilder.EscapePathSegment(portfolioUuid);
        var sym = QueryStringBuilder.EscapePathSegment(symbol);
        return GetFromJsonAsync<GetPerpetualPositionResponse>(
            $"/api/v3/brokerage/intx/positions/{id}/{sym}",
            "Empty get-perpetual-position response.",
            ct);
    }

    /// <inheritdoc cref="ICoinbasePerpetualClient.GetPortfolioBalancesAsync"/>
    public Task<GetPortfolioBalancesResponse> GetPortfolioBalancesAsync(
        string portfolioUuid,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(portfolioUuid);
        var id = QueryStringBuilder.EscapePathSegment(portfolioUuid);
        return GetFromJsonAsync<GetPortfolioBalancesResponse>(
            $"/api/v3/brokerage/intx/balances/{id}",
            "Empty get-portfolio-balances response.",
            ct);
    }

    /// <inheritdoc cref="ICoinbasePerpetualClient.ListPerpetualPositionsAsync"/>
    public Task<ListPerpetualPositionsResponse> ListPerpetualPositionsAsync(
        string portfolioUuid,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(portfolioUuid);
        var id = QueryStringBuilder.EscapePathSegment(portfolioUuid);
        return GetFromJsonAsync<ListPerpetualPositionsResponse>(
            $"/api/v3/brokerage/intx/positions/{id}",
            "Empty list-perpetual-positions response.",
            ct);
    }
}
