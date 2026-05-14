using System.Threading.RateLimiting;

namespace GrznarAi.Trading.ReadOnly.RateLimiting;

/// <remarks>
/// Wraps <see cref="SlidingWindowRateLimiter"/> (built-in, FIFO-fair, cancellation-aware).
/// <see cref="QueueProcessingOrder.OldestFirst"/> ensures callers that arrived first are
/// served first, avoiding starvation under sustained burst.
/// When used via <see cref="KeyedRateLimiter"/>, one window is created per credential key;
/// idle windows are evicted and disposed automatically so total memory is bounded.
/// </remarks>
internal sealed class SlidingWindow : IDisposable
{
    private const int QueueFactor = 8;

    private readonly SlidingWindowRateLimiter _limiter;
    private readonly TimeProvider _timeProvider;
    private DateTimeOffset _lastUsed;
    private readonly object _lastUsedLock = new();

    public SlidingWindow(int permitLimit, TimeSpan window, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _lastUsed = timeProvider.GetUtcNow();
        _limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            // SegmentsPerWindow = 1 keeps the same timing semantics as the original:
            // a permit used at time T becomes available again at T + Window.
            SegmentsPerWindow = 1,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = permitLimit * QueueFactor,
            AutoReplenishment = true
        });
    }

    public bool IsIdle(DateTimeOffset now, TimeSpan threshold)
    {
        lock (_lastUsedLock)
        {
            return now - _lastUsed > threshold;
        }
    }

    public async Task WaitAsync(CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        lock (_lastUsedLock) { _lastUsed = now; }

        using var lease = await _limiter.AcquireAsync(permitCount: 1, ct).ConfigureAwait(false);
        if (!lease.IsAcquired)
            throw new InvalidOperationException(
                "Rate limiter queue exhausted. Reduce concurrent request rate or increase RateLimitOptions.PermitLimit.");
    }

    public void Dispose() => _limiter.Dispose();
}
