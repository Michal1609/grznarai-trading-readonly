namespace GrznarAi.Trading.ReadOnly.Configuration;

/// <summary>
/// Error-handling options that control how non-success HTTP responses are captured and surfaced via <see cref="EToro.Client.EToroApiException"/>.
/// <br/>CZ: Možnosti zpracování chyb, které řídí způsob zachycení a zobrazení neúspěšných HTTP odpovědí prostřednictvím <see cref="EToro.Client.EToroApiException"/>.
/// </summary>
public sealed class ErrorHandlingOptions
{
    /// <summary>Default maximum number of characters of the response body included in exceptions (4 096).<br/>CZ: Výchozí maximální počet znaků těla odpovědi zahrnutých do výjimek (4 096).</summary>
    public const int DefaultMaxResponseBodyLength = 4096;

    /// <summary>
    /// When <see langword="true"/>, the response body is attached to <see cref="EToro.Client.EToroApiException.ResponseBody"/> for diagnostics.
    /// Defaults to <see langword="true"/>. Set to <see langword="false"/> to suppress body capture entirely (reduces memory on large error bodies).
    /// <br/>CZ: Pokud je <see langword="true"/>, tělo odpovědi je připojeno k <see cref="EToro.Client.EToroApiException.ResponseBody"/> pro diagnostiku.
    /// Výchozí je <see langword="true"/>. Nastavte na <see langword="false"/> pro úplné potlačení zachytávání těla.
    /// </summary>
    public bool IncludeResponseBody { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, sensitive fields (API keys, tokens, passwords) in the response body are replaced with <c>[REDACTED]</c> before the exception is created.
    /// Defaults to <see langword="true"/>. Only effective when <see cref="IncludeResponseBody"/> is also <see langword="true"/>.
    /// <br/>CZ: Pokud je <see langword="true"/>, citlivá pole (klíče API, tokeny, hesla) v těle odpovědi jsou před vytvořením výjimky nahrazena hodnotou <c>[REDACTED]</c>.
    /// Výchozí je <see langword="true"/>. Účinné pouze pokud je <see cref="IncludeResponseBody"/> také <see langword="true"/>.
    /// </summary>
    public bool RedactResponseBody { get; set; } = true;

    /// <summary>
    /// Maximum number of characters of the response body to capture. Body is truncated beyond this limit.
    /// Defaults to <see cref="DefaultMaxResponseBodyLength"/> (4 096). Set to <see langword="null"/> for unlimited (not recommended — hostile upstreams can exhaust memory).
    /// <br/>CZ: Maximální počet znaků těla odpovědi k zachycení. Tělo je za tímto limitem ořezáno.
    /// Výchozí je <see cref="DefaultMaxResponseBodyLength"/> (4 096). Nastavte na <see langword="null"/> pro neomezené (nedoporučeno — nepřátelský upstream může vyčerpat paměť).
    /// </summary>
    public int? MaxResponseBodyLength { get; set; } = DefaultMaxResponseBodyLength;
}
