namespace GrznarAi.Trading.ReadOnly.Configuration;

/// <summary>
/// Opt-in diagnostic capture options. When <see cref="Enabled"/> is <see langword="false"/> (default)
/// the <c>DiagnosticCapturingHandler</c> is not added to the HTTP pipeline and there is zero overhead.
/// </summary>
public sealed class DiagnosticOptions
{
    /// <summary>Enables diagnostic capture. Defaults to <see langword="false"/> (zero overhead).</summary>
    public bool Enabled { get; set; }

    /// <summary>Capture request URL and method. Defaults to <see langword="true"/> when enabled.</summary>
    public bool CaptureRequest { get; set; } = true;

    /// <summary>
    /// Capture request headers. Defaults to <see langword="false"/> — headers may contain credentials.
    /// Values listed in <see cref="RedactedHeaders"/> are replaced with <c>[REDACTED]</c> when captured.
    /// </summary>
    public bool CaptureRequestHeaders { get; set; }

    /// <summary>Capture response HTTP status code and reason phrase. Defaults to <see langword="true"/>.</summary>
    public bool CaptureResponseStatus { get; set; } = true;

    /// <summary>Capture response headers. Defaults to <see langword="true"/>.</summary>
    public bool CaptureResponseHeaders { get; set; } = true;

    /// <summary>Capture response body. Defaults to <see langword="true"/>.</summary>
    public bool CaptureResponseBody { get; set; } = true;

    /// <summary>Maximum number of bytes of response body to capture. Defaults to 64 KiB.</summary>
    public int MaxBodyBytes { get; set; } = 64 * 1024;

    /// <summary>Number of most-recent requests kept in <see cref="Diagnostics.IApiDiagnostics.History"/>. Defaults to 8.</summary>
    public int HistorySize { get; set; } = 8;

    /// <summary>
    /// Header names whose values are replaced with <c>[REDACTED]</c> in captured snapshots.
    /// Comparison is case-insensitive. Defaults to Authorization, Cookie, Set-Cookie, X-API-Key.
    /// </summary>
    public ISet<string> RedactedHeaders { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Cookie", "Set-Cookie", "X-API-Key"
    };

    /// <summary>Also emit each captured snapshot via <c>ILogger</c> at Debug level. Defaults to <see langword="false"/>.</summary>
    public bool LogToILogger { get; set; }
}
