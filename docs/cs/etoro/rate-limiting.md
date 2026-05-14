# Rate Limiting a Retry

Knihovna obsahuje proaktivní rate limiting a automatické retry pro HTTP `429 Too Many Requests`.

## Výchozí nastavení

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

## Proaktivní limitování

GET requesty jsou limitované před odesláním. Výchozí limit je `60` requestů za minutu.

Pro jeden user key stačí výchozí singleton limiter:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

Pro více user keys v jednom procesu použijte keyed limiter:

```csharp
builder.Services.AddEToro(builder.Configuration, useKeyedRateLimiter: true);
```

## Retry chování

Když API vrátí `429`, handler:

- Respektuje `Retry-After` hlavičku, pokud existuje.
- Použije exponential backoff, pokud `Retry-After` není k dispozici.
- Omezí čekání pomocí `MaxRetryDelay`.
- Přidá jitter podle `RetryJitterRatio`.
- Ve výchozím stavu opakuje `GET` a `HEAD`.
- Respektuje `CancellationToken`.

`MaxRetries = 0` vypne automatické retry, ale původní request se pořád odešle jednou.

## Non-idempotent requesty

`POST`, `PATCH` a `DELETE` se ve výchozím stavu neopakují. `RetryNonIdempotentRequests` zapínejte jen tehdy, když je duplicitní provedení pro daný endpoint bezpečné.

```json
{
  "EToroOptions": {
    "RateLimit": {
      "RetryNonIdempotentRequests": false
    }
  }
}
```
