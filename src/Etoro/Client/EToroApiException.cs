using System.Net;
using GrznarAi.Trading.ReadOnly.Exceptions;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

public sealed class EToroApiException : TradingApiException
{
    public EToroApiException(
        HttpStatusCode statusCode,
        string endpoint,
        string? responseBody,
        string? requestId,
        TimeSpan? retryAfter)
        : base("eToro", statusCode, endpoint, responseBody, requestId, retryAfter)
    {
    }
}
