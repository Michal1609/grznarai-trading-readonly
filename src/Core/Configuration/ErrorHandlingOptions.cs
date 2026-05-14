namespace GrznarAi.Trading.ReadOnly.Configuration;

/// <summary>
/// Error-handling options that control how non-success HTTP responses are captured and surfaced
/// via platform-specific exceptions derived from <see cref="Exceptions.TradingApiException"/>.
/// </summary>
public sealed class ErrorHandlingOptions
{
    /// <summary>Default maximum number of characters of the response body included in exceptions (4 096).</summary>
    public const int DefaultMaxResponseBodyLength = 4096;

    /// <summary>
    /// When <see langword="true"/>, the response body is attached to the exception for diagnostics.
    /// Defaults to <see langword="true"/>. Set to <see langword="false"/> to suppress body capture entirely.
    /// </summary>
    public bool IncludeResponseBody { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, sensitive fields (API keys, tokens, passwords) in the response body
    /// are replaced with <c>[REDACTED]</c> before the exception is created.
    /// Defaults to <see langword="true"/>. Only effective when <see cref="IncludeResponseBody"/> is also <see langword="true"/>.
    /// </summary>
    public bool RedactResponseBody { get; set; } = true;

    /// <summary>
    /// Maximum number of characters of the response body to capture. Body is truncated beyond this limit.
    /// Defaults to <see cref="DefaultMaxResponseBodyLength"/> (4 096).
    /// Set to <see langword="null"/> for unlimited (not recommended — hostile upstreams can exhaust memory).
    /// </summary>
    public int? MaxResponseBodyLength { get; set; } = DefaultMaxResponseBodyLength;
}
