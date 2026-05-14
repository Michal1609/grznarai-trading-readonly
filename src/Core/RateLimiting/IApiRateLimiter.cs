namespace GrznarAi.Trading.ReadOnly.RateLimiting;

/// <summary>
/// Abstracts rate limiting strategy for platform API requests.
/// </summary>
/// <remarks>
/// The default implementation (<see cref="ApiRateLimiter"/>) is a singleton scoped to a single credential set.
/// Use <see cref="KeyedRateLimiter"/> when one application instance serves multiple credential sets simultaneously.
/// </remarks>
public interface IApiRateLimiter
{
    /// <summary>
    /// Waits until the request can be sent within the configured rate limit.
    /// Throws <see cref="OperationCanceledException"/> if <paramref name="ct"/> is cancelled.
    /// </summary>
    Task WaitAsync(HttpRequestMessage request, CancellationToken ct);
}
