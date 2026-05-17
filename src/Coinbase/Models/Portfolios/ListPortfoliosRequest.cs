using GrznarAi.Trading.ReadOnly.Coinbase.Models.DataApi;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

/// <summary>
/// Request object for <c>GET /api/v3/brokerage/portfolios</c>.
/// </summary>
public sealed class ListPortfoliosRequest
{
    /// <summary>
    /// Filter by portfolio type.
    /// See <see cref="PortfolioType"/> for known string values
    /// (<c>DEFAULT</c>, <c>CONSUMER</c>, <c>INTX</c>).
    /// </summary>
    public string? PortfolioType { get; init; }
}
