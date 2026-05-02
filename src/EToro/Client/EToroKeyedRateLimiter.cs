using System.Collections.Concurrent;
using GrznarAi.Trading.ReadOnly.Configuration;
using Microsoft.Extensions.Options;

namespace GrznarAi.Trading.ReadOnly.Client;

/// <summary>
/// Per-user-key sliding-window rate limiter for multi-user scenarios.
/// </summary>
/// <remarks>
/// The eToro API enforces rate limits per user key (60 RPM for standard read operations,
/// 20 RPM for write/transactional operations). When a single application instance serves
/// multiple user keys, each key must be throttled independently.
/// <para>
/// This implementation reads the <c>x-user-key</c> header set by <see cref="EToroAuthHandler"/>
/// and maintains a separate sliding window per user key. Windows that have been idle for more
/// than twice the configured window duration are evicted and disposed automatically every
/// <see cref="EvictionCheckInterval"/> calls to prevent unbounded memory growth in multi-tenant
/// or key-rotation scenarios.
/// </para>
/// <para>
/// Rate limit parameters (<see cref="RateLimitOptions.PermitLimit"/>, <see cref="RateLimitOptions.Window"/>)
/// are read once at construction time. Runtime changes via <c>IOptionsMonitor</c> are not
/// reflected; restart the application to apply new rate-limit configuration.
/// </para>
/// <para>
/// Register via <c>AddEToro(..., useKeyedRateLimiter: true)</c>.
/// </para>
/// </remarks>
public sealed class EToroKeyedRateLimiter : IEToroRateLimiter, IDisposable
{
    private readonly ConcurrentDictionary<string, EToroSlidingWindow> _windows = new();
    private readonly RateLimitOptions _options;
    private readonly TimeProvider _timeProvider;
    private int _callCount;

    private const int EvictionCheckInterval = 100;

    public EToroKeyedRateLimiter(IOptions<EToroOptions> options)
        : this(options, TimeProvider.System)
    {
    }

    internal EToroKeyedRateLimiter(IOptions<EToroOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value.RateLimit;
        _timeProvider = timeProvider;
    }

    public async Task WaitAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (!_options.Enabled || request.Method != HttpMethod.Get)
            return;

        var userKey = GetUserKey(request);
        var window = _windows.GetOrAdd(
            userKey,
            _ => new EToroSlidingWindow(_options.PermitLimit, _options.Window, _timeProvider));

        await window.WaitAsync(ct).ConfigureAwait(false);

        if (Interlocked.Increment(ref _callCount) % EvictionCheckInterval == 0)
            EvictIdleWindows();
    }

    private void EvictIdleWindows()
    {
        var threshold = TimeSpan.FromTicks(_options.Window.Ticks * 2);
        var now = _timeProvider.GetUtcNow();
        foreach (var (key, window) in _windows)
        {
            if (window.IsIdle(now, threshold) && _windows.TryRemove(key, out var removed))
                removed.Dispose();
        }
    }

    public void Dispose()
    {
        foreach (var (key, window) in _windows)
        {
            if (_windows.TryRemove(key, out var removed))
                removed.Dispose();
        }
    }

    private static string GetUserKey(HttpRequestMessage request)
    {
        if (request.Headers.TryGetValues("x-user-key", out var values))
            return values.FirstOrDefault() ?? string.Empty;

        return string.Empty;
    }
}
