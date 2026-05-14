# Getting started

## Install

```xml
<PackageReference Include="GrznarAi.Trading.ReadOnly.Coinbase" Version="1.0.0-alpha.3" />
```

## `appsettings.json`

```json
{
  "Coinbase": {
    "KeyName": "organizations/{org_id}/apiKeys/{key_id}",
    "PrivateKeyPem": "-----BEGIN EC PRIVATE KEY-----\n...\n-----END EC PRIVATE KEY-----\n",
    "BaseUrl": "https://api.coinbase.com",
    "JwtLifetimeSeconds": 120
  }
}
```

Create a CDP key in the [Coinbase Developer Platform](https://www.coinbase.com/developer-platform). Pick an EC-256 (ECDSA) key.

## DI registration

```csharp
using GrznarAi.Trading.ReadOnly.Coinbase;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCoinbaseClient(builder.Configuration);
```

## First call

```csharp
var client = host.Services.GetRequiredService<ICoinbaseClient>();

var portfolios = await client.ListPortfoliosAsync();
var first = portfolios.Portfolios?.FirstOrDefault();

if (first?.Uuid is not null)
{
    var breakdown = await client.GetPortfolioBreakdownAsync(first.Uuid, currency: "USD");
    Console.WriteLine($"Total: {breakdown.Breakdown?.PortfolioBalances?.TotalBalance?.Value}");
}
```

## Domain interfaces

The client implements `ICoinbaseClient` (facade). For a smaller surface, inject a specific domain interface:

- `ICoinbaseAccountsClient`
- `ICoinbasePortfoliosClient`
