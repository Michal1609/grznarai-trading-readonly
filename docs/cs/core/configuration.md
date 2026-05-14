# Konfigurace

Core poskytuje sdilene option typy, ktere pouzivaji platformni balicky.

## `RateLimitOptions`

Ridi proactive sliding-window rate limiting a retry pro HTTP `429`.

Dulezite vlastnosti:

- `Enabled`
- `PermitLimit`
- `Window`
- `MaxRetries`
- `DefaultRetryDelay`
- `MaxRetryDelay`
- `RetryJitterRatio`
- `RetryNonIdempotentRequests`

## `ResilienceOptions`

Ridi retry pro transient HTTP chyby jako `408`, `502`, `503`, `504` a sitove vyjimky.

HTTP `429` resi `RateLimitOptions`, ne transient retry.

## `ErrorHandlingOptions`

Ridi zachyceni response body pro vyjimky odvozene z `TradingApiException`.

## `DiagnosticOptions`

Ridi request/response snapshoty. Diagnostika je ve vychozim stavu vypnuta.

Nazvy konfiguracnich sekci patri platformnim balickum:

- eToro: `EToroOptions`
- Coinbase: `Coinbase`

