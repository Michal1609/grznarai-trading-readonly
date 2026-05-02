namespace GrznarAi.Trading.ReadOnly.Client;

/// <summary>
/// Abstracts rate limiting strategy for eToro API requests.
/// </summary>
/// <remarks>
/// The default implementation (<see cref="EToroRateLimiter"/>) is a singleton scoped to a single user key.
/// Use <see cref="EToroKeyedRateLimiter"/> when serving multiple user keys within one application instance.
/// </remarks>
public interface IEToroRateLimiter
{
    /// <summary>
    /// Waits until the request can be sent within the configured rate limit.
    /// </summary>
    Task WaitAsync(HttpRequestMessage request, CancellationToken ct);
}
