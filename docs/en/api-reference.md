# API Reference

`IEToroClient` is the main facade registered by `AddEToro`. It combines trading, market data, user info, social, feed, and watchlist interfaces.

The facade implements the domain interfaces below. If your own code wants a smaller surface, accept the specific interface in internal methods or add your own DI mapping from `IEToroClient` to that interface.

## Trading

Interface: `IEToroTradingClient`

| Method | Purpose |
| --- | --- |
| `GetAgentPortfoliosAsync` | Returns agent portfolios owned by the authenticated user. |
| `GetPnlAsync` | Returns demo or real account PnL. |
| `GetPortfolioAsync` | Returns portfolio information including positions, orders, and account state. |
| `GetTradeHistoryAsync` | Returns closed trade history from a minimum date with paging. |
| `GetOrderAsync` | Returns order information and related positions. |

Example:

```csharp
var pnl = await client.GetPnlAsync(EToroEnvironment.Real, ct);
var portfolio = await client.GetPortfolioAsync(EToroEnvironment.Real, ct);
var history = await client.GetTradeHistoryAsync(new DateOnly(2024, 1, 1), page: 0, pageSize: 100, ct);
```

## Market Data

Interface: `IEToroMarketDataClient`

| Method | Purpose |
| --- | --- |
| `GetExchangesAsync` | Returns supported exchanges. |
| `GetInstrumentTypesAsync` | Returns instrument types. |
| `GetInstrumentMetadataAsync` | Returns instrument metadata filtered by IDs, exchanges, industries, or types. |
| `GetHistoricalClosingPricesAsync` | Returns historical closing prices. |
| `GetStocksIndustriesAsync` | Returns stock industry metadata. |
| `SearchInstrumentsAsync` | Searches instruments by fields, text, sort, and paging. |
| `GetRatesAsync` | Returns current market rates for instrument IDs. |
| `GetCandlesAsync` | Returns historical candle data. |
| `GetCopiersPublicInfoAsync` | Returns public information about users copying your portfolio. |

Example:

```csharp
using GrznarAi.Trading.ReadOnly.Models.Market;

var rates = await client.GetRatesAsync([100000, 100001], ct);

var candles = await client.GetCandlesAsync(
    instrumentId: 100000,
    interval: CandleInterval.OneDay,
    candlesCount: 100,
    ct: ct);
```

Instrument search requires at least one field and supports up to five fields:

```csharp
var result = await client.SearchInstrumentsAsync(new InstrumentSearchRequest
{
    Fields = [InstrumentFields.Symbol, InstrumentFields.DisplayName],
    SearchText = "AAPL",
    PageSize = 20,
    PageNumber = 0
}, ct);
```

## User Info

Interface: `IEToroUserInfoClient`

| Method | Purpose |
| --- | --- |
| `GetIdentityAsync` | Returns authenticated user identity and customer IDs. |
| `GetUserProfilesAsync` | Returns detailed user profiles by username or customer ID. |
| `SearchUsersAsync` | Searches users with filters. |
| `GetUserDailyGainAsync` | Returns daily or cumulative gain for a date range. |
| `GetUserGainAsync` | Returns monthly and yearly gain history. |
| `GetUserLivePortfolioAsync` | Returns a user's live portfolio. |
| `GetUserTradeInfoAsync` | Returns user trade statistics for a period. |

## Social

Interface: `IEToroSocialClient`

| Method | Purpose |
| --- | --- |
| `GetPopularInvestorsAsync` | Returns popular investors with performance metrics. |

## Feed

Interface: `IEToroFeedClient`

| Method | Purpose |
| --- | --- |
| `GetInstrumentFeedPostsAsync` | Returns feed posts for an instrument. |
| `GetUserFeedPostsAsync` | Returns feed posts for a user. |

## Watchlists

Interface: `IEToroWatchlistClient`

| Method | Purpose |
| --- | --- |
| `GetCuratedListsAsync` | Returns curated investment lists. Can return `null` for HTTP 204. |
| `GetMarketRecommendationsAsync` | Returns personalized market recommendations. Can return `null` for HTTP 204. |
| `GetUserWatchlistsAsync` | Returns authenticated user's watchlists. |
| `GetDefaultWatchlistItemsAsync` | Returns items from default watchlists. |
| `GetUsersPublicWatchlistsAsync` | Returns public watchlists for a user. |
| `GetSinglePublicWatchlistAsync` | Returns one public watchlist. |
| `GetSingleWatchlistAsync` | Returns one authenticated-user watchlist. |

## API Errors

Non-success API responses are represented by `EToroApiException`. See [Error handling](error-handling.md) for response-body redaction and truncation behavior.

The exception exposes:

- `StatusCode`
- `Endpoint`
- `ResponseBody`
- `RequestId`
- `RetryAfter`

```csharp
try
{
    var rates = await client.GetRatesAsync([100000], ct);
}
catch (EToroApiException ex)
{
    logger.LogWarning(
        ex,
        "eToro API failed with {StatusCode} at {Endpoint}. RequestId: {RequestId}",
        ex.StatusCode,
        ex.Endpoint,
        ex.RequestId);
}
```
