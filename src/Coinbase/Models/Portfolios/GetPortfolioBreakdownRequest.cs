namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

/// <summary>
/// Request object for <c>GET /api/v3/brokerage/portfolios/{portfolio_uuid}</c>.
/// </summary>
public sealed class GetPortfolioBreakdownRequest
{
    /// <summary>Portfolio UUID (path).</summary>
    public required string PortfolioUuid { get; init; }

    /// <summary>Optional ISO 4217 currency code (query, e.g. <c>USD</c>).</summary>
    public string? Currency { get; init; }
}
