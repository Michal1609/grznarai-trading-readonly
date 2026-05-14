using System.Net;
using GrznarAi.Trading.ReadOnly.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Exceptions;

/// <summary>
/// Base exception for all platform API failures across GrznarAi.Trading.ReadOnly.* clients.
/// Platform-specific exceptions (e.g. <c>EToroApiException</c>, <c>CoinbaseApiException</c>) inherit from this type.
/// </summary>
public class TradingApiException : HttpRequestException
{
    protected TradingApiException(
        string platform,
        HttpStatusCode statusCode,
        string endpoint,
        string? responseBody,
        string? requestId,
        TimeSpan? retryAfter)
        : base(BuildMessage(platform, statusCode, endpoint, requestId, retryAfter), null, statusCode)
    {
        Platform = platform;
        Endpoint = endpoint;
        ResponseBody = responseBody;
        RequestId = requestId;
        RetryAfter = retryAfter;
    }

    /// <summary>Platform identifier (e.g. "eToro", "Coinbase").</summary>
    public string Platform { get; }

    /// <summary>Sanitized API endpoint path that triggered the failure.</summary>
    public string Endpoint { get; }

    /// <summary>Raw response body (may be redacted). <see langword="null"/> when body capture is disabled.</summary>
    public string? ResponseBody { get; }

    /// <summary>Server-issued request / trace ID, if present in response headers.</summary>
    public string? RequestId { get; }

    /// <summary>Retry-After value parsed from response headers, if present.</summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Diagnostic snapshot captured at error time.
    /// Always populated on HTTP errors regardless of <c>DiagnosticOptions.Enabled</c>.
    /// </summary>
    public ApiResponseSnapshot? Snapshot { get; init; }

    private static string BuildMessage(
        string platform,
        HttpStatusCode statusCode,
        string endpoint,
        string? requestId,
        TimeSpan? retryAfter)
    {
        var message = $"{platform} API request failed with status {(int)statusCode} ({statusCode}) for '{endpoint}'.";

        if (!string.IsNullOrWhiteSpace(requestId))
            message += $" RequestId: {requestId}.";

        if (retryAfter.HasValue)
            message += $" RetryAfter: {retryAfter.Value}.";

        return message;
    }
}
