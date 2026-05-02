# Documentation

English documentation for the eToro .NET client.

## ⚠️ Disclaimer

This project is **not affiliated with, endorsed by, or connected to eToro in any way**.

This software is provided for **informational and educational purposes only**.

- It does **not provide financial advice**.
- It should **not be used as the sole basis for trading decisions**.
- Data retrieved may be **inaccurate, delayed, or incomplete**.
- Currently the library exposes **read-only** endpoints. Mutating endpoints
  (POST / PUT / DELETE — order placement, watchlist edits, etc.) will be added
  later; consumers using such operations do so at their own risk.

The author and contributors **disclaim all liability** for any direct,
indirect, incidental, consequential, or punitive damages arising from the use
of this software, including but not limited to financial loss, missed trades,
incorrect calculations, downtime, data loss, or breach of eToro's terms of
service. Use entirely at your own risk.

By using this library you acknowledge that you understand the risks of
algorithmic and API-driven trading and that you alone are responsible for
your trading decisions and compliance with applicable laws and eToro's API
terms of use.

- [Getting started](getting-started.md)
- [Configuration](configuration.md)
- [API reference](api-reference.md)
- [Error handling](error-handling.md)
- [Rate limiting and retries](rate-limiting.md)
- [Account calculations](account-calculations.md)
- [Testing and contributing](testing-and-contributing.md)
- [CI/CD pipelines](ci-cd.md)

Main package: `GrznarAi.Trading.ReadOnly`

Main services:

- `IEToroClient`
- `IEToroCalculationService`

Main registration method:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

## Future Write Endpoints

Before POST, PUT, or DELETE endpoints are added to the public surface:

- Idempotency keys will be required per write request.
- `RetryNonIdempotentRequests` will stay `false` by default; write retries must be explicit.
- Write operations will use per-method rate limits, separate from read request limits.
- Calculation helpers will not place orders automatically from heuristics.
- A dedicated order client interface will stay separate from read interfaces.
- Write failures will use distinct exception types carrying broker-side error codes.
- Every write attempt will expose an audit-log hook with redacted payload data.
