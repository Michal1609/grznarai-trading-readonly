using GrznarAi.Trading.ReadOnly.Configuration;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Configuration;

/// <summary>
/// Configuration options for the Coinbase Advanced Trade API client.
/// Bind from <c>appsettings.json</c> under the <c>Coinbase</c> section via <c>AddCoinbaseClient</c>.
/// <br/>CZ: KonfiguraÄŤnĂ­ moĹľnosti klienta Coinbase Advanced Trade API.
/// </summary>
public sealed class CoinbaseOptions
{
    public const string SectionName = "Coinbase";

    public const string DefaultBaseUrl = "https://api.coinbase.com";

    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);

    /// <summary>CDP API key name in form <c>organizations/{org_id}/apiKeys/{key_id}</c>.</summary>
    public string KeyName { get; set; } = string.Empty;

    /// <summary>PEM-encoded EC private key associated with <see cref="KeyName"/>.</summary>
    public string PrivateKeyPem { get; set; } = string.Empty;

    /// <summary>Base address of the Coinbase API. Defaults to <see cref="DefaultBaseUrl"/>.</summary>
    public string BaseUrl { get; set; } = DefaultBaseUrl;

    /// <summary>JWT lifetime sent in <c>exp</c> claim (seconds). Defaults to 120.</summary>
    public int JwtLifetimeSeconds { get; set; } = 120;

    /// <summary>HTTP request timeout. Defaults to 100 s.</summary>
    public TimeSpan Timeout { get; set; } = DefaultTimeout;

    /// <summary>Value sent as <c>User-Agent</c>. Defaults to <c>"GrznarAi.Trading.ReadOnly.Coinbase"</c>.</summary>
    public string UserAgent { get; set; } = "GrznarAi.Trading.ReadOnly.Coinbase";

    /// <summary>
    /// When <see langword="true"/>, allows any HTTPS host as <see cref="BaseUrl"/>,
    /// disabling the host-pinning security control restricting requests to <c>api.coinbase.com</c>.
    /// </summary>
    public bool AllowCustomBaseAddress { get; set; }

    public RateLimitOptions RateLimit { get; set; } = new()
    {
        PermitLimit = 10_000,
        Window = TimeSpan.FromHours(1),
        DefaultRetryDelay = TimeSpan.FromSeconds(1),
        MaxRetryDelay = TimeSpan.FromSeconds(30)
    };

    public ErrorHandlingOptions ErrorHandling { get; set; } = new();

    public ResilienceOptions Resilience { get; set; } = new();

    /// <summary>
    /// Diagnostic capture configuration. Controls request/response snapshot history.
    /// Disabled by default — enable via <see cref="DiagnosticOptions.Enabled"/>.
    /// </summary>
    public DiagnosticOptions Diagnostics { get; set; } = new();

    public override string ToString() =>
        $"CoinbaseOptions {{ KeyName=[REDACTED], BaseUrl={BaseUrl}, Timeout={Timeout}, UserAgent={UserAgent} }}";
}
