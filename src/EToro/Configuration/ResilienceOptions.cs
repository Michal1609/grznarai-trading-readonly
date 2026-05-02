namespace GrznarAi.Trading.ReadOnly.Configuration;

/// <summary>
/// Transient HTTP failure retry options for the eToro API client.
/// <br/>CZ: Nastaveni opakovani prechodnych HTTP chyb pro klienta eToro API.
/// </summary>
public sealed class ResilienceOptions
{
    /// <summary>
    /// Enables conservative retry of transient HTTP failures. Defaults to <see langword="true"/>.
    /// <br/>CZ: Zapina konzervativni opakovani prechodnych HTTP chyb. Vychozi je <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts for transient failures. Defaults to 2.
    /// <br/>CZ: Maximalni pocet opakovani pro prechodne chyby. Vychozi hodnota je 2.
    /// </summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>
    /// Base delay for exponential backoff when retrying transient failures.
    /// <br/>CZ: Zakladni prodleva pro exponencialni backoff pri opakovani prechodnych chyb.
    /// </summary>
    public TimeSpan DefaultRetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Upper bound on computed retry delay. Defaults to 10 seconds.
    /// <br/>CZ: Horni mez vypoctene prodlevy opakovani. Vychozi hodnota je 10 sekund.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Fraction of retry delay added as random jitter. Defaults to 0.1.
    /// <br/>CZ: Podil prodlevy pridany jako nahodny rozptyl. Vychozi hodnota je 0,1.
    /// </summary>
    public double RetryJitterRatio { get; set; } = 0.1;

    /// <summary>
    /// When <see langword="true"/>, non-idempotent requests are retried for transient failures.
    /// Defaults to <see langword="false"/>.
    /// <br/>CZ: Pokud je <see langword="true"/>, opakuji se pri prechodnych chybach i neindempotentni pozadavky.
    /// Vychozi je <see langword="false"/>.
    /// </summary>
    public bool RetryNonIdempotentRequests { get; set; }
}
