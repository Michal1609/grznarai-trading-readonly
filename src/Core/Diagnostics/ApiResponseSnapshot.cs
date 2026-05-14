namespace GrznarAi.Trading.ReadOnly.Diagnostics;

/// <summary>
/// Immutable point-in-time capture of a single HTTP request/response pair.
/// Safe to share across threads — no mutable state after construction.
/// </summary>
public sealed record ApiResponseSnapshot
{
    public DateTimeOffset Timestamp { get; init; }
    public TimeSpan Elapsed { get; init; }

    public string Method { get; init; } = "";
    public Uri? RequestUri { get; init; }

    /// <summary>Captured request headers. <see langword="null"/> when <c>CaptureRequestHeaders</c> is disabled.</summary>
    public IReadOnlyDictionary<string, string[]>? RequestHeaders { get; init; }

    public int StatusCode { get; init; }
    public string? ReasonPhrase { get; init; }

    /// <summary>Captured response headers. <see langword="null"/> when <c>CaptureResponseHeaders</c> is disabled.</summary>
    public IReadOnlyDictionary<string, string[]>? ResponseHeaders { get; init; }

    /// <summary>Captured response body. <see langword="null"/> when <c>CaptureResponseBody</c> is disabled.</summary>
    public string? ResponseBody { get; init; }

    /// <summary><see langword="true"/> when the body was truncated to <c>MaxBodyBytes</c>.</summary>
    public bool ResponseBodyTruncated { get; init; }

    public string? ContentType { get; init; }
}
