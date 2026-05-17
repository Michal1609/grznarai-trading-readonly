using GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    /// <inheritdoc cref="ICoinbaseConvertClient.GetConvertTradeAsync(string,string,string,CancellationToken)"/>
    public async Task<GetConvertTradeResponse> GetConvertTradeAsync(
        string tradeId,
        string fromAccount,
        string toAccount,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tradeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fromAccount);
        ArgumentException.ThrowIfNullOrWhiteSpace(toAccount);

        var qs = new QueryStringBuilder()
            .Add("from_account", fromAccount)
            .Add("to_account", toAccount);

        return await GetFromJsonAsync<GetConvertTradeResponse>(
            $"/api/v3/brokerage/convert/trade/{QueryStringBuilder.EscapePathSegment(tradeId)}{qs}",
            "Empty get-convert-trade response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseConvertClient.GetConvertTradeAsync(GetConvertTradeRequest,CancellationToken)"/>
    public Task<GetConvertTradeResponse> GetConvertTradeAsync(
        GetConvertTradeRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetConvertTradeAsync(request.TradeId, request.FromAccount, request.ToAccount, ct);
    }
}
