# Configuration

The library reads settings from the `EToroOptions` configuration section or from an `Action<EToroOptions>`.

## appsettings.json

```json
{
  "EToroOptions": {
    "ApiKey": "replace-me-api-key",
    "UserKey": "replace-me-user-key",
    "Environment": "real",
    "BaseAddress": "https://public-api.etoro.com/api/v1/",
    "AllowCustomBaseAddress": false,
    "Timeout": "00:01:40",
    "UserAgent": "MyApp",
    "RateLimit": {
      "Enabled": true,
      "PermitLimit": 60,
      "Window": "00:01:00",
      "MaxRetries": 3,
      "DefaultRetryDelay": "00:01:00",
      "MaxRetryDelay": "00:01:00",
      "RetryJitterRatio": 0.1,
      "RetryNonIdempotentRequests": false
    },
    "ErrorHandling": {
      "IncludeResponseBody": true,
      "RedactResponseBody": true,
      "MaxResponseBodyLength": 4096
    }
  }
}
```

## Code Configuration

```csharp
using GrznarAi.Trading.ReadOnly.Client;

builder.Services.AddEToro(options =>
{
    options.ApiKey = builder.Configuration["EToroOptions:ApiKey"]!;
    options.UserKey = builder.Configuration["EToroOptions:UserKey"]!;
    options.UserAgent = "MyApp";
    options.RateLimit.Enabled = true;
});
```

## Options

| Option | Default | Description |
| --- | --- | --- |
| `ApiKey` | empty | eToro API key. Required. |
| `UserKey` | empty | eToro user key. Required. |
| `Environment` | `real` | Default environment segment for internal helpers. Endpoint methods usually accept `EToroEnvironment` explicitly. |
| `BaseAddress` | `https://public-api.etoro.com/api/v1/` | API base URL. Must be HTTPS, absolute, and end with `/`. |
| `AllowCustomBaseAddress` | `false` | Allows a custom host for tests or proxies. |
| `Timeout` | `00:01:40` | HTTP timeout. Must be greater than zero and no more than five minutes. |
| `UserAgent` | `EToro` | User agent sent by `HttpClient`. Required. |
| `RateLimit` | enabled | Rate-limit and retry behavior. |
| `ErrorHandling` | safe diagnostics | Error response body capture, redaction, and truncation behavior. |

## Validation

Options are validated on startup. Invalid credentials, invalid base address, missing user agent, invalid timeout, and invalid rate-limit settings fail fast during application startup.
`ErrorHandling.MaxResponseBodyLength` must be `null` or zero or greater.

## Multi-User Applications

For a single eToro user key, use the default registration:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

For one application instance serving multiple eToro user keys, enable the keyed limiter:

```csharp
builder.Services.AddEToro(builder.Configuration, useKeyedRateLimiter: true);
```

The keyed limiter keeps independent rate-limit windows per user key.
