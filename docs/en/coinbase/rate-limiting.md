# Rate limiting & retry

Coinbase Advanced Trade public limits (as of 2024+):

- **30 req/s** per IP
- **10,000 req/h** per profile

The client uses a **sliding-window** limiter (`System.Threading.RateLimiting.SlidingWindowRateLimiter`) defaulting to 10,000 requests over 1 hour. Override via `Coinbase:RateLimit:PermitLimit` and `Coinbase:RateLimit:Window`.

## HTTP pipeline

Order of `DelegatingHandler` in the pipeline:

1. **`RateLimitHandler`** (outermost) — waits for a permit, retries on HTTP 429.
2. **`TransientHttpErrorHandler`** — retries on 408/502/503/504 + `HttpRequestException`.
3. **`CdpJwtAuthHandler`** (innermost) — generates a fresh JWT for each physical outgoing request (including retries).

## Retry semantics

- Retries only **idempotent** methods (GET, HEAD). For POST/PUT/DELETE set `RetryNonIdempotentRequests = true` (not recommended without an idempotency key).
- HTTP 429 — uses `Retry-After` (delta or Date). If missing, exponential backoff `DefaultRetryDelay × 2^attempt` ± jitter.
- 5xx — exponential backoff from the `Resilience` section.
- Cap: `MaxRetryDelay`.

## Single-host limit

The limiter is a singleton shared across all requests. For multi-user scenarios (multiple CDP keys in a single application) a keyed limiter is required — not yet implemented.
