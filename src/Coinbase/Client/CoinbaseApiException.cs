using System.Net;
using GrznarAi.Trading.ReadOnly.Exceptions;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed class CoinbaseApiException : TradingApiException
{
    public CoinbaseApiException(
        HttpStatusCode statusCode,
        string endpoint,
        string? responseBody,
        string? requestId,
        TimeSpan? retryAfter)
        : base("Coinbase", statusCode, endpoint, responseBody, requestId, retryAfter)
    {
    }
}
