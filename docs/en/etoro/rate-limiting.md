# Rate Limiting and Retries

The library includes proactive rate limiting and automatic retry handling for HTTP `429 Too Many Requests`.

## Defaults

```json
{
  "RateLimit": {
    "Enabled": true,
    "PermitLimit": 60,
    "Window": "00:01:00",
    "MaxRetries": 3,
    "DefaultRetryDelay": "00:01:00",
    "MaxRetryDelay": "00:01:00",
    "RetryJitterRatio": 0.1,
    "RetryNonIdempotentRequests": false
  }
}
```

## Proactive Limiting

GET requests are limited before they are sent. By default, the limiter allows `60` requests per minute.

For one user key, the default singleton limiter is enough:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

For multiple user keys in one process, use the keyed limiter:

```csharp
builder.Services.AddEToro(builder.Configuration, useKeyedRateLimiter: true);
```

## Retry Behavior

When the API returns `429`, the handler:

- Respects the `Retry-After` header when present.
- Uses exponential backoff when `Retry-After` is not present.
- Caps delays by `MaxRetryDelay`.
- Adds jitter according to `RetryJitterRatio`.
- Retries `GET` and `HEAD` by default.
- Honors `CancellationToken`.

`MaxRetries = 0` disables automatic retries while still sending the original request once.

## Non-Idempotent Requests

`POST`, `PATCH`, and `DELETE` are not retried by default. Enable `RetryNonIdempotentRequests` only when duplicate execution is safe for the endpoint you are calling.

```json
{
  "EToroOptions": {
    "RateLimit": {
      "RetryNonIdempotentRequests": false
    }
  }
}
```
