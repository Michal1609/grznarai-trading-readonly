using System.Net;
using GrznarAi.Trading.ReadOnly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GrznarAi.Trading.ReadOnly.Http;

/// <summary>
/// Delegating handler that retries requests on transient HTTP failures (5xx status codes and network errors).
/// HTTP 429 is handled by <see cref="RateLimitHandler"/>, not here.
/// </summary>
public sealed partial class TransientHttpErrorHandler : DelegatingHandler
{
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private readonly ILogger<TransientHttpErrorHandler> _logger;
    private readonly ResilienceOptions _options;

    public TransientHttpErrorHandler(
        ResilienceOptions options,
        ILogger<TransientHttpErrorHandler>? logger = null)
        : this(options, DelayAsync, logger)
    {
    }

    internal TransientHttpErrorHandler(
        ResilienceOptions options,
        Func<TimeSpan, CancellationToken, Task>? delay = null,
        ILogger<TransientHttpErrorHandler>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        _delay = delay ?? DelayAsync;
        _logger = logger ?? NullLogger<TransientHttpErrorHandler>.Instance;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled || _options.MaxRetries == 0 || !CanRetry(request))
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var attempt = 0;
        HttpRequestMessage? ownedClone = null;

        while (true)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (!IsTransientStatusCode(response.StatusCode) || attempt >= _options.MaxRetries)
                    return response;

                var delay = GetRetryDelay(attempt);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    var endpoint = GetEndpoint(request);
                    LogTransientStatusRetry(
                        _logger,
                        (int)response.StatusCode,
                        request.Method.Method,
                        endpoint,
                        attempt + 1,
                        delay);
                }

                response.Dispose();
                await _delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException exception) when (attempt < _options.MaxRetries)
            {
                var delay = GetRetryDelay(attempt);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    var endpoint = GetEndpoint(request);
                    LogTransientNetworkRetry(
                        _logger,
                        exception,
                        request.Method.Method,
                        endpoint,
                        attempt + 1,
                        delay);
                }

                await _delay(delay, cancellationToken).ConfigureAwait(false);
            }

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

    private static bool IsTransientStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private TimeSpan GetRetryDelay(int attempt)
    {
        var multiplier = Math.Pow(2, attempt);
        var delay = TimeSpan.FromMilliseconds(_options.DefaultRetryDelay.TotalMilliseconds * multiplier);

        if (_options.RetryJitterRatio > 0)
        {
            var jitter = delay.TotalMilliseconds * _options.RetryJitterRatio * Random.Shared.NextDouble();
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds + jitter);
        }

        if (delay < TimeSpan.Zero)
            return TimeSpan.Zero;

        return delay <= _options.MaxRetryDelay ? delay : _options.MaxRetryDelay;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage original,
        CancellationToken ct)
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
            var bytes = await original.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
            clone.Content = new ByteArrayContent(bytes);

            foreach (var header in original.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    private static string GetEndpoint(HttpRequestMessage request) =>
        request.RequestUri?.IsAbsoluteUri == true
            ? request.RequestUri.AbsolutePath
            : request.RequestUri?.OriginalString ?? "<unknown>";

    private static Task DelayAsync(TimeSpan delay, CancellationToken ct) => Task.Delay(delay, ct);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Retrying API request after transient HTTP {StatusCode}. Method: {Method}; Endpoint: {Endpoint}; Attempt: {Attempt}; Delay: {Delay}.")]
    private static partial void LogTransientStatusRetry(
        ILogger logger,
        int statusCode,
        string method,
        string endpoint,
        int attempt,
        TimeSpan delay);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Retrying API request after transient network failure. Method: {Method}; Endpoint: {Endpoint}; Attempt: {Attempt}; Delay: {Delay}.")]
    private static partial void LogTransientNetworkRetry(
        ILogger logger,
        Exception exception,
        string method,
        string endpoint,
        int attempt,
        TimeSpan delay);
}
