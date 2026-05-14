using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.UserInfo;
using NUnit.Framework;
using System.Diagnostics;
using System.Linq;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// ProdukÄŤnĂ­ / prĹŻzkumnĂ© testy User-Info API.
/// CĂ­l: odhalit chovĂˇnĂ­ eToro nedokumentovanĂ© nebo odliĹˇnĂ© od dokumentace â€”
/// pageSize/page chovĂˇnĂ­ (page od 1!), filtry, DailyGain converter, vĹˇechny periody.
/// Rate limit: 60 req/min â€” zajiĹˇĹĄuje RateLimitHandler automaticky.
///
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "FullyQualifiedName~UserInfoProductionApiTests"
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class UserInfoProductionApiTests
{
    private IEToroClient _client = null!;

    // Seed data z OneTimeSetUp
    private string  _ownUsername   = string.Empty;
    private int     _ownGcid;
    private int     _ownRealCid;
    private string  _popularUsername = string.Empty;  // popular investor â€” bohatĹˇĂ­ data
    private bool    _hasPopularUser;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();

        // VlastnĂ­ identita â†’ RealCid â†’ vlastnĂ­ profil â†’ username
        var identity = await _client.GetIdentityAsync();
        _ownRealCid = identity.RealCid;

        var ownProfile = await _client.GetUserProfilesAsync(cidList: [_ownRealCid]);
        Assert.That(ownProfile.Users, Is.Not.Empty, "VlastnĂ­ profil nenalezen podle CID");
        _ownUsername = ownProfile.Users[0].Username;
        _ownGcid     = ownProfile.Users[0].Gcid;

        // Popular investor â€” bohatĹˇĂ­ data pro TradeInfo / LivePortfolio
        var popularSearch = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period          = UserInfoPeriod.CurrYear,
            //PopularInvestor = true,
            PageSize        = 1
        });
        _hasPopularUser  = popularSearch.Items.Count > 0;
        _popularUsername = _hasPopularUser ? popularSearch.Items[0].UserName ?? string.Empty : string.Empty;

        Debug.WriteLine($"[SetUp] ownUsername={_ownUsername} gcid={_ownGcid} realCid={_ownRealCid}");
        Debug.WriteLine($"[SetUp] popularUsername={_popularUsername} hasPopularUser={_hasPopularUser}");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetUserProfilesAsync â€” user-info/people
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetUserProfiles_NoFilter_ReturnsSomeUsers()
    {
        var result = await _client.GetUserProfilesAsync();

        Assert.That(result.Users, Is.Not.Null);
        Debug.WriteLine($"[NoFilter] users={result.Users.Count}");
    }

    [Test]
    public async Task GetUserProfiles_OwnUsername_ReturnsOwnProfile()
    {
        var result = await _client.GetUserProfilesAsync(usernames: [_ownUsername]);

        Assert.That(result.Users, Is.Not.Empty);
        var own = result.Users.FirstOrDefault(u => u.Username == _ownUsername);
        Assert.That(own, Is.Not.Null, $"VlastnĂ­ username '{_ownUsername}' nenalezen v odpovÄ›di");
        Assert.That(own!.Gcid,    Is.EqualTo(_ownGcid));
        Assert.That(own.RealCid,  Is.EqualTo(_ownRealCid));
    }

    [Test]
    public async Task GetUserProfiles_OwnCid_ReturnsOwnProfile()
    {
        var result = await _client.GetUserProfilesAsync(cidList: [_ownRealCid]);

        Assert.That(result.Users, Is.Not.Empty);
        Assert.That(result.Users[0].Username, Is.EqualTo(_ownUsername));
    }

    [Test]
    public async Task GetUserProfiles_UsernameAndCid_Consistency()
    {
        var byName = await _client.GetUserProfilesAsync(usernames: [_ownUsername]);
        var byCid  = await _client.GetUserProfilesAsync(cidList: [_ownRealCid]);

        Assert.That(byName.Users,      Is.Not.Empty);
        Assert.That(byCid.Users,       Is.Not.Empty);
        Assert.That(byName.Users[0].Gcid,    Is.EqualTo(byCid.Users[0].Gcid),    "Gcid se liĹˇĂ­");
        Assert.That(byName.Users[0].RealCid, Is.EqualTo(byCid.Users[0].RealCid), "RealCid se liĹˇĂ­");
        Assert.That(byName.Users[0].Username, Is.EqualTo(byCid.Users[0].Username), "Username se liĹˇĂ­");
    }

    [Test]
    public async Task GetUserProfiles_ProfileData_IsValid()
    {
        var result = await _client.GetUserProfilesAsync(usernames: [_ownUsername]);

        Assert.That(result.Users, Is.Not.Empty);
        var u = result.Users[0];
        Assert.That(u.Gcid,             Is.GreaterThan(0));
        Assert.That(u.RealCid,          Is.GreaterThan(0));
        Assert.That(u.Username,         Is.Not.Null.And.Not.Empty);
        Assert.That(u.VerificationLevel, Is.GreaterThanOrEqualTo(0));
        Debug.WriteLine($"[ProfileData] isPi={u.IsPi} piLevel={u.PiLevel} isVerified={u.IsVerified} " +
                        $"country={u.Country} avatars={u.Avatars?.Count ?? 0}");
    }

    [Test]
    public async Task GetUserProfiles_NonExistentUsername_ReturnsEmpty()
    {
        var result = await _client.GetUserProfilesAsync(
            usernames: ["__NEEXISTUJICI_UZIVATEL_XYZ_99999__"]);

        // Dokumentace Ĺ™Ă­kĂˇ: vrĂˇtĂ­ prĂˇzdnĂ˝ seznam, ne chybu
        Assert.That(result.Users, Is.Not.Null);
        if (result.Users.Count > 0)
            Debug.WriteLine($"âš  NeexistujĂ­cĂ­ username vrĂˇtil {result.Users.Count} uĹľivatelĹŻ");
        else
            Debug.WriteLine("[NonExistentUsername] SprĂˇvnÄ› vrĂˇtil prĂˇzdnĂ˝ seznam");
    }

    [Test]
    public async Task GetUserProfiles_MultipleUsernames_ReturnsAll()
    {
        if (!_hasPopularUser)
            Assert.Ignore("Nenalezen popular investor â€” test pĹ™eskoÄŤen");

        var result = await _client.GetUserProfilesAsync(
            usernames: [_ownUsername, _popularUsername]);

        Debug.WriteLine($"[MultipleUsernames] requested=2 returned={result.Users.Count}");
        if (result.Users.Count < 2)
            Debug.WriteLine($"âš  VrĂˇceno {result.Users.Count} mĂ­sto 2 profilĹŻ");
        Assert.That(result.Users.Count, Is.GreaterThanOrEqualTo(1));
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // SearchUsersAsync â€” user-info/people/search
    // PoznĂˇmka: Page zaÄŤĂ­nĂˇ od 1 (ne od 0)!
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    [TestCase(UserInfoPeriod.CurrMonth)]
    [TestCase(UserInfoPeriod.CurrQuarter)]
    [TestCase(UserInfoPeriod.CurrYear)]
    [TestCase(UserInfoPeriod.LastYear)]
    [TestCase(UserInfoPeriod.LastTwoYears)]
    [TestCase(UserInfoPeriod.OneMonthAgo)]
    [TestCase(UserInfoPeriod.TwoMonthsAgo)]
    [TestCase(UserInfoPeriod.ThreeMonthsAgo)]
    [TestCase(UserInfoPeriod.SixMonthsAgo)]
    [TestCase(UserInfoPeriod.OneYearAgo)]
    public async Task SearchUsers_AllPeriods_ReturnResults(UserInfoPeriod period)
    {
        var result = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period   = period,
            PageSize = 5
        });

        Assert.That(result.TotalItems, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.Items,      Is.Not.Null);
        Debug.WriteLine($"[Period={period}] totalItems={result.TotalItems} returned={result.Items.Count}");
        if (result.Items.Count > result.TotalItems)
            Debug.WriteLine($"âš  Items.Count({result.Items.Count}) > TotalItems({result.TotalItems})");
    }

    [Test]
    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    [TestCase(20)]
    [TestCase(50)]
    [TestCase(100)]
    [TestCase(200)]
    public async Task SearchUsers_PageSize_ItemsCountRespectsLimit(int pageSize)
    {
        var result = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period   = UserInfoPeriod.CurrYear,
            PageSize = pageSize
        });

        Debug.WriteLine($"[PageSize={pageSize}] returned={result.Items.Count} totalItems={result.TotalItems}");
        Assert.That(result.Items.Count, Is.LessThanOrEqualTo(pageSize),
            $"API vrĂˇtilo {result.Items.Count} > pageSize {pageSize}");
        if (result.Items.Count < pageSize && result.TotalItems > pageSize)
            Debug.WriteLine($"âš  API vrĂˇtilo mĂ©nÄ› ({result.Items.Count}) neĹľ pageSize ({pageSize}), " +
                            $"ale totalItems={result.TotalItems} â€” moĹľnĂ˝ bug eToro");
    }

    [Test]
    public async Task SearchUsers_Page1VsPage2_DifferentItems()
    {
        var page1 = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period   = UserInfoPeriod.CurrYear,
            Page     = 1,
            PageSize = 10
        });
        var page2 = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period   = UserInfoPeriod.CurrYear,
            Page     = 2,
            PageSize = 10
        });

        if (page1.TotalItems <= 10)
        {
            Assert.Ignore("MĂ©nÄ› neĹľ 10 vĂ˝sledkĹŻ â€” strĂˇnkovĂˇnĂ­ nelze testovat");
            return;
        }

        var ids1 = page1.Items.Select(i => i.UserName).ToHashSet();
        var ids2 = page2.Items.Select(i => i.UserName).ToHashSet();
        var overlap = ids1.Intersect(ids2).Count();

        Debug.WriteLine($"[PageOffset] page1={page1.Items.Count} page2={page2.Items.Count} overlap={overlap}");
        if (overlap > 0)
            Debug.WriteLine($"âš  StrĂˇnky 1 a 2 sdĂ­lĂ­ {overlap} UserName â€” moĹľnĂˇ chyba strĂˇnkovĂˇnĂ­");
        Assert.That(overlap, Is.EqualTo(0), "Page 1 a Page 2 by nemÄ›ly sdĂ­let zĂˇznamy");
    }

    [Test]
    [TestCase(1, 3)]
    [TestCase(1, 7)]
    [TestCase(2, 5)]
    [TestCase(4, 6)]
    public async Task SearchUsers_MaxDailyRiskScoreFilter_ResultsInRange(int min, int max)
    {
        var result = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period                = UserInfoPeriod.CurrYear,
            MaxDailyRiskScoreMin  = min,
            MaxDailyRiskScoreMax  = max,
            PageSize              = 20
        });

        Debug.WriteLine($"[RiskScore={min}-{max}] returned={result.Items.Count} totalItems={result.TotalItems}");
        foreach (var item in result.Items.Where(item => item.MaxDailyRiskScore < min || item.MaxDailyRiskScore > max))
        {
            Debug.WriteLine($"âš  CustomerId={item.CustomerId} MaxDailyRiskScore={item.MaxDailyRiskScore} " +
                            $"mimo rozsah [{min},{max}]");
        }
    }

    [Test]
    public async Task SearchUsers_TotalItemsVsActual_Consistency()
    {
        var result = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period   = UserInfoPeriod.CurrYear,
            PageSize = 200
        });

        Debug.WriteLine($"[TotalItems] totalItems={result.TotalItems} returned={result.Items.Count}");
        if (result.TotalItems < result.Items.Count)
            Debug.WriteLine($"âš  TotalItems({result.TotalItems}) < skuteÄŤnĂ˝ poÄŤet({result.Items.Count})");
    }

    [Test]
    public void SearchUsers_PageZero_ThrowsArgumentOutOfRange()
    {
        // Page=0 je neplatnĂ© â€” validace v klientovi (musĂ­ bĂ˝t >= 1)
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _client.SearchUsersAsync(new UserSearchRequest
            {
                Period = UserInfoPeriod.CurrYear,
                Page   = 0
            }));
    }

    [Test]
    public void SearchUsers_PageNegative_ThrowsArgumentOutOfRange()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _client.SearchUsersAsync(new UserSearchRequest
            {
                Period = UserInfoPeriod.CurrYear,
                Page   = -1
            }));
    }

    [Test]
    public void SearchUsers_PageSizeZero_ThrowsArgumentOutOfRange()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _client.SearchUsersAsync(new UserSearchRequest
            {
                Period   = UserInfoPeriod.CurrYear,
                PageSize = 0
            }));
    }

    [Test]
    public void SearchUsers_PageSizeOver200_ThrowsArgumentOutOfRange()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _client.SearchUsersAsync(new UserSearchRequest
            {
                Period   = UserInfoPeriod.CurrYear,
                PageSize = 201
            }));
    }

    [Test]
    public async Task SearchUsers_ItemData_IsValid()
    {
        var result = await _client.SearchUsersAsync(new UserSearchRequest
        {
            Period   = UserInfoPeriod.CurrYear,
            PageSize = 5
        });

        if (result.Items.Count == 0)
        {
            Assert.Ignore("Ĺ˝ĂˇdnĂ© vĂ˝sledky â€” data nelze validovat");
            return;
        }

        foreach (var item in result.Items)
        {
            Assert.That(item.RiskScore, Is.InRange(1, 10), $"RiskScore mimo rozsah: {item.RiskScore}");
            Assert.That(item.WinRatio,  Is.InRange(0m, 1m), $"WinRatio mimo rozsah: {item.WinRatio}");
        }
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetUserDailyGainAsync â€” user-info/people/{username}/daily-gain
    // PoznĂˇmka: type=Daily â†’ IsDaily=true (pole entries), type=Period â†’ IsPeriod=true (objekt s gain)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetUserDailyGain_TypeDaily_ReturnsDailyEntries()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from  = today.AddDays(-30);

        var result = await _client.GetUserDailyGainAsync(
            _ownUsername, from, today, UserDailyGainType.Daily);

        Assert.That(result,         Is.Not.Null);
        Assert.That(result.IsDaily, Is.True, "Typ Daily by mÄ›l vrĂˇtit IsDaily=true");
        Assert.That(result.Daily,   Is.Not.Null);
        Debug.WriteLine($"[DailyGain/Daily] entries={result.Daily!.Count}");
    }

    [Test]
    public async Task GetUserDailyGain_TypePeriod_ReturnsPeriodGain()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from  = today.AddDays(-30);

        var result = await _client.GetUserDailyGainAsync(
            _ownUsername, from, today, UserDailyGainType.Period);

        Assert.That(result,          Is.Not.Null);
        Assert.That(result.IsPeriod, Is.True, "Typ Period by mÄ›l vrĂˇtit IsPeriod=true");
        Assert.That(result.Gain,     Is.Not.Null);
        Debug.WriteLine($"[DailyGain/Period] gain={result.Gain}");
    }

    [Test]
    [TestCase(7)]
    [TestCase(30)]
    [TestCase(90)]
    [TestCase(180)]
    [TestCase(365)]
    public async Task GetUserDailyGain_DateRanges_ReturnsWithoutError(int days)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from  = today.AddDays(-days);

        var result = await _client.GetUserDailyGainAsync(
            _ownUsername, from, today, UserDailyGainType.Daily);

        Assert.That(result, Is.Not.Null);
        var count = result.Daily?.Count ?? 0;
        Debug.WriteLine($"[DailyGain days={days}] entries={count}");
        if (result.IsDaily && count > days)
            Debug.WriteLine($"âš  VrĂˇceno {count} entries pro {days} dnĂ­");
    }

    [Test]
    public async Task GetUserDailyGain_Daily_TimestampsInRequestedRange()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from  = today.AddDays(-30);

        var result = await _client.GetUserDailyGainAsync(
            _ownUsername, from, today, UserDailyGainType.Daily);

        if (result.Daily is null || result.Daily.Count == 0)
        {
            Debug.WriteLine("[DailyTimestamps] Ĺ˝ĂˇdnĂ© zĂˇznamy â€” skip");
            return;
        }

        var minAllowed = new DateTimeOffset(from.Year, from.Month, from.Day, 0, 0, 0, TimeSpan.Zero).AddDays(-1);
        var maxAllowed = new DateTimeOffset(today.Year, today.Month, today.Day, 23, 59, 59, TimeSpan.Zero).AddDays(1);
        foreach (var entry in result.Daily.Where(entry => entry.Timestamp < minAllowed || entry.Timestamp > maxAllowed))
        {
            Debug.WriteLine($"âš  Timestamp {entry.Timestamp:yyyy-MM-dd} mimo rozsah [{from},{today}]");
        }
    }

    [Test]
    public async Task GetUserDailyGain_PopularInvestor_TypeBothWork()
    {
        if (!_hasPopularUser)
            Assert.Ignore("Nenalezen popular investor â€” test pĹ™eskoÄŤen");

        var today  = DateOnly.FromDateTime(DateTime.UtcNow);
        var from   = today.AddDays(-90);

        var daily  = await _client.GetUserDailyGainAsync(_popularUsername, from, today, UserDailyGainType.Daily);
        var period = await _client.GetUserDailyGainAsync(_popularUsername, from, today, UserDailyGainType.Period);

        Assert.That(daily.IsDaily,   Is.True);
        Assert.That(period.IsPeriod, Is.True);
        Debug.WriteLine($"[PI DailyGain] dailyEntries={daily.Daily?.Count ?? 0} periodGain={period.Gain}");
    }

    [Test]
    public void GetUserDailyGain_MaxDateBeforeMinDate_ThrowsArgumentException()
    {
        var today     = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        Assert.ThrowsAsync<ArgumentException>(() =>
            _client.GetUserDailyGainAsync(
                _ownUsername, minDate: today, maxDate: yesterday));
    }

    [Test]
    public void GetUserDailyGain_EmptyUsername_ThrowsArgumentException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from  = today.AddDays(-7);

        Assert.ThrowsAsync<ArgumentException>(() =>
            _client.GetUserDailyGainAsync("", from, today));
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetUserGainAsync â€” user-info/people/{username}/gain
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetUserGain_OwnUser_ReturnsData()
    {
        var result = await _client.GetUserGainAsync(_ownUsername);

        Assert.That(result, Is.Not.Null);
        Debug.WriteLine($"[UserGain] monthly={result.Monthly?.Count ?? 0} yearly={result.Yearly?.Count ?? 0}");
    }

    [Test]
    public async Task GetUserGain_PopularInvestor_ReturnsData()
    {
        if (!_hasPopularUser)
            Assert.Ignore("Nenalezen popular investor â€” test pĹ™eskoÄŤen");

        var result = await _client.GetUserGainAsync(_popularUsername);

        Assert.That(result, Is.Not.Null);
        Debug.WriteLine($"[UserGain PI] monthly={result.Monthly?.Count ?? 0} yearly={result.Yearly?.Count ?? 0}");
    }

    [Test]
    public async Task GetUserGain_MonthlyEntries_TimestampsOrdered()
    {
        var result = await _client.GetUserGainAsync(_ownUsername);

        if (result.Monthly is null || result.Monthly.Count < 2)
        {
            Debug.WriteLine("[UserGain Order] MĂ©nÄ› neĹľ 2 monthly zĂˇznamy â€” skip");
            return;
        }

        for (int i = 1; i < result.Monthly.Count; i++)
        {
            if (result.Monthly[i].Timestamp < result.Monthly[i - 1].Timestamp)
                Debug.WriteLine($"âš  Monthly zĂˇznamy nejsou vzestupnÄ› seĹ™azeny " +
                                $"na indexech {i - 1} a {i}");
        }
    }

    [Test]
    public void GetUserGain_EmptyUsername_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(() =>
            _client.GetUserGainAsync(""));
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetUserLivePortfolioAsync â€” user-info/people/{username}/portfolio/live
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetUserLivePortfolio_OwnUser_ReturnsResponse()
    {
        var result = await _client.GetUserLivePortfolioAsync(_ownUsername);

        Assert.That(result, Is.Not.Null);
        Debug.WriteLine($"[LivePortfolio own] positions={result.Positions?.Count ?? 0} " +
                        $"socialTrades={result.SocialTrades?.Count ?? 0} " +
                        $"realizedPct={result.RealizedCreditPct} unrealizedPct={result.UnrealizedCreditPct}");
    }

    [Test]
    public async Task GetUserLivePortfolio_PopularInvestor_ReturnsResponse()
    {
        if (!_hasPopularUser)
            Assert.Ignore("Nenalezen popular investor â€” test pĹ™eskoÄŤen");

        var result = await _client.GetUserLivePortfolioAsync(_popularUsername);

        Assert.That(result, Is.Not.Null);
        Debug.WriteLine($"[LivePortfolio PI] positions={result.Positions?.Count ?? 0} " +
                        $"socialTrades={result.SocialTrades?.Count ?? 0}");
    }

    [Test]
    public async Task GetUserLivePortfolio_PositionData_IsValid()
    {
        var result = await _client.GetUserLivePortfolioAsync(_ownUsername);

        if (result.Positions is null || result.Positions.Count == 0)
        {
            Debug.WriteLine("[LivePortfolio positions] Ĺ˝ĂˇdnĂ© pozice â€” skip validace");
            return;
        }

        foreach (var pos in result.Positions)
        {
            Assert.That(pos.PositionId,    Is.GreaterThan(0));
            Assert.That(pos.InstrumentId,  Is.GreaterThan(0));
            Assert.That(pos.Leverage,      Is.GreaterThanOrEqualTo(1));
            Assert.That(pos.InvestmentPct, Is.GreaterThanOrEqualTo(0));
            if (pos.OpenTimestamp.HasValue)
                Assert.That(pos.OpenTimestamp.Value, Is.LessThan(DateTimeOffset.UtcNow.AddMinutes(5)));
        }
    }

    [Test]
    public async Task GetUserLivePortfolio_SocialTradeData_IsValid()
    {
        var result = await _client.GetUserLivePortfolioAsync(_ownUsername);

        if (result.SocialTrades is null || result.SocialTrades.Count == 0)
        {
            Debug.WriteLine("[LivePortfolio socialTrades] Ĺ˝ĂˇdnĂ© social trades â€” skip validace");
            return;
        }

        foreach (var st in result.SocialTrades)
        {
            Assert.That(st.SocialTradeId,   Is.GreaterThan(0));
            Assert.That(st.InvestmentPct,   Is.GreaterThanOrEqualTo(0));
            Debug.WriteLine($"[SocialTrade] id={st.SocialTradeId} parent={st.ParentUsername} " +
                            $"positions={st.Positions?.Count ?? 0}");
        }
    }

    [Test]
    public void GetUserLivePortfolio_EmptyUsername_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(() =>
            _client.GetUserLivePortfolioAsync(""));
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetUserTradeInfoAsync â€” user-info/people/{username}/tradeinfo
    // VĹˇechny periody musĂ­ vrĂˇtit validnĂ­ data
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    [TestCase(UserInfoPeriod.CurrMonth)]
    [TestCase(UserInfoPeriod.CurrQuarter)]
    [TestCase(UserInfoPeriod.CurrYear)]
    [TestCase(UserInfoPeriod.LastYear)]
    [TestCase(UserInfoPeriod.LastTwoYears)]
    [TestCase(UserInfoPeriod.OneMonthAgo)]
    [TestCase(UserInfoPeriod.TwoMonthsAgo)]
    [TestCase(UserInfoPeriod.ThreeMonthsAgo)]
    [TestCase(UserInfoPeriod.SixMonthsAgo)]
    [TestCase(UserInfoPeriod.OneYearAgo)]
    public async Task GetUserTradeInfo_AllPeriods_ReturnsData(UserInfoPeriod period)
    {
        if (!_hasPopularUser)
            Assert.Ignore("Nenalezen popular investor â€” test pĹ™eskoÄŤen");

        var result = await _client.GetUserTradeInfoAsync(_popularUsername, period);

        Assert.That(result, Is.Not.Null);
        Debug.WriteLine($"[TradeInfo period={period}] gain={result.Gain:F2} riskScore={result.RiskScore} " +
                        $"trades={result.Trades} copiers={result.Copiers}");
    }

    [Test]
    public async Task GetUserTradeInfo_OwnUser_ReturnsData()
    {
        var result = await _client.GetUserTradeInfoAsync(_ownUsername, UserInfoPeriod.CurrYear);

        Assert.That(result, Is.Not.Null);
        Debug.WriteLine($"[TradeInfo own] gain={result.Gain:F2} riskScore={result.RiskScore} " +
                        $"trades={result.Trades} copiers={result.Copiers}");
    }

    [Test]
    public async Task GetUserTradeInfo_DataRanges_AreValid()
    {
        if (!_hasPopularUser)
            Assert.Ignore("Nenalezen popular investor â€” test pĹ™eskoÄŤen");

        var result = await _client.GetUserTradeInfoAsync(_popularUsername, UserInfoPeriod.CurrYear);

        Assert.That(result.RiskScore,          Is.InRange(1, 10));
        Assert.That(result.MaxDailyRiskScore,   Is.InRange(1, 10));
        Assert.That(result.MaxMonthlyRiskScore, Is.InRange(1, 10));
        Assert.That(result.WinRatio,            Is.InRange(0m, 1000m));
        Assert.That(result.Copiers,             Is.GreaterThanOrEqualTo(0));
        Assert.That(result.Trades,              Is.GreaterThanOrEqualTo(0));

        var leverageSum = result.HighLeveragePct + result.MediumLeveragePct + result.LowLeveragePct;
        if (Math.Abs(leverageSum - 1m) > 0.05m && leverageSum > 0)
            Debug.WriteLine($"âš  LeveragePct souÄŤet={leverageSum:F3} (oÄŤekĂˇvĂˇno ~1.0)");
    }

    [Test]
    public async Task GetUserTradeInfo_AllPeriods_OwnUser_ReturnsWithoutError()
    {
        var periods = Enum.GetValues<UserInfoPeriod>();
        foreach (var period in periods)
        {
            var result = await _client.GetUserTradeInfoAsync(_ownUsername, period);
            Assert.That(result, Is.Not.Null, $"Null response pro period={period}");
            Debug.WriteLine($"[TradeInfo own period={period}] gain={result.Gain:F2}");
        }
    }

    [Test]
    public void GetUserTradeInfo_EmptyUsername_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(() =>
            _client.GetUserTradeInfoAsync("", UserInfoPeriod.CurrYear));
    }
}
