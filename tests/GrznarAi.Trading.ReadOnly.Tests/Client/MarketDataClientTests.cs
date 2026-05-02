using System.Net;
using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Market;
using GrznarAi.Trading.ReadOnly.Tests.Helpers;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class MarketDataClientTests
{
    private const string BaseUrl = "https://public-api.etoro.com/api/v1/";

    private static EToroClient CreateClient(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        return new EToroClient(httpClient);
    }

    // ─── GetExchangesAsync ────────────────────────────────────────────────────

    [Test]
    public async Task GetExchangesAsync_CallsCorrectEndpoint()
    {
        var handler = new MockHttpMessageHandler("""{ "exchangeInfo": [] }""");

        await CreateClient(handler).GetExchangesAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("market-data/exchanges"));
    }

    [Test]
    public async Task GetExchangesAsync_PassesExchangeIds()
    {
        var handler = new MockHttpMessageHandler("""{ "exchangeInfo": [] }""");

        await CreateClient(handler).GetExchangesAsync(exchangeIds: [1, 2, 3]);

        Assert.That(handler.LastRequestUri, Does.Contain("exchangeIds=1,2,3"));
    }

    [Test]
    public async Task GetExchangesAsync_OmitsParamWhenNull()
    {
        var handler = new MockHttpMessageHandler("""{ "exchangeInfo": [] }""");

        await CreateClient(handler).GetExchangesAsync(exchangeIds: null);

        Assert.That(handler.LastRequestUri, Does.Not.Contain("exchangeIds"));
    }

    [Test]
    public async Task GetExchangesAsync_Deserializes()
    {
        var json = """
        {
          "exchangeInfo": [
            { "exchangeID": 1, "exchangeDescription": "NYSE" },
            { "exchangeID": 2, "exchangeDescription": "NASDAQ" }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetExchangesAsync();

        Assert.That(result.ExchangeInfo, Has.Count.EqualTo(2));
        Assert.That(result.ExchangeInfo[0].ExchangeId, Is.EqualTo(1));
        Assert.That(result.ExchangeInfo[0].ExchangeDescription, Is.EqualTo("NYSE"));
        Assert.That(result.ExchangeInfo[1].ExchangeId, Is.EqualTo(2));
    }

    [Test]
    public async Task GetExchangesAsync_ThrowsOnError()
    {
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.Unauthorized);

        var exception = Assert.ThrowsAsync<EToroApiException>(
            () => CreateClient(handler).GetExchangesAsync());

        Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(exception.Endpoint, Does.Contain("market-data/exchanges"));
    }

    // ─── GetInstrumentTypesAsync ──────────────────────────────────────────────

    [Test]
    public async Task GetInstrumentTypesAsync_CallsCorrectEndpoint()
    {
        var handler = new MockHttpMessageHandler("""{ "instrumentTypes": [] }""");

        await CreateClient(handler).GetInstrumentTypesAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("market-data/instrument-types"));
    }

    [Test]
    public async Task GetInstrumentTypesAsync_PassesInstrumentTypeIds()
    {
        var handler = new MockHttpMessageHandler("""{ "instrumentTypes": [] }""");

        await CreateClient(handler).GetInstrumentTypesAsync(instrumentTypeIds: [5, 10]);

        Assert.That(handler.LastRequestUri, Does.Contain("instrumentTypeIds=5,10"));
    }

    [Test]
    public async Task GetInstrumentTypesAsync_Deserializes()
    {
        var json = """
        {
          "instrumentTypes": [
            { "instrumentTypeID": 1, "instrumentTypeDescription": "Stocks" },
            { "instrumentTypeID": 2, "instrumentTypeDescription": "ETFs" }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetInstrumentTypesAsync();

        Assert.That(result.InstrumentTypes, Has.Count.EqualTo(2));
        Assert.That(result.InstrumentTypes[0].InstrumentTypeId, Is.EqualTo(1));
        Assert.That(result.InstrumentTypes[0].InstrumentTypeDescription, Is.EqualTo("Stocks"));
    }

    [Test]
    public async Task GetInstrumentTypesAsync_ThrowsOnError()
    {
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.Forbidden);

        var exception = Assert.ThrowsAsync<EToroApiException>(
            () => CreateClient(handler).GetInstrumentTypesAsync());

        Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(exception.Endpoint, Does.Contain("market-data/instrument-types"));
    }

    // ─── GetInstrumentMetadataAsync ───────────────────────────────────────────

    [Test]
    public async Task GetInstrumentMetadataAsync_CallsCorrectEndpoint()
    {
        var handler = new MockHttpMessageHandler("""{ "instrumentDisplayDatas": [] }""");

        await CreateClient(handler).GetInstrumentMetadataAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("market-data/instruments"));
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_PassesInstrumentIds()
    {
        var handler = new MockHttpMessageHandler("""{ "instrumentDisplayDatas": [] }""");

        await CreateClient(handler).GetInstrumentMetadataAsync(instrumentIds: [100, 200]);

        Assert.That(handler.LastRequestUri, Does.Contain("instrumentIds=100,200"));
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_PassesAllFilters()
    {
        var handler = new MockHttpMessageHandler("""{ "instrumentDisplayDatas": [] }""");

        await CreateClient(handler).GetInstrumentMetadataAsync(
            instrumentIds: [1],
            exchangeIds: [2],
            stocksIndustryIds: [3],
            instrumentTypeIds: [4]);

        Assert.That(handler.LastRequestUri, Does.Contain("instrumentIds=1"));
        Assert.That(handler.LastRequestUri, Does.Contain("exchangeIds=2"));
        Assert.That(handler.LastRequestUri, Does.Contain("stocksIndustryIds=3"));
        Assert.That(handler.LastRequestUri, Does.Contain("instrumentTypeIds=4"));
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_Deserializes()
    {
        var json = """
        {
          "instrumentDisplayDatas": [
            {
              "instrumentID": 1586,
              "instrumentDisplayName": "Apple",
              "instrumentTypeID": 1,
              "exchangeID": 6,
              "symbolFull": "AAPL.US",
              "stocksIndustryId": 11,
              "priceSource": "Nasdaq",
              "hasExpirationDate": false,
              "isInternalInstrument": false,
              "images": [
                { "url": "https://cdn.etoro.com/aapl.png", "width": 35, "height": 35, "theme": "light" }
              ]
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetInstrumentMetadataAsync();

        Assert.That(result.InstrumentDisplayDatas, Has.Count.EqualTo(1));
        var inst = result.InstrumentDisplayDatas[0];
        Assert.That(inst.InstrumentId, Is.EqualTo(1586));
        Assert.That(inst.InstrumentDisplayName, Is.EqualTo("Apple"));
        Assert.That(inst.SymbolFull, Is.EqualTo("AAPL.US"));
        Assert.That(inst.PriceSource, Is.EqualTo("Nasdaq"));
        Assert.That(inst.HasExpirationDate, Is.False);
        Assert.That(inst.Images, Has.Count.EqualTo(1));
        Assert.That(inst.Images![0].Url, Is.EqualTo("https://cdn.etoro.com/aapl.png"));
        Assert.That(inst.Images[0].Width, Is.EqualTo(35m));
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_ThrowsOnError()
    {
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.Unauthorized);

        var exception = Assert.ThrowsAsync<EToroApiException>(
            () => CreateClient(handler).GetInstrumentMetadataAsync());

        Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(exception.Endpoint, Does.Contain("market-data/instruments"));
    }

    // ─── GetHistoricalClosingPricesAsync ──────────────────────────────────────

    [Test]
    public async Task GetHistoricalClosingPricesAsync_CallsCorrectEndpoint()
    {
        var handler = new MockHttpMessageHandler("[]");

        await CreateClient(handler).GetHistoricalClosingPricesAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("market-data/instruments/history/closing-price"));
    }

    [Test]
    public async Task GetHistoricalClosingPricesAsync_Deserializes()
    {
        // API returns plain array with space-separated date format
        var json = """
        [
          {
            "instrumentId": 1586,
            "officialClosingPrice": 189.5,
            "isMarketOpen": false,
            "closingPrices": {
              "daily":   { "price": 189.5,  "date": "2025-04-30 00:00:00Z" },
              "weekly":  { "price": 192.0,  "date": "2025-04-25 00:00:00Z" },
              "monthly": { "price": 175.0,  "date": "2025-03-31 00:00:00Z" }
            }
          }
        ]
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetHistoricalClosingPricesAsync();

        Assert.That(result, Has.Count.EqualTo(1));
        var inst = result[0];
        Assert.That(inst.InstrumentId, Is.EqualTo(1586));
        Assert.That(inst.OfficialClosingPrice, Is.EqualTo(189.5m));
        Assert.That(inst.IsMarketOpen, Is.False);
        Assert.That(inst.ClosingPrices, Is.Not.Null);
        Assert.That(inst.ClosingPrices!.Daily!.Price, Is.EqualTo(189.5m));
        Assert.That(inst.ClosingPrices!.Daily.Date, Is.EqualTo("2025-04-30 00:00:00Z"));
        Assert.That(inst.ClosingPrices.Weekly!.Price, Is.EqualTo(192.0m));
        Assert.That(inst.ClosingPrices.Monthly!.Price, Is.EqualTo(175.0m));
    }

    [Test]
    public async Task GetHistoricalClosingPricesAsync_ThrowsOnError()
    {
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.Unauthorized);

        var exception = Assert.ThrowsAsync<EToroApiException>(
            () => CreateClient(handler).GetHistoricalClosingPricesAsync());

        Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(exception.Endpoint, Does.Contain("market-data/instruments/history/closing-price"));
    }

    // ─── GetStocksIndustriesAsync ─────────────────────────────────────────────

    [Test]
    public async Task GetStocksIndustriesAsync_CallsCorrectEndpoint()
    {
        var handler = new MockHttpMessageHandler("""{ "stocksIndustries": [] }""");

        await CreateClient(handler).GetStocksIndustriesAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("market-data/stocks-industries"));
    }

    [Test]
    public async Task GetStocksIndustriesAsync_PassesIndustryIds()
    {
        var handler = new MockHttpMessageHandler("""{ "stocksIndustries": [] }""");

        await CreateClient(handler).GetStocksIndustriesAsync(stocksIndustryIds: [11, 22]);

        Assert.That(handler.LastRequestUri, Does.Contain("stocksIndustryIds=11,22"));
    }

    [Test]
    public async Task GetStocksIndustriesAsync_OmitsParamWhenNull()
    {
        var handler = new MockHttpMessageHandler("""{ "stocksIndustries": [] }""");

        await CreateClient(handler).GetStocksIndustriesAsync(stocksIndustryIds: null);

        Assert.That(handler.LastRequestUri, Does.Not.Contain("stocksIndustryIds"));
    }

    [Test]
    public async Task GetStocksIndustriesAsync_Deserializes()
    {
        var json = """
        {
          "stocksIndustries": [
            { "industryID": 11, "industryName": "Technology" },
            { "industryID": 20, "industryName": "Healthcare" }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetStocksIndustriesAsync();

        Assert.That(result.StocksIndustries, Has.Count.EqualTo(2));
        Assert.That(result.StocksIndustries[0].IndustryId, Is.EqualTo(11));
        Assert.That(result.StocksIndustries[0].IndustryName, Is.EqualTo("Technology"));
        Assert.That(result.StocksIndustries[1].IndustryId, Is.EqualTo(20));
    }

    [Test]
    public async Task GetStocksIndustriesAsync_ThrowsOnError()
    {
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.Forbidden);

        var exception = Assert.ThrowsAsync<EToroApiException>(
            () => CreateClient(handler).GetStocksIndustriesAsync());

        Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(exception.Endpoint, Does.Contain("market-data/stocks-industries"));
    }

    // ─── GetRatesAsync (extended fields) ─────────────────────────────────────

    [Test]
    public async Task GetRatesAsync_DeserializesExtendedFields()
    {
        var json = """
        {
          "rates": [
            {
              "instrumentID": 1586,
              "ask": 190.1,
              "bid": 190.0,
              "lastExecution": 190.05,
              "conversionRateAsk": 1.0,
              "conversionRateBid": 1.0,
              "date": "2025-04-30T10:00:00Z",
              "priceRateID": 999,
              "unitMargin": 190.05,
              "unitMarginAsk": 190.1,
              "unitMarginBid": 190.0
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetRatesAsync([1586]);

        Assert.That(result.Rates, Has.Count.EqualTo(1));
        Assert.That(result.Rates[0].PriceRateId, Is.EqualTo(999));
        Assert.That(result.Rates[0].UnitMargin, Is.EqualTo(190.05m));
    }

    // ─── GetCandlesAsync (extended fields) ───────────────────────────────────

    [Test]
    public async Task GetCandlesAsync_DeserializesRangeFields()
    {
        var json = """
        {
          "interval": "OneDay",
          "candles": [
            {
              "instrumentID": 1586,
              "fromDate": "2025-04-29T00:00:00Z",
              "open": 188.0,
              "high": 191.0,
              "low": 187.5,
              "close": 189.5,
              "volume": 55000000,
              "rangeOpen": 188.0,
              "rangeClose": 189.5,
              "rangeHigh": 191.0,
              "rangeLow": 187.5
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetCandlesAsync(1586);

        Assert.That(result.Candles, Has.Count.EqualTo(1));
        var candle = result.Candles[0];
        Assert.That(candle.RangeOpen, Is.EqualTo(188.0m));
        Assert.That(candle.RangeClose, Is.EqualTo(189.5m));
        Assert.That(candle.RangeHigh, Is.EqualTo(191.0m));
        Assert.That(candle.RangeLow, Is.EqualTo(187.5m));
    }
}
