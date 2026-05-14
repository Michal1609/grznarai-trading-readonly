# ZaÄŤĂ­nĂˇme

## âš ď¸Ź UpozornÄ›nĂ­

Tento projekt **nenĂ­ nijak spojen s eToro, nenĂ­ jĂ­m podporovĂˇn ani schvĂˇlen**.

Software je poskytovĂˇn **vĂ˝hradnÄ› pro informaÄŤnĂ­ a vzdÄ›lĂˇvacĂ­ ĂşÄŤely**.

- **Neposkytuje finanÄŤnĂ­ poradenstvĂ­.**
- **NesmĂ­ bĂ˝t jedinĂ˝m podkladem pro obchodnĂ­ rozhodnutĂ­.**
- ZĂ­skanĂˇ data mohou bĂ˝t **nepĹ™esnĂˇ, zpoĹľdÄ›nĂˇ nebo neĂşplnĂˇ**.
- Knihovna v souÄŤasnosti poskytuje pouze **read-only** koncovĂ© body.
  ZĂˇpisovĂ© operace (POST/PUT/DELETE) pĹ™ijdou pozdÄ›ji; jejich pouĹľitĂ­ je
  na vlastnĂ­ riziko uĹľivatele.

Autor a pĹ™ispÄ›vatelĂ© **odmĂ­tajĂ­ veĹˇkerou odpovÄ›dnost** za jakĂ©koli pĹ™Ă­mĂ©,
nepĹ™Ă­mĂ©, vedlejĹˇĂ­, nĂˇslednĂ© nebo sankÄŤnĂ­ Ĺˇkody vzniklĂ© v souvislosti
s pouĹľĂ­vĂˇnĂ­m tohoto softwaru, zejmĂ©na za finanÄŤnĂ­ ztrĂˇty, propasenĂ© obchody,
nesprĂˇvnĂ© vĂ˝poÄŤty, vĂ˝padky, ztrĂˇtu dat ÄŤi poruĹˇenĂ­ podmĂ­nek eToro.
Software pouĹľĂ­vejte vĂ˝hradnÄ› na vlastnĂ­ nebezpeÄŤĂ­.

## PoĹľadavky

- .NET 9 SDK nebo novÄ›jĹˇĂ­.
- eToro API key a user key.
- Aplikace pouĹľĂ­vajĂ­cĂ­ dependency injection a `IHttpClientFactory`.

## Instalace

```powershell
dotnet add package GrznarAi.Trading.ReadOnly.Etoro
```

## Konfigurace pĹ™Ă­stupĹŻ

PĹ™ihlaĹˇovacĂ­ Ăşdaje uklĂˇdejte mimo source control:

```powershell
dotnet user-secrets set "EToroOptions:ApiKey" "<api-key>"
dotnet user-secrets set "EToroOptions:UserKey" "<user-key>"
```

Nebo pouĹľijte environment promÄ›nnĂ©:

```powershell
$env:EToroOptions__ApiKey="<api-key>"
$env:EToroOptions__UserKey="<user-key>"
```

## Registrace sluĹľeb

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEToro(builder.Configuration);
```

Registrace pĹ™idĂˇ:

- `IEToroClient` jako typed `HttpClient`.
- Auth hlaviÄŤky `x-api-key`, `x-user-key` a `x-request-id`.
- Rate-limit a retry handling.
- `IEToroCalculationService`.

## VolĂˇnĂ­ API

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;

var portfolio = await client.GetPortfolioAsync(EToroEnvironment.Real, ct);
var pnl = await client.GetPnlAsync(EToroEnvironment.Real, ct);
var rates = await client.GetRatesAsync([100000], ct);
```

Historie obchodĹŻ:

```csharp
var history = await client.GetTradeHistoryAsync(
    minDate: new DateOnly(2024, 1, 1),
    page: 0,
    pageSize: 100,
    ct);
```

## Demo vs Real ĂşÄŤet

Endpointy, kterĂ© pracujĂ­ s ĂşÄŤtem, pĹ™ijĂ­majĂ­ `EToroEnvironment.Real` nebo `EToroEnvironment.Demo`.

```csharp
var realPnl = await client.GetPnlAsync(EToroEnvironment.Real, ct);
var demoPnl = await client.GetPnlAsync(EToroEnvironment.Demo, ct);
```
