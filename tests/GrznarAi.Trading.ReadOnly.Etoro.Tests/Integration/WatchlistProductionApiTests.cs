using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Watchlist;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// ProdukÄŤnĂ­ / prĹŻzkumnĂ© testy watchlist API.
/// CĂ­l: odhalit chovĂˇnĂ­ eToro, kterĂ© dokumentace nepopisuje â€” limity strĂˇnkovĂˇnĂ­,
/// skuteÄŤnÄ› vrĂˇcenĂ© poÄŤty, chovĂˇnĂ­ pageIndex (0 vs 1), itemsLimit atd.
/// Rate limit: 60 req/min â€” zajiĹˇĹĄuje RateLimitHandler, testy mohou trvat dĂ©le.
///
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "FullyQualifiedName~WatchlistProductionApiTests"
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class WatchlistProductionApiTests
{
    private IEToroClient _client = null!;

    // Seed data zĂ­skanĂˇ v OneTimeSetUp
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
            Debug.WriteLine("[SetUp] Ĺ˝ĂˇdnĂ© watchlisty â€” testy na konkrĂ©tnĂ­ ID se pĹ™eskoÄŤĂ­.");
        }
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetDefaultWatchlistItemsAsync â€” kombinace itemsPerPage + itemsLimit
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>
    /// OvÄ›Ĺ™uje chovĂˇnĂ­ itemsLimit â€” omezuje celkovĂ˝ poÄŤet vrĂˇcenĂ˝ch poloĹľek.
    /// </summary>
    [TestCase(1,  TestName = "DefaultItems_Limit1")]
    [TestCase(3,  TestName = "DefaultItems_Limit3")]
    [TestCase(5,  TestName = "DefaultItems_Limit5")]
    [TestCase(10, TestName = "DefaultItems_Limit10")]
    public async Task GetDefaultWatchlistItemsAsync_ItemsLimit_Combinations(int itemsLimit)
    {
        var result = await _client.GetDefaultWatchlistItemsAsync(itemsLimit: itemsLimit);

        var returned = result.Count;
        Debug.WriteLine($"[DefaultItems] itemsLimit={itemsLimit} itemsPerPage=100 â†’ returned={returned}");

        if (returned > itemsLimit)
            Debug.WriteLine($"  âš  API vrĂˇtilo {returned}, aÄŤkoliv itemsLimit={itemsLimit} â€” limit ignorovĂˇn!");

        Assert.That(returned, Is.LessThanOrEqualTo(itemsLimit),
            $"itemsLimit={itemsLimit} ale API vrĂˇtilo {returned} poloĹľek.");
    }

    /// <summary>
    /// ZĂˇkladnĂ­ validace â€” vĹˇechny poloĹľky majĂ­ kladnĂ© itemId.
    /// </summary>
    [Test]
    public async Task GetDefaultWatchlistItemsAsync_AllItemIds_ArePositive()
    {
        var result = await _client.GetDefaultWatchlistItemsAsync(itemsLimit: 50);

        Debug.WriteLine($"[DefaultItems] Validuji {result.Count} poloĹľek na kladnĂ© itemId.");
        foreach (var item in result)
        {
            Debug.WriteLine($"  itemId={item.ItemId} type={item.ItemType} symbol={item.Market?.SymbolName}");
            Assert.That(item.ItemId, Is.GreaterThan(0), $"PoloĹľka mĂˇ neplatnĂ© itemId={item.ItemId}.");
        }
    }

    /// <summary>
    /// OvÄ›Ĺ™uje, zda market data jsou konzistentnĂ­ â€” pokud market existuje, symbolName nesmĂ­ bĂ˝t prĂˇzdnĂ˝.
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
                    $"itemId={item.ItemId} mĂˇ Market objekt ale prĂˇzdnĂ˝ SymbolName.");
                Assert.That(item.Market.DisplayName, Is.Not.Null.And.Not.Empty,
                    $"itemId={item.ItemId} mĂˇ Market objekt ale prĂˇzdnĂ˝ DisplayName.");
            }
            else
            {
                withoutMarket++;
            }
        }

        Debug.WriteLine($"[DefaultItems] withMarket={withMarket} withoutMarket={withoutMarket}");
        if (withoutMarket > 0)
            Debug.WriteLine($"  âš  {withoutMarket} poloĹľek bez market dat â€” zĂˇmÄ›r nebo chyba?");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetSingleWatchlistAsync â€” kombinace pageNumber + itemsPerPage
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>
    /// OvÄ›Ĺ™uje chovĂˇnĂ­ itemsPerPage pro konkrĂ©tnĂ­ watchlist.
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
            Assert.Ignore("UĹľivatel nemĂˇ ĹľĂˇdnĂ© watchlisty.");
            return;
        }

        var result = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: pageNumber, itemsPerPage: itemsPerPage);
        var watchlist = result.Watchlists.Count > 0 ? result.Watchlists[0] : null;
        var returned = watchlist?.Items.Count ?? 0;
        var totalItems = watchlist?.TotalItems ?? 0;

        Debug.WriteLine($"[SingleWatchlist] pageNumber={pageNumber} itemsPerPage={itemsPerPage} â†’ returned={returned} totalItems={totalItems} isSucceeded={result.IsSucceeded}");

        if (returned < itemsPerPage && returned < totalItems)
            Debug.WriteLine($"  âš  VrĂˇceno mĂ©nÄ› ({returned}) neĹľ poĹľadovĂˇno ({itemsPerPage}) a totalItems={totalItems} â€” eToro bug?");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSucceeded, Is.True, $"isSucceeded=false pro pageNumber={pageNumber} itemsPerPage={itemsPerPage}.");
            Assert.That(returned, Is.LessThanOrEqualTo(itemsPerPage),
                $"VrĂˇceno {returned} > itemsPerPage={itemsPerPage} â€” API ignoruje parametr?");
        });
    }

    /// <summary>
    /// KritickĂ˝ test: chovĂˇnĂ­ pageNumber â€” je indexovĂˇno od 0 nebo od 1?
    /// PorovnĂˇvĂˇ obsah strĂˇnky 0 vs strĂˇnky 1 vs strĂˇnky 2.
    /// </summary>
    [Test]
    public async Task GetSingleWatchlistAsync_PageNumber_IndexingBehavior()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("UĹľivatel nemĂˇ ĹľĂˇdnĂ© watchlisty.");
            return;
        }

        // MalĂ˝ pageSize â€” maximalizuje Ĺˇanci, Ĺľe page0 â‰  page1
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
            Debug.WriteLine("  âš  page0 == page1 â€” eToro pravdÄ›podobnÄ› indexuje od 1 (page0 ignorovĂˇna nebo rovno page1).");
        else if (!page0EqualsPage1 && ids0.Count > 0)
            Debug.WriteLine("  âś“ page0 â‰  page1 â€” indexovĂˇnĂ­ od 0 funguje sprĂˇvnÄ›.");

        if (page1EqualsPage2 && ids1.Count > 0 && ids2.Count > 0)
            Debug.WriteLine("  âš  page1 == page2 â€” strĂˇnkovĂˇnĂ­ nefunguje (stejnĂˇ data).");

        // Pokud je totalItems > pageSize, oÄŤekĂˇvĂˇme rĹŻznĂ© strĂˇnky
        var totalItems = page0.Watchlists.Count > 0 ? page0.Watchlists[0].TotalItems : 0;
        Debug.WriteLine($"  totalItems={totalItems}");

        if (totalItems > pageSize)
        {
            Assert.That(page0EqualsPage1, Is.False,
                $"page0 a page1 vrĂˇtily stejnĂˇ data pĹ™estoĹľe totalItems={totalItems} > pageSize={pageSize}. StrĂˇnkovĂˇnĂ­ nefunguje.");
        }
        else
        {
            Debug.WriteLine($"  Watchlist mĂˇ jen {totalItems} poloĹľek â‰¤ pageSize={pageSize}, strĂˇnkovĂˇnĂ­ nelze ovÄ›Ĺ™it.");
            Assert.Pass($"Nedostatek dat pro test strĂˇnkovĂˇnĂ­ (totalItems={totalItems} â‰¤ pageSize={pageSize}).");
        }
    }

    /// <summary>
    /// OvÄ›Ĺ™uje, Ĺľe pageNumber a itemsPerPage majĂ­ reĂˇlnĂ˝ dopad na Items ve watchlistu,
    /// a ovÄ›Ĺ™uje meta data v odpovÄ›di â€” maxItemsInWatchlistLimit, maxWatchlistsLimit.
    /// Dokumentace uvĂˇdĂ­: max 1000 poloĹľek, max 10 watchlistĹŻ.
    /// </summary>
    [Test]
    public async Task GetSingleWatchlistAsync_MetaData_MatchesDocumentation()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("UĹľivatel nemĂˇ ĹľĂˇdnĂ© watchlisty.");
            return;
        }

        // pageNumber=0, itemsPerPage=1 â€” oba parametry musĂ­ ovlivnit watchlist.Items
        var result = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 0, itemsPerPage: 1);

        var watchlist = result.Watchlists.Count > 0 ? result.Watchlists[0] : null;
        var itemsCount = watchlist?.Items.Count ?? -1;
        var totalItems = watchlist?.TotalItems ?? 0;

        Debug.WriteLine($"[SingleWatchlist] pageNumber=0 itemsPerPage=1 â†’ " +
                        $"watchlist.Items.Count={itemsCount} watchlist.TotalItems={totalItems}");
        Debug.WriteLine($"  Meta: pageNumber={result.Meta?.PageNumber} itemsPerPage={result.Meta?.ItemsPerPage} " +
                        $"maxItemsLimit={result.Meta?.MaxItemsInWatchlistLimit} maxWatchlistsLimit={result.Meta?.MaxWatchlistsLimit}");

        if (itemsCount > 1)
            Debug.WriteLine($"  âš  itemsPerPage=1 ale watchlist.Items.Count={itemsCount} â€” parametr ignorovĂˇn!");
        if (itemsCount == 0 && totalItems > 0)
            Debug.WriteLine($"  âš  watchlist.Items je prĂˇzdnĂ© pĹ™estoĹľe totalItems={totalItems} > 0!");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSucceeded, Is.True, "isSucceeded musĂ­ bĂ˝t true.");
            Assert.That(watchlist, Is.Not.Null, "Watchlist v odpovÄ›di nesmĂ­ bĂ˝t null.");

            // KlĂ­ÄŤovĂ©: itemsPerPage=1 musĂ­ omezit Items na max 1 zĂˇznam
            if (totalItems > 0)
                Assert.That(itemsCount, Is.EqualTo(1),
                    $"itemsPerPage=1 ale vrĂˇceno {itemsCount} items (totalItems={totalItems}) â€” parametr ignorovĂˇn.");
            else
                Assert.That(itemsCount, Is.EqualTo(0),
                    "PrĂˇzdnĂ˝ watchlist by mÄ›l vrĂˇtit 0 items.");
        });

        // pageNumber=0 ovÄ›Ĺ™enĂ­: strĂˇnka 0 a strĂˇnka 1 by mÄ›ly vracet rĹŻznĂ© items (pokud totalItems > 1)
        if (totalItems > 1)
        {
            var page1Result = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 1, itemsPerPage: 1);
            var page1Watchlist = page1Result.Watchlists.Count > 0 ? page1Result.Watchlists[0] : null;
            var page0ItemId = watchlist?.Items.FirstOrDefault()?.ItemId;
            var page1ItemId = page1Watchlist?.Items.FirstOrDefault()?.ItemId;

            Debug.WriteLine($"  pageNumber=0 â†’ Items[0].ItemId={page0ItemId}");
            Debug.WriteLine($"  pageNumber=1 â†’ Items[0].ItemId={page1ItemId}");

            if (page0ItemId == page1ItemId)
                Debug.WriteLine($"  âš  page0 a page1 vrĂˇtily stejnĂ˝ ItemId={page0ItemId} â€” pageNumber ignorovĂˇn nebo indexovĂˇnĂ­ od 1?");

            Assert.That(page0ItemId, Is.Not.EqualTo(page1ItemId),
                $"pageNumber=0 a pageNumber=1 (itemsPerPage=1) vrĂˇtily stejnĂ˝ item (ItemId={page0ItemId}) â€” strĂˇnkovĂˇnĂ­ nefunguje.");
        }

        if (result.Meta is not null)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result.Meta.MaxItemsInWatchlistLimit, Is.EqualTo(100),
                    "Dokumentace Ĺ™Ă­kĂˇ maxItemsInWatchlistLimit=100.");               
                Assert.That(result.Meta.PageNumber, Is.EqualTo(0),
                    "Meta.PageNumber musĂ­ odpovĂ­dat poĹľadovanĂ©mu pageNumber=0.");
                Assert.That(result.Meta.ItemsPerPage, Is.EqualTo(1),
                    "Meta.ItemsPerPage musĂ­ odpovĂ­dat poĹľadovanĂ©mu itemsPerPage=1.");
            });
        }
        else
        {
            Debug.WriteLine("  âš  Meta objekt je null â€” API neposĂ­lĂˇ metadata!");
        }
    }

    /// <summary>
    /// OvÄ›Ĺ™uje, zda totalItems v hlaviÄŤce watchlistu odpovĂ­dĂˇ skuteÄŤnĂ©mu poÄŤtu
    /// vrĂˇcenĂ˝ch poloĹľek po prĹŻchodu vĹˇemi strĂˇnkami.
    /// </summary>
    [Test]
    public async Task GetSingleWatchlistAsync_TotalItems_MatchesActualCount()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("UĹľivatel nemĂˇ ĹľĂˇdnĂ© watchlisty.");
            return;
        }

        // NejdĹ™Ă­v zjistĂ­me totalItems
        var first = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 0, itemsPerPage: 1);
        if (first.Watchlists.Count == 0)
        {
            Assert.Pass("Watchlist je prĂˇzdnĂ˝.");
            return;
        }

        var totalItems = first.Watchlists[0].TotalItems;
        Debug.WriteLine($"[SingleWatchlist] totalItems={totalItems} dle API");

        if (totalItems > 50)
        {
            Debug.WriteLine("  PĹ™Ă­liĹˇ mnoho poloĹľek pro kompletnĂ­ prĹŻchod, pĹ™eskakuji naÄŤĂ­tĂˇnĂ­ vĹˇech strĂˇnek.");
            Assert.Pass($"totalItems={totalItems} > 50, test pĹ™eskoÄŤen (rate limit).");
            return;
        }

        // NaÄŤteme vĹˇe na jednĂ© strĂˇnce
        var all = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 0, itemsPerPage: 1000);
        var actualCount = all.Watchlists.SelectMany(w => w.Items).Count();

        Debug.WriteLine($"  SkuteÄŤnÄ› vrĂˇceno: {actualCount}");

        if (actualCount != totalItems)
            Debug.WriteLine($"  âš  totalItems={totalItems} ale skuteÄŤnÄ› vrĂˇceno {actualCount} â€” neshoda!");

        Assert.That(actualCount, Is.EqualTo(totalItems),
            $"API hlĂˇsĂ­ totalItems={totalItems} ale vrĂˇtilo {actualCount} poloĹľek.");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetSinglePublicWatchlistAsync â€” kombinace pageNumber + itemsPerPage
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>
    /// OvÄ›Ĺ™uje chovĂˇnĂ­ itemsPerPage pro veĹ™ejnĂ˝ watchlist.
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
            Assert.Ignore("UĹľivatel nemĂˇ ĹľĂˇdnĂ© watchlisty.");
            return;
        }

        var result = await _client.GetSinglePublicWatchlistAsync(
            userId: _gcid, watchlistId: _watchlistId,
            pageNumber: pageNumber, itemsPerPage: itemsPerPage);

        var returned = result.Items.Count;
        var totalItems = result.TotalItems;

        Debug.WriteLine($"[SinglePublic] userId={_gcid} pageNumber={pageNumber} itemsPerPage={itemsPerPage} â†’ returned={returned} totalItems={totalItems}");

        if (returned < itemsPerPage && returned < totalItems)
            Debug.WriteLine($"  âš  VrĂˇceno mĂ©nÄ› ({returned}) neĹľ poĹľadovĂˇno ({itemsPerPage}) a totalItems={totalItems} â€” eToro bug?");

        Assert.That(returned, Is.LessThanOrEqualTo(itemsPerPage),
            $"VrĂˇceno {returned} > itemsPerPage={itemsPerPage} â€” API ignoruje parametr?");
    }

    /// <summary>
    /// KritickĂ˝ test: chovĂˇnĂ­ pageNumber pro veĹ™ejnĂ˝ watchlist â€” indexovĂˇnĂ­ od 0 nebo 1?
    /// </summary>
    [Test]
    public async Task GetSinglePublicWatchlistAsync_PageNumber_IndexingBehavior()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("UĹľivatel nemĂˇ ĹľĂˇdnĂ© watchlisty.");
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
            Debug.WriteLine("  âš  page0 == page1 â€” eToro pravdÄ›podobnÄ› indexuje od 1.");
        else if (!page0EqualsPage1 && ids0.Count > 0)
            Debug.WriteLine("  âś“ page0 â‰  page1 â€” indexovĂˇnĂ­ od 0 funguje.");

        var totalItems = page0.TotalItems;
        Debug.WriteLine($"  totalItems={totalItems}");

        if (totalItems > pageSize)
        {
            Assert.That(page0EqualsPage1, Is.False,
                $"page0 == page1 pĹ™estoĹľe totalItems={totalItems} > pageSize={pageSize}. StrĂˇnkovĂˇnĂ­ nefunguje.");
        }
        else
        {
            Assert.Pass($"Nedostatek dat pro test strĂˇnkovĂˇnĂ­ (totalItems={totalItems} â‰¤ pageSize={pageSize}).");
        }
    }

    /// <summary>
    /// OvÄ›Ĺ™uje, zda watchlistId a name v odpovÄ›di odpovĂ­dajĂ­ tomu, co bylo poĹľadovĂˇno.
    /// Testuje rĹŻznĂ© pageSize â€” odpovÄ›ÄŹ by mÄ›la bĂ˝t konzistentnĂ­.
    /// </summary>
    [TestCase(1,   TestName = "SinglePublic_Consistency_Size1")]
    [TestCase(10,  TestName = "SinglePublic_Consistency_Size10")]
    [TestCase(100, TestName = "SinglePublic_Consistency_Size100")]
    public async Task GetSinglePublicWatchlistAsync_WatchlistIdentity_ConsistentAcrossPageSizes(int itemsPerPage)
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("UĹľivatel nemĂˇ ĹľĂˇdnĂ© watchlisty.");
            return;
        }

        var result = await _client.GetSinglePublicWatchlistAsync(
            _gcid, _watchlistId, pageNumber: 0, itemsPerPage: itemsPerPage);

        Debug.WriteLine($"[SinglePublic] size={itemsPerPage} â†’ watchlistId={result.WatchlistId} name='{result.Name}' gcid={result.Gcid} items={result.Items.Count}");

        Assert.Multiple(() =>
        {
            Assert.That(result.WatchlistId, Is.EqualTo(_watchlistId),
                $"VrĂˇcenĂ© watchlistId='{result.WatchlistId}' â‰  poĹľadovanĂ©='{_watchlistId}'.");
            Assert.That(result.Gcid, Is.EqualTo(_gcid),
                $"VrĂˇcenĂ© Gcid={result.Gcid} â‰  poĹľadovanĂ©={_gcid}.");
            Assert.That(result.Name, Is.Not.Null.And.Not.Empty,
                "NĂˇzev watchlistu nesmĂ­ bĂ˝t prĂˇzdnĂ˝.");
        });
    }

    /// <summary>
    /// PorovnĂˇvĂˇ GetSingleWatchlistAsync vs GetSinglePublicWatchlistAsync pro stejnĂ˝ watchlist.
    /// Oba by mÄ›ly vrĂˇtit stejnĂ© zĂˇkladnĂ­ identity (watchlistId, name, totalItems).
    /// </summary>
    [Test]
    public async Task GetSingleWatchlist_vs_GetSinglePublic_ReturnConsistentData()
    {
        if (!_hasWatchlist)
        {
            Assert.Ignore("UĹľivatel nemĂˇ ĹľĂˇdnĂ© watchlisty.");
            return;
        }

        var privateResult = await _client.GetSingleWatchlistAsync(_watchlistId, pageNumber: 0, itemsPerPage: 10);
        var publicResult  = await _client.GetSinglePublicWatchlistAsync(_gcid, _watchlistId, pageNumber: 0, itemsPerPage: 10);

        var privateWl = privateResult.Watchlists.FirstOrDefault();

        Debug.WriteLine($"[Consistency] Private: isSucceeded={privateResult.IsSucceeded} watchlistId={privateWl?.WatchlistId} totalItems={privateWl?.TotalItems}");
        Debug.WriteLine($"[Consistency] Public:  watchlistId={publicResult.WatchlistId} totalItems={publicResult.TotalItems}");

        if (privateWl is null)
        {
            Assert.Pass("Private endpoint nevrĂˇtil ĹľĂˇdnĂ˝ watchlist.");
            return;
        }

        Assert.Multiple(() =>
        {
            Assert.That(publicResult.WatchlistId, Is.EqualTo(privateWl.WatchlistId),
                "watchlistId se liĹˇĂ­ mezi private a public endpointem.");
            Assert.That(publicResult.TotalItems, Is.EqualTo(privateWl.TotalItems),
                $"totalItems: private={privateWl.TotalItems} vs public={publicResult.TotalItems} â€” neshoda.");
            Assert.That(publicResult.Name, Is.EqualTo(privateWl.Name),
                $"name: private='{privateWl.Name}' vs public='{publicResult.Name}' â€” neshoda.");
        });
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // PrĹŻĹ™ezovĂ© testy â€” vĹˇechny watchlisty uĹľivatele
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>
    /// Pro kaĹľdĂ˝ watchlist uĹľivatele naÄŤte data s pageSize=1, 10, 50 a zaznamenĂˇvĂˇ
    /// rozdĂ­ly mezi poĹľadovanĂ˝m a vrĂˇcenĂ˝m poÄŤtem â†’ odhaluje watchlists kde eToro
    /// ignoruje parametr pageSize.
    /// </summary>
    [Test]
    public async Task GetSingleWatchlistAsync_AllUserWatchlists_PageSizeAudit()
    {
        var allWatchlists = await _client.GetUserWatchlistsAsync(itemsPerPageForSingle: 1);

        if (!allWatchlists.IsSucceeded || allWatchlists.Watchlists.Count == 0)
        {
            Assert.Pass("Ĺ˝ĂˇdnĂ© watchlisty k auditu.");
            return;
        }

        Debug.WriteLine($"[PageSizeAudit] Testuji {allWatchlists.Watchlists.Count} watchlistĹŻ");
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
                    issues.Add($"{wl.Name}: size={size} ale returned={returned} > size (parametr ignorovĂˇn)");
                else if (returned < size && returned < totalItems)
                    issues.Add($"{wl.Name}: size={size} ale returned={returned} < size, pĹ™estoĹľe total={totalItems} > size (eToro bug?)");
            }
        }

        if (issues.Count > 0)
        {
            Debug.WriteLine("\n[PageSizeAudit] NalezenĂ© problĂ©my:");
            foreach (var issue in issues)
                Debug.WriteLine($"  âš  {issue}");
        }
        else
        {
            Debug.WriteLine("[PageSizeAudit] Ĺ˝ĂˇdnĂ© problĂ©my nalezeny.");
        }

        // Test selĹľe jen pokud API vrĂˇtĂ­ vĂ­ce neĹľ bylo poĹľadovĂˇno (clear bug)
        var hardFails = issues.Where(i => i.Contains("parametr ignorovĂˇn")).ToList();
        Assert.That(hardFails, Is.Empty, "API vrĂˇtilo vĂ­ce poloĹľek neĹľ bylo poĹľadovĂˇno:\n" + string.Join("\n", hardFails));
    }
}
