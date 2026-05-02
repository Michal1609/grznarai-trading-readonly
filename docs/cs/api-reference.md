# API Reference

`IEToroClient` je hlavní facade registrovaná přes `AddEToro`. Kombinuje trading, market data, user info, social, feed a watchlist rozhraní.

Facade implementuje níže uvedená doménová rozhraní. Pokud vlastní kód potřebuje menší surface, přijímejte konkrétní rozhraní v interních metodách nebo si doplňte vlastní DI mapping z `IEToroClient` na dané rozhraní.

## Trading

Rozhraní: `IEToroTradingClient`

| Metoda | Účel |
| --- | --- |
| `GetAgentPortfoliosAsync` | Vrací agent portfolia přihlášeného uživatele. |
| `GetPnlAsync` | Vrací PnL pro demo nebo real účet. |
| `GetPortfolioAsync` | Vrací portfolio včetně pozic, příkazů a stavu účtu. |
| `GetTradeHistoryAsync` | Vrací historii uzavřených obchodů od minimálního data se stránkováním. |
| `GetOrderAsync` | Vrací informace o příkazu a souvisejících pozicích. |

Příklad:

```csharp
var pnl = await client.GetPnlAsync(EToroEnvironment.Real, ct);
var portfolio = await client.GetPortfolioAsync(EToroEnvironment.Real, ct);
var history = await client.GetTradeHistoryAsync(new DateOnly(2024, 1, 1), page: 0, pageSize: 100, ct);
```

## Market Data

Rozhraní: `IEToroMarketDataClient`

| Metoda | Účel |
| --- | --- |
| `GetExchangesAsync` | Vrací podporované burzy. |
| `GetInstrumentTypesAsync` | Vrací typy instrumentů. |
| `GetInstrumentMetadataAsync` | Vrací metadata instrumentů filtrovaná podle ID, burz, odvětví nebo typů. |
| `GetHistoricalClosingPricesAsync` | Vrací historické zavírací ceny. |
| `GetStocksIndustriesAsync` | Vrací metadata akciových odvětví. |
| `SearchInstrumentsAsync` | Vyhledává instrumenty podle polí, textu, řazení a stránkování. |
| `GetRatesAsync` | Vrací aktuální market rates pro instrument IDs. |
| `GetCandlesAsync` | Vrací historická candle data. |
| `GetCopiersPublicInfoAsync` | Vrací veřejné informace o uživatelích kopírujících vaše portfolio. |

Příklad:

```csharp
using GrznarAi.Trading.ReadOnly.Models.Market;

var rates = await client.GetRatesAsync([100000, 100001], ct);

var candles = await client.GetCandlesAsync(
    instrumentId: 100000,
    interval: CandleInterval.OneDay,
    candlesCount: 100,
    ct: ct);
```

Vyhledávání instrumentů vyžaduje alespoň jedno pole a podporuje maximálně pět polí:

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

Rozhraní: `IEToroUserInfoClient`

| Metoda | Účel |
| --- | --- |
| `GetIdentityAsync` | Vrací identitu přihlášeného uživatele a customer IDs. |
| `GetUserProfilesAsync` | Vrací detailní profily podle username nebo customer ID. |
| `SearchUsersAsync` | Vyhledává uživatele pomocí filtrů. |
| `GetUserDailyGainAsync` | Vrací denní nebo kumulativní gain pro rozsah dat. |
| `GetUserGainAsync` | Vrací měsíční a roční gain historii. |
| `GetUserLivePortfolioAsync` | Vrací live portfolio uživatele. |
| `GetUserTradeInfoAsync` | Vrací obchodní statistiky uživatele za období. |

## Social

Rozhraní: `IEToroSocialClient`

| Metoda | Účel |
| --- | --- |
| `GetPopularInvestorsAsync` | Vrací popular investors s výkonnostními metrikami. |

## Feed

Rozhraní: `IEToroFeedClient`

| Metoda | Účel |
| --- | --- |
| `GetInstrumentFeedPostsAsync` | Vrací feed posty pro instrument. |
| `GetUserFeedPostsAsync` | Vrací feed posty pro uživatele. |

## Watchlists

Rozhraní: `IEToroWatchlistClient`

| Metoda | Účel |
| --- | --- |
| `GetCuratedListsAsync` | Vrací curated investment lists. Při HTTP 204 může vrátit `null`. |
| `GetMarketRecommendationsAsync` | Vrací personalizovaná market recommendations. Při HTTP 204 může vrátit `null`. |
| `GetUserWatchlistsAsync` | Vrací watchlists přihlášeného uživatele. |
| `GetDefaultWatchlistItemsAsync` | Vrací položky z default watchlists. |
| `GetUsersPublicWatchlistsAsync` | Vrací veřejné watchlists daného uživatele. |
| `GetSinglePublicWatchlistAsync` | Vrací jeden veřejný watchlist. |
| `GetSingleWatchlistAsync` | Vrací jeden watchlist přihlášeného uživatele. |

## API chyby

Neúspěšné API odpovědi reprezentuje `EToroApiException`. Chování redakce a oříznutí response body je popsané v [Error handling](error-handling.md).

Výjimka obsahuje:

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
