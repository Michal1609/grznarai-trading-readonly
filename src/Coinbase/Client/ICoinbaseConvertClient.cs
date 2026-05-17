using GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Read-only client for Coinbase Advanced Trade Convert endpoints.
/// </summary>
public interface ICoinbaseConvertClient
{
    /// <summary>
    /// Get a previously created convert trade by its ID.
    /// <para>
    /// Endpoint: <c>GET /api/v3/brokerage/convert/trade/{trade_id}</c>
    /// </para>
    /// </summary>
    /// <param name="tradeId">The unique ID of the convert trade to retrieve.</param>
    /// <param name="fromAccount">Currency of the source account (e.g. "USD").</param>
    /// <param name="toAccount">Currency of the target account (e.g. "USDC").</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetConvertTradeResponse> GetConvertTradeAsync(
        string tradeId,
        string fromAccount,
        string toAccount,
        CancellationToken ct = default);

    /// <summary>
    /// Get a previously created convert trade using a request object.
    /// <para>
    /// Endpoint: <c>GET /api/v3/brokerage/convert/trade/{trade_id}</c>
    /// </para>
    /// </summary>
    Task<GetConvertTradeResponse> GetConvertTradeAsync(
        GetConvertTradeRequest request,
        CancellationToken ct = default);
}
