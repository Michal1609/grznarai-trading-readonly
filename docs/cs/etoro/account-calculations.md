# VĂ˝poÄŤty ĂşÄŤtu

`IEToroCalculationService` poskytuje pomocnĂ© vĂ˝poÄŤty nad PnL, portfoliem a historiĂ­ obchodĹŻ.

## Registrace

SluĹľba se registruje automaticky pĹ™es:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

## PouĹľitĂ­

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;
using GrznarAi.Trading.ReadOnly.Etoro.Services;

var metrics = await calculations.GetAccountMetricsAsync(
    EToroEnvironment.Real,
    fromDate: new DateOnly(2024, 1, 1),
    pageSize: 100,
    maxPages: 50,
    ct);
```

## Metriky

| Metrika | VĂ˝znam |
| --- | --- |
| `AvailableCash` | Credit minus manuĂˇlnĂ­ pending orders a MIT orders. |
| `InvestedPrincipal` | ÄŚĂˇstky otevĹ™enĂ˝ch pozic, principal copy portfoliĂ­, manuĂˇlnĂ­ pending orders s nĂˇklady a MIT orders. |
| `UnrealizedPnL` | NerealizovanĂ© PnL z otevĹ™enĂ˝ch manuĂˇlnĂ­ch a copy pozic. |
| `RealizedPnL` | SouÄŤet `NetProfit` uzavĹ™enĂ˝ch obchodĹŻ za zadanĂ˝ rozsah trade history. |
| `Equity` | Available cash plus invested principal plus unrealized PnL. |
| `EstimatedNetDeposits` | **Odhad** vypoÄŤĂ­tanĂ˝ jako `Equity - TotalReturn`. NejednĂˇ se o skuteÄŤnĂ˝ vkladovĂ˝ vĂ˝pis â€” vĂ˝bÄ›ry, bonusy, dividendy a dalĹˇĂ­ penÄ›ĹľnĂ­ toky nemusĂ­ bĂ˝t zahrnuty. |
| `TotalReturn` | Realized PnL plus unrealized PnL. |
| `TotalReturnPct` | `TotalReturn / EstimatedNetDeposits * 100`, nebo `0`, kdyĹľ jsou odhadovanĂ© net deposits nulovĂ©. |

> **PoznĂˇmka:** VĹˇechny metriky jsou odvozeny z PnL a trade-history endpointĹŻ eToro a jednĂˇ se pouze o odhady. Nejsou to autoritativnĂ­ vĂ˝pisy ĂşÄŤtu.

## PĹ™Ă­mĂ© vĂ˝poÄŤty

SluĹľba nabĂ­zĂ­ i pĹ™Ă­mĂ© vĂ˝poÄŤty, pokud uĹľ mĂˇte `PnlResponse` nebo closed trades:

```csharp
var availableCash = calculations.CalculateAvailableCash(pnl);
var invested = calculations.CalculateInvestedPrincipal(pnl);
var unrealized = calculations.CalculateUnrealizedPnL(pnl);
var realized = calculations.CalculateRealizedProfit(history.Trades);
```

> **PoznĂˇmka k `CalculateUnrealizedPnL`:** Metoda sÄŤĂ­tĂˇ PnL z jednotlivĂ˝ch zĂˇznamĹŻ pozic.
> VĂ˝sledek se mĹŻĹľe liĹˇit od hodnoty `PnlResponse.UnrealizedPnL` vrĂˇcenĂ© pĹ™Ă­mo API kvĹŻli zaokrouhlovĂˇnĂ­
> nebo jinĂ©mu zpĹŻsobu agregace na stranÄ› serveru. Pro autoritativnĂ­ hodnotu ÄŤtÄ›te `pnl.UnrealizedPnL` pĹ™Ă­mo.

## RozloĹľenĂ­ portfolia

Calculation service umĂ­ z otevĹ™enĂ˝ch PnL pozic spoÄŤĂ­tat i analytiku rozloĹľenĂ­ portfolia:

```csharp
var instruments = await calculations.GetPortfolioInstrumentAllocationAsync(
    EToroEnvironment.Real,
    ct);

var byAssetClass = calculations.CalculatePortfolioAssetClassAllocation(instruments);
var byIndustry = calculations.CalculatePortfolioIndustryAllocation(instruments);
```

### RozloĹľenĂ­ podle instrumentu

`GetPortfolioInstrumentAllocationAsync` naÄŤte PnL pro zvolenĂ˝ ĂşÄŤet, zahrne manuĂˇlnĂ­ otevĹ™enĂ© pozice i pozice uvnitĹ™ kopĂ­rovanĂ˝ch lidĂ­/portfoliĂ­ z PnL `mirrors`, doplnĂ­ market metadata, seskupĂ­ pozice podle instrumentu a vrĂˇtĂ­ Ĺ™Ăˇdky seĹ™azenĂ© sestupnÄ› podle investovanĂ© ÄŤĂˇstky.

KaĹľdĂ˝ `PortfolioInstrumentAllocation` Ĺ™Ăˇdek obsahuje:

| Vlastnost | VĂ˝znam |
| --- | --- |
| `InstrumentId` | eToro ID instrumentu. |
| `Symbol` | Symbol instrumentu, pĹ™Ă­padnÄ› fallback label, kdyĹľ metadata nejsou dostupnĂˇ. |
| `AssetClassId` / `AssetClass` | eToro ID asset class a jejĂ­ nĂˇzev, pokud jsou dostupnĂ©. |
| `IndustryId` / `Industry` | eToro ID akciovĂ©ho odvÄ›tvĂ­ a jeho nĂˇzev, pokud jsou dostupnĂ©. |
| `InvestedAmount` | SouÄŤet hodnot `Amount` otevĹ™enĂ˝ch pozic pro instrument. |
| `Share` | `InvestedAmount / celkovĂˇ investovanĂˇ ÄŤĂˇstka otevĹ™enĂ˝ch pozic`, v rozsahu `0..1`. |
| `ManualAmount` | InvestovanĂˇ ÄŤĂˇstka z manuĂˇlnÄ› otevĹ™enĂ˝ch pozic. |
| `MirrorAmount` | InvestovanĂˇ ÄŤĂˇstka z copy/mirror pozic. |
| `PositionCount` | PoÄŤet otevĹ™enĂ˝ch PnL pozic v bucketu instrumentu. |

### SkupinovĂ© rozloĹľenĂ­

`CalculatePortfolioAssetClassAllocation` seskupĂ­ instrument allocation Ĺ™Ăˇdky podle asset class.

`CalculatePortfolioIndustryAllocation` seskupĂ­ instrument allocation Ĺ™Ăˇdky podle akciovĂ©ho odvÄ›tvĂ­. Instrumenty bez industry metadat jsou zaĹ™azeny pod fallback `Unknown industry`.

KaĹľdĂ˝ `PortfolioGroupAllocation` Ĺ™Ăˇdek obsahuje:

| Vlastnost | VĂ˝znam |
| --- | --- |
| `GroupId` | ID asset class nebo industry, pokud ho metadata poskytujĂ­. |
| `GroupName` | ZobrazovanĂ˝ nĂˇzev skupiny. |
| `InvestedAmount` | CelkovĂˇ investovanĂˇ ÄŤĂˇstka otevĹ™enĂ˝ch pozic ve skupinÄ›. |
| `Share` | `InvestedAmount / celkovĂˇ investovanĂˇ ÄŤĂˇstka otevĹ™enĂ˝ch pozic`, v rozsahu `0..1`. |
| `InstrumentCount` | PoÄŤet rĹŻznĂ˝ch instrumentĹŻ ve skupinÄ›. |
| `PositionCount` | PoÄŤet otevĹ™enĂ˝ch PnL pozic ve skupinÄ›. |

> **Rozsah:** Allocation analytika pouĹľĂ­vĂˇ hodnoty `Amount` z otevĹ™enĂ˝ch pozic jako investovanĂ˝ principal.
> Nezahrnuje cash, pending orders, realized PnL, unrealized PnL, poplatky, spready,
> dividendy ani uzavĹ™enĂ© pozice.

## BezpeÄŤnĂ© strĂˇnkovĂˇnĂ­

`GetAccountMetricsAsync` ÄŤte historii uzavĹ™enĂ˝ch obchodĹŻ s omezenĂ˝m strĂˇnkovĂˇnĂ­m. Parametry `pageSize` a `maxPages` urÄŤujĂ­ API zĂˇtÄ›Ĺľ a maximĂˇlnĂ­ rozsah skenovĂˇnĂ­.

Pokud skenovĂˇnĂ­ dosĂˇhne limitu `maxPages` bez nalezenĂ­ prĂˇzdnĂ© terminĂˇlnĂ­ strĂˇnky, metoda vyvolĂˇ `InvalidOperationException`, aby zabrĂˇnila tichĂ© neĂşplnosti vĂ˝poÄŤtu realized PnL. V takovĂ©m pĹ™Ă­padÄ› zvyĹˇte `maxPages` nebo zĂşĹľte rozsah `fromDate`.

DuplicitnĂ­ hodnoty `PositionId` napĹ™Ă­ÄŤ strĂˇnkami jsou automaticky dedupikovĂˇny â€” kaĹľdĂ˝ uzavĹ™enĂ˝ obchod je zapoÄŤĂ­tĂˇn nejvĂ˝Ĺˇe jednou.
