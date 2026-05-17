# Documentation

English documentation for the Coinbase Advanced Trade .NET client.

## âš ď¸Ź Disclaimer

This project is **not affiliated with, endorsed by, or connected to Coinbase**.

The software is provided **for informational and educational purposes only**.

- **Not financial advice.**
- **Must not be the sole basis for trading decisions.**
- Data returned may be **inaccurate, delayed, or incomplete**.
- The library exposes **read-only** endpoints only. **All documented Coinbase Advanced Trade GET endpoints are implemented.** No write/trade operations (place order, cancel, convert commit, etc.) are exposed and there is no roadmap to add them — this package is read-only by design.

The author and contributors **disclaim all liability** for any direct, indirect, incidental,
consequential, or punitive damages arising from the use of this software, including
financial losses, missed trades, incorrect calculations, outages, data loss, or violations
of Coinbase terms. Use the software entirely at your own risk.

- [Getting started](getting-started.md)
- [Configuration](configuration.md)
- [API reference](api-reference.md)
- [Error handling](error-handling.md)
- [Rate limiting & retry](rate-limiting.md)
- [Testing & contributing](testing-and-contributing.md)
- [CI/CD pipelines](ci-cd.md)

Primary package: `GrznarAi.Trading.ReadOnly.Coinbase`

Primary services:

- `ICoinbaseClient` (facade combining all domain interfaces)
- `ICoinbaseAccountsClient`
- `ICoinbasePortfoliosClient`
- `ICoinbaseOrdersClient`
- `ICoinbaseProductsClient`
- `ICoinbasePublicClient`
- `ICoinbaseFeesClient`
- `ICoinbaseFuturesClient`
- `ICoinbasePerpetualClient`
- `ICoinbasePaymentMethodsClient`
- `ICoinbaseConvertClient`
- `ICoinbaseDataApiClient`

See the [API reference](api-reference.md) for the full method catalogue. There is no separate examples app per endpoint — the most exhaustive usage samples are the unit and integration tests in `tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/`.

Registration:

```csharp
builder.Services.AddCoinbaseClient(builder.Configuration);
```
