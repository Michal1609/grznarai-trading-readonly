# Error handling

## `CoinbaseApiException`

Thrown for any non-success HTTP response (anything outside `2xx`).

| Property | Description |
| --- | --- |
| `StatusCode` | Response `HttpStatusCode`. |
| `Endpoint` | Sanitised path (UUIDs replaced by placeholders). |
| `ResponseBody` | Response body (optional, redacted, truncated). |
| `RequestId` | `x-request-id` or `cb-trace-id`. |
| `RetryAfter` | Parsed from the `Retry-After` header. |

## Path sanitisation

Path UUIDs are replaced with a placeholder to avoid leaking identifiers in logs:

- `/api/v3/brokerage/portfolios/abc-123` → `/api/v3/brokerage/portfolios/{uuid}`
- `/api/v3/brokerage/accounts/xyz` → `/api/v3/brokerage/accounts/{uuid}`

## Body redaction

When `RedactResponseBody = true`:

- JSON fields `apiKey`, `privateKey`, `token`, `accessToken`, `refreshToken`, `authorization`, `secret`, `password` → `"[REDACTED]"`.
- `Bearer <token>` → `Bearer [REDACTED]`.
- JWT tokens (3-segment `eyJ...`) → `[REDACTED_JWT]`.

## Example

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
