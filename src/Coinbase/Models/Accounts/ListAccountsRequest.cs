namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;

/// <summary>
/// Request parameters for <c>GET /api/v3/brokerage/accounts</c>.
/// </summary>
public sealed record ListAccountsRequest
{
    /// <summary>
    /// Maximum number of accounts per page (1–250). Defaults to 49 when omitted.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Pagination cursor returned in the previous response.
    /// </summary>
    public string? Cursor { get; init; }

    /// <summary>
    /// Filter accounts by retail portfolio ID. Deprecated by Coinbase.
    /// </summary>
    public string? RetailPortfolioId { get; init; }
}
