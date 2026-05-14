using GrznarAi.Trading.ReadOnly.Configuration;

namespace GrznarAi.Trading.ReadOnly.Etoro.Configuration;

/// <summary>
/// Configuration options for the eToro API client.
/// Bind from <c>appsettings.json</c> under the <c>EToroOptions</c> section via <c>AddEToro(configuration)</c>.
/// <br/>CZ: KonfiguraÄŤnĂ­ moĹľnosti klienta eToro API. NavaĹľte z <c>appsettings.json</c> pod sekcĂ­ <c>EToroOptions</c>.
/// </summary>
public sealed class EToroOptions
{
    /// <summary>Default base address for the eToro public API v1.<br/>CZ: VĂ˝chozĂ­ zĂˇkladnĂ­ adresa veĹ™ejnĂ©ho API eToro v1.</summary>
    public const string DefaultBaseAddress = "https://public-api.etoro.com/api/v1/";

    /// <summary>Default HTTP request timeout (100 seconds).<br/>CZ: VĂ˝chozĂ­ ÄŤasovĂ˝ limit HTTP poĹľadavku (100 sekund).</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);

    /// <summary>
    /// eToro API key (<c>x-api-key</c> header). Required â€” obtain from your eToro developer portal.
    /// <br/>CZ: KlĂ­ÄŤ eToro API (hlaviÄŤka <c>x-api-key</c>). PovinnĂ© â€” zĂ­skejte z vĂ˝vojĂˇĹ™skĂ©ho portĂˇlu eToro.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// eToro user key (<c>x-user-key</c> header). Required â€” identifies the authenticated user.
    /// <br/>CZ: UĹľivatelskĂ˝ klĂ­ÄŤ eToro (hlaviÄŤka <c>x-user-key</c>). PovinnĂ© â€” identifikuje pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele.
    /// </summary>
    public string UserKey { get; set; } = string.Empty;

    /// <summary>
    /// Target trading environment. Accepted values: <c>"real"</c> (default) or <c>"demo"</c> (case-insensitive).
    /// Any other value silently targets the real environment â€” validate carefully to avoid accidental live-account requests.
    /// <br/>CZ: CĂ­lovĂ© obchodnĂ­ prostĹ™edĂ­. PovolenĂ© hodnoty: <c>"real"</c> (vĂ˝chozĂ­) nebo <c>"demo"</c> (bez ohledu na velikost).
    /// JinĂˇ hodnota tiĹˇe cĂ­lĂ­ na reĂˇlnĂ© prostĹ™edĂ­ â€” ovÄ›Ĺ™te peÄŤlivÄ›, aby nedoĹˇlo k nechtÄ›nĂ©mu dotazu na ĹľivĂ˝ ĂşÄŤet.
    /// </summary>
    public string Environment { get; set; } = "real";

    /// <summary>
    /// Base address of the eToro API. Defaults to <see cref="DefaultBaseAddress"/>.
    /// Override only when <see cref="AllowCustomBaseAddress"/> is <see langword="true"/>.
    /// <br/>CZ: ZĂˇkladnĂ­ adresa API eToro. VĂ˝chozĂ­ je <see cref="DefaultBaseAddress"/>.
    /// PĹ™epiĹˇte pouze pokud je <see cref="AllowCustomBaseAddress"/> nastaveno na <see langword="true"/>.
    /// </summary>
    public Uri BaseAddress { get; set; } = new(DefaultBaseAddress);

    /// <summary>
    /// When <see langword="true"/>, allows any HTTPS host as <see cref="BaseAddress"/>,
    /// disabling the built-in host-pinning security control that restricts requests to
    /// <c>public-api.etoro.com</c>. Use only for testing or proxy scenarios.
    /// <br/>CZ: Pokud je <see langword="true"/>, povolĂ­ libovolnĂ©ho HTTPS hostitele jako <see cref="BaseAddress"/>,
    /// ÄŤĂ­mĹľ zakĂˇĹľe vestavÄ›nĂ© ovÄ›Ĺ™enĂ­ hostitele. PouĹľĂ­vejte pouze pro testovĂˇnĂ­ nebo proxy scĂ©nĂˇĹ™e.
    /// </summary>
    public bool AllowCustomBaseAddress { get; set; }

    /// <summary>
    /// HTTP request timeout. Defaults to <see cref="DefaultTimeout"/> (100 seconds).
    /// <br/>CZ: ÄŚasovĂ˝ limit HTTP poĹľadavku. VĂ˝chozĂ­ je <see cref="DefaultTimeout"/> (100 sekund).
    /// </summary>
    public TimeSpan Timeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Value sent as the <c>User-Agent</c> HTTP header. Defaults to <c>"EToro"</c>.
    /// <br/>CZ: Hodnota odesĂ­lanĂˇ jako hlaviÄŤka <c>User-Agent</c>. VĂ˝chozĂ­ je <c>"EToro"</c>.
    /// </summary>
    public string UserAgent { get; set; } = "EToro";

    /// <summary>
    /// Rate-limiting configuration. Controls sliding-window permit limit, retry policy, and jitter.
    /// <br/>CZ: Konfigurace omezenĂ­ rychlosti. ĹĂ­dĂ­ limit povolenĂ­ v klouzavĂ©m oknÄ›, politiku opakovĂˇnĂ­ a rozptyl.
    /// </summary>
    public RateLimitOptions RateLimit { get; set; } = new();

    /// <summary>
    /// Error-handling configuration. Controls response body inclusion, redaction, and size limits.
    /// <br/>CZ: Konfigurace zpracovĂˇnĂ­ chyb. ĹĂ­dĂ­ zahrnutĂ­ tÄ›la odpovÄ›di, redigovĂˇnĂ­ a limity velikosti.
    /// </summary>
    public ErrorHandlingOptions ErrorHandling { get; set; } = new();

    /// <summary>
    /// Resilience configuration. Controls conservative retry of transient HTTP failures (5xx/network).
    /// <br/>CZ: Konfigurace odolnosti. Ridi konzervativni opakovani prechodnych HTTP chyb (5xx/sit).
    /// </summary>
    public ResilienceOptions Resilience { get; set; } = new();

    /// <summary>
    /// Diagnostic capture configuration. Controls request/response snapshot history.
    /// Disabled by default — enable via <see cref="DiagnosticOptions.Enabled"/>.
    /// </summary>
    public DiagnosticOptions Diagnostics { get; set; } = new();

    /// <summary>
    /// Resolved URL path segment for the current environment (<c>"demo"</c> or <c>"real"</c>).
    /// <br/>CZ: RozvinutĂ˝ URL segment cesty pro aktuĂˇlnĂ­ prostĹ™edĂ­ (<c>"demo"</c> nebo <c>"real"</c>).
    /// </summary>
    public string EnvironmentSegment =>
        Environment.Equals("demo", StringComparison.OrdinalIgnoreCase) ? "demo" : "real";

    public override string ToString() =>
        $"EToroOptions {{ ApiKey=[REDACTED], UserKey=[REDACTED], Environment={Environment}, BaseAddress={BaseAddress}, Timeout={Timeout}, UserAgent={UserAgent} }}";
}
