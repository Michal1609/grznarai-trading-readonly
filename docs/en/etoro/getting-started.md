# Getting Started

## âš ď¸Ź Disclaimer

This project is **not affiliated with, endorsed by, or connected to eToro in any way**.

This software is provided for **informational and educational purposes only**.

- It does **not provide financial advice**.
- It should **not be used as the sole basis for trading decisions**.
- Data retrieved may be **inaccurate, delayed, or incomplete**.
- Currently the library exposes **read-only** endpoints. Mutating endpoints
  (POST / PUT / DELETE â€” order placement, watchlist edits, etc.) will be added
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

## Requirements

- .NET 9 SDK or newer.
- eToro API key and user key.
- An application using dependency injection and `IHttpClientFactory`.

## Install

```powershell
dotnet add package GrznarAi.Trading.ReadOnly.Etoro
```

## Add Configuration

Store credentials outside source control:

```powershell
dotnet user-secrets set "EToroOptions:ApiKey" "<api-key>"
dotnet user-secrets set "EToroOptions:UserKey" "<user-key>"
```

Or use environment variables:

```powershell
$env:EToroOptions__ApiKey="<api-key>"
$env:EToroOptions__UserKey="<user-key>"
```

## Register Services

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEToro(builder.Configuration);
```

The registration adds:

- `IEToroClient` as a typed `HttpClient`.
- Authentication headers for `x-api-key`, `x-user-key`, and `x-request-id`.
- Rate-limit and retry handling.
- `IEToroCalculationService`.

## Call the API

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Portfolio;

public sealed class PortfolioService(IEToroClient client)
{
    public Task<PortfolioResponse> GetRealPortfolioAsync(CancellationToken ct)
    {
        return client.GetPortfolioAsync(EToroEnvironment.Real, ct);
    }
}
```

Prefer direct async calls in your own services:

```csharp
var pnl = await client.GetPnlAsync(EToroEnvironment.Real, ct);
var rates = await client.GetRatesAsync([100000], ct);
var history = await client.GetTradeHistoryAsync(new DateOnly(2024, 1, 1), page: 0, pageSize: 100, ct);
```

## Demo vs Real Account

Endpoints that need an account environment accept `EToroEnvironment.Real` or `EToroEnvironment.Demo`.

```csharp
var realPnl = await client.GetPnlAsync(EToroEnvironment.Real, ct);
var demoPnl = await client.GetPnlAsync(EToroEnvironment.Demo, ct);
```
