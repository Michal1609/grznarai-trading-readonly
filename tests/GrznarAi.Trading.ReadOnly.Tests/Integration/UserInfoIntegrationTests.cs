using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.UserInfo;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Tests.Integration;

/// <summary>
/// Integracni testy pro sekci USER INFO.
/// Spusteni: dotnet test --filter "Category=Integration"
/// Nutne: nastavit EToroOptions__ApiKey + EToroOptions__UserKey nebo lokalni appsettings.test.json
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze rucni spusteni - vyzaduje realne API klice mimo git")]
public class UserInfoIntegrationTests
{
    private IEToroClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();
    }

    [Test]
    public async Task SearchUsersAsync_ReturnsUsers()
    {
        var result = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period = UserInfoPeriod.CurrYear,
            PageSize = 5
        });

        Debug.WriteLine($"TotalItems={result.TotalItems} Items={result.Items.Count}");
        foreach (var item in result.Items)
            Debug.WriteLine($"  User={item.UserName} Gain={item.Gain} Copiers={item.Copiers}");

        Assert.That(result.Items, Is.Not.Null);
    }

    [Test]
    public async Task GetUserProfilesAsync_ReturnsProfileForSearchResult()
    {
        var search = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period = UserInfoPeriod.CurrYear,
            PageSize = 1
        });
        if (search.Items.Count == 0 || string.IsNullOrWhiteSpace(search.Items[0].UserName))
            Assert.Pass("No user returned by search.");

        var result = await _client.GetUserProfilesAsync(usernames: [search.Items[0].UserName!]);

        Debug.WriteLine($"Profiles={result.Users.Count}");
        foreach (var user in result.Users)
            Debug.WriteLine($"  Username={user.Username} Gcid={user.Gcid} Verified={user.IsVerified}");

        Assert.That(result.Users, Is.Not.Empty);
    }

    [Test]
    public async Task GetUserDailyGainAsync_ReturnsDailyOrEmptySeries()
    {
        var username = await GetAnyUsernameAsync();
        if (username is null) Assert.Pass("No user returned by search.");

        var maxDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var minDate = maxDate.AddDays(-30);
        var result = await _client.GetUserDailyGainAsync(username!, minDate, maxDate);

        Debug.WriteLine($"Daily={result.Daily?.Count ?? 0} PeriodGain={result.Gain}");

        Assert.That(result.IsDaily, Is.True);
    }

    [Test]
    public async Task GetUserDailyGainAsync_PeriodReturnsGainObject()
    {
        var username = await GetAnyUsernameAsync();
        if (username is null) Assert.Pass("No user returned by search.");

        var maxDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var minDate = maxDate.AddDays(-30);
        var result = await _client.GetUserDailyGainAsync(username!, minDate, maxDate, UserDailyGainType.Period);

        Debug.WriteLine($"PeriodGain={result.Gain}");

        Assert.That(result.IsPeriod, Is.True);
    }

    [Test]
    public async Task GetUserGainAsync_ReturnsHistoricalGain()
    {
        var username = await GetAnyUsernameAsync();
        if (username is null) Assert.Pass("No user returned by search.");

        var result = await _client.GetUserGainAsync(username!);

        Debug.WriteLine($"Monthly={result.Monthly?.Count ?? 0} Yearly={result.Yearly?.Count ?? 0}");

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetUserLivePortfolioAsync_ReturnsPortfolio()
    {
        var username = await GetAnyUsernameAsync();
        if (username is null) Assert.Pass("No user returned by search.");

        var result = await _client.GetUserLivePortfolioAsync(username!);

        Debug.WriteLine($"Positions={result.Positions?.Count ?? 0} SocialTrades={result.SocialTrades?.Count ?? 0}");

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetUserTradeInfoAsync_ReturnsTradeInfo()
    {
        var username = await GetAnyUsernameAsync();
        if (username is null) Assert.Pass("No user returned by search.");

        var result = await _client.GetUserTradeInfoAsync(username!, UserInfoPeriod.LastYear);

        Debug.WriteLine($"User={result.UserName} Gain={result.Gain} Trades={result.Trades}");

        Assert.That(result.UserName, Is.Not.Null.And.Not.Empty);
    }

    private async Task<string?> GetAnyUsernameAsync()
    {
        var search = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period = UserInfoPeriod.CurrYear,
            PageSize = 1
        });

        return search.Items.FirstOrDefault()?.UserName;
    }
}
