# ZaÄŤĂ­nĂˇme

## Instalace

```xml
<PackageReference Include="GrznarAi.Trading.ReadOnly.Coinbase" Version="1.0.0-alpha.3" />
```

## Konfigurace v `appsettings.json`

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

CDP klĂ­ÄŤ zĂ­skĂˇte v [Coinbase Developer Platform](https://www.coinbase.com/developer-platform). Vyberte EC-256 (ECDSA) klĂ­ÄŤ.

## DI registrace

```csharp
using GrznarAi.Trading.ReadOnly.Coinbase;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCoinbaseClient(builder.Configuration);
```

## PrvnĂ­ volĂˇnĂ­

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

## DomĂ©novĂˇ rozhranĂ­

Klient implementuje `ICoinbaseClient` (facade). Pokud potĹ™ebujete menĹˇĂ­ surface, injectnÄ›te konkrĂ©tnĂ­ domĂ©novĂ© rozhranĂ­:

- `ICoinbaseAccountsClient`
- `ICoinbasePortfoliosClient`
