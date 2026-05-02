# Výpočty účtu

`IEToroCalculationService` poskytuje pomocné výpočty nad PnL, portfoliem a historií obchodů.

## Registrace

Služba se registruje automaticky přes:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

## Použití

```csharp
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Services;

var metrics = await calculations.GetAccountMetricsAsync(
    EToroEnvironment.Real,
    fromDate: new DateOnly(2024, 1, 1),
    pageSize: 100,
    maxPages: 50,
    ct);
```

## Metriky

| Metrika | Význam |
| --- | --- |
| `AvailableCash` | Credit minus manuální pending orders a MIT orders. |
| `InvestedPrincipal` | Částky otevřených pozic, principal copy portfolií, manuální pending orders s náklady a MIT orders. |
| `UnrealizedPnL` | Nerealizované PnL z otevřených manuálních a copy pozic. |
| `RealizedPnL` | Součet `NetProfit` uzavřených obchodů za zadaný rozsah trade history. |
| `Equity` | Available cash plus invested principal plus unrealized PnL. |
| `EstimatedNetDeposits` | **Odhad** vypočítaný jako `Equity - TotalReturn`. Nejedná se o skutečný vkladový výpis — výběry, bonusy, dividendy a další peněžní toky nemusí být zahrnuty. |
| `TotalReturn` | Realized PnL plus unrealized PnL. |
| `TotalReturnPct` | `TotalReturn / EstimatedNetDeposits * 100`, nebo `0`, když jsou odhadované net deposits nulové. |

> **Poznámka:** Všechny metriky jsou odvozeny z PnL a trade-history endpointů eToro a jedná se pouze o odhady. Nejsou to autoritativní výpisy účtu.

## Přímé výpočty

Služba nabízí i přímé výpočty, pokud už máte `PnlResponse` nebo closed trades:

```csharp
var availableCash = calculations.CalculateAvailableCash(pnl);
var invested = calculations.CalculateInvestedPrincipal(pnl);
var unrealized = calculations.CalculateUnrealizedPnL(pnl);
var realized = calculations.CalculateRealizedProfit(history.Trades);
```

> **Poznámka k `CalculateUnrealizedPnL`:** Metoda sčítá PnL z jednotlivých záznamů pozic.
> Výsledek se může lišit od hodnoty `PnlResponse.UnrealizedPnL` vrácené přímo API kvůli zaokrouhlování
> nebo jinému způsobu agregace na straně serveru. Pro autoritativní hodnotu čtěte `pnl.UnrealizedPnL` přímo.

## Bezpečné stránkování

`GetAccountMetricsAsync` čte historii uzavřených obchodů s omezeným stránkováním. Parametry `pageSize` a `maxPages` určují API zátěž a maximální rozsah skenování.

Pokud skenování dosáhne limitu `maxPages` bez nalezení prázdné terminální stránky, metoda vyvolá `InvalidOperationException`, aby zabránila tiché neúplnosti výpočtu realized PnL. V takovém případě zvyšte `maxPages` nebo zúžte rozsah `fromDate`.

Duplicitní hodnoty `PositionId` napříč stránkami jsou automaticky dedupikovány — každý uzavřený obchod je započítán nejvýše jednou.
