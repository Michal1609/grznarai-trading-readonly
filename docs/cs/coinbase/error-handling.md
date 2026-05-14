# Error handling

## `CoinbaseApiException`

Hází se z klienta na non-success HTTP odpověď (cokoliv mimo `2xx`).

| Property | Popis |
| --- | --- |
| `StatusCode` | `HttpStatusCode` odpovědi. |
| `Endpoint` | Sanitovaná cesta (UUID nahrazena placeholderem). |
| `ResponseBody` | Tělo odpovědi (volitelné, redigované, oříznuté). |
| `RequestId` | `x-request-id` nebo `cb-trace-id`. |
| `RetryAfter` | Parsováno z hlavičky `Retry-After`. |

## Sanitace cesty

Path UUID se nahrazují placeholderem aby se v logu nešířily citlivé identifikátory:

- `/api/v3/brokerage/portfolios/abc-123` → `/api/v3/brokerage/portfolios/{uuid}`
- `/api/v3/brokerage/accounts/xyz` → `/api/v3/brokerage/accounts/{uuid}`

## Redakce těla

Pokud je `RedactResponseBody = true`:

- JSON pole `apiKey`, `privateKey`, `token`, `accessToken`, `refreshToken`, `authorization`, `secret`, `password` → `"[REDACTED]"`
- `Bearer <token>` → `Bearer [REDACTED]`
- JWT tokeny (3 segmenty `eyJ...`) → `[REDACTED_JWT]`

## Příklad

```csharp
try
{
    var breakdown = await client.GetPortfolioBreakdownAsync("missing-uuid");
}
catch (CoinbaseApiException ex)
{
    logger.LogError("Coinbase {Status} on {Endpoint}. RequestId={Id}. Body={Body}",
        ex.StatusCode, ex.Endpoint, ex.RequestId, ex.ResponseBody);
}
```
