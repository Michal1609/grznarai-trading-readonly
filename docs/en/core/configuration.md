# Configuration

Core exposes shared option types used by platform packages.

## `RateLimitOptions`

Controls proactive sliding-window rate limiting and retry handling for HTTP `429`.

Important properties:

- `Enabled`
- `PermitLimit`
- `Window`
- `MaxRetries`
- `DefaultRetryDelay`
- `MaxRetryDelay`
- `RetryJitterRatio`
- `RetryNonIdempotentRequests`

## `ResilienceOptions`

Controls retry for transient HTTP failures such as `408`, `502`, `503`, `504`, and network exceptions.

HTTP `429` is handled by `RateLimitOptions`, not by transient retry.

## `ErrorHandlingOptions`

Controls response body capture for exceptions derived from `TradingApiException`.

## `DiagnosticOptions`

Controls request/response snapshot capture. Diagnostics are disabled by default.

Platform options own the final configuration section names:

- eToro: `EToroOptions`
- Coinbase: `Coinbase`

