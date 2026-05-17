# GrznarAi Trading ReadOnly

Typed .NET clients for trading platform APIs.

This package family is split into:

| Package | Purpose |
|---|---|
| `GrznarAi.Trading.ReadOnly` | Shared Core infrastructure. |
| `GrznarAi.Trading.ReadOnly.Etoro` | eToro public API client. |
| `GrznarAi.Trading.ReadOnly.Coinbase` | Coinbase Advanced Trade API client. Covers all documented GET endpoints; strictly read-only. |

Starting with `1.0.0-alpha.3`, `GrznarAi.Trading.ReadOnly` no longer contains the eToro client. Install `GrznarAi.Trading.ReadOnly.Etoro` for eToro usage.

## Install

```powershell
dotnet add package GrznarAi.Trading.ReadOnly.Etoro
dotnet add package GrznarAi.Trading.ReadOnly.Coinbase
```

Install `GrznarAi.Trading.ReadOnly` directly only when you need the shared Core types without a platform client.

## eToro

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Client;

builder.Services.AddEToro(builder.Configuration);
```

Configuration section: `EToroOptions`.

## Coinbase

```csharp
using GrznarAi.Trading.ReadOnly.Coinbase;

builder.Services.AddCoinbaseClient(builder.Configuration);
```

Configuration section: `Coinbase`.

The Coinbase client implements every documented GET endpoint across Accounts, Portfolios, Orders, Products, Public market data, Fees, Futures, Perpetuals, Payment Methods, Convert, and Data API. No write/trade operations are exposed — the client is read-only by design.

Usage samples per endpoint live in the test suite (`tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Client/*` and `.../Integration/*`). The full method catalogue is in the Coinbase API reference doc linked below.

## Diagnostics

Enable diagnostics in platform options to capture request/response snapshots through Core:

```json
{
  "EToroOptions": {
    "Diagnostics": {
      "Enabled": true,
      "CaptureResponseBody": true
    }
  }
}
```

Inject `GrznarAi.Trading.ReadOnly.Diagnostics.IApiDiagnostics` to inspect the latest API call or recent history.

## Documentation

Full documentation and source: https://github.com/Michal1609/grznarai-trading-readonly

- Core: https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/core/index.md
- eToro: https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/etoro/index.md
- Coinbase: https://github.com/Michal1609/grznarai-trading-readonly/blob/main/docs/en/coinbase/index.md

## Disclaimer

This project is not affiliated with, endorsed by, or connected to eToro or Coinbase. It does not provide financial advice. Use at your own risk.

## License

MIT - Copyright (c) 2026 Michal Grznar.
