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

## Paging Safety

`GetAccountMetricsAsync` reads closed trade history with bounded paging. Use `pageSize` and `maxPages` to control API load and how far the service can scan.

If the trade history scan reaches `maxPages` without finding an empty terminal page, the method throws `InvalidOperationException` to prevent silently incomplete realized-PnL totals. Increase `maxPages` or narrow the `fromDate` range if this occurs.

Duplicate `PositionId` values across pages are deduplicated automatically — each closed trade is counted at most once.
