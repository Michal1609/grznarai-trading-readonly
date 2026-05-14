# Rate limiting a retry

Coinbase Advanced Trade limity (public, k 2024+):

- **30 req/s** per IP
- **10 000 req/h** per profile

Klient používá **sliding-window** limiter (`System.Threading.RateLimiting.SlidingWindowRateLimiter`) s defaultním nastavením 10 000 požadavků za 1 hodinu. Změňte v `Coinbase:RateLimit:PermitLimit` a `Coinbase:RateLimit:Window`.

## HTTP pipeline

Pořadí `DelegatingHandler` v pipeline:

1. **`RateLimitHandler`** (vnější) — čeká na permit, retry na HTTP 429.
2. **`TransientHttpErrorHandler`** — retry na 408/502/503/504 + `HttpRequestException`.
3. **`CdpJwtAuthHandler`** (vnitřní) — generuje JWT pro každý fyzický odchozí request (i retry).

## Retry semantika

- Retry pouze pro **idempotentní** metody (GET, HEAD). Pro POST/PUT/DELETE nastavte `RetryNonIdempotentRequests = true` (nedoporučeno bez idempotency key).
- HTTP 429 — používá `Retry-After` (delta nebo Date). Pokud chybí, exponenciální backoff z `DefaultRetryDelay × 2^attempt` ± jitter.
- 5xx — exponenciální backoff dle `Resilience` sekce.
- Strop: `MaxRetryDelay`.

## Single-host limit

Limiter je singleton sdílený napříč všemi requesty. Pro multi-user scénáře (víc CDP klíčů v jedné aplikaci) je potřeba keyed limiter — není zatím implementován.
