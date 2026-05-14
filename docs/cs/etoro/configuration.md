# Konfigurace

Knihovna ÄŤte nastavenĂ­ ze sekce `EToroOptions` nebo pĹ™es `Action<EToroOptions>`.

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

## Konfigurace v kĂłdu

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Client;

builder.Services.AddEToro(options =>
{
    options.ApiKey = builder.Configuration["EToroOptions:ApiKey"]!;
    options.UserKey = builder.Configuration["EToroOptions:UserKey"]!;
    options.UserAgent = "MyApp";
    options.RateLimit.Enabled = true;
});
```

## Volby

| Volba | VĂ˝chozĂ­ hodnota | Popis |
| --- | --- | --- |
| `ApiKey` | prĂˇzdnĂ© | eToro API key. PovinnĂ©. |
| `UserKey` | prĂˇzdnĂ© | eToro user key. PovinnĂ©. |
| `Environment` | `real` | VĂ˝chozĂ­ environment segment pro internĂ­ helpery. Metody endpointĹŻ obvykle pĹ™ijĂ­majĂ­ `EToroEnvironment` explicitnÄ›. |
| `BaseAddress` | `https://public-api.etoro.com/api/v1/` | ZĂˇkladnĂ­ URL API. MusĂ­ bĂ˝t HTTPS, absolutnĂ­ a konÄŤit `/`. |
| `AllowCustomBaseAddress` | `false` | PovolĂ­ vlastnĂ­ host pro testy nebo proxy. |
| `Timeout` | `00:01:40` | HTTP timeout. MusĂ­ bĂ˝t vÄ›tĹˇĂ­ neĹľ nula a nejvĂ˝Ĺˇe pÄ›t minut. |
| `UserAgent` | `EToro` | User agent odesĂ­lanĂ˝ pĹ™es `HttpClient`. PovinnĂ©. |
| `RateLimit` | zapnuto | Rate-limit a retry chovĂˇnĂ­. |
| `ErrorHandling` | bezpeÄŤnĂˇ diagnostika | ZachycenĂ­, redakce a oĹ™Ă­znutĂ­ error response body. |

## Validace

NastavenĂ­ se validuje pĹ™i startu aplikace. ChybÄ›jĂ­cĂ­ credentials, ĹˇpatnĂˇ base adresa, chybÄ›jĂ­cĂ­ user agent, neplatnĂ˝ timeout nebo neplatnĂ˝ rate-limit zpĹŻsobĂ­ rychlĂ© selhĂˇnĂ­ pĹ™i startu.
`ErrorHandling.MaxResponseBodyLength` musĂ­ bĂ˝t `null`, nula nebo vÄ›tĹˇĂ­ hodnota.

## Multi-user aplikace

Pro jeden eToro user key staÄŤĂ­ vĂ˝chozĂ­ registrace:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

Pokud jedna instance aplikace obsluhuje vĂ­ce eToro user keys, zapnÄ›te keyed limiter:

```csharp
builder.Services.AddEToro(builder.Configuration, useKeyedRateLimiter: true);
```

Keyed limiter drĹľĂ­ samostatnĂ© rate-limit okno pro kaĹľdĂ˝ user key.
