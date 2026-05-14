namespace GrznarAi.Trading.ReadOnly.Configuration;

/// <summary>
/// Rate-limiting options for a platform API client (sliding-window limiter + HTTP 429 retry policy).
/// Default values are conservative; platform packages override them in their own options initializer.
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>Default maximum number of requests per <see cref="Window"/> (60).</summary>
    public const int DefaultPermitLimit = 60;

    /// <summary>Default sliding window duration (1 minute).</summary>
    public static readonly TimeSpan DefaultWindow = TimeSpan.FromMinutes(1);

    /// <summary>Default base delay before retrying after HTTP 429 (60 seconds).</summary>
    public static readonly TimeSpan DefaultRetryDelayValue = TimeSpan.FromSeconds(60);

    /// <summary>Default maximum retry delay cap (60 seconds).</summary>
    public static readonly TimeSpan DefaultMaxRetryDelay = TimeSpan.FromSeconds(60);

    /// <summary>Whether the sliding-window rate limiter is active. Defaults to <see langword="true"/>.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Maximum number of requests allowed within <see cref="Window"/>. Defaults to <see cref="DefaultPermitLimit"/> (60).</summary>
    public int PermitLimit { get; set; } = DefaultPermitLimit;

    /// <summary>Duration of the sliding window. Defaults to <see cref="DefaultWindow"/> (1 minute).</summary>
    public TimeSpan Window { get; set; } = DefaultWindow;

    /// <summary>Maximum number of retry attempts on HTTP 429 before propagating the error. Defaults to 3.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base retry delay used when the server does not return a <c>Retry-After</c> header.
    /// Defaults to <see cref="DefaultRetryDelayValue"/> (60 seconds).
    /// </summary>
    public TimeSpan DefaultRetryDelay { get; set; } = DefaultRetryDelayValue;

    /// <summary>Upper bound on the computed retry delay (including jitter). Defaults to <see cref="DefaultMaxRetryDelay"/> (60 seconds).</summary>
    public TimeSpan MaxRetryDelay { get; set; } = DefaultMaxRetryDelay;

    /// <summary>
    /// Fraction of <see cref="DefaultRetryDelay"/> added as random jitter to spread retry storms. Defaults to 0.1 (±10 %).
    /// Uses <see cref="System.Random.Shared"/> — not cryptographic, intentionally so.
    /// </summary>
    public double RetryJitterRatio { get; set; } = 0.1;

    /// <summary>
    /// When <see langword="true"/>, non-idempotent requests (e.g., POST) are also retried on HTTP 429.
    /// Defaults to <see langword="false"/> — retry only safe/idempotent methods (GET, HEAD).
    /// </summary>
    public bool RetryNonIdempotentRequests { get; set; }
}
