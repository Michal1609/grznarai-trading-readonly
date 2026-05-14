# API Reference

`ICoinbaseClient` je hlavní facade registrovaná přes `AddCoinbaseClient`. Kombinuje rozhraní pro accounts a portfolios.

## Accounts

Rozhraní: `ICoinbaseAccountsClient`

| Metoda | Účel |
| --- | --- |
| `ListAccountsAsync(limit?, cursor?, ct)` | Vrátí stránkovaný seznam brokerage účtů. |
| `GetAccountAsync(accountUuid, ct)` | Vrátí jeden brokerage účet podle UUID. |

## Portfolios

Rozhraní: `ICoinbasePortfoliosClient`

| Metoda | Účel |
| --- | --- |
| `ListPortfoliosAsync(portfolioType?, ct)` | Vrátí všechna portfolia. Filtr podle typu (`DEFAULT`, `CONSUMER`, `INTX`). |
| `GetPortfolioBreakdownAsync(portfolioUuid, currency?, ct)` | Vrátí detailní rozpad: balances + spot/perp/futures pozice. |
| `GetPortfolioBreakdownAsync(GetPortfolioBreakdownRequest, ct)` | Varianta s request objektem. |

Příklad:

```csharp
var breakdown = await client.GetPortfolioBreakdownAsync(
    portfolioUuid: "11111111-1111-1111-1111-111111111111",
    currency: "USD");

var balances = breakdown.Breakdown!.PortfolioBalances!;
Console.WriteLine($"Total: {balances.TotalBalance!.Value} {balances.TotalBalance.Currency}");

foreach (var spot in breakdown.Breakdown.SpotPositions ?? [])
    Console.WriteLine($"{spot.Asset}: {spot.TotalBalanceFiat}");
```

## Modely Portfolio Breakdown

- `GetPortfolioBreakdownResponse`
  - `Breakdown : PortfolioBreakdown`
- `PortfolioBreakdown`
  - `Portfolio : Portfolio` (name, uuid, type, deleted)
  - `PortfolioBalances : PortfolioBalances`
  - `SpotPositions : List<SpotPosition>`
  - `PerpPositions : List<PerpPosition>`
  - `FuturesPositions : List<FuturesPosition>`
- `PortfolioBalances` — total/futures/cash-equivalent/crypto balance + futures/perp unrealized PnL (vše `MoneyValue`)
- `SpotPosition` — asset, account_uuid, total balance (fiat + crypto), available, allocation, one_day_change, cost_basis, unrealized_pnl, atd.
- `PerpPosition` — product_id, symbol, vwap, position_side, net_size, leverage, mark_price, liquidation_price, margin_type, atd.
- `FuturesPosition` — product_id, contract_size, side, amount, avg_entry_price, current_price, unrealized_pnl, expiry, atd.
- `MoneyValue` — `{ value: decimal, currency: string }`. Pole `value` je deserializováno z JSON stringu na `decimal` (vlastní `DecimalStringConverter`).
- Enumy: `PositionSide`, `MarginType` — string enumy přes `[JsonStringEnumMemberName]`.
