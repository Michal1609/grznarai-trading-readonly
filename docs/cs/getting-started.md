# Začínáme

## ⚠️ Upozornění

Tento projekt **není nijak spojen s eToro, není jím podporován ani schválen**.

Software je poskytován **výhradně pro informační a vzdělávací účely**.

- **Neposkytuje finanční poradenství.**
- **Nesmí být jediným podkladem pro obchodní rozhodnutí.**
- Získaná data mohou být **nepřesná, zpožděná nebo neúplná**.
- Knihovna v současnosti poskytuje pouze **read-only** koncové body.
  Zápisové operace (POST/PUT/DELETE) přijdou později; jejich použití je
  na vlastní riziko uživatele.

Autor a přispěvatelé **odmítají veškerou odpovědnost** za jakékoli přímé,
nepřímé, vedlejší, následné nebo sankční škody vzniklé v souvislosti
s používáním tohoto softwaru, zejména za finanční ztráty, propasené obchody,
nesprávné výpočty, výpadky, ztrátu dat či porušení podmínek eToro.
Software používejte výhradně na vlastní nebezpečí.

## Požadavky

- .NET 9 SDK nebo novější.
- eToro API key a user key.
- Aplikace používající dependency injection a `IHttpClientFactory`.

## Instalace

```powershell
dotnet add package GrznarAi.Trading.ReadOnly
```

## Konfigurace přístupů

Přihlašovací údaje ukládejte mimo source control:

```powershell
dotnet user-secrets set "EToroOptions:ApiKey" "<api-key>"
dotnet user-secrets set "EToroOptions:UserKey" "<user-key>"
```

Nebo použijte environment proměnné:

```powershell
$env:EToroOptions__ApiKey="<api-key>"
$env:EToroOptions__UserKey="<user-key>"
```

## Registrace služeb

```csharp
using GrznarAi.Trading.ReadOnly.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEToro(builder.Configuration);
```

Registrace přidá:

- `IEToroClient` jako typed `HttpClient`.
- Auth hlavičky `x-api-key`, `x-user-key` a `x-request-id`.
- Rate-limit a retry handling.
- `IEToroCalculationService`.

## Volání API

```csharp
using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Common;

var portfolio = await client.GetPortfolioAsync(EToroEnvironment.Real, ct);
var pnl = await client.GetPnlAsync(EToroEnvironment.Real, ct);
var rates = await client.GetRatesAsync([100000], ct);
```

Historie obchodů:

```csharp
var history = await client.GetTradeHistoryAsync(
    minDate: new DateOnly(2024, 1, 1),
    page: 0,
    pageSize: 100,
    ct);
```

## Demo vs Real účet

Endpointy, které pracují s účtem, přijímají `EToroEnvironment.Real` nebo `EToroEnvironment.Demo`.

```csharp
var realPnl = await client.GetPnlAsync(EToroEnvironment.Real, ct);
var demoPnl = await client.GetPnlAsync(EToroEnvironment.Demo, ct);
```
