namespace GrznarAi.Trading.ReadOnly.Configuration;

/// <summary>
/// Rate-limiting options for the eToro API client (sliding-window limiter + HTTP 429 retry policy).
/// <br/>CZ: Možnosti omezení rychlosti pro klienta eToro API (limiter klouzavého okna + politika opakování HTTP 429).
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>Default maximum number of requests per <see cref="Window"/> (60).<br/>CZ: Výchozí maximální počet požadavků za <see cref="Window"/> (60).</summary>
    public const int DefaultPermitLimit = 60;

    /// <summary>Default sliding window duration (1 minute).<br/>CZ: Výchozí délka klouzavého okna (1 minuta).</summary>
    public static readonly TimeSpan DefaultWindow = TimeSpan.FromMinutes(1);

    /// <summary>Default base delay before retrying after HTTP 429 (60 seconds).<br/>CZ: Výchozí základní zpoždění před opakováním po HTTP 429 (60 sekund).</summary>
    public static readonly TimeSpan DefaultRetryDelayValue = TimeSpan.FromSeconds(60);

    /// <summary>Default maximum retry delay cap (60 seconds).<br/>CZ: Výchozí maximální strop zpoždění pro opakování (60 sekund).</summary>
    public static readonly TimeSpan DefaultMaxRetryDelay = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Whether the sliding-window rate limiter is active. Defaults to <see langword="true"/>.
    /// <br/>CZ: Zda je aktivní limiter klouzavého okna. Výchozí je <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of requests allowed within <see cref="Window"/>. Defaults to <see cref="DefaultPermitLimit"/> (60).
    /// <br/>CZ: Maximální počet požadavků povolených v rámci <see cref="Window"/>. Výchozí je <see cref="DefaultPermitLimit"/> (60).
    /// </summary>
    public int PermitLimit { get; set; } = DefaultPermitLimit;

    /// <summary>
    /// Duration of the sliding window. Defaults to <see cref="DefaultWindow"/> (1 minute).
    /// <br/>CZ: Délka klouzavého okna. Výchozí je <see cref="DefaultWindow"/> (1 minuta).
    /// </summary>
    public TimeSpan Window { get; set; } = DefaultWindow;

    /// <summary>
    /// Maximum number of retry attempts on HTTP 429 before propagating the error. Defaults to 3.
    /// <br/>CZ: Maximální počet pokusů o opakování při HTTP 429 před předáním chyby. Výchozí je 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base retry delay used when the server does not return a <c>Retry-After</c> header.
    /// Defaults to <see cref="DefaultRetryDelayValue"/> (60 seconds).
    /// <br/>CZ: Základní zpoždění pro opakování, pokud server nevrátí hlavičku <c>Retry-After</c>.
    /// Výchozí je <see cref="DefaultRetryDelayValue"/> (60 sekund).
    /// </summary>
    public TimeSpan DefaultRetryDelay { get; set; } = DefaultRetryDelayValue;

    /// <summary>
    /// Upper bound on the computed retry delay (including jitter). Defaults to <see cref="DefaultMaxRetryDelay"/> (60 seconds).
    /// <br/>CZ: Horní mez vypočítaného zpoždění opakování (včetně rozptylu). Výchozí je <see cref="DefaultMaxRetryDelay"/> (60 sekund).
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = DefaultMaxRetryDelay;

    /// <summary>
    /// Fraction of <see cref="DefaultRetryDelay"/> added as random jitter to spread retry storms. Defaults to 0.1 (±10 %).
    /// Uses <see cref="System.Random.Shared"/> — not cryptographic, intentionally so.
    /// <br/>CZ: Podíl <see cref="DefaultRetryDelay"/> přidaný jako náhodný rozptyl k rozmístění bouří opakování. Výchozí je 0,1 (±10 %).
    /// </summary>
    public double RetryJitterRatio { get; set; } = 0.1;

    /// <summary>
    /// When <see langword="true"/>, non-idempotent requests (e.g., POST) are also retried on HTTP 429.
    /// Defaults to <see langword="false"/> — retry only safe/idempotent methods (GET, HEAD).
    /// <br/>CZ: Pokud je <see langword="true"/>, jsou i neindempotentní požadavky (např. POST) opakovány při HTTP 429.
    /// Výchozí je <see langword="false"/> — opakují se pouze bezpečné/idempotentní metody (GET, HEAD).
    /// </summary>
    public bool RetryNonIdempotentRequests { get; set; }
}
