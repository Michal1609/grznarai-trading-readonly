# API Reference

`IEToroClient` je hlavnĂ­ facade registrovanĂˇ pĹ™es `AddEToro`. Kombinuje trading, market data, user info, social, feed a watchlist rozhranĂ­.

Facade implementuje nĂ­Ĺľe uvedenĂˇ domĂ©novĂˇ rozhranĂ­. Pokud vlastnĂ­ kĂłd potĹ™ebuje menĹˇĂ­ surface, pĹ™ijĂ­mejte konkrĂ©tnĂ­ rozhranĂ­ v internĂ­ch metodĂˇch nebo si doplĹte vlastnĂ­ DI mapping z `IEToroClient` na danĂ© rozhranĂ­.

## Trading

RozhranĂ­: `IEToroTradingClient`

| Metoda | ĂšÄŤel |
| --- | --- |
| `GetAgentPortfoliosAsync` | VracĂ­ agent portfolia pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele. |
| `GetPnlAsync` | VracĂ­ PnL pro demo nebo real ĂşÄŤet. |
| `GetPortfolioAsync` | VracĂ­ portfolio vÄŤetnÄ› pozic, pĹ™Ă­kazĹŻ a stavu ĂşÄŤtu. |
| `GetTradeHistoryAsync` | VracĂ­ historii uzavĹ™enĂ˝ch obchodĹŻ od minimĂˇlnĂ­ho data se strĂˇnkovĂˇnĂ­m. |
| `GetOrderAsync` | VracĂ­ informace o pĹ™Ă­kazu a souvisejĂ­cĂ­ch pozicĂ­ch. |

PĹ™Ă­klad:

```csharp
var pnl = await client.GetPnlAsync(EToroEnvironment.Real, ct);
var portfolio = await client.GetPortfolioAsync(EToroEnvironment.Real, ct);
var history = await client.GetTradeHistoryAsync(new DateOnly(2024, 1, 1), page: 0, pageSize: 100, ct);
```

## Market Data

RozhranĂ­: `IEToroMarketDataClient`

| Metoda | ĂšÄŤel |
| --- | --- |
| `GetExchangesAsync` | VracĂ­ podporovanĂ© burzy. |
| `GetInstrumentTypesAsync` | VracĂ­ typy instrumentĹŻ. |
| `GetInstrumentMetadataAsync` | VracĂ­ metadata instrumentĹŻ filtrovanĂˇ podle ID, burz, odvÄ›tvĂ­ nebo typĹŻ. |
| `GetHistoricalClosingPricesAsync` | VracĂ­ historickĂ© zavĂ­racĂ­ ceny. |
| `GetStocksIndustriesAsync` | VracĂ­ metadata akciovĂ˝ch odvÄ›tvĂ­. |
| `SearchInstrumentsAsync` | VyhledĂˇvĂˇ instrumenty podle polĂ­, textu, Ĺ™azenĂ­ a strĂˇnkovĂˇnĂ­. |
| `GetRatesAsync` | VracĂ­ aktuĂˇlnĂ­ market rates pro instrument IDs. |
| `GetCandlesAsync` | VracĂ­ historickĂˇ candle data. |
| `GetCopiersPublicInfoAsync` | VracĂ­ veĹ™ejnĂ© informace o uĹľivatelĂ­ch kopĂ­rujĂ­cĂ­ch vaĹˇe portfolio. |

PĹ™Ă­klad:

```csharp
using GrznarAi.Trading.ReadOnly.Etoro.Models.Market;

var rates = await client.GetRatesAsync([100000, 100001], ct);

var candles = await client.GetCandlesAsync(
    instrumentId: 100000,
    interval: CandleInterval.OneDay,
    candlesCount: 100,
    ct: ct);
```

VyhledĂˇvĂˇnĂ­ instrumentĹŻ vyĹľaduje alespoĹ jedno pole a podporuje maximĂˇlnÄ› pÄ›t polĂ­:

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

RozhranĂ­: `IEToroUserInfoClient`

| Metoda | ĂšÄŤel |
| --- | --- |
| `GetIdentityAsync` | VracĂ­ identitu pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele a customer IDs. |
| `GetUserProfilesAsync` | VracĂ­ detailnĂ­ profily podle username nebo customer ID. |
| `SearchUsersAsync` | VyhledĂˇvĂˇ uĹľivatele pomocĂ­ filtrĹŻ. |
| `GetUserDailyGainAsync` | VracĂ­ dennĂ­ nebo kumulativnĂ­ gain pro rozsah dat. |
| `GetUserGainAsync` | VracĂ­ mÄ›sĂ­ÄŤnĂ­ a roÄŤnĂ­ gain historii. |
| `GetUserLivePortfolioAsync` | VracĂ­ live portfolio uĹľivatele. |
| `GetUserTradeInfoAsync` | VracĂ­ obchodnĂ­ statistiky uĹľivatele za obdobĂ­. |

## Social

RozhranĂ­: `IEToroSocialClient`

| Metoda | ĂšÄŤel |
| --- | --- |
| `GetPopularInvestorsAsync` | VracĂ­ popular investors s vĂ˝konnostnĂ­mi metrikami. |

## Feed

RozhranĂ­: `IEToroFeedClient`

| Metoda | ĂšÄŤel |
| --- | --- |
| `GetInstrumentFeedPostsAsync` | VracĂ­ feed posty pro instrument. |
| `GetUserFeedPostsAsync` | VracĂ­ feed posty pro uĹľivatele. |

## Watchlists

RozhranĂ­: `IEToroWatchlistClient`

| Metoda | ĂšÄŤel |
| --- | --- |
| `GetCuratedListsAsync` | VracĂ­ curated investment lists. PĹ™i HTTP 204 mĹŻĹľe vrĂˇtit `null`. |
| `GetMarketRecommendationsAsync` | VracĂ­ personalizovanĂˇ market recommendations. PĹ™i HTTP 204 mĹŻĹľe vrĂˇtit `null`. |
| `GetUserWatchlistsAsync` | VracĂ­ watchlists pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele. |
| `GetDefaultWatchlistItemsAsync` | VracĂ­ poloĹľky z default watchlists. |
| `GetUsersPublicWatchlistsAsync` | VracĂ­ veĹ™ejnĂ© watchlists danĂ©ho uĹľivatele. |
| `GetSinglePublicWatchlistAsync` | VracĂ­ jeden veĹ™ejnĂ˝ watchlist. |
| `GetSingleWatchlistAsync` | VracĂ­ jeden watchlist pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele. |

## API chyby

NeĂşspÄ›ĹˇnĂ© API odpovÄ›di reprezentuje `EToroApiException`. ChovĂˇnĂ­ redakce a oĹ™Ă­znutĂ­ response body je popsanĂ© v [Error handling](error-handling.md).

VĂ˝jimka obsahuje:

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
