# GrznarAi Trading ReadOnly

Read-only typed .NET client for trading platform public APIs. Currently targets the eToro public API. This project is not affiliated with eToro.

**Package:** `GrznarAi.Trading.ReadOnly` | **Namespace:** `GrznarAi.Trading.ReadOnly` | **Framework:** `.NET 9` | **License:** MIT

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

## Installation

```powershell
dotnet add package GrznarAi.Trading.ReadOnly
```

## Quick Start

```powershell
dotnet user-secrets set "EToroOptions:ApiKey" "<api-key>"
dotnet user-secrets set "EToroOptions:UserKey" "<user-key>"
```

```csharp
using GrznarAi.Trading.ReadOnly.Client;

builder.Services.AddEToro(builder.Configuration);
```

```csharp
app.MapGet("/portfolio", async (IEToroClient client, CancellationToken ct) =>
    Results.Ok(await client.GetPortfolioAsync(EToroEnvironment.Real, ct)));
```

## Configuration

```json
{
  "EToroOptions": {
    "ApiKey": "replace-me",
    "UserKey": "replace-me",
    "Environment": "real",
    "RateLimit": { "Enabled": true, "PermitLimit": 60, "Window": "00:01:00" }
  }
}
```

## API Areas

- **Trading:** PnL, portfolio, trade history, orders, agent portfolios.
- **Market data:** exchanges, instruments, rates, candles, stock industries.
- **User info:** identity, profiles, gain history, live portfolio.
- **Social:** popular investors.
- **Feed:** instrument and user feed posts.
- **Watchlists:** own, default, public, recommendations.
- **Calculations:** available cash, invested principal, unrealized/realized PnL, equity, total return.

## Future Write Endpoints

Before POST, PUT, or DELETE endpoints are added to the public surface:

- Idempotency keys will be required per write request.
- `RetryNonIdempotentRequests` will stay `false` by default; write retries must be explicit.
- Write operations will use per-method rate limits, separate from read request limits.
- Calculation helpers will not place orders automatically from heuristics.
- A dedicated order client interface will stay separate from read interfaces.
- Write failures will use distinct exception types carrying broker-side error codes.
- Every write attempt will expose an audit-log hook with redacted payload data.

## Documentation

Full documentation and source: https://github.com/Michal1609/grznarai-trading-readonly

- [Getting started](https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/getting-started.md)
- [Configuration](https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/configuration.md)
- [API reference](https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/api-reference.md)
- [Error handling](https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/error-handling.md)
- [Rate limiting and retries](https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/rate-limiting.md)
- [Account calculations](https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/account-calculations.md)
- [Testing and contributing](https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/testing-and-contributing.md)

## License

MIT — Copyright (c) 2026 Michal Grznár.
