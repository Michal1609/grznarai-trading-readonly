# Account Calculations

`IEToroCalculationService` provides helper calculations on top of PnL, portfolio, and trade-history data.

## Register

The service is registered automatically by:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

## Use

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

## Metrics

| Metric | Meaning |
| --- | --- |
| `AvailableCash` | Credit minus manual pending orders and MIT orders. |
| `InvestedPrincipal` | Open position amounts, copy portfolio principal, manual pending orders with costs, and MIT orders. |
| `UnrealizedPnL` | Unrealized PnL from open manual and copy positions. |
| `RealizedPnL` | Sum of closed trade `NetProfit` over the requested trade-history range. |
| `Equity` | Available cash plus invested principal plus unrealized PnL. |
| `EstimatedNetDeposits` | **Estimate** inferred as `Equity - TotalReturn`. Not a real deposit ledger — withdrawals, bonuses, dividends, and other cash flows may not be reflected. |
| `TotalReturn` | Realized PnL plus unrealized PnL. |
| `TotalReturnPct` | `TotalReturn / EstimatedNetDeposits * 100`, or `0` when estimated net deposits are zero. |

> **Note:** All metrics are derived from eToro's PnL and trade-history endpoints and are estimates only. They are not authoritative account statements.

## Direct Calculation Methods

The service also exposes direct calculation methods when you already have a `PnlResponse` or closed trades:

```csharp
var availableCash = calculations.CalculateAvailableCash(pnl);
var invested = calculations.CalculateInvestedPrincipal(pnl);
var unrealized = calculations.CalculateUnrealizedPnL(pnl);
var realized = calculations.CalculateRealizedProfit(history.Trades);
```

> **Note on `CalculateUnrealizedPnL`:** This method sums PnL from individual position records.
> The result may differ from the API-reported `PnlResponse.UnrealizedPnL` field due to rounding
> or different server-side grouping. Read `pnl.UnrealizedPnL` directly when you need the
> API-authoritative value.

## Portfolio Allocation

The calculation service can also build portfolio-allocation analytics from open PnL positions:

```csharp
var instruments = await calculations.GetPortfolioInstrumentAllocationAsync(
    EToroEnvironment.Real,
    ct);

var byAssetClass = calculations.CalculatePortfolioAssetClassAllocation(instruments);
var byIndustry = calculations.CalculatePortfolioIndustryAllocation(instruments);
```

### Instrument Allocation

`GetPortfolioInstrumentAllocationAsync` fetches PnL for the selected account, includes manual open positions and positions inside copied people/portfolios from PnL `mirrors`, enriches them with market metadata, groups them by instrument, and returns rows sorted by invested amount descending.

Each `PortfolioInstrumentAllocation` row contains:

| Property | Meaning |
| --- | --- |
| `InstrumentId` | eToro instrument ID. |
| `Symbol` | Instrument symbol, or a fallback label when metadata is unavailable. |
| `AssetClassId` / `AssetClass` | eToro asset class ID and display name when available. |
| `IndustryId` / `Industry` | eToro stock industry ID and display name when available. |
| `InvestedAmount` | Sum of open-position `Amount` values for the instrument. |
| `Share` | `InvestedAmount / total open-position invested amount`, in the `0..1` range. |
| `ManualAmount` | Invested amount from manually opened positions. |
| `MirrorAmount` | Invested amount from copy/mirror positions. |
| `PositionCount` | Number of open PnL positions in the instrument bucket. |

### Group Allocation

`CalculatePortfolioAssetClassAllocation` groups instrument allocation rows by asset class.

`CalculatePortfolioIndustryAllocation` groups instrument allocation rows by stock industry. Instruments without industry metadata are grouped under an `Unknown industry` fallback.

Each `PortfolioGroupAllocation` row contains:

| Property | Meaning |
| --- | --- |
| `GroupId` | Asset class or industry ID when metadata provides one. |
| `GroupName` | Display name of the group. |
| `InvestedAmount` | Total invested open-position amount in the group. |
| `Share` | `InvestedAmount / total open-position invested amount`, in the `0..1` range. |
| `InstrumentCount` | Number of distinct instruments in the group. |
| `PositionCount` | Number of open PnL positions in the group. |

> **Scope:** Allocation analytics use open-position `Amount` values as invested principal.
> They do not include cash, pending orders, realized PnL, unrealized PnL, fees, spreads,
> dividends, or closed positions.

## Paging Safety

`GetAccountMetricsAsync` reads closed trade history with bounded paging. Use `pageSize` and `maxPages` to control API load and how far the service can scan.

If the trade history scan reaches `maxPages` without finding an empty terminal page, the method throws `InvalidOperationException` to prevent silently incomplete realized-PnL totals. Increase `maxPages` or narrow the `fromDate` range if this occurs.

Duplicate `PositionId` values across pages are deduplicated automatically — each closed trade is counted at most once.
