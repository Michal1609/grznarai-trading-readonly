# Error Handling

Klient vyhazuje `EToroApiException` pro neúspěšné HTTP odpovědi.

## Data ve výjimce

`EToroApiException` obsahuje:

| Property | Popis |
| --- | --- |
| `StatusCode` | HTTP status code vrácený eToro API. |
| `Endpoint` | Sanitizovaná cesta endpointu včetně query stringu. |
| `ResponseBody` | Redaktované a oříznuté response body, nebo `null`, pokud body není dostupné. |
| `RequestId` | Request ID z response headers nebo z vygenerované request hlavičky, pokud je dostupné. |
| `RetryAfter` | Parsovaná hodnota `Retry-After`, pokud ji API pošle. |

Příklad:

```csharp
try
{
    var portfolio = await client.GetPortfolioAsync(EToroEnvironment.Real, ct);
}
catch (EToroApiException ex)
{
    logger.LogWarning(
        ex,
        "eToro API failed with {StatusCode} at {Endpoint}. RequestId: {RequestId}. RetryAfter: {RetryAfter}",
        ex.StatusCode,
        ex.Endpoint,
        ex.RequestId,
        ex.RetryAfter);
}
```

## Redakce response body

`ResponseBody` záměrně není raw response body.

Před vyhozením výjimky knihovna:

- Ořízne whitespace na začátku a konci response body.
- Redaktuje běžné secret fieldy jako `apiKey`, `userKey`, `token`, `access_token`, `refresh_token`, `authorization`, `secret`, `password`, `x-api-key` a `x-user-key`.
- Finální hodnotu ořízne na 4096 znaků.

Diagnostika tak zůstává použitelná, ale snižuje se riziko, že se do logů zapíšou credentials nebo příliš velké payloady.

## Konfigurace

Zpracování response body je konfigurovatelné přes `EToroOptions.ErrorHandling`.

```json
{
  "EToroOptions": {
    "ErrorHandling": {
      "IncludeResponseBody": true,
      "RedactResponseBody": true,
      "MaxResponseBodyLength": 4096
    }
  }
}
```

| Volba | Výchozí hodnota | Popis |
| --- | --- | --- |
| `IncludeResponseBody` | `true` | Uloží response body do `EToroApiException.ResponseBody`. Nastavte `false`, pokud nechcete body vůbec zachytávat. |
| `RedactResponseBody` | `true` | Před uložením body redaktuje běžné secret fieldy. |
| `MaxResponseBodyLength` | `4096` | Maximální délka uloženého body. Nastavte `null` pro žádné oříznutí nebo `0` pro prázdný string. |

Pro lokální debug můžete dočasně ponechat celé raw body:

```json
{
  "EToroOptions": {
    "ErrorHandling": {
      "IncludeResponseBody": true,
      "RedactResponseBody": false,
      "MaxResponseBodyLength": null
    }
  }
}
```

Raw body používejte jen v kontrolovaném lokálním debugování. Raw eToro error body nelogujte v produkci bez kontroly na credentials a osobní údaje.

## Prázdné nebo nevalidní úspěšné odpovědi

Pokud API vrátí úspěšný status code, ale body je prázdné nebo nejde deserializovat do očekávaného typu, klient vyhodí `InvalidOperationException`.

Validační chyby vstupů, například neplatná ID, page sizes nebo search fields, vyhazují standardní .NET výjimky jako `ArgumentException` nebo `ArgumentOutOfRangeException` ještě před odesláním requestu.
