# Konfigurace

Knihovna čte nastavení ze sekce `EToroOptions` nebo přes `Action<EToroOptions>`.

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

## Konfigurace v kódu

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

## Volby

| Volba | Výchozí hodnota | Popis |
| --- | --- | --- |
| `ApiKey` | prázdné | eToro API key. Povinné. |
| `UserKey` | prázdné | eToro user key. Povinné. |
| `Environment` | `real` | Výchozí environment segment pro interní helpery. Metody endpointů obvykle přijímají `EToroEnvironment` explicitně. |
| `BaseAddress` | `https://public-api.etoro.com/api/v1/` | Základní URL API. Musí být HTTPS, absolutní a končit `/`. |
| `AllowCustomBaseAddress` | `false` | Povolí vlastní host pro testy nebo proxy. |
| `Timeout` | `00:01:40` | HTTP timeout. Musí být větší než nula a nejvýše pět minut. |
| `UserAgent` | `EToro` | User agent odesílaný přes `HttpClient`. Povinné. |
| `RateLimit` | zapnuto | Rate-limit a retry chování. |
| `ErrorHandling` | bezpečná diagnostika | Zachycení, redakce a oříznutí error response body. |

## Validace

Nastavení se validuje při startu aplikace. Chybějící credentials, špatná base adresa, chybějící user agent, neplatný timeout nebo neplatný rate-limit způsobí rychlé selhání při startu.
`ErrorHandling.MaxResponseBodyLength` musí být `null`, nula nebo větší hodnota.

## Multi-user aplikace

Pro jeden eToro user key stačí výchozí registrace:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

Pokud jedna instance aplikace obsluhuje více eToro user keys, zapněte keyed limiter:

```csharp
builder.Services.AddEToro(builder.Configuration, useKeyedRateLimiter: true);
```

Keyed limiter drží samostatné rate-limit okno pro každý user key.
