namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Request parameters for <c>GET /api/v3/brokerage/convert/trade/{trade_id}</c>.
/// </summary>
public sealed record GetConvertTradeRequest
{
    /// <summary>
    /// The ID of the trade to retrieve. Maps to the <c>trade_id</c> path parameter.
    /// </summary>
    public required string TradeId { get; init; }

    /// <summary>
    /// Currency of the account to convert from (e.g. "USD").
    /// </summary>
    public required string FromAccount { get; init; }

    /// <summary>
    /// Currency of the account to convert to (e.g. "USDC").
    /// </summary>
    public required string ToAccount { get; init; }
}
