# Konfigurace

Klient se konfiguruje pĹ™es `CoinbaseOptions` (sekce `Coinbase` v `appsettings.json`).

## PovinnĂ©

| KlĂ­ÄŤ | Popis |
| --- | --- |
| `KeyName` | CDP API key name (`organizations/{org}/apiKeys/{id}`). |
| `PrivateKeyPem` | PEM-encoded EC private key. |
| `BaseUrl` | `https://api.coinbase.com` (default). |

## VolitelnĂ©

| KlĂ­ÄŤ | Default | Popis |
| --- | --- | --- |
| `JwtLifetimeSeconds` | `120` | Ĺ˝ivotnost JWT (claim `exp`). |
| `Timeout` | `00:01:40` | HTTP request timeout. |
| `UserAgent` | `GrznarAi.Trading.ReadOnly.Coinbase` | HTTP User-Agent. |
| `AllowCustomBaseAddress` | `false` | Povolit jinĂ˝ host neĹľ `api.coinbase.com`. |

## Sekce `RateLimit`

| KlĂ­ÄŤ | Default | Popis |
| --- | --- | --- |
| `Enabled` | `true` | Aktivuje sliding-window limiter. |
| `PermitLimit` | `10000` | MaximĂˇlnĂ­ poÄŤet poĹľadavkĹŻ za okno (Coinbase: 10 000/h per profile). |
| `Window` | `01:00:00` | DĂ©lka okna. |
| `MaxRetries` | `3` | Max. poÄŤet retry na HTTP 429. |
| `DefaultRetryDelay` | `00:00:01` | ZĂˇkladnĂ­ backoff pokud chybĂ­ `Retry-After`. |
| `MaxRetryDelay` | `00:00:30` | HornĂ­ mez retry delay. |
| `RetryJitterRatio` | `0.1` | Â±10 % jitter. |
| `RetryNonIdempotentRequests` | `false` | Retry POSTĹŻ? |

## Sekce `ErrorHandling`

| KlĂ­ÄŤ | Default | Popis |
| --- | --- | --- |
| `IncludeResponseBody` | `true` | Zahrnout tÄ›lo odpovÄ›di do `CoinbaseApiException`. |
| `RedactResponseBody` | `true` | Redigovat tajnosti (apiKey, token, JWT, Bearer). |
| `MaxResponseBodyLength` | `4096` | Limit dĂ©lky tÄ›la. |

## Sekce `Resilience`

| KlĂ­ÄŤ | Default | Popis |
| --- | --- | --- |
| `Enabled` | `true` | Retry pro 5xx/timeout/sĂ­ĹĄovĂ© chyby. |
| `MaxRetries` | `2` | Max. poÄŤet retry. |
| `DefaultRetryDelay` | `00:00:01` | ZĂˇkladnĂ­ backoff. |
| `MaxRetryDelay` | `00:00:10` | HornĂ­ mez. |
| `RetryJitterRatio` | `0.1` | Jitter Â±10 %. |
| `RetryNonIdempotentRequests` | `false` | Retry POSTĹŻ? |
