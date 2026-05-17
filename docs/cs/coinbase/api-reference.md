# API Reference

`ICoinbaseClient` je hlavní facade registrovaná přes `AddCoinbaseClient`. Agreguje všechna doménová rozhraní níže — můžete injektovat buď facade, nebo konkrétní rozhraní (oboje se řeší z téhož singletonu).

Klient je **striktně read-only**: každá metoda mapuje na dokumentovaný `GET` endpoint Coinbase Advanced Trade API. Žádná operace měnící stav (zadání/zrušení orderu, convert commit atd.) není vystavena. Pokryty jsou všechny dokumentované GET endpointy.

> Funkční ukázky pro každý endpoint najdete v unit testech v [`tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Client/`](../../../tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Client/) (mockované HTTP) a v integračních testech řízených přihlašovacími údaji v [`tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Integration/`](../../../tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Integration/).

## Katalog metod

### Accounts — `ICoinbaseAccountsClient`

| Metoda | Účel |
| --- | --- |
| `ListAccountsAsync(limit?, cursor?, ct)` | Stránkovaný seznam brokerage účtů. |
| `ListAccountsAsync(ListAccountsRequest, ct)` | Varianta s request objektem (filtry typ/platforma/atd.). |
| `GetAccountAsync(accountUuid, ct)` | Jeden účet podle UUID. |

### Portfolios — `ICoinbasePortfoliosClient`

| Metoda | Účel |
| --- | --- |
| `ListPortfoliosAsync(portfolioType?, ct)` | Všechna portfolia. Filtr podle typu (`DEFAULT`, `CONSUMER`, `INTX`). |
| `ListPortfoliosAsync(ListPortfoliosRequest, ct)` | Varianta s request objektem. |
| `GetPortfolioBreakdownAsync(portfolioUuid, currency?, ct)` | Balances + spot/perp/futures pozice pro jedno portfolio. |
| `GetPortfolioBreakdownAsync(GetPortfolioBreakdownRequest, ct)` | Varianta s request objektem. |

### Orders — `ICoinbaseOrdersClient`

| Metoda | Účel |
| --- | --- |
| `GetOrderAsync(orderId, ct)` / overload | Jeden order podle id. |
| `ListOrdersAsync(...)` / overload | Historie orderů (filtr product/status/čas/strana/typ). |
| `ListFillsAsync(...)` / overload | Historie fillů (filtr product/order/čas). |

### Products — `ICoinbaseProductsClient` (autentizovaná market data)

| Metoda | Účel |
| --- | --- |
| `ListProductsAsync(...)` / overload | Katalog produktů (filtr typ/venue/status/atd.). |
| `GetProductAsync(...)` / overload | Metadata jednoho produktu. |
| `GetProductBookAsync(...)` / overload | L2 snapshot order booku. |
| `GetBestBidAskAsync(...)` / overload | Best bid/ask pro jeden či více produktů. |
| `GetMarketTradesAsync(...)` / overload | Poslední tržní obchody. |
| `GetProductCandlesAsync(...)` / overload | OHLC svíčky podle granularity. |

### Public market data — `ICoinbasePublicClient` (bez autentizace)

| Metoda | Účel |
| --- | --- |
| `GetServerTimeAsync(ct)` | Čas Coinbase serveru. |
| `ListPublicProductsAsync(...)` / overload | Veřejný katalog produktů (bez API klíče). |
| `GetPublicProductAsync(...)` / overload | Jeden veřejný produkt. |
| `GetPublicProductBookAsync(...)` / overload | Veřejný snapshot order booku. |
| `GetPublicProductCandlesAsync(...)` / overload | Veřejné OHLC svíčky. |
| `GetPublicMarketTradesAsync(...)` / overload | Veřejné poslední tržní obchody. |

### Fees — `ICoinbaseFeesClient`

| Metoda | Účel |
| --- | --- |
| `GetTransactionSummaryAsync(...)` / overload | Maker/taker fee tier a 30denní volume. |

### Futures — `ICoinbaseFuturesClient`

| Metoda | Účel |
| --- | --- |
| `GetFuturesBalanceSummaryAsync(ct)` | Souhrn futures balance (CFM). |
| `ListFuturesPositionsAsync(ct)` | Otevřené futures pozice. |
| `GetFuturesPositionAsync(productId, ct)` | Jedna futures pozice. |
| `GetCurrentMarginWindowAsync(...)` / overload | Aktivní margin okno. |
| `ListFuturesSweepsAsync(ct)` | Plánované sweepy futures → spot. |
| `GetIntradayMarginSettingAsync(ct)` | Aktuální nastavení intraday marginu. |

### Perpetuals — `ICoinbasePerpetualClient`

| Metoda | Účel |
| --- | --- |
| `GetPerpetualPortfolioSummaryAsync(portfolioUuid, ct)` | Souhrn perp portfolia. |
| `ListPerpetualPositionsAsync(portfolioUuid, ct)` | Otevřené perp pozice. |
| `GetPerpetualPositionAsync(portfolioUuid, symbol, ct)` | Jedna perp pozice. |
| `GetPortfolioBalancesAsync(portfolioUuid, ct)` | Balances portfolia (INTX). |

### Payment Methods — `ICoinbasePaymentMethodsClient`

| Metoda | Účel |
| --- | --- |
| `ListPaymentMethodsAsync(ct)` | Všechny platební metody na účtu. |
| `GetPaymentMethodAsync(paymentMethodId, ct)` | Jedna platební metoda. |

### Convert — `ICoinbaseConvertClient`

| Metoda | Účel |
| --- | --- |
| `GetConvertTradeAsync(...)` / overload | Stav existujícího convert trade (read-only; quote/commit nejsou vystaveny). |

### Data API — `ICoinbaseDataApiClient`

| Metoda | Účel |
| --- | --- |
| `GetApiKeyPermissionsAsync(ct)` | Oprávnění aktuálního API klíče. |

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
