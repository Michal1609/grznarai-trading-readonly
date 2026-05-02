using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Tests.Integration;

/// <summary>
/// Integrační testy pro sekci WATCHLIST.
/// Spuštění: dotnet test --filter "Category=Integration"
/// Nutné: nastavit EToroOptions__ApiKey + EToroOptions__UserKey nebo lokální appsettings.test.json
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruční spuštění — vyžaduje reálné API klíče mimo git")]
public class WatchlistIntegrationTests
{
    private IEToroClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();
    }

    // ─── GetCuratedListsAsync ─────────────────────────────────────────────────

    [Test]
    public async Task GetCuratedListsAsync_ReturnsResponseOrNull()
    {
        var result = await _client.GetCuratedListsAsync();

        if (result is null)
        {
            Debug.WriteLine("GetCuratedListsAsync returned null (HTTP 204 — no curated lists).");
            Assert.Pass("API returned 204 No Content.");
            return;
        }

        Debug.WriteLine($"Curated lists count: {result.CuratedLists.Count}");
        foreach (var list in result.CuratedLists)
            Debug.WriteLine($"  UUID={list.Uuid} Name='{list.Name}' Items={list.Items.Count}");

        Assert.That(result.CuratedLists, Is.Not.Null);
    }

    [Test]
    public async Task GetCuratedListsAsync_ItemsHaveValidInstrumentIds()
    {
        var result = await _client.GetCuratedListsAsync();
        if (result is null) Assert.Pass("No curated lists (HTTP 204).");

        foreach (var list in result!.CuratedLists)
        foreach (var item in list.Items)
            Assert.That(item.InstrumentId, Is.GreaterThan(0),
                $"List '{list.Name}' has item with non-positive instrumentId.");
    }

    // ─── GetMarketRecommendationsAsync ────────────────────────────────────────

    [Test]
    public async Task GetMarketRecommendationsAsync_ReturnsResponseOrNull()
    {
        var result = await _client.GetMarketRecommendationsAsync(itemsCount: 5);

        if (result is null)
        {
            Debug.WriteLine("GetMarketRecommendationsAsync returned null (HTTP 204).");
            Assert.Pass("API returned 204 No Content.");
            return;
        }

        Debug.WriteLine($"ResponseType={result.ResponseType} Recommendations={string.Join(",", result.Recommendations)}");

        Assert.That(result.Recommendations, Is.Not.Null);
    }

    [Test]
    public async Task GetMarketRecommendationsAsync_CountDoesNotExceedRequested()
    {
        var result = await _client.GetMarketRecommendationsAsync(itemsCount: 3);
        if (result is null) Assert.Pass("No recommendations (HTTP 204).");

        Assert.That(result!.Recommendations.Count, Is.LessThanOrEqualTo(3));
    }

    // ─── GetUserWatchlistsAsync ───────────────────────────────────────────────

    [Test]
    public async Task GetUserWatchlistsAsync_ReturnsWatchlists()
    {
        var result = await _client.GetUserWatchlistsAsync(itemsPerPageForSingle: 10);

        Debug.WriteLine($"isSucceeded={result.IsSucceeded} Watchlists={result.Watchlists.Count}");
        foreach (var w in result.Watchlists)
            Debug.WriteLine($"  ID={w.WatchlistId} Name='{w.Name}' Type={w.WatchlistType} Items={w.TotalItems}");

        Assert.That(result.IsSucceeded, Is.True);
        Assert.That(result.Watchlists, Is.Not.Null);
    }

    [Test]
    public async Task GetUserWatchlistsAsync_AllWatchlistsHaveIds()
    {
        var result = await _client.GetUserWatchlistsAsync();

        foreach (var w in result.Watchlists)
            Assert.That(w.WatchlistId, Is.Not.Null.And.Not.Empty,
                $"Watchlist '{w.Name}' has empty ID.");
    }

    // ─── GetDefaultWatchlistItemsAsync ────────────────────────────────────────

    [Test]
    public async Task GetDefaultWatchlistItemsAsync_ReturnsItems()
    {
        var result = await _client.GetDefaultWatchlistItemsAsync(itemsPerPage: 10);

        Debug.WriteLine($"Default watchlist items: {result.Count}");
        foreach (var item in result)
            Debug.WriteLine($"  ItemId={item.ItemId} Type={item.ItemType} Symbol={item.Market?.SymbolName}");

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetDefaultWatchlistItemsAsync_AllItemsHaveValidIds()
    {
        var result = await _client.GetDefaultWatchlistItemsAsync();

        foreach (var item in result)
            Assert.That(item.ItemId, Is.GreaterThan(0),
                $"Item has non-positive itemId.");
    }

    // ─── GetUsersPublicWatchlistsAsync ────────────────────────────────────────

    [Test]
    public async Task GetUsersPublicWatchlistsAsync_ValidUserReturnsWatchlists()
    {
        var myWatchlists = await _client.GetUserWatchlistsAsync();
        if (myWatchlists.Watchlists.Count == 0) Assert.Pass("No watchlists to reference a userId.");

        var gcid = myWatchlists.Watchlists[0].Gcid;
        var result = await _client.GetUsersPublicWatchlistsAsync(userId: gcid);

        Debug.WriteLine($"Public watchlists for user {gcid}: {result.Watchlists.Count}");

        Assert.That(result.IsSucceeded, Is.True);
        Assert.That(result.Watchlists, Is.Not.Null);
    }

    // ─── GetSinglePublicWatchlistAsync ────────────────────────────────────────

    [Test]
    public async Task GetSinglePublicWatchlistAsync_ValidIdsReturnWatchlist()
    {
        var myWatchlists = await _client.GetUserWatchlistsAsync();
        if (myWatchlists.Watchlists.Count == 0) Assert.Pass("No watchlists available.");

        var wl = myWatchlists.Watchlists[0];
        var result = await _client.GetSinglePublicWatchlistAsync(
            userId: wl.Gcid, watchlistId: wl.WatchlistId);

        Debug.WriteLine($"Single public watchlist: ID={result.WatchlistId} Name='{result.Name}'");

        Assert.That(result.WatchlistId, Is.EqualTo(wl.WatchlistId));
    }

    // ─── GetSingleWatchlistAsync ──────────────────────────────────────────────

    [Test]
    public async Task GetSingleWatchlistAsync_ValidIdReturnsWatchlist()
    {
        var myWatchlists = await _client.GetUserWatchlistsAsync();
        if (myWatchlists.Watchlists.Count == 0) Assert.Pass("No watchlists available.");

        var watchlistId = myWatchlists.Watchlists[0].WatchlistId;
        var result = await _client.GetSingleWatchlistAsync(watchlistId, pageNumber: 0, itemsPerPage: 10);

        Debug.WriteLine($"Single watchlist response: isSucceeded={result.IsSucceeded} count={result.Watchlists.Count}");

        Assert.That(result.IsSucceeded, Is.True);
        Assert.That(result.Watchlists, Is.Not.Null);
    }
}
