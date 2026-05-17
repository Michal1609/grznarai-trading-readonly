# GrznarAi Trading Library

Typed .NET clients for trading platform APIs. The repository is split into a shared Core package and per-platform packages for eToro and Coinbase.

Target framework: `.NET 9`  
License: MIT

## Packages

| Package | Install | What is inside |
|---|---|---|
| `GrznarAi.Trading.ReadOnly` | `dotnet add package GrznarAi.Trading.ReadOnly` | Shared infrastructure: HTTP handlers, rate limiting, resilience, diagnostics, JSON helpers, base exception types. Usually installed transitively. |
| `GrznarAi.Trading.ReadOnly.Etoro` | `dotnet add package GrznarAi.Trading.ReadOnly.Etoro` | Read-only typed client for the eToro public API. |
| `GrznarAi.Trading.ReadOnly.Coinbase` | `dotnet add package GrznarAi.Trading.ReadOnly.Coinbase` | Typed client for Coinbase Advanced Trade API. **Covers all documented GET endpoints. Strictly read-only — no write/trade methods are exposed.** |

Starting with `1.0.0-alpha.3`, `GrznarAi.Trading.ReadOnly` is Core only. Existing eToro consumers should install `GrznarAi.Trading.ReadOnly.Etoro`.

## Disclaimer

This project is not affiliated with, endorsed by, or connected to eToro or Coinbase.

This software is provided for informational and educational purposes only. It does not provide financial advice, data may be inaccurate or delayed, and you are responsible for your trading decisions, API usage, and compliance with applicable terms and laws.

## eToro Quick Start

```powershell
dotnet add package GrznarAi.Trading.ReadOnly.Etoro
dotnet user-secrets set "EToroOptions:ApiKey" "<api-key>"
dotnet user-secrets set "EToroOptions:UserKey" "<user-key>"
```

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEToro(builder.Configuration);
```

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;

app.MapGet("/portfolio", async (IEToroClient client, CancellationToken ct) =>
{
    var portfolio = await client.GetPortfolioAsync(EToroEnvironment.Real, ct);
    return Results.Ok(portfolio);
});
```

## Coinbase Quick Start

```powershell
dotnet add package GrznarAi.Trading.ReadOnly.Coinbase
dotnet user-secrets set "Coinbase:KeyName" "organizations/{org}/apiKeys/{id}"
dotnet user-secrets set "Coinbase:PrivateKeyPem" "-----BEGIN EC PRIVATE KEY-----\n...\n-----END EC PRIVATE KEY-----\n"
```

```csharp
using GrznarAi.Trading.ReadOnly.Coinbase;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCoinbaseClient(builder.Configuration);
```

```csharp
app.MapGet("/accounts", async (ICoinbaseAccountsClient client, CancellationToken ct) =>
{
    var accounts = await client.ListAccountsAsync(ct);
    return Results.Ok(accounts);
});
```

## Diagnostics

Diagnostics are opt-in and shared through Core. When enabled, the HTTP pipeline captures request/response snapshots into `IApiDiagnostics`.

```csharp
builder.Services.AddEToro(builder.Configuration);
// appsettings.json:
// "EToroOptions": { "Diagnostics": { "Enabled": true } }

app.MapGet("/debug-last-api-call", (GrznarAi.Trading.ReadOnly.Diagnostics.IApiDiagnostics diagnostics) =>
    diagnostics.Last);
```

Snapshots include method, URL, elapsed time, status, headers, response body, truncation flag, and redacted sensitive headers. See the Core diagnostics guide for details.

## Examples

Runnable console apps in the `examples/` directory:

**eToro**
- `Demo01.PortfolioHealthCheck` — fetch portfolio and print a health summary
- `Demo02.PortfolioConcentrationAnalyzer` — detect concentration risk across positions
- `Demo03.WhatIfSimulator` — simulate portfolio changes before executing
- `Demo04.PortfolioAllocation` — break down allocation by symbol, asset class, and industry

**Coinbase**
- `Demo01.PortfolioBreakdown` — list accounts and print balances

Each demo reads credentials from `dotnet user-secrets`. See the example's `Program.cs` for setup instructions.

> There is no dedicated example app per Coinbase endpoint. The most complete usage samples are the unit tests under `tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Client/` (one file per domain client) and the credential-driven integration tests under `tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Integration/`. The full method catalogue is listed in [docs/en/coinbase/api-reference.md](docs/en/coinbase/api-reference.md).

## Documentation

English:

- [Core](docs/en/core/index.md)
- [Core diagnostics](docs/en/core/diagnostics.md)
- [eToro](docs/en/etoro/index.md)
- [Coinbase](docs/en/coinbase/index.md)

Cesky:

- [Core](docs/cs/core/index.md)
- [Core diagnostika](docs/cs/core/diagnostics.md)
- [eToro](docs/cs/etoro/index.md)
- [Coinbase](docs/cs/coinbase/index.md)

## Development

```powershell
dotnet restore
dotnet build
dotnet test
```

Native AOT smoke tests live in `tests/*Aot.SmokeTest`. CI publishes Core, eToro, and Coinbase smoke projects and packs all three NuGet packages.

## License

MIT License. Copyright (c) 2026 Michal Grznar.
