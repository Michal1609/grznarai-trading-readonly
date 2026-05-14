namespace GrznarAi.Trading.ReadOnly.Configuration;

/// <summary>
/// Transient HTTP failure retry options for platform API clients.
/// HTTP 429 is handled by the dedicated <see cref="RateLimitOptions"/> retry policy, not here.
/// </summary>
public sealed class ResilienceOptions
{
    /// <summary>Enables conservative retry of transient HTTP failures. Defaults to <see langword="true"/>.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Maximum number of retry attempts for transient failures. Defaults to 2.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Base delay for exponential back-off when retrying transient failures. Defaults to 1 second.</summary>
    public TimeSpan DefaultRetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Upper bound on computed retry delay. Defaults to 10 seconds.</summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Fraction of retry delay added as random jitter. Defaults to 0.1 (±10 %).</summary>
    public double RetryJitterRatio { get; set; } = 0.1;

    /// <summary>
    /// When <see langword="true"/>, non-idempotent requests are retried for transient failures.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool RetryNonIdempotentRequests { get; set; }
}
