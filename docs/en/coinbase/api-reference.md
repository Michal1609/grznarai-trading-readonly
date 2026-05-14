# API Reference

`ICoinbaseClient` is the main facade registered via `AddCoinbaseClient`. It combines the accounts and portfolios interfaces.

## Accounts

Interface: `ICoinbaseAccountsClient`

| Method | Purpose |
| --- | --- |
| `ListAccountsAsync(limit?, cursor?, ct)` | Returns paginated brokerage accounts. |
| `GetAccountAsync(accountUuid, ct)` | Returns a single account by UUID. |

## Portfolios

Interface: `ICoinbasePortfoliosClient`

| Method | Purpose |
| --- | --- |
| `ListPortfoliosAsync(portfolioType?, ct)` | Returns all portfolios. Optional filter by type (`DEFAULT`, `CONSUMER`, `INTX`). |
| `GetPortfolioBreakdownAsync(portfolioUuid, currency?, ct)` | Returns detailed breakdown: balances + spot/perp/futures positions. |
| `GetPortfolioBreakdownAsync(GetPortfolioBreakdownRequest, ct)` | Request-object overload. |

Example:

```csharp
var breakdown = await client.GetPortfolioBreakdownAsync(
    portfolioUuid: "11111111-1111-1111-1111-111111111111",
    currency: "USD");

var balances = breakdown.Breakdown!.PortfolioBalances!;
Console.WriteLine($"Total: {balances.TotalBalance!.Value} {balances.TotalBalance.Currency}");

foreach (var spot in breakdown.Breakdown.SpotPositions ?? [])
    Console.WriteLine($"{spot.Asset}: {spot.TotalBalanceFiat}");
```

## Portfolio Breakdown models

- `GetPortfolioBreakdownResponse`
  - `Breakdown : PortfolioBreakdown`
- `PortfolioBreakdown`
  - `Portfolio : Portfolio` (name, uuid, type, deleted)
  - `PortfolioBalances : PortfolioBalances`
  - `SpotPositions : List<SpotPosition>`
  - `PerpPositions : List<PerpPosition>`
  - `FuturesPositions : List<FuturesPosition>`
- `PortfolioBalances` — total/futures/cash-equivalent/crypto balance + futures/perp unrealized PnL (each `MoneyValue`).
- `SpotPosition` — asset, account_uuid, total balance (fiat + crypto), available, allocation, one_day_change, cost_basis, unrealized_pnl, etc.
- `PerpPosition` — product_id, symbol, vwap, position_side, net_size, leverage, mark_price, liquidation_price, margin_type, etc.
- `FuturesPosition` — product_id, contract_size, side, amount, avg_entry_price, current_price, unrealized_pnl, expiry, etc.
- `MoneyValue` — `{ value: decimal, currency: string }`. The `value` field is deserialised from a JSON string into a `decimal` via a custom `DecimalStringConverter`.
- Enums: `PositionSide`, `MarginType` — string enums using `[JsonStringEnumMemberName]`.
