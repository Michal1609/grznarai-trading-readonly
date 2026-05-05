using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Watchlist;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Tests.Integration;

/// <summary>
/// Produkční / průzkumné testy watchlist API.
/// Cíl: odhalit chování eToro, které dokumentace nepopisuje — limity stránkování,
/// skutečně vrácené počty, chování pageIndex (0 vs 1), itemsLimit atd.
/// Rate limit: 60 req/min — zajišťuje RateLimitHandler, testy mohou trvat déle.
///
/// Spuštění: dotnet test --filter "FullyQualifiedName~WatchlistProductionApiTests"
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruční spuštění — vyžaduje reálné API klíče mimo git")]
public class WatchlistProductionApiTests
{
    private IEToroClient _client = null!;

    // Seed data získaná v OneTimeSetUp
    private int _gcid;
    private string _watchlistId = null!;
    private bool _hasWatchlist;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();

        var wl = await _client.GetUserWatchlistsAsync(itemsPerPageForSingle: 1);
        _hasWatchlist = wl.IsSucceeded && wl.Watchlists.Count > 0;

        if (_hasWatchlist)
        {
            _gcid = wl.Watchlists[0].Gcid;
            _watchlistId = wl.Watchlists[0].WatchlistId;
            Debug.WriteLine($"[SetUp] gcid={_gcid} watchlistId={_watchlistId}");
        }
        else
        {
            Debug.WriteLine("[SetUp] Žádné watchlisty — testy na konkrétní ID se přeskočí.");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetDefaultWatchlistItemsAsync — kombinace itemsPerPage + itemsLimit
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ověřuje chování itemsLimit — omezuje celkový počet vrácených položek.
    /// </summary>
    [TestCase(1,  TestName = "DefaultItems_Limit1")]
    [TestCase(3,  TestName = "DefaultItems_Limit3")]
    [TestCase(5,  TestName = "DefaultItems_Limit5")]
    [TestCase(10, TestName = "DefaultItems_Limit10")]
    public async Task GetDefaultWatchlistItemsAsync_ItemsLimit_Combinations(int itemsLimit)
    {
        var result = await _client.GetDefaultWatchlistItemsAsync(itemsLimit: itemsLimit);

        var returned = result.Count;
        Debug.WriteLine($"[DefaultItems] itemsLimit={itemsLimit} itemsPerPage=100 → returned={returned}");

        if (returned > itemsLimit)
            Debug.WriteLine($"  ⚠ API vrátilo {returned}, ačkoliv itemsLimit={itemsLimit} — limit ignorován!");

        Assert.That(returned, Is.LessThanOrEqualTo(itemsLimit),
            $"itemsLimit={itemsLimit} ale API vrátilo {returned} položek.");
    }

    /// <summary>
    /// Základní validace — všechny položky mají kladné itemId.
    /// </summary>
    [Test]
    public async Task GetDefaultWatchlistItemsAsync_AllItemIds_ArePositive()
    {
        var result = await _client.GetDefaultWatchlistItemsAsync(itemsLimit: 50);

        Debug.WriteLine($"[DefaultItems] Validuji {result.Count} položek na kladné itemId.");
        foreach (var item in result)
        {
            Debug.WriteLine($"  itemId={item.ItemId} type={item.ItemType} symbol={item.Market?.SymbolName}");
            Assert.That(item.ItemId, Is.GreaterThan(0), $"Položka má neplatné itemId={item.ItemId}.");
        }
    }

    /// <summary>
    /// Ověřuje, zda market data jsou konzistentní — pokud market existuje, symbolName nesmí být prázdný.
    /// </summary>
    [Test]
    public async Task GetDefaultWatchlistItemsAsync_MarketData_IsConsistent()
    {
        var result = await _client.GetDefaultWatchlistItemsAsync(itemsLimit: 50);

        var withMarket = 0;
        var withoutMarket = 0;
        foreach (var item in result)
        {
            if (item.Market is not null)
            {
                withMarket++;
                Assert.That(item.Market.SymbolName, Is.Not.Null.And.Not.Empty,
                    $"itemId={item.ItemId} má Market objekt ale prázdný SymbolName.");
                Assert.That(item.Market.DisplayName, Is.Not.Null.And.Not.Empty,
                    $"itemId={item.ItemId} má Market objekt ale prázdný DisplayName.");
            }
            else
            {
                withoutMarket++;
            }
        }

        Debug.WriteLine($"[DefaultItems] withMarket={withMarket} withoutMarket={withoutMarket}");
        if (withoutMarket > 0)
            Debug.WriteLine($"  ⚠ {withoutMarket} položek bez market dat — záměr nebo chyba?");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetSingleWatchlistAsync — kombinace pageNumber + itemsPerPage
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ověřuje chování itemsPerPage pro konkrétní watchlist.
    /// Dokumentace: min=1, max=1000, default=100.
    /// </summary>
    [TestCase(0, 1,    TestName = "SingleWatchlist_Page0_Size1")]
    [TestCase(0, 5,    TestName = "SingleWatchlist_Page0_Size5")]
    [TestCase(0, 10,   TestName = "SingleWatchlist_Page0_Size10")]
    [TestCase(0, 25,   TestName = "SingleWatchlist_Page0_Size25")]
    [TestCase(0, 50,   TestName = "SingleWatchlist_Page0_Size50")]
    [TestCase(0, 100,  TestName = "SingleWatchlist_Page0_Size100")]
    [TestCase(0, 1000, TestName = "SingleWatchlist_Page0_Size1000")]
    public async Task GetSingleWatchlistAsync_PageSize_Combinations(int pageNumber, int itemsPerPage)
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("Uživatel nemá žádné watchlisty.");
            return;
        }

        var result = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: pageNumber, itemsPerPage: itemsPerPage);
        var watchlist = result.Watchlists.Count > 0 ? result.Watchlists[0] : null;
        var returned = watchlist?.Items.Count ?? 0;
        var totalItems = watchlist?.TotalItems ?? 0;

        Debug.WriteLine($"[SingleWatchlist] pageNumber={pageNumber} itemsPerPage={itemsPerPage} → returned={returned} totalItems={totalItems} isSucceeded={result.IsSucceeded}");

        if (returned < itemsPerPage && returned < totalItems)
            Debug.WriteLine($"  ⚠ Vráceno méně ({returned}) než požadováno ({itemsPerPage}) a totalItems={totalItems} — eToro bug?");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSucceeded, Is.True, $"isSucceeded=false pro pageNumber={pageNumber} itemsPerPage={itemsPerPage}.");
            Assert.That(returned, Is.LessThanOrEqualTo(itemsPerPage),
                $"Vráceno {returned} > itemsPerPage={itemsPerPage} — API ignoruje parametr?");
        });
    }

    /// <summary>
    /// Kritický test: chování pageNumber — je indexováno od 0 nebo od 1?
    /// Porovnává obsah stránky 0 vs stránky 1 vs stránky 2.
    /// </summary>
    [Test]
    public async Task GetSingleWatchlistAsync_PageNumber_IndexingBehavior()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("Uživatel nemá žádné watchlisty.");
            return;
        }

        // Malý pageSize — maximalizuje šanci, že page0 ≠ page1
        const int pageSize = 5;

        var page0 = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 0, itemsPerPage: pageSize);
        var page1 = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 1, itemsPerPage: pageSize);
        var page2 = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 2, itemsPerPage: pageSize);

        var ids0 = page0.Watchlists.SelectMany(w => w.Items).Select(i => i.ItemId).ToList();
        var ids1 = page1.Watchlists.SelectMany(w => w.Items).Select(i => i.ItemId).ToList();
        var ids2 = page2.Watchlists.SelectMany(w => w.Items).Select(i => i.ItemId).ToList();

        Debug.WriteLine($"[SingleWatchlist] PageIndexing test, pageSize={pageSize}");
        Debug.WriteLine($"  page0 items: [{string.Join(", ", ids0)}]");
        Debug.WriteLine($"  page1 items: [{string.Join(", ", ids1)}]");
        Debug.WriteLine($"  page2 items: [{string.Join(", ", ids2)}]");

        bool page0EqualsPage1 = ids0.SequenceEqual(ids1);
        bool page1EqualsPage2 = ids1.SequenceEqual(ids2);

        if (page0EqualsPage1 && ids0.Count > 0 && ids1.Count > 0)
            Debug.WriteLine("  ⚠ page0 == page1 — eToro pravděpodobně indexuje od 1 (page0 ignorována nebo rovno page1).");
        else if (!page0EqualsPage1 && ids0.Count > 0)
            Debug.WriteLine("  ✓ page0 ≠ page1 — indexování od 0 funguje správně.");

        if (page1EqualsPage2 && ids1.Count > 0 && ids2.Count > 0)
            Debug.WriteLine("  ⚠ page1 == page2 — stránkování nefunguje (stejná data).");

        // Pokud je totalItems > pageSize, očekáváme různé stránky
        var totalItems = page0.Watchlists.Count > 0 ? page0.Watchlists[0].TotalItems : 0;
        Debug.WriteLine($"  totalItems={totalItems}");

        if (totalItems > pageSize)
        {
            Assert.That(page0EqualsPage1, Is.False,
                $"page0 a page1 vrátily stejná data přestože totalItems={totalItems} > pageSize={pageSize}. Stránkování nefunguje.");
        }
        else
        {
            Debug.WriteLine($"  Watchlist má jen {totalItems} položek ≤ pageSize={pageSize}, stránkování nelze ověřit.");
            Assert.Pass($"Nedostatek dat pro test stránkování (totalItems={totalItems} ≤ pageSize={pageSize}).");
        }
    }

    /// <summary>
    /// Ověřuje, že pageNumber a itemsPerPage mají reálný dopad na Items ve watchlistu,
    /// a ověřuje meta data v odpovědi — maxItemsInWatchlistLimit, maxWatchlistsLimit.
    /// Dokumentace uvádí: max 1000 položek, max 10 watchlistů.
    /// </summary>
    [Test]
    public async Task GetSingleWatchlistAsync_MetaData_MatchesDocumentation()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("Uživatel nemá žádné watchlisty.");
            return;
        }

        // pageNumber=0, itemsPerPage=1 — oba parametry musí ovlivnit watchlist.Items
        var result = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 0, itemsPerPage: 1);

        var watchlist = result.Watchlists.Count > 0 ? result.Watchlists[0] : null;
        var itemsCount = watchlist?.Items.Count ?? -1;
        var totalItems = watchlist?.TotalItems ?? 0;

        Debug.WriteLine($"[SingleWatchlist] pageNumber=0 itemsPerPage=1 → " +
                        $"watchlist.Items.Count={itemsCount} watchlist.TotalItems={totalItems}");
        Debug.WriteLine($"  Meta: pageNumber={result.Meta?.PageNumber} itemsPerPage={result.Meta?.ItemsPerPage} " +
                        $"maxItemsLimit={result.Meta?.MaxItemsInWatchlistLimit} maxWatchlistsLimit={result.Meta?.MaxWatchlistsLimit}");

        if (itemsCount > 1)
            Debug.WriteLine($"  ⚠ itemsPerPage=1 ale watchlist.Items.Count={itemsCount} — parametr ignorován!");
        if (itemsCount == 0 && totalItems > 0)
            Debug.WriteLine($"  ⚠ watchlist.Items je prázdné přestože totalItems={totalItems} > 0!");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSucceeded, Is.True, "isSucceeded musí být true.");
            Assert.That(watchlist, Is.Not.Null, "Watchlist v odpovědi nesmí být null.");

            // Klíčové: itemsPerPage=1 musí omezit Items na max 1 záznam
            if (totalItems > 0)
                Assert.That(itemsCount, Is.EqualTo(1),
                    $"itemsPerPage=1 ale vráceno {itemsCount} items (totalItems={totalItems}) — parametr ignorován.");
            else
                Assert.That(itemsCount, Is.EqualTo(0),
                    "Prázdný watchlist by měl vrátit 0 items.");
        });

        // pageNumber=0 ověření: stránka 0 a stránka 1 by měly vracet různé items (pokud totalItems > 1)
        if (totalItems > 1)
        {
            var page1Result = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 1, itemsPerPage: 1);
            var page1Watchlist = page1Result.Watchlists.Count > 0 ? page1Result.Watchlists[0] : null;
            var page0ItemId = watchlist?.Items.FirstOrDefault()?.ItemId;
            var page1ItemId = page1Watchlist?.Items.FirstOrDefault()?.ItemId;

            Debug.WriteLine($"  pageNumber=0 → Items[0].ItemId={page0ItemId}");
            Debug.WriteLine($"  pageNumber=1 → Items[0].ItemId={page1ItemId}");

            if (page0ItemId == page1ItemId)
                Debug.WriteLine($"  ⚠ page0 a page1 vrátily stejný ItemId={page0ItemId} — pageNumber ignorován nebo indexování od 1?");

            Assert.That(page0ItemId, Is.Not.EqualTo(page1ItemId),
                $"pageNumber=0 a pageNumber=1 (itemsPerPage=1) vrátily stejný item (ItemId={page0ItemId}) — stránkování nefunguje.");
        }

        if (result.Meta is not null)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result.Meta.MaxItemsInWatchlistLimit, Is.EqualTo(100),
                    "Dokumentace říká maxItemsInWatchlistLimit=100.");               
                Assert.That(result.Meta.PageNumber, Is.EqualTo(0),
                    "Meta.PageNumber musí odpovídat požadovanému pageNumber=0.");
                Assert.That(result.Meta.ItemsPerPage, Is.EqualTo(1),
                    "Meta.ItemsPerPage musí odpovídat požadovanému itemsPerPage=1.");
            });
        }
        else
        {
            Debug.WriteLine("  ⚠ Meta objekt je null — API neposílá metadata!");
        }
    }

    /// <summary>
    /// Ověřuje, zda totalItems v hlavičce watchlistu odpovídá skutečnému počtu
    /// vrácených položek po průchodu všemi stránkami.
    /// </summary>
    [Test]
    public async Task GetSingleWatchlistAsync_TotalItems_MatchesActualCount()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("Uživatel nemá žádné watchlisty.");
            return;
        }

        // Nejdřív zjistíme totalItems
        var first = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 0, itemsPerPage: 1);
        if (first.Watchlists.Count == 0)
        {
            Assert.Pass("Watchlist je prázdný.");
            return;
        }

        var totalItems = first.Watchlists[0].TotalItems;
        Debug.WriteLine($"[SingleWatchlist] totalItems={totalItems} dle API");

        if (totalItems > 50)
        {
            Debug.WriteLine("  Příliš mnoho položek pro kompletní průchod, přeskakuji načítání všech stránek.");
            Assert.Pass($"totalItems={totalItems} > 50, test přeskočen (rate limit).");
            return;
        }

        // Načteme vše na jedné stránce
        var all = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 0, itemsPerPage: 1000);
        var actualCount = all.Watchlists.SelectMany(w => w.Items).Count();

        Debug.WriteLine($"  Skutečně vráceno: {actualCount}");

        if (actualCount != totalItems)
            Debug.WriteLine($"  ⚠ totalItems={totalItems} ale skutečně vráceno {actualCount} — neshoda!");

        Assert.That(actualCount, Is.EqualTo(totalItems),
            $"API hlásí totalItems={totalItems} ale vrátilo {actualCount} položek.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetSinglePublicWatchlistAsync — kombinace pageNumber + itemsPerPage
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ověřuje chování itemsPerPage pro veřejný watchlist.
    /// Dokumentace: pageNumber default=0, itemsPerPage default=100.
    /// </summary>
    [TestCase(0, 1,   TestName = "SinglePublic_Page0_Size1")]
    [TestCase(0, 5,   TestName = "SinglePublic_Page0_Size5")]
    [TestCase(0, 10,  TestName = "SinglePublic_Page0_Size10")]
    [TestCase(0, 25,  TestName = "SinglePublic_Page0_Size25")]
    [TestCase(0, 50,  TestName = "SinglePublic_Page0_Size50")]
    [TestCase(0, 100, TestName = "SinglePublic_Page0_Size100")]
    public async Task GetSinglePublicWatchlistAsync_PageSize_Combinations(int pageNumber, int itemsPerPage)
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("Uživatel nemá žádné watchlisty.");
            return;
        }

        var result = await _client.GetSinglePublicWatchlistAsync(
            userId: _gcid, watchlistId: _watchlistId,
            pageNumber: pageNumber, itemsPerPage: itemsPerPage);

        var returned = result.Items.Count;
        var totalItems = result.TotalItems;

        Debug.WriteLine($"[SinglePublic] userId={_gcid} pageNumber={pageNumber} itemsPerPage={itemsPerPage} → returned={returned} totalItems={totalItems}");

        if (returned < itemsPerPage && returned < totalItems)
            Debug.WriteLine($"  ⚠ Vráceno méně ({returned}) než požadováno ({itemsPerPage}) a totalItems={totalItems} — eToro bug?");

        Assert.That(returned, Is.LessThanOrEqualTo(itemsPerPage),
            $"Vráceno {returned} > itemsPerPage={itemsPerPage} — API ignoruje parametr?");
    }

    /// <summary>
    /// Kritický test: chování pageNumber pro veřejný watchlist — indexování od 0 nebo 1?
    /// </summary>
    [Test]
    public async Task GetSinglePublicWatchlistAsync_PageNumber_IndexingBehavior()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("Uživatel nemá žádné watchlisty.");
            return;
        }

        const int pageSize = 5;

        var page0 = await _client.GetSinglePublicWatchlistAsync(
            _gcid, _watchlistId, pageNumber: 0, itemsPerPage: pageSize);
        var page1 = await _client.GetSinglePublicWatchlistAsync(
            _gcid, _watchlistId, pageNumber: 1, itemsPerPage: pageSize);
        var page2 = await _client.GetSinglePublicWatchlistAsync(
            _gcid, _watchlistId, pageNumber: 2, itemsPerPage: pageSize);

        var ids0 = page0.Items.Select(i => i.ItemId).ToList();
        var ids1 = page1.Items.Select(i => i.ItemId).ToList();
        var ids2 = page2.Items.Select(i => i.ItemId).ToList();

        Debug.WriteLine($"[SinglePublic] PageIndexing, pageSize={pageSize}");
        Debug.WriteLine($"  page0: [{string.Join(", ", ids0)}]");
        Debug.WriteLine($"  page1: [{string.Join(", ", ids1)}]");
        Debug.WriteLine($"  page2: [{string.Join(", ", ids2)}]");

        bool page0EqualsPage1 = ids0.SequenceEqual(ids1);

        if (page0EqualsPage1 && ids0.Count > 0 && ids1.Count > 0)
            Debug.WriteLine("  ⚠ page0 == page1 — eToro pravděpodobně indexuje od 1.");
        else if (!page0EqualsPage1 && ids0.Count > 0)
            Debug.WriteLine("  ✓ page0 ≠ page1 — indexování od 0 funguje.");

        var totalItems = page0.TotalItems;
        Debug.WriteLine($"  totalItems={totalItems}");

        if (totalItems > pageSize)
        {
            Assert.That(page0EqualsPage1, Is.False,
                $"page0 == page1 přestože totalItems={totalItems} > pageSize={pageSize}. Stránkování nefunguje.");
        }
        else
        {
            Assert.Pass($"Nedostatek dat pro test stránkování (totalItems={totalItems} ≤ pageSize={pageSize}).");
        }
    }

    /// <summary>
    /// Ověřuje, zda watchlistId a name v odpovědi odpovídají tomu, co bylo požadováno.
    /// Testuje různé pageSize — odpověď by měla být konzistentní.
    /// </summary>
    [TestCase(1,   TestName = "SinglePublic_Consistency_Size1")]
    [TestCase(10,  TestName = "SinglePublic_Consistency_Size10")]
    [TestCase(100, TestName = "SinglePublic_Consistency_Size100")]
    public async Task GetSinglePublicWatchlistAsync_WatchlistIdentity_ConsistentAcrossPageSizes(int itemsPerPage)
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("Uživatel nemá žádné watchlisty.");
            return;
        }

        var result = await _client.GetSinglePublicWatchlistAsync(
            _gcid, _watchlistId, pageNumber: 0, itemsPerPage: itemsPerPage);

        Debug.WriteLine($"[SinglePublic] size={itemsPerPage} → watchlistId={result.WatchlistId} name='{result.Name}' gcid={result.Gcid} items={result.Items.Count}");

        Assert.Multiple(() =>
        {
            Assert.That(result.WatchlistId, Is.EqualTo(_watchlistId),
                $"Vrácené watchlistId='{result.WatchlistId}' ≠ požadované='{_watchlistId}'.");
            Assert.That(result.Gcid, Is.EqualTo(_gcid),
                $"Vrácené Gcid={result.Gcid} ≠ požadované={_gcid}.");
            Assert.That(result.Name, Is.Not.Null.And.Not.Empty,
                "Název watchlistu nesmí být prázdný.");
        });
    }

    /// <summary>
    /// Porovnává GetSingleWatchlistAsync vs GetSinglePublicWatchlistAsync pro stejný watchlist.
    /// Oba by měly vrátit stejné základní identity (watchlistId, name, totalItems).
    /// </summary>
    [Test]
    public async Task GetSingleWatchlist_vs_GetSinglePublic_ReturnConsistentData()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("Uživatel nemá žádné watchlisty.");
            return;
        }

        var privateResult = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 0, itemsPerPage: 10);
        var publicResult  = await _client.GetSinglePublicWatchlistAsync(_gcid, _watchlistId, pageNumber: 0, itemsPerPage: 10);

        var privateWl = privateResult.Watchlists.FirstOrDefault();

        Debug.WriteLine($"[Consistency] Private: isSucceeded={privateResult.IsSucceeded} watchlistId={privateWl?.WatchlistId} totalItems={privateWl?.TotalItems}");
        Debug.WriteLine($"[Consistency] Public:  watchlistId={publicResult.WatchlistId} totalItems={publicResult.TotalItems}");

        if (privateWl is null)
        {
            Assert.Pass("Private endpoint nevrátil žádný watchlist.");
            return;
        }

        Assert.Multiple(() =>
        {
            Assert.That(publicResult.WatchlistId, Is.EqualTo(privateWl.WatchlistId),
                "watchlistId se liší mezi private a public endpointem.");
            Assert.That(publicResult.TotalItems, Is.EqualTo(privateWl.TotalItems),
                $"totalItems: private={privateWl.TotalItems} vs public={publicResult.TotalItems} — neshoda.");
            Assert.That(publicResult.Name, Is.EqualTo(privateWl.Name),
                $"name: private='{privateWl.Name}' vs public='{publicResult.Name}' — neshoda.");
        });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Průřezové testy — všechny watchlisty uživatele
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Pro každý watchlist uživatele načte data s pageSize=1, 10, 50 a zaznamenává
    /// rozdíly mezi požadovaným a vráceným počtem → odhaluje watchlists kde eToro
    /// ignoruje parametr pageSize.
    /// </summary>
    [Test]
    public async Task GetSingleWatchlistAsync_AllUserWatchlists_PageSizeAudit()
    {
        var allWatchlists = await _client.GetUserWatchlistsAsync(itemsPerPageForSingle: 1);

        if (!allWatchlists.IsSucceeded || allWatchlists.Watchlists.Count == 0)
        {
            Assert.Pass("Žádné watchlisty k auditu.");
            return;
        }

        Debug.WriteLine($"[PageSizeAudit] Testuji {allWatchlists.Watchlists.Count} watchlistů");
        var issues = new List<string>();

        foreach (var wl in allWatchlists.Watchlists)
        {
            foreach (var size in new[] { 1, 5, 10 })
            {
                var result = await _client.GetSingleWatchlistAsync(wl.WatchlistId, pageNumber: 0, itemsPerPage: size);
                var returned = result.Watchlists.SelectMany(w => w.Items).Count();
                var totalItems = result.Watchlists.FirstOrDefault()?.TotalItems ?? 0;

                Debug.WriteLine($"  {wl.Name} ({wl.WatchlistId}): size={size} returned={returned} total={totalItems}");

                if (returned > size)
                    issues.Add($"{wl.Name}: size={size} ale returned={returned} > size (parametr ignorován)");
                else if (returned < size && returned < totalItems)
                    issues.Add($"{wl.Name}: size={size} ale returned={returned} < size, přestože total={totalItems} > size (eToro bug?)");
            }
        }

        if (issues.Count > 0)
        {
            Debug.WriteLine("\n[PageSizeAudit] Nalezené problémy:");
            foreach (var issue in issues)
                Debug.WriteLine($"  ⚠ {issue}");
        }
        else
        {
            Debug.WriteLine("[PageSizeAudit] Žádné problémy nalezeny.");
        }

        // Test selže jen pokud API vrátí více než bylo požadováno (clear bug)
        var hardFails = issues.Where(i => i.Contains("parametr ignorován")).ToList();
        Assert.That(hardFails, Is.Empty, "API vrátilo více položek než bylo požadováno:\n" + string.Join("\n", hardFails));
    }
}
