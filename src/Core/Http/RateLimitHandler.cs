using System.Net;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GrznarAi.Trading.ReadOnly.Http;

/// <summary>
/// Delegating handler that enforces the rate limit before each request and retries on HTTP 429.
/// </summary>
/// <remarks>
/// <para>
/// Intermediate 429 responses are disposed internally before each retry. The final response
/// — whether a success or an exhausted-retry 429 — is returned to the caller, who is
/// responsible for disposing it.
/// </para>
/// </remarks>
public sealed partial class RateLimitHandler : DelegatingHandler
{
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private readonly ILogger<RateLimitHandler> _logger;
    private readonly IApiRateLimiter _rateLimiter;
    private readonly RateLimitOptions _options;

    public RateLimitHandler(
        RateLimitOptions options,
        IApiRateLimiter rateLimiter,
        ILogger<RateLimitHandler>? logger = null)
        : this(options, rateLimiter, DelayAsync, logger)
    {
    }

    internal RateLimitHandler(
        RateLimitOptions options,
        IApiRateLimiter rateLimiter,
        Func<TimeSpan, CancellationToken, Task>? delay,
        ILogger<RateLimitHandler>? logger = null)
    {
        _options = options;
        _rateLimiter = rateLimiter;
        _delay = delay ?? DelayAsync;
        _logger = logger ?? NullLogger<RateLimitHandler>.Instance;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var attempt = 0;
        HttpRequestMessage? ownedClone = null;

        while (true)
        {
            await _rateLimiter.WaitAsync(request, cancellationToken).ConfigureAwait(false);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
                return response;

            if (!CanRetry(request) || attempt >= _options.MaxRetries)
                return response;

            var delay = GetRetryDelay(response, attempt);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var endpoint = GetEndpoint(request);
                LogRateLimitRetry(_logger, request.Method.Method, endpoint, attempt + 1, delay);
            }

            response.Dispose();
            await _delay(delay, cancellationToken).ConfigureAwait(false);

            var previousClone = ownedClone;
            ownedClone = await CloneRequestAsync(request, cancellationToken).ConfigureAwait(false);
            previousClone?.Dispose();
            request = ownedClone;
            attempt++;
        }
    }

    private bool CanRetry(HttpRequestMessage request) =>
        IsIdempotentMethod(request.Method) || _options.RetryNonIdempotentRequests;

    private static bool IsIdempotentMethod(HttpMethod method) =>
        method == HttpMethod.Get || method == HttpMethod.Head;

    private TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        var retryAfter = response.Headers.RetryAfter;
        TimeSpan delay;

        if (retryAfter?.Delta is { } delta)
        {
            delay = delta;
        }
        else if (retryAfter?.Date is { } date)
        {
            var wait = date - DateTimeOffset.UtcNow;
            delay = wait > TimeSpan.Zero ? wait : _options.DefaultRetryDelay;
        }
        else
        {
            delay = GetBackoffDelay(attempt);
        }

        return CapDelay(delay);
    }

    private TimeSpan GetBackoffDelay(int attempt)
    {
        var multiplier = Math.Pow(2, attempt);
        var delay = TimeSpan.FromMilliseconds(_options.DefaultRetryDelay.TotalMilliseconds * multiplier);

        if (_options.RetryJitterRatio <= 0)
            return delay;

        var jitter = delay.TotalMilliseconds * _options.RetryJitterRatio * Random.Shared.NextDouble();
        return TimeSpan.FromMilliseconds(delay.TotalMilliseconds + jitter);
    }

    private TimeSpan CapDelay(TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
            return TimeSpan.Zero;

        return delay <= _options.MaxRetryDelay ? delay : _options.MaxRetryDelay;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage original, CancellationToken ct)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version,
            VersionPolicy = original.VersionPolicy
        };

        foreach (var option in original.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (original.Content is not null)
        {
            // Materialises the full body for retry. Acceptable for the current GET-only surface;
            // revisit with MemoryPool pooling when write endpoints (POST/PUT) are introduced.
            var bytes = await original.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
            clone.Content = new ByteArrayContent(bytes);

            foreach (var header in original.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    private static Task DelayAsync(TimeSpan delay, CancellationToken ct) => Task.Delay(delay, ct);

    private static string GetEndpoint(HttpRequestMessage request) =>
        request.RequestUri?.IsAbsoluteUri == true
            ? request.RequestUri.AbsolutePath
            : request.RequestUri?.OriginalString ?? "<unknown>";

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Retrying API request after HTTP 429. Method: {Method}; Endpoint: {Endpoint}; Attempt: {Attempt}; Delay: {Delay}.")]
    private static partial void LogRateLimitRetry(
        ILogger logger,
        string method,
        string endpoint,
        int attempt,
        TimeSpan delay);
}
