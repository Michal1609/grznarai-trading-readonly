# Rate Limiting

Core obsahuje sdilene rate limiting primitivy pro platformni klienty.

## Handlery

`RateLimitHandler` ceka pred odeslanim requestu podle `IApiRateLimiter`. Zaroven retryuje finalni HTTP `429 Too Many Requests` podle `RateLimitOptions`.

`TransientHttpErrorHandler` je oddeleny a resi transient chyby. Zamerne neresi `429`.

## Jedny credentials

`ApiRateLimiter` pouziva jedno sliding window a hodi se pro klienta nakonfigurovaneho na jednu sadu credentials.

## Vice credentials

`KeyedRateLimiter` drzi samostatne sliding window pro kazdy credential key. Platformni balicky dodavaji logiku pro ziskani klice. Credential key musi byt opaque a nesmi obsahovat raw API key.

## Non-idempotent requesty

Retry pro non-idempotent metody je ve vychozim stavu vypnuty. `RetryNonIdempotentRequests` zapinejte jen tehdy, kdyz je konkretni API operace bezpecna pro opakovani.

