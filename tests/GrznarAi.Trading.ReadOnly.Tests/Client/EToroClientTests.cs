using System.Net;
using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Models.Market;
using GrznarAi.Trading.ReadOnly.Models.Social;
using GrznarAi.Trading.ReadOnly.Tests.Helpers;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class EToroClientTests
{
    private const string BaseUrl = "https://public-api.etoro.com/api/v1/";

    private static EToroClient CreateClient(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        return new EToroClient(httpClient);
    }

    private static EToroClient CreateClient(
        MockHttpMessageHandler handler,
        Action<ErrorHandlingOptions> configureErrorHandling)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var options = new EToroOptions();
        configureErrorHandling(options.ErrorHandling);
        return new EToroClient(httpClient, Options.Create(options));
    }

    [Test]
    public void FailedRequest_RedactsSensitiveJsonResponseBody()
    {
        const string json = """
        {
          "apiKey": "api-secret",
          "userKey": "user-secret",
          "token": "token-secret",
          "access_token": "access-secret",
          "refresh_token": "refresh-secret",
          "authorization": "Bearer auth-secret",
          "secret": "plain-secret",
          "password": "password-secret",
          "x-api-key": "header-api-secret",
          "x-user-key": "header-user-secret",
          "message": "safe detail"
        }
        """;
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.BadRequest);

        var exception = Assert.ThrowsAsync<EToroApiException>(() =>
            CreateClient(handler).GetPnlAsync(EToroEnvironment.Real));

        Assert.That(exception!.ResponseBody, Does.Contain("safe detail"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("api-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("user-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("token-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("access-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("refresh-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("auth-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("plain-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("password-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("header-api-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("header-user-secret"));
        Assert.That(exception.Message, Does.Not.Contain("safe detail"));
        Assert.That(exception.Message, Does.Not.Contain("api-secret"));
    }

    [Test]
    public void FailedRequest_RedactsSensitiveTextResponseBody()
    {
        const string body = "error apiKey=api-secret authorization=Bearer-secret x-user-key:header-user-secret message=safe";
        var handler = new MockHttpMessageHandler(body, HttpStatusCode.BadRequest);

        var exception = Assert.ThrowsAsync<EToroApiException>(() =>
            CreateClient(handler).GetPnlAsync(EToroEnvironment.Real));

        Assert.That(exception!.ResponseBody, Does.Contain("message=safe"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("api-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("Bearer-secret"));
        Assert.That(exception.ResponseBody, Does.Not.Contain("header-user-secret"));
        Assert.That(exception.Message, Does.Not.Contain("api-secret"));
    }

    [Test]
    public void FailedRequest_WhenRedactionDisabled_KeepsRawResponseBody()
    {
        const string body = "error apiKey=api-secret message=safe";
        var handler = new MockHttpMessageHandler(body, HttpStatusCode.BadRequest);

        var exception = Assert.ThrowsAsync<EToroApiException>(() =>
            CreateClient(handler, options => options.RedactResponseBody = false)
                .GetPnlAsync(EToroEnvironment.Real));

        Assert.That(exception!.ResponseBody, Is.EqualTo(body));
    }

    [Test]
    public void FailedRequest_WhenResponseBodyDisabled_DoesNotReadBodyIntoException()
    {
        const string body = "error message=safe";
        var handler = new MockHttpMessageHandler(body, HttpStatusCode.BadRequest);

        var exception = Assert.ThrowsAsync<EToroApiException>(() =>
            CreateClient(handler, options => options.IncludeResponseBody = false)
                .GetPnlAsync(EToroEnvironment.Real));

        Assert.That(exception!.ResponseBody, Is.Null);
    }

    [Test]
    public void FailedRequest_TruncatesResponseBodyUsingConfiguredLimit()
    {
        const string body = "1234567890";
        var handler = new MockHttpMessageHandler(body, HttpStatusCode.BadRequest);

        var exception = Assert.ThrowsAsync<EToroApiException>(() =>
            CreateClient(handler, options => options.MaxResponseBodyLength = 4)
                .GetPnlAsync(EToroEnvironment.Real));

        Assert.That(exception!.ResponseBody, Is.EqualTo("1234"));
    }

    [Test]
    public void FailedRequest_WhenMaxResponseBodyLengthIsNull_KeepsFullBody()
    {
        var body = new string('a', ErrorHandlingOptions.DefaultMaxResponseBodyLength + 10);
        var handler = new MockHttpMessageHandler(body, HttpStatusCode.BadRequest);

        var exception = Assert.ThrowsAsync<EToroApiException>(() =>
            CreateClient(handler, options => options.MaxResponseBodyLength = null)
                .GetPnlAsync(EToroEnvironment.Real));

        Assert.That(exception!.ResponseBody, Has.Length.EqualTo(body.Length));
    }

    // ─── PnL ─────────────────────────────────────────────────────────────────

    [Test]
    public async Task GetPnlAsync_Real_CallsCorrectEndpoint()
    {
        var json = """
        {
          "clientPortfolio": {
            "credit": 1000,
            "bonusCredit": 0,
            "positions": [],
            "mirrors": [],
            "ordersForOpen": [],
            "orders": []
          }
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetPnlAsync(EToroEnvironment.Real);

        Assert.That(handler.LastRequestUri, Does.Contain("trading/info/real/pnl"));
        Assert.That(result.Credit, Is.EqualTo(1000m));
    }

    [Test]
    public async Task GetPnlAsync_Demo_CallsCorrectEndpoint()
    {
        var json = """
        {
          "clientPortfolio": {
            "credit": 50000,
            "bonusCredit": 0,
            "positions": [],
            "mirrors": [],
            "ordersForOpen": [],
            "orders": []
          }
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetPnlAsync(EToroEnvironment.Demo);

        Assert.That(handler.LastRequestUri, Does.Contain("trading/info/demo/pnl"));
        Assert.That(result.Credit, Is.EqualTo(50000m));
    }

    [Test]
    public async Task GetPnlAsync_UnwrapsClientPortfolio()
    {
        var json = """
        {
          "clientPortfolio": {
            "credit": 2000,
            "bonusCredit": 100,
            "positions": [
              { "positionID": 1, "instrumentID": 1001, "amount": 500, "unrealizedPnL": { "pnL": 25.5, "pnlAssetCurrency": 25.5, "exposureInAccountCurrency": 525.5, "exposureInAssetCurrency": 525.5, "marginInAccountCurrency": 500, "marginInAssetCurrency": 500 } }
            ],
            "mirrors": [],
            "ordersForOpen": [],
            "orders": []
          }
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetPnlAsync(EToroEnvironment.Real);

        Assert.That(result.Credit, Is.EqualTo(2000m));
        Assert.That(result.BonusCredit, Is.EqualTo(100m));
        Assert.That(result.Positions, Has.Count.EqualTo(1));
        Assert.That(result.Positions[0].UnrealizedPnL!.PnL, Is.EqualTo(25.5m));
    }

    // ─── Portfolio ────────────────────────────────────────────────────────────

    [Test]
    public async Task GetPortfolioAsync_Real_CallsCorrectEndpoint()
    {
        var json = """{ "clientPortfolio": { "positions": [], "credit": 500, "bonusCredit": 0 } }""";
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetPortfolioAsync(EToroEnvironment.Real);

        Assert.That(handler.LastRequestUri, Does.Contain("trading/info/portfolio"));
        Assert.That(handler.LastRequestUri, Does.Not.Contain("trading/info/real/portfolio"));
        Assert.That(result.Positions, Is.Empty);
    }

    [Test]
    public async Task GetPortfolioAsync_Demo_CallsCorrectEndpoint()
    {
        var json = """{ "clientPortfolio": { "positions": [], "credit": 50000, "bonusCredit": 0 } }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetPortfolioAsync(EToroEnvironment.Demo);

        Assert.That(handler.LastRequestUri, Does.Contain("trading/info/portfolio"));
        Assert.That(handler.LastRequestUri, Does.Not.Contain("trading/info/demo/portfolio"));
    }

    [Test]
    public async Task GetPortfolioAsync_UnwrapsClientPortfolio()
    {
        var json = """
        {
          "clientPortfolio": {
            "positions": [
              {
                "positionID": 123456,
                "instrumentID": 2002,
                "isBuy": true,
                "leverage": 1,
                "investedAmount": 300,
                "units": 10,
                "openRate": 30,
                "openDateTime": "2024-01-01T10:00:00Z",
                "currentRate": 35,
                "netProfit": 50,
                "mirrorID": 0
              }
            ],
            "credit": 700,
            "bonusCredit": 0
          }
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetPortfolioAsync(EToroEnvironment.Real);

        Assert.That(result.Positions, Has.Count.EqualTo(1));
        Assert.That(result.Positions[0].PositionId, Is.EqualTo(123456));
        Assert.That(result.Credit, Is.EqualTo(700m));
    }

    // ─── Trade History ────────────────────────────────────────────────────────

    [Test]
    public async Task GetTradeHistoryAsync_CallsCorrectEndpointWithAllParams()
    {
        var handler = new MockHttpMessageHandler("[]");
        var minDate = new DateOnly(2024, 1, 1);

        await CreateClient(handler).GetTradeHistoryAsync(minDate, page: 2, pageSize: 50);

        Assert.That(handler.LastRequestUri, Does.Contain("trading/info/trade/history"));
        Assert.That(handler.LastRequestUri, Does.Contain("minDate=2024-01-01"));
        Assert.That(handler.LastRequestUri, Does.Contain("page=2"));
        Assert.That(handler.LastRequestUri, Does.Contain("pageSize=50"));
    }

    [Test]
    public void GetTradeHistoryAsync_InvalidPageSize_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler("[]");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetTradeHistoryAsync(new DateOnly(2024, 1, 1), pageSize: 0));
    }

    [Test]
    public async Task GetTradeHistoryAsync_EmptyArray_ReturnsEmptyList()
    {
        var handler = new MockHttpMessageHandler("[]");

        var result = await CreateClient(handler).GetTradeHistoryAsync(DateTimeOffset.UtcNow.AddYears(-1));

        Assert.That(result.Trades, Is.Empty);
    }

    [Test]
    public async Task GetTradeHistoryAsync_DeserializesTrades()
    {
        // JSON pole — reálný formát API odpovědi
        var json = """
        [
          {
            "positionId": 9001,
            "instrumentId": 3003,
            "isBuy": true,
            "leverage": 1,
            "investment": 49.89,
            "initialInvestment": 49.89,
            "openRate": 100.0,
            "closeRate": 120.5,
            "openTimestamp": "2024-01-01T09:00:00Z",
            "closeTimestamp": "2024-06-01T15:00:00Z",
            "netProfit": 10.50,
            "fees": 0.0,
            "units": 0.5,
            "stopLossRate": 80.0,
            "takeProfitRate": 150.0,
            "trailingStopLoss": false,
            "orderId": 903092332,
            "socialTradeId": 0,
            "parentPositionId": 0
          }
        ]
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetTradeHistoryAsync(DateTimeOffset.UtcNow.AddYears(-1));

        Assert.That(result.Trades, Has.Count.EqualTo(1));
        Assert.That(result.Trades[0].PositionId, Is.EqualTo(9001));
        Assert.That(result.Trades[0].NetProfit, Is.EqualTo(10.50m));
        Assert.That(result.Trades[0].Investment, Is.EqualTo(49.89m));
        Assert.That(result.Trades[0].CloseRate, Is.EqualTo(120.5m));
    }

    // ─── Order ────────────────────────────────────────────────────────────────

    [Test]
    public async Task GetOrderAsync_Real_CallsCorrectEndpoint()
    {
        var json = """
        {
          "orderID": 555001,
          "CID": 100001,
          "statusID": 1,
          "orderType": 1,
          "instrumentID": 1001,
          "requestOccurred": "2024-03-15T10:30:00Z",
          "positions": []
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetOrderAsync(EToroEnvironment.Real, 555001);

        Assert.That(handler.LastRequestUri, Does.Contain("trading/info/real/orders/555001"));
        Assert.That(result.OrderId, Is.EqualTo(555001));
        Assert.That(result.StatusId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetOrderAsync_Demo_CallsCorrectEndpoint()
    {
        var json = """
        {
          "orderID": 666002,
          "CID": 100001,
          "statusID": 0,
          "orderType": 2,
          "instrumentID": 2002,
          "requestOccurred": "2024-04-01T08:00:00Z"
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetOrderAsync(EToroEnvironment.Demo, 666002);

        Assert.That(handler.LastRequestUri, Does.Contain("trading/info/demo/orders/666002"));
    }

    [Test]
    public async Task GetOrderAsync_WithPositions_DeserializesCorrectly()
    {
        var json = """
        {
          "orderID": 777003,
          "CID": 100002,
          "statusID": 1,
          "orderType": 1,
          "instrumentID": 3003,
          "requestOccurred": "2024-05-10T14:00:00Z",
          "token": "tok-abc",
          "amount": 500.00,
          "units": 3.5,
          "positions": [
            {
              "positionID": 888001,
              "orderType": 1,
              "occurred": "2024-05-10T14:01:00Z",
              "rate": 142.50,
              "units": 3.5,
              "amount": 498.75,
              "isOpen": true,
              "conversionRate": 1.0
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetOrderAsync(EToroEnvironment.Real, 777003);

        Assert.That(result.OrderId, Is.EqualTo(777003));
        Assert.That(result.Token, Is.EqualTo("tok-abc"));
        Assert.That(result.Amount, Is.EqualTo(500.00m));
        Assert.That(result.Positions, Has.Count.EqualTo(1));
        Assert.That(result.Positions![0].PositionId, Is.EqualTo(888001));
        Assert.That(result.Positions[0].Rate, Is.EqualTo(142.50m));
        Assert.That(result.Positions[0].IsOpen, Is.True);
    }

    // ─── Search Instruments ───────────────────────────────────────────────────

    [Test]
    public async Task SearchInstrumentsAsync_CallsCorrectEndpoint()
    {
        var json = """{ "items": [], "totalItems": 0, "page": 1, "pageSize": 20 }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = [InstrumentFields.InstrumentId, InstrumentFields.DisplayName]
        });

        Assert.That(handler.LastRequestUri, Does.Contain("market-data/search"));
        Assert.That(handler.LastRequestUri, Does.Contain("fields="));
    }

    [Test]
    public async Task SearchInstrumentsAsync_WithSearchText_AppendsParam()
    {
        var json = """{ "items": [], "totalItems": 0, "page": 1, "pageSize": 20 }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = [InstrumentFields.InstrumentId],
            SearchText = "AAPL"
        });

        Assert.That(handler.LastRequestUri, Does.Contain("searchText=AAPL"));
    }

    [Test]
    public async Task SearchInstrumentsAsync_WithPagination_AppendsPageSizeParam()
    {
        var json = """{ "items": [], "totalItems": 0, "page": 1, "pageSize": 10 }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = [InstrumentFields.InstrumentId],
            PageSize = 10,
        });

        Assert.That(handler.LastRequestUri, Does.Contain("pageSize=10"));
    }

    [Test]
    public void SearchInstrumentsAsync_MoreThan5Fields_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields =
            [
                InstrumentFields.InstrumentId,
                InstrumentFields.DisplayName,
                InstrumentFields.Symbol,
                InstrumentFields.ExchangeId,
                InstrumentFields.InstrumentType,
                InstrumentFields.CurrentRate
            ]
        }));
    }

    [Test]
    public void SearchInstrumentsAsync_EmptyFields_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentException>(() => client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = []
        }));
    }

    [Test]
    public void SearchInstrumentsAsync_NullRequest_ThrowsArgumentNullException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentNullException>(() => client.SearchInstrumentsAsync(null!));
    }

    [Test]
    public void SearchInstrumentsAsync_NullFields_ThrowsArgumentNullException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentNullException>(() => client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = null!
        }));
    }

    [Test]
    public void SearchInstrumentsAsync_UnknownField_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentException>(() => client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = ["NotARealField"]
        }));
    }

    [Test]
    public void SearchInstrumentsAsync_PageSizeAboveLimit_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = [InstrumentFields.InstrumentId],
            PageSize = EToroRequestLimits.MaxPageSize + 1
        }));
    }

    [Test]
    public void SearchInstrumentsAsync_SearchTextWithControlCharacter_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentException>(() => client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = [InstrumentFields.InstrumentId],
            SearchText = "AAPL\nMSFT"
        }));
    }

    // ─── Rates ────────────────────────────────────────────────────────────────

    [Test]
    public async Task GetRatesAsync_CallsCorrectEndpointWithIds()
    {
        var json = """{ "rates": [{ "instrumentID": 1001, "ask": 150.5, "bid": 149.5, "lastExecution": 150, "conversionRateAsk": 1, "conversionRateBid": 1, "date": "2024-01-01T12:00:00Z" }] }""";
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetRatesAsync([1001, 1002]);

        Assert.That(handler.LastRequestUri, Does.Contain("market-data/instruments/rates"));
        Assert.That(handler.LastRequestUri, Does.Contain("instrumentIds=1001,1002"));
        Assert.That(result.Rates[0].Ask, Is.EqualTo(150.5m));
    }

    [Test]
    public void GetRatesAsync_NullInstrumentIds_ThrowsArgumentNullException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentNullException>(() => client.GetRatesAsync(null!));
    }

    [Test]
    public void GetRatesAsync_TooManyInstrumentIds_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentException>(() =>
            client.GetRatesAsync(Enumerable.Range(1, EToroRequestLimits.MaxCsvIds + 1)));
    }

    // ─── Candles ─────────────────────────────────────────────────────────────

    [Test]
    public async Task GetCandlesAsync_CallsCorrectPathWithAllSegments()
    {
        var json = """{ "interval": "OneDay", "candles": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetCandlesAsync(2002, CandleInterval.OneDay, CandleDirection.Desc, 50);

        Assert.That(handler.LastRequestUri, Does.Contain("market-data/instruments/2002/history/candles/desc/OneDay/50"));
    }

    [Test]
    public async Task GetCandlesAsync_AscDirection_UsesAscSegment()
    {
        var json = """{ "interval": "OneWeek", "candles": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetCandlesAsync(3003, CandleInterval.OneWeek, CandleDirection.Asc, 10);

        Assert.That(handler.LastRequestUri, Does.Contain("/asc/"));
        Assert.That(handler.LastRequestUri, Does.Contain("OneWeek"));
        Assert.That(handler.LastRequestUri, Does.Contain("/10"));
    }

    [Test]
    public void GetCandlesAsync_ZeroInstrumentId_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetCandlesAsync(0));
    }

    [Test]
    public void GetCandlesAsync_CandlesCountAboveLimit_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetCandlesAsync(1, candlesCount: EToroRequestLimits.MaxCandlesCount + 1));
    }

    // ─── Watchlists ───────────────────────────────────────────────────────────

    [Test]
    public async Task GetUserWatchlistsAsync_CallsCorrectEndpointWithParams()
    {
        var json = """{ "status": 200, "isSucceeded": true, "watchlists": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetUserWatchlistsAsync(
            itemsPerPageForSingle: 50, ensureBuiltinWatchlists: false, addRelatedAssets: true);

        Assert.That(handler.LastRequestUri, Does.Contain("watchlists"));
        Assert.That(handler.LastRequestUri, Does.Contain("itemsPerPageForSingle=50"));
        Assert.That(handler.LastRequestUri, Does.Contain("ensureBuiltinWatchlists=false"));
        Assert.That(handler.LastRequestUri, Does.Contain("addRelatedAssets=true"));
    }

    // ─── Popular Investors ────────────────────────────────────────────────────

    [Test]
    public async Task GetPopularInvestorsAsync_CallsCorrectEndpointWithPeriod()
    {
        var json = """{ "totalItems": 0, "items": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetPopularInvestorsAsync(new PopularInvestorsRequest
        {
            Period = PopularInvestorPeriod.CurrYear
        });

        Assert.That(handler.LastRequestUri, Does.Contain("user-info/people/search"));
        Assert.That(handler.LastRequestUri, Does.Contain("period=CurrYear"));
    }

    [Test]
    public async Task GetPopularInvestorsAsync_WithFilters_AppendsAllParams()
    {
        var json = """{ "totalItems": 0, "items": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetPopularInvestorsAsync(new PopularInvestorsRequest
        {
            Period = PopularInvestorPeriod.SixMonthsAgo,
            PopularInvestor = true,
            GainMax = 200,
            MaxDailyRiskScoreMin = 1,
            MaxDailyRiskScoreMax = 4,
            CountryId = 42,
            Page = 2,
            PageSize = 10,
            Sort = "-copiers"
        });

        var uri = handler.LastRequestUri!;
        Assert.That(uri, Does.Contain("popularInvestor=true"));
        Assert.That(uri, Does.Contain("gainMax=200"));
        Assert.That(uri, Does.Contain("maxDailyRiskScoreMin=1"));
        Assert.That(uri, Does.Contain("maxDailyRiskScoreMax=4"));
        Assert.That(uri, Does.Contain("countryId=42"));
        Assert.That(uri, Does.Contain("page=2"));
        Assert.That(uri, Does.Contain("pageSize=10"));
        Assert.That(uri, Does.Contain("sort="));
    }
}
