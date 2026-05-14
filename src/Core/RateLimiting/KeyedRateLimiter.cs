using System.Collections.Concurrent;
using GrznarAi.Trading.ReadOnly.Configuration;

namespace GrznarAi.Trading.ReadOnly.RateLimiting;

/// <summary>
/// Per-credential sliding-window rate limiter for multi-credential scenarios.
/// </summary>
/// <remarks>
/// Maintains an independent <see cref="SlidingWindow"/> per credential key.
/// The key is derived from each request via the <c>credentialKeyExtractor</c> delegate —
/// the delegate must return a stable, opaque hash (never plaintext credentials).
/// <para>
/// Windows idle for longer than twice <see cref="RateLimitOptions.Window"/> are evicted and disposed
/// every <see cref="EvictionCheckInterval"/> calls to prevent unbounded memory growth.
/// </para>
/// <para>
/// Uses <c>ConcurrentDictionary&lt;string, Lazy&lt;SlidingWindow&gt;&gt;</c> so the
/// <see cref="SlidingWindow"/> constructor runs exactly once per key even under concurrent race —
/// the <c>Lazy</c> instance that loses the race is discarded without ever materialising its value.
/// </para>
/// </remarks>
public sealed class KeyedRateLimiter : IApiRateLimiter, IDisposable
{
    private readonly ConcurrentDictionary<string, Lazy<SlidingWindow>> _windows = new();
    private readonly RateLimitOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly Func<HttpRequestMessage, string> _credentialKeyExtractor;
    private int _callCount;

    private const int EvictionCheckInterval = 100;

    public KeyedRateLimiter(RateLimitOptions options, Func<HttpRequestMessage, string> credentialKeyExtractor)
        : this(options, credentialKeyExtractor, TimeProvider.System)
    {
    }

    internal KeyedRateLimiter(
        RateLimitOptions options,
        Func<HttpRequestMessage, string> credentialKeyExtractor,
        TimeProvider timeProvider)
    {
        _options = options;
        _credentialKeyExtractor = credentialKeyExtractor;
        _timeProvider = timeProvider;
    }

    public async Task WaitAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (!_options.Enabled || request.Method != HttpMethod.Get)
            return;

        var credentialKey = _credentialKeyExtractor(request);
        var window = _windows.GetOrAdd(
            credentialKey,
            // Lazy ensures the SlidingWindow constructor runs exactly once per key.
            // If two threads race on GetOrAdd, only one Lazy is stored; the other is
            // discarded before its Value is ever accessed — no wasted allocation.
            _ => new Lazy<SlidingWindow>(
                () => new SlidingWindow(_options.PermitLimit, _options.Window, _timeProvider),
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;

        await window.WaitAsync(ct).ConfigureAwait(false);

        if (Interlocked.Increment(ref _callCount) % EvictionCheckInterval == 0)
            EvictIdleWindows();
    }

    private void EvictIdleWindows()
    {
        var threshold = TimeSpan.FromTicks(_options.Window.Ticks * 2);
        var now = _timeProvider.GetUtcNow();
        foreach (var (key, lazyWindow) in _windows)
        {
            if (lazyWindow.IsValueCreated
                && lazyWindow.Value.IsIdle(now, threshold)
                && _windows.TryRemove(key, out var removed)
                && removed.IsValueCreated)
            {
                removed.Value.Dispose();
            }
        }
    }

    public void Dispose()
    {
        foreach (var (key, lazyWindow) in _windows)
        {
            if (_windows.TryRemove(key, out var removed) && removed.IsValueCreated)
                removed.Value.Dispose();
        }
    }
}
