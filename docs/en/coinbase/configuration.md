# Configuration

The client is configured through `CoinbaseOptions` (section `Coinbase` in `appsettings.json`).

## Required

| Key | Description |
| --- | --- |
| `KeyName` | CDP API key name (`organizations/{org}/apiKeys/{id}`). |
| `PrivateKeyPem` | PEM-encoded EC private key. |
| `BaseUrl` | `https://api.coinbase.com` (default). |

## Optional

| Key | Default | Description |
| --- | --- | --- |
| `JwtLifetimeSeconds` | `120` | JWT lifetime (`exp` claim). |
| `Timeout` | `00:01:40` | HTTP request timeout. |
| `UserAgent` | `GrznarAi.Trading.ReadOnly.Coinbase` | HTTP User-Agent header. |
| `AllowCustomBaseAddress` | `false` | Allow hosts other than `api.coinbase.com`. |

## `RateLimit` section

| Key | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | Activates the sliding-window limiter. |
| `PermitLimit` | `10000` | Max requests per window (Coinbase: 10,000/h per profile). |
| `Window` | `01:00:00` | Window length. |
| `MaxRetries` | `3` | Max retries on HTTP 429. |
| `DefaultRetryDelay` | `00:00:01` | Base backoff when `Retry-After` is missing. |
| `MaxRetryDelay` | `00:00:30` | Upper bound on retry delay. |
| `RetryJitterRatio` | `0.1` | Â±10% jitter. |
| `RetryNonIdempotentRequests` | `false` | Retry POSTs? |

## `ErrorHandling` section

| Key | Default | Description |
| --- | --- | --- |
| `IncludeResponseBody` | `true` | Include response body in `CoinbaseApiException`. |
| `RedactResponseBody` | `true` | Redact secrets (apiKey, token, JWT, Bearer). |
| `MaxResponseBodyLength` | `4096` | Max body length. |

## `Resilience` section

| Key | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | Retry on 5xx/timeout/network errors. |
| `MaxRetries` | `2` | Max retries. |
| `DefaultRetryDelay` | `00:00:01` | Base backoff. |
| `MaxRetryDelay` | `00:00:10` | Upper bound. |
| `RetryJitterRatio` | `0.1` | Â±10% jitter. |
| `RetryNonIdempotentRequests` | `false` | Retry POSTs? |
