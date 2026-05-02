using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.UserInfo;
using GrznarAi.Trading.ReadOnly.Tests.Helpers;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class UserInfoClientTests
{
    private const string BaseUrl = "https://public-api.etoro.com/api/v1/";

    private static EToroClient CreateClient(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        return new EToroClient(httpClient);
    }

    [Test]
    public async Task GetUserProfilesAsync_CallsEndpointWithUsernamesAndCidList()
    {
        var handler = new MockHttpMessageHandler("""{ "users": [] }""");

        await CreateClient(handler).GetUserProfilesAsync(["trader one", "trader2"], [123, 456]);

        Assert.That(handler.LastRequestUri, Does.Contain("user-info/people"));
        Assert.That(handler.LastRequestUri, Does.Contain("usernames=trader one,trader2"));
        Assert.That(handler.LastRequestUri, Does.Contain("cidList=123,456"));
    }

    [Test]
    public async Task GetUserProfilesAsync_EscapesCsvStringValues()
    {
        var handler = new MockHttpMessageHandler("""{ "users": [] }""");

        await CreateClient(handler).GetUserProfilesAsync(["a+b"]);

        Assert.That(handler.LastRequestUri, Does.Contain("usernames=a%2Bb"));
    }

    [Test]
    public async Task GetUserProfilesAsync_DeserializesProfile()
    {
        var json = """
        {
          "users": [
            {
              "gcid": 1536861,
              "realCID": 1563191,
              "demoCID": 1563192,
              "username": "exampleuser",
              "language": 1,
              "languageIsoCode": "en-GB",
              "country": 54,
              "allowDisplayFullName": false,
              "userBio": { "gcid": 1536861, "languageCode": "en", "aboutMe": "bio", "strategyID": 10 },
              "whiteLabel": 1,
              "optOut": true,
              "homepage": null,
              "playerStatus": null,
              "piLevel": 0,
              "isPi": false,
              "avatars": [{ "url": "https://example.com/avatar.png", "width": 35, "height": 35, "type": "Resized" }],
              "masterAccountCid": null,
              "accountType": 1,
              "fundType": null,
              "isVerified": false,
              "verificationLevel": 1,
              "accountStatus": 1,
              "gdprInfo": { "accountStatus": 1, "playerStatus": 2, "playerStatusReason": 3 },
              "firstName": "Example",
              "lastName": "User",
              "aboutMeShort": "short",
              "customerRestrictions": [{ "CID": 1563191, "restrictionTypeID": 2, "reasonID": 3, "occured": "2025-01-01T00:00:00Z" }],
              "userFlowSignature": "signature"
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetUserProfilesAsync(["exampleuser"]);

        Assert.That(result.Users, Has.Count.EqualTo(1));
        var user = result.Users[0];
        Assert.That(user.Username, Is.EqualTo("exampleuser"));
        Assert.That(user.RealCid, Is.EqualTo(1563191));
        Assert.That(user.UserBio!.StrategyId, Is.EqualTo(10));
        Assert.That(user.Avatars![0].Type, Is.EqualTo("Resized"));
        Assert.That(user.CustomerRestrictions![0].RestrictionTypeId, Is.EqualTo(2));
    }

    [Test]
    public async Task SearchUsersAsync_CallsEndpointWithAllFilters()
    {
        var handler = new MockHttpMessageHandler("""{ "totalItems": 0, "items": [] }""");

        await CreateClient(handler).SearchUsersAsync(new UserSearchRequest
        {
            Period = UserInfoPeriod.LastYear,
            IsTestAccount = false,
            OptIn = true,
            Blocked = false,
            PopularInvestor = true,
            GainMax = 100,
            MaxDailyRiskScoreMin = 1,
            MaxDailyRiskScoreMax = 7,
            MaxMonthlyRiskScoreMin = 1,
            MaxMonthlyRiskScoreMax = 6,
            WeeksSinceRegistrationMin = 75,
            CountryId = 101,
            InstrumentId = -5,
            InstrumentPctMin = 10,
            InstrumentPctMax = 100,
            Page = 2,
            PageSize = 50,
            Sort = "-copiers"
        });

        Assert.That(handler.LastRequestUri, Does.Contain("user-info/people/search"));
        Assert.That(handler.LastRequestUri, Does.Contain("period=LastYear"));
        Assert.That(handler.LastRequestUri, Does.Contain("isTestAccount=false"));
        Assert.That(handler.LastRequestUri, Does.Contain("optIn=true"));
        Assert.That(handler.LastRequestUri, Does.Contain("blocked=false"));
        Assert.That(handler.LastRequestUri, Does.Contain("popularInvestor=true"));
        Assert.That(handler.LastRequestUri, Does.Contain("gainMax=100"));
        Assert.That(handler.LastRequestUri, Does.Contain("weeksSinceRegistrationMin=75"));
        Assert.That(handler.LastRequestUri, Does.Contain("instrumentId=-5"));
        Assert.That(handler.LastRequestUri, Does.Contain("instrumentPctMax=100"));
        Assert.That(handler.LastRequestUri, Does.Contain("page=2"));
        Assert.That(handler.LastRequestUri, Does.Contain("pageSize=50"));
        Assert.That(handler.LastRequestUri, Does.Contain("sort=-copiers"));
    }

    [Test]
    public async Task SearchUsersAsync_DeserializesCompleteItem()
    {
        var json = """
        {
          "totalItems": 1,
          "items": [
            {
              "customerId": 42, "userName": "trader", "fullName": "Trader Name",
              "hasAvatar": true, "isSocialConnected": true, "isTestAccount": false,
              "displayFullName": true, "bonusOnly": false, "blocked": false,
              "verified": true, "popularInvestor": true, "copyBlock": false,
              "isFund": false, "isBronze": true, "fundType": 2, "tags": [1, 2],
              "gain": 12.5, "dailyGain": 0.5, "thisWeekGain": 1.5,
              "riskScore": 4, "maxDailyRiskScore": 6, "maxMonthlyRiskScore": 7,
              "copiers": 100, "copiedTrades": 50, "copyTradesPct": 20.5,
              "copyInvestmentPct": 30.5, "baseLineCopiers": 90, "copiersGain": 10.5,
              "aumTier": 3, "aumTierV2": 4, "aumTierDesc": "Tier 4",
              "virtualCopiers": 10, "trades": 200, "winRatio": 55.5,
              "dailyDd": 1.1, "weeklyDd": 2.2, "profitableWeeksPct": 60,
              "profitableMonthsPct": 70, "velocity": 0.3, "exposure": 80,
              "avgPosSize": 4.4, "optimalCopyPosSize": 5.5,
              "highLeveragePct": 6.6, "mediumLeveragePct": 7.7, "lowLeveragePct": 8.8,
              "peakToValley": 9.9, "peakToValleyStart": "2025-01-01T00:00:00Z",
              "peakToValleyEnd": "2025-02-01T00:00:00Z", "longPosPct": 75,
              "topTradedInstrumentId": 1001, "topTradedAssetClassId": 4,
              "topTradedInstrumentPct": 33.3, "totalTradedInstruments": 12,
              "activeWeeks": 40, "firstActivity": 2, "lastActivity": 3,
              "activeWeeksPct": 90, "weeksSinceRegistration": 200,
              "country": "CZ", "affiliateId": 5, "instrumentPct": 25,
              "countryId": 54, "isPopularInvestor": true, "topTradedAssetId": 4
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).SearchUsersAsync(new UserSearchRequest
        {
            Period = UserInfoPeriod.LastYear
        });

        Assert.That(result.TotalItems, Is.EqualTo(1));
        var item = result.Items[0];
        Assert.That(item.UserName, Is.EqualTo("trader"));
        Assert.That(item.Copiers, Is.EqualTo(100));
        Assert.That(item.Tags, Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(item.TopTradedInstrumentId, Is.EqualTo(1001));
    }

    [Test]
    public void SearchUsersAsync_NullRequest_ThrowsArgumentNullException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentNullException>(() => client.SearchUsersAsync(null!));
    }

    [Test]
    public void SearchUsersAsync_PageSizeAboveLimit_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.SearchUsersAsync(new UserSearchRequest
        {
            Period = UserInfoPeriod.CurrYear,
            PageSize = EToroRequestLimits.MaxPageSize + 1
        }));
    }

    [Test]
    public async Task GetUserDailyGainAsync_CallsEndpointWithDatesAndDailyType()
    {
        var handler = new MockHttpMessageHandler("""[]""");

        await CreateClient(handler).GetUserDailyGainAsync(
            "trader/one",
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31));

        Assert.That(handler.LastRequestUri, Does.Contain("user-info/people/trader%2Fone/daily-gain"));
        Assert.That(handler.LastRequestUri, Does.Contain("minDate=2024-01-01"));
        Assert.That(handler.LastRequestUri, Does.Contain("maxDate=2024-12-31"));
        Assert.That(handler.LastRequestUri, Does.Contain("type=Daily"));
    }

    [Test]
    public async Task GetUserDailyGainAsync_DeserializesDailyArray()
    {
        var handler = new MockHttpMessageHandler("""
        [
          { "timestamp": "2023-01-01T00:00:00Z", "gain": 0 },
          { "timestamp": "2023-01-02T00:00:00Z", "gain": 0.14 }
        ]
        """);

        var result = await CreateClient(handler).GetUserDailyGainAsync(
            "trader",
            new DateOnly(2023, 1, 1),
            new DateOnly(2023, 1, 2));

        Assert.That(result.IsDaily, Is.True);
        Assert.That(result.Daily, Has.Count.EqualTo(2));
        Assert.That(result.Daily![1].Gain, Is.EqualTo(0.14m));
    }

    [Test]
    public async Task GetUserDailyGainAsync_DeserializesPeriodObject()
    {
        var handler = new MockHttpMessageHandler("""{ "gain": 7.52 }""");

        var result = await CreateClient(handler).GetUserDailyGainAsync(
            "trader",
            new DateOnly(2023, 1, 1),
            new DateOnly(2023, 1, 31),
            UserDailyGainType.Period);

        Assert.That(result.IsPeriod, Is.True);
        Assert.That(result.Gain, Is.EqualTo(7.52m));
    }

    [Test]
    public async Task GetUserGainAsync_DeserializesMonthlyAndYearly()
    {
        var handler = new MockHttpMessageHandler("""
        {
          "monthly": [{ "timestamp": "2024-01-01T00:00:00Z", "gain": 1.23 }],
          "yearly": [{ "timestamp": "2024-01-01T00:00:00Z", "gain": 12.34 }]
        }
        """);

        var result = await CreateClient(handler).GetUserGainAsync("trader");

        Assert.That(handler.LastRequestUri, Does.Contain("user-info/people/trader/gain"));
        Assert.That(result.Monthly![0].Gain, Is.EqualTo(1.23m));
        Assert.That(result.Yearly![0].Gain, Is.EqualTo(12.34m));
    }

    [Test]
    public void GetUserGainAsync_LongUsername_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);
        var username = new string('u', EToroRequestLimits.MaxUsernameLength + 1);

        Assert.ThrowsAsync<ArgumentException>(() => client.GetUserGainAsync(username));
    }

    [Test]
    public void GetUserGainAsync_UsernameWithControlCharacter_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentException>(() => client.GetUserGainAsync("bad\nuser"));
    }

    [Test]
    public async Task GetUserLivePortfolioAsync_DeserializesPortfolio()
    {
        var handler = new MockHttpMessageHandler("""
        {
          "realizedCreditPct": 10.5,
          "unrealizedCreditPct": 2.5,
          "positions": [
            {
              "positionId": 123, "openTimestamp": "2025-01-01T00:00:00Z",
              "openRate": 100.1, "instrumentId": 1001, "isBuy": true,
              "leverage": 1, "takeProfitRate": 150, "stopLossRate": 80,
              "socialTradeId": null, "parentPositionId": null,
              "investmentPct": 20, "netProfit": 3.4, "trailingStopLoss": false
            }
          ],
          "socialTrades": [
            {
              "socialTradeId": 987, "parentUsername": "parent",
              "stopLossPercentage": 40, "openTimestamp": "2025-01-02T00:00:00Z",
              "investmentPct": 30, "openInvestmentPct": 10, "netProfit": 5,
              "openNetProfit": 2, "closedNetProfit": 3, "realizedPct": 70,
              "unrealizedPct": 30, "isClosing": false, "positions": []
            }
          ]
        }
        """);

        var result = await CreateClient(handler).GetUserLivePortfolioAsync("trader");

        Assert.That(handler.LastRequestUri, Does.Contain("user-info/people/trader/portfolio/live"));
        Assert.That(result.Positions![0].InstrumentId, Is.EqualTo(1001));
        Assert.That(result.SocialTrades![0].ParentUsername, Is.EqualTo("parent"));
    }

    [Test]
    public async Task GetUserTradeInfoAsync_CallsEndpointAndDeserializes()
    {
        var handler = new MockHttpMessageHandler("""
        {
          "userName": "trader", "fullName": "Trader Name", "weeksSinceRegistration": 100,
          "countryId": 54, "affiliateId": 1, "isPopularInvestor": true, "isFund": false,
          "hasAvatar": true, "gain": 12.5, "dailyGain": 0.5, "thisWeekGain": 1.5,
          "riskScore": 4, "maxDailyRiskScore": 6, "maxMonthlyRiskScore": 7,
          "copiers": 100, "copiedTrades": 50, "copyTradesPct": 20.5,
          "copyInvestmentPct": 30.5, "baseLineCopiers": 90, "copiersGain": 10.5,
          "aumTier": 3, "aumTierDesc": "Tier 3", "fundType": null,
          "virtualCopiers": 10, "trades": 200, "topTradedInstrumentId": 1001,
          "topTradedAssetId": 4, "winRatio": 55.5, "dailyDd": 1.1,
          "weeklyDd": 2.2, "peakToValley": 9.9, "profitableWeeksPct": 60,
          "profitableMonthsPct": 70, "avgPosSize": 4.4, "highLeveragePct": 6.6,
          "mediumLeveragePct": 7.7, "lowLeveragePct": 8.8, "firstActivity": 2,
          "lastActivity": 3, "activeWeeksPct": 90, "instrumentPct": 25
        }
        """);

        var result = await CreateClient(handler).GetUserTradeInfoAsync("trader", UserInfoPeriod.LastTwoYears);

        Assert.That(handler.LastRequestUri, Does.Contain("user-info/people/trader/tradeinfo"));
        Assert.That(handler.LastRequestUri, Does.Contain("period=LastTwoYears"));
        Assert.That(result.UserName, Is.EqualTo("trader"));
        Assert.That(result.Copiers, Is.EqualTo(100));
    }
}
