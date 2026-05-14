using GrznarAi.Trading.ReadOnly.Configuration;

namespace GrznarAi.Trading.ReadOnly.RateLimiting;

/// <summary>
/// Sliding-window rate limiter for a single credential set.
/// </summary>
/// <remarks>
/// Registered as a singleton — shared across all requests for one API key.
/// Multi-credential scenarios require <see cref="KeyedRateLimiter"/> instead.
/// Rate-limit parameters are snapshot at construction time; restart the application to apply new configuration.
/// </remarks>
public sealed class ApiRateLimiter : IApiRateLimiter, IDisposable
{
    private readonly SlidingWindow _window;
    private readonly RateLimitOptions _options;

    public ApiRateLimiter(RateLimitOptions options)
        : this(options, TimeProvider.System)
    {
    }

    internal ApiRateLimiter(RateLimitOptions options, TimeProvider timeProvider)
    {
        _options = options;
        _window = new SlidingWindow(options.PermitLimit, options.Window, timeProvider);
    }

    public async Task WaitAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (!_options.Enabled || request.Method != HttpMethod.Get)
            return;

        await _window.WaitAsync(ct).ConfigureAwait(false);
    }

    public void Dispose() => _window.Dispose();
}
