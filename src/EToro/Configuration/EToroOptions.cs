namespace GrznarAi.Trading.ReadOnly.Configuration;

/// <summary>
/// Configuration options for the eToro API client.
/// Bind from <c>appsettings.json</c> under the <c>EToroOptions</c> section via <c>AddEToro(configuration)</c>.
/// <br/>CZ: Konfigurační možnosti klienta eToro API. Navažte z <c>appsettings.json</c> pod sekcí <c>EToroOptions</c>.
/// </summary>
public sealed class EToroOptions
{
    /// <summary>Default base address for the eToro public API v1.<br/>CZ: Výchozí základní adresa veřejného API eToro v1.</summary>
    public const string DefaultBaseAddress = "https://public-api.etoro.com/api/v1/";

    /// <summary>Default HTTP request timeout (100 seconds).<br/>CZ: Výchozí časový limit HTTP požadavku (100 sekund).</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);

    /// <summary>
    /// eToro API key (<c>x-api-key</c> header). Required — obtain from your eToro developer portal.
    /// <br/>CZ: Klíč eToro API (hlavička <c>x-api-key</c>). Povinné — získejte z vývojářského portálu eToro.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// eToro user key (<c>x-user-key</c> header). Required — identifies the authenticated user.
    /// <br/>CZ: Uživatelský klíč eToro (hlavička <c>x-user-key</c>). Povinné — identifikuje přihlášeného uživatele.
    /// </summary>
    public string UserKey { get; set; } = string.Empty;

    /// <summary>
    /// Target trading environment. Accepted values: <c>"real"</c> (default) or <c>"demo"</c> (case-insensitive).
    /// Any other value silently targets the real environment — validate carefully to avoid accidental live-account requests.
    /// <br/>CZ: Cílové obchodní prostředí. Povolené hodnoty: <c>"real"</c> (výchozí) nebo <c>"demo"</c> (bez ohledu na velikost).
    /// Jiná hodnota tiše cílí na reálné prostředí — ověřte pečlivě, aby nedošlo k nechtěnému dotazu na živý účet.
    /// </summary>
    public string Environment { get; set; } = "real";

    /// <summary>
    /// Base address of the eToro API. Defaults to <see cref="DefaultBaseAddress"/>.
    /// Override only when <see cref="AllowCustomBaseAddress"/> is <see langword="true"/>.
    /// <br/>CZ: Základní adresa API eToro. Výchozí je <see cref="DefaultBaseAddress"/>.
    /// Přepište pouze pokud je <see cref="AllowCustomBaseAddress"/> nastaveno na <see langword="true"/>.
    /// </summary>
    public Uri BaseAddress { get; set; } = new(DefaultBaseAddress);

    /// <summary>
    /// When <see langword="true"/>, allows any HTTPS host as <see cref="BaseAddress"/>,
    /// disabling the built-in host-pinning security control that restricts requests to
    /// <c>public-api.etoro.com</c>. Use only for testing or proxy scenarios.
    /// <br/>CZ: Pokud je <see langword="true"/>, povolí libovolného HTTPS hostitele jako <see cref="BaseAddress"/>,
    /// čímž zakáže vestavěné ověření hostitele. Používejte pouze pro testování nebo proxy scénáře.
    /// </summary>
    public bool AllowCustomBaseAddress { get; set; }

    /// <summary>
    /// HTTP request timeout. Defaults to <see cref="DefaultTimeout"/> (100 seconds).
    /// <br/>CZ: Časový limit HTTP požadavku. Výchozí je <see cref="DefaultTimeout"/> (100 sekund).
    /// </summary>
    public TimeSpan Timeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Value sent as the <c>User-Agent</c> HTTP header. Defaults to <c>"EToro"</c>.
    /// <br/>CZ: Hodnota odesílaná jako hlavička <c>User-Agent</c>. Výchozí je <c>"EToro"</c>.
    /// </summary>
    public string UserAgent { get; set; } = "EToro";

    /// <summary>
    /// Rate-limiting configuration. Controls sliding-window permit limit, retry policy, and jitter.
    /// <br/>CZ: Konfigurace omezení rychlosti. Řídí limit povolení v klouzavém okně, politiku opakování a rozptyl.
    /// </summary>
    public RateLimitOptions RateLimit { get; set; } = new();

    /// <summary>
    /// Error-handling configuration. Controls response body inclusion, redaction, and size limits.
    /// <br/>CZ: Konfigurace zpracování chyb. Řídí zahrnutí těla odpovědi, redigování a limity velikosti.
    /// </summary>
    public ErrorHandlingOptions ErrorHandling { get; set; } = new();

    /// <summary>
    /// Resilience configuration. Controls conservative retry of transient HTTP failures (5xx/network).
    /// <br/>CZ: Konfigurace odolnosti. Ridi konzervativni opakovani prechodnych HTTP chyb (5xx/sit).
    /// </summary>
    public ResilienceOptions Resilience { get; set; } = new();

    /// <summary>
    /// Resolved URL path segment for the current environment (<c>"demo"</c> or <c>"real"</c>).
    /// <br/>CZ: Rozvinutý URL segment cesty pro aktuální prostředí (<c>"demo"</c> nebo <c>"real"</c>).
    /// </summary>
    public string EnvironmentSegment =>
        Environment.Equals("demo", StringComparison.OrdinalIgnoreCase) ? "demo" : "real";

    public override string ToString() =>
        $"EToroOptions {{ ApiKey=[REDACTED], UserKey=[REDACTED], Environment={Environment}, BaseAddress={BaseAddress}, Timeout={Timeout}, UserAgent={UserAgent} }}";
}
