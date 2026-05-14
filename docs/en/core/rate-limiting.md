# Rate Limiting

Core contains the shared rate limiting primitives used by platform clients.

## Handlers

`RateLimitHandler` waits before sending requests through an `IApiRateLimiter`. It also retries final HTTP `429 Too Many Requests` responses according to `RateLimitOptions`.

`TransientHttpErrorHandler` is separate and handles transient failures. It intentionally does not handle `429`.

## Single Credential

`ApiRateLimiter` uses one sliding window and is suitable when a client instance is configured for one credential set.

## Multiple Credentials

`KeyedRateLimiter` keeps independent sliding windows per credential key. Platform packages provide the key extraction logic. Credential keys must be opaque and must not expose raw API keys.

## Non-Idempotent Requests

Retries for non-idempotent methods are disabled by default. Enable `RetryNonIdempotentRequests` only when the calling API operation is safe to repeat.

