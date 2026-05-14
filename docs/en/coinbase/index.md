# Documentation

English documentation for the Coinbase Advanced Trade .NET client.

## âš ď¸Ź Disclaimer

This project is **not affiliated with, endorsed by, or connected to Coinbase**.

The software is provided **for informational and educational purposes only**.

- **Not financial advice.**
- **Must not be the sole basis for trading decisions.**
- Data returned may be **inaccurate, delayed, or incomplete**.
- The library currently exposes **read-only** endpoints only.
  Write operations (POST/PUT/DELETE) may come later; their use is at the user's risk.

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

- `ICoinbaseClient` (facade combining domain interfaces)
- `ICoinbaseAccountsClient`
- `ICoinbasePortfoliosClient`

Registration:

```csharp
builder.Services.AddCoinbaseClient(builder.Configuration);
```
