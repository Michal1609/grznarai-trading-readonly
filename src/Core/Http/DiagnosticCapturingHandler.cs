using System.Net.Http.Headers;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GrznarAi.Trading.ReadOnly.Http;

/// <summary>
/// Opt-in delegating handler that captures request/response snapshots into <see cref="ApiDiagnostics"/>.
/// Add to the HTTP pipeline only when <see cref="DiagnosticOptions.Enabled"/> is <see langword="true"/>.
/// Position: outermost handler — captures the final response after all retries have completed.
/// </summary>
/// <remarks>
/// Body buffering: after reading the response body the handler replaces
/// <c>response.Content</c> with a <see cref="ByteArrayContent"/> holding the same bytes,
/// so downstream deserializers can still read the body normally.
/// The lock inside <see cref="ApiDiagnostics.Add"/> is held only for a short critical section;
/// the expensive body read is performed before acquiring the lock.
/// </remarks>
public sealed partial class DiagnosticCapturingHandler : DelegatingHandler
{
    private readonly ApiDiagnostics _diagnostics;
    private readonly DiagnosticOptions _options;
    private readonly ILogger<DiagnosticCapturingHandler> _logger;

    public DiagnosticCapturingHandler(
        ApiDiagnostics diagnostics,
        DiagnosticOptions options,
        ILogger<DiagnosticCapturingHandler>? logger = null)
    {
        _diagnostics = diagnostics;
        _options = options;
        _logger = logger ?? NullLogger<DiagnosticCapturingHandler>.Instance;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var timestamp = DateTimeOffset.UtcNow;
        var startTicks = System.Diagnostics.Stopwatch.GetTimestamp();

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(startTicks);

        try
        {
            var snapshot = await BuildSnapshotAsync(request, response, timestamp, elapsed, cancellationToken)
                .ConfigureAwait(false);

            _diagnostics.Add(snapshot);

            if (_options.LogToILogger && _logger.IsEnabled(LogLevel.Debug))
                LogSnapshot(_logger, snapshot.Method, snapshot.RequestUri?.AbsolutePath ?? "", snapshot.StatusCode, elapsed);
        }
        catch
        {
            // Diagnostic capture must never break the caller's request.
        }

        return response;
    }

    private async Task<ApiResponseSnapshot> BuildSnapshotAsync(
        HttpRequestMessage request,
        HttpResponseMessage response,
        DateTimeOffset timestamp,
        TimeSpan elapsed,
        CancellationToken ct)
    {
        IReadOnlyDictionary<string, string[]>? requestHeaders = null;
        if (_options.CaptureRequestHeaders)
            requestHeaders = CaptureHeaders(request.Headers, _options.RedactedHeaders);

        IReadOnlyDictionary<string, string[]>? responseHeaders = null;
        if (_options.CaptureResponseHeaders && response.Headers is not null)
            responseHeaders = CaptureHeaders(response.Headers, _options.RedactedHeaders);

        string? body = null;
        bool bodyTruncated = false;

        if (_options.CaptureResponseBody && response.Content is not null)
        {
            var originalContent = response.Content;

            // Read body bytes and replace content so downstream deserializers can read normally.
            var bytes = await originalContent.ReadAsByteArrayAsync(ct).ConfigureAwait(false);

            var newContent = new ByteArrayContent(bytes);
            foreach (var header in originalContent.Headers)
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            response.Content = newContent;

            int captureLength = bytes.Length;
            if (bytes.Length > _options.MaxBodyBytes)
            {
                captureLength = _options.MaxBodyBytes;
                bodyTruncated = true;
            }

            body = System.Text.Encoding.UTF8.GetString(bytes, 0, captureLength);
        }

        return new ApiResponseSnapshot
        {
            Timestamp = timestamp,
            Elapsed = elapsed,
            Method = _options.CaptureRequest ? request.Method.Method : "",
            RequestUri = _options.CaptureRequest ? request.RequestUri : null,
            RequestHeaders = requestHeaders,
            StatusCode = _options.CaptureResponseStatus ? (int)response.StatusCode : 0,
            ReasonPhrase = _options.CaptureResponseStatus ? response.ReasonPhrase : null,
            ResponseHeaders = responseHeaders,
            ContentType = response.Content?.Headers.ContentType?.ToString(),
            ResponseBody = body,
            ResponseBodyTruncated = bodyTruncated
        };
    }

    private static Dictionary<string, string[]> CaptureHeaders(
        HttpHeaders headers,
        ISet<string> redactedNames)
    {
        var dict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in headers)
        {
            dict[header.Key] = redactedNames.Contains(header.Key)
                ? ["[REDACTED]"]
                : header.Value.ToArray();
        }

        return dict;
    }

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "Diagnostic snapshot captured. Method: {Method}; Endpoint: {Endpoint}; StatusCode: {StatusCode}; Elapsed: {Elapsed}.")]
    private static partial void LogSnapshot(
        ILogger logger,
        string method,
        string endpoint,
        int statusCode,
        TimeSpan elapsed);
}
