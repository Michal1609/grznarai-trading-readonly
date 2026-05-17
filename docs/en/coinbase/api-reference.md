# API Reference

`ICoinbaseClient` is the main facade registered via `AddCoinbaseClient`. It aggregates every domain interface listed below. You can inject either the facade or a single domain interface — both resolve from the same singleton.

The client is **strictly read-only**: every method maps to a documented Coinbase Advanced Trade `GET` endpoint. No order placement, cancellation, conversion, or other state-changing operation is exposed. All documented GET endpoints are implemented.

> For working code samples per endpoint see the unit tests under [`tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Client/`](../../../tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Client/) (mocked HTTP) and the credential-driven integration tests under [`tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Integration/`](../../../tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/Integration/).

## Method catalogue

### Accounts — `ICoinbaseAccountsClient`

| Method | Purpose |
| --- | --- |
| `ListAccountsAsync(limit?, cursor?, ct)` | Paginated list of brokerage accounts. |
| `ListAccountsAsync(ListAccountsRequest, ct)` | Request-object overload (filters by type/platform/etc.). |
| `GetAccountAsync(accountUuid, ct)` | Single account by UUID. |

### Portfolios — `ICoinbasePortfoliosClient`

| Method | Purpose |
| --- | --- |
| `ListPortfoliosAsync(portfolioType?, ct)` | All portfolios. Optional filter by type (`DEFAULT`, `CONSUMER`, `INTX`). |
| `ListPortfoliosAsync(ListPortfoliosRequest, ct)` | Request-object overload. |
| `GetPortfolioBreakdownAsync(portfolioUuid, currency?, ct)` | Balances plus spot/perp/futures positions for one portfolio. |
| `GetPortfolioBreakdownAsync(GetPortfolioBreakdownRequest, ct)` | Request-object overload. |

### Orders — `ICoinbaseOrdersClient`

| Method | Purpose |
| --- | --- |
| `GetOrderAsync(orderId, ct)` | Single order by id. |
| `GetOrderAsync(GetOrderRequest, ct)` | Request-object overload. |
| `ListOrdersAsync(ct)` | List historical orders. |
| `ListOrdersAsync(ListOrdersRequest, ct)` | Filter by product, status, time range, side, type, etc. |
| `ListFillsAsync(ct)` | List historical fills. |
| `ListFillsAsync(ListFillsRequest, ct)` | Filter fills by product, order, time range. |

### Products — `ICoinbaseProductsClient` (authenticated market data)

| Method | Purpose |
| --- | --- |
| `ListProductsAsync(...)` / `ListProductsAsync(ListProductsRequest, ct)` | Product catalogue with filters (type, venue, status, etc.). |
| `GetProductAsync(productId, ...)` / overload | Single product metadata. |
| `GetProductBookAsync(productId, limit?, aggregationPriceIncrement?, ct)` / overload | Level-2 order book snapshot. |
| `GetBestBidAskAsync(productIds?, ct)` / overload | Best bid/ask for one or more products. |
| `GetMarketTradesAsync(productId, limit, start?, end?, ct)` / overload | Recent trades. |
| `GetProductCandlesAsync(productId, start, end, granularity, ct)` / overload | OHLC candles by granularity. |

### Public market data — `ICoinbasePublicClient` (unauthenticated)

| Method | Purpose |
| --- | --- |
| `GetServerTimeAsync(ct)` | Coinbase server time. |
| `ListPublicProductsAsync(...)` / overload | Public product catalogue (no API key required). |
| `GetPublicProductAsync(productId, ct)` / overload | Single public product. |
| `GetPublicProductBookAsync(...)` / overload | Public order book snapshot. |
| `GetPublicProductCandlesAsync(...)` / overload | Public OHLC candles. |
| `GetPublicMarketTradesAsync(...)` / overload | Public recent trades. |

### Fees — `ICoinbaseFeesClient`

| Method | Purpose |
| --- | --- |
| `GetTransactionSummaryAsync(...)` / overload | Maker/taker fee tier and 30-day volume summary. |

### Futures — `ICoinbaseFuturesClient`

| Method | Purpose |
| --- | --- |
| `GetFuturesBalanceSummaryAsync(ct)` | Futures balance summary (CFM). |
| `ListFuturesPositionsAsync(ct)` | All open futures positions. |
| `GetFuturesPositionAsync(productId, ct)` | One futures position. |
| `GetCurrentMarginWindowAsync(...)` / overload | Active margin window. |
| `ListFuturesSweepsAsync(ct)` | Scheduled futures-to-spot sweeps. |
| `GetIntradayMarginSettingAsync(ct)` | Current intraday margin setting. |

### Perpetuals — `ICoinbasePerpetualClient`

| Method | Purpose |
| --- | --- |
| `GetPerpetualPortfolioSummaryAsync(portfolioUuid, ct)` | Perp portfolio summary. |
| `ListPerpetualPositionsAsync(portfolioUuid, ct)` | All open perp positions for a portfolio. |
| `GetPerpetualPositionAsync(portfolioUuid, symbol, ct)` | One perp position. |
| `GetPortfolioBalancesAsync(portfolioUuid, ct)` | Portfolio balances (INTX). |

### Payment Methods — `ICoinbasePaymentMethodsClient`

| Method | Purpose |
| --- | --- |
| `ListPaymentMethodsAsync(ct)` | All payment methods on the account. |
| `GetPaymentMethodAsync(paymentMethodId, ct)` | One payment method. |

### Convert — `ICoinbaseConvertClient`

| Method | Purpose |
| --- | --- |
| `GetConvertTradeAsync(tradeId, fromAccount, toAccount, ct)` / overload | Status of an existing convert trade (read-only; quote/commit are not exposed). |

### Data API — `ICoinbaseDataApiClient`

| Method | Purpose |
| --- | --- |
| `GetApiKeyPermissionsAsync(ct)` | Permissions granted to the current API key. |

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
