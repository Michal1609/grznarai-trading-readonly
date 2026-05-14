using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Etoro.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

/// <summary>
/// Typed read-oriented client for the eToro public API.
/// </summary>
/// <remarks>
/// This project is not affiliated with, endorsed by, or connected to eToro in any way.
/// It does not provide financial advice; see the README disclaimer before using the client.
/// </remarks>
public sealed partial class EToroClient : IEToroClient
{
    private static readonly EToroJsonContext JsonContext = EToroJsonContext.Default;

    // Not disposed here: lifetime is managed by IHttpClientFactory / the DI container.
    private readonly HttpClient _httpClient;
    private readonly ErrorHandlingOptions _errorHandlingOptions;
    private readonly ILogger<EToroClient> _logger;

    internal EToroClient(HttpClient httpClient)
        : this(httpClient, Options.Create(new EToroOptions()))
    {
    }

    public EToroClient(HttpClient httpClient, IOptions<EToroOptions> options)
        : this(httpClient, options, NullLogger<EToroClient>.Instance)
    {
    }

    [ActivatorUtilitiesConstructor]
    public EToroClient(
        HttpClient httpClient,
        IOptions<EToroOptions> options,
        ILogger<EToroClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _errorHandlingOptions = options.Value.ErrorHandling ?? new ErrorHandlingOptions();
        _logger = logger;
    }

    private static string ToEnvSegment(Models.Common.EToroEnvironment env) =>
        env == Models.Common.EToroEnvironment.Demo ? "demo" : "real";

    private async Task<T> GetFromJsonAsync<T>(
        string url,
        string emptyResponseMessage,
        CancellationToken ct)
    {
        LogSendingRequest(_logger, "GET", url);
        using var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
        await EnsureSuccessAsync(response, ct).ConfigureAwait(false);

        using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync(stream, GetJsonTypeInfo<T>(), ct).ConfigureAwait(false)
               ?? throw new InvalidOperationException(emptyResponseMessage);
    }

    private async Task<T?> GetFromJsonOrNoContentAsync<T>(
        string url,
        string emptyResponseMessage,
        CancellationToken ct)
    {
        LogSendingRequest(_logger, "GET", url);
        using var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
        if ((int)response.StatusCode == 204)
            return default;

        await EnsureSuccessAsync(response, ct).ConfigureAwait(false);

        using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync(stream, GetJsonTypeInfo<T>(), ct).ConfigureAwait(false)
               ?? throw new InvalidOperationException(emptyResponseMessage);
    }

    private static JsonTypeInfo<T> GetJsonTypeInfo<T>() =>
        JsonContext.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
        ?? throw new InvalidOperationException($"JSON type metadata for {typeof(T).FullName} is not registered.");

    /// <summary>
    /// Throws <see cref="EToroApiException"/> for non-success responses.
    /// </summary>
    /// <remarks>
    /// Callers must own and dispose <paramref name="response"/> â€” this method does not dispose it.
    /// All callers in this class use <c>using var response = â€¦</c>, ensuring disposal even when
    /// this method throws.
    /// </remarks>
    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        // When an explicit limit is configured, stream-read only up to that bound to prevent OOM
        // on oversized/hostile bodies. When null (no explicit limit), fall back to ReadAsStringAsync
        // which is still bounded by HttpClient.MaxResponseContentBufferSize (2 GB by default).
        string? rawBody = null;
        if (_errorHandlingOptions.IncludeResponseBody && response.Content is not null)
        {
            rawBody = _errorHandlingOptions.MaxResponseBodyLength is { } maxBytes
                ? await ReadBoundedBodyAsync(response.Content, maxBytes, ct).ConfigureAwait(false)
                : await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }

        var body = rawBody is null ? null : FormatResponseBody(rawBody, _errorHandlingOptions);
        var endpoint = GetSanitizedEndpoint(response);
        var requestId = GetRequestId(response);
        var retryAfter = GetRetryAfter(response);
        LogRequestFailed(_logger, (int)response.StatusCode, endpoint, requestId, retryAfter);

        throw new EToroApiException(
            response.StatusCode,
            endpoint,
            body,
            requestId,
            retryAfter);
    }

    // Reads at most maxBytes from the response stream to prevent OOM on oversized bodies.
    private static async Task<string> ReadBoundedBodyAsync(HttpContent content, int maxBytes, CancellationToken ct)
    {
        using var stream = await content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var buffer = new byte[maxBytes];
        var total = 0;
        int read;
        while (total < maxBytes
               && (read = await stream.ReadAsync(buffer.AsMemory(total, maxBytes - total), ct).ConfigureAwait(false)) > 0)
            total += read;
        return System.Text.Encoding.UTF8.GetString(buffer, 0, total);
    }

    private static string GetSanitizedEndpoint(HttpResponseMessage response)
    {
        var requestUri = response.RequestMessage?.RequestUri;
        if (requestUri is null)
            return "<unknown>";

        // Use AbsolutePath (no query string) to avoid leaking PII in query params such as ?usernames=...
        var path = requestUri.IsAbsoluteUri ? requestUri.AbsolutePath : requestUri.OriginalString;
        // Replace /people/{username} path segments with a placeholder
        return PeoplePathRegex().Replace(path, "/people/{username}");
    }

    private static string? GetRequestId(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-request-id", out var requestIds))
            return requestIds.FirstOrDefault();

        if (response.Headers.TryGetValues("request-id", out requestIds))
            return requestIds.FirstOrDefault();

        if (response.RequestMessage?.Headers.TryGetValues("x-request-id", out requestIds) == true)
            return requestIds.FirstOrDefault();

        return null;
    }

    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;

        if (retryAfter?.Delta is { } delta)
            return delta;

        if (retryAfter?.Date is { } date)
        {
            var wait = date - DateTimeOffset.UtcNow;
            return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
        }

        return null;
    }

    private static string FormatResponseBody(string body, ErrorHandlingOptions options)
    {
        var formatted = body.Trim();

        if (options.RedactResponseBody)
        {
            formatted = JsonSecretRegex().Replace(formatted, "${prefix}\"[REDACTED]\"");
            formatted = TextSecretRegex().Replace(formatted, "${prefix}[REDACTED]");
            formatted = BearerTokenRegex().Replace(formatted, "Bearer [REDACTED]");
            formatted = JwtTokenRegex().Replace(formatted, "[REDACTED_JWT]");
        }

        return options.MaxResponseBodyLength is { } maxLength && formatted.Length > maxLength
            ? formatted[..maxLength]
            : formatted;
    }

    [GeneratedRegex(
        "(?<prefix>[\"']?(?:apiKey|userKey|token|accessToken|refreshToken|access_token|refresh_token|authorization|secret|password|x-api-key|x-user-key)[\"']?\\s*:\\s*)[\"'](?:\\\\.|[^\"'\\\\])*[\"']",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex JsonSecretRegex();

    [GeneratedRegex(
        "(?<prefix>\\b(?:apiKey|userKey|token|accessToken|refreshToken|access_token|refresh_token|authorization|secret|password|x-api-key|x-user-key)\\b\\s*[=:]\\s*)[^\\s,;}&\\]]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TextSecretRegex();

    [GeneratedRegex(
        @"Bearer\s+[A-Za-z0-9._~+/\-]+=*",
        RegexOptions.CultureInvariant)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(
        @"eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex JwtTokenRegex();

    [GeneratedRegex(
        @"/people/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex PeoplePathRegex();

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "Sending eToro request. Method: {Method}; Endpoint: {Endpoint}.")]
    private static partial void LogSendingRequest(
        ILogger logger,
        string method,
        string endpoint);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Warning,
        Message = "eToro request failed. StatusCode: {StatusCode}; Endpoint: {Endpoint}; RequestId: {RequestId}; RetryAfter: {RetryAfter}.")]
    private static partial void LogRequestFailed(
        ILogger logger,
        int statusCode,
        string endpoint,
        string? requestId,
        TimeSpan? retryAfter);
}
