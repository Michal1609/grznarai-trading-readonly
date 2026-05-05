using System.Net;
using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Tests.Helpers;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class WatchlistClientTests
{
    private const string BaseUrl = "https://public-api.etoro.com/api/v1/";

    private static EToroClient CreateClient(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        return new EToroClient(httpClient);
    }

    // ─── GetCuratedListsAsync ─────────────────────────────────────────────────

    [Test]
    public async Task GetCuratedListsAsync_CallsCorrectEndpoint()
    {
        var json = """{ "curatedLists": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetCuratedListsAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("curated-lists"));
    }

    [Test]
    public async Task GetCuratedListsAsync_Returns204AsNull()
    {
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.NoContent);

        var result = await CreateClient(handler).GetCuratedListsAsync();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetCuratedListsAsync_DeserializesList()
    {
        var json = """
        {
          "curatedLists": [
            {
              "uuid": "abc-123",
              "name": "Top Tech",
              "description": "Best tech stocks",
              "listImageUrl": "https://example.com/img.png",
              "items": [
                { "instrumentId": 1001 },
                { "instrumentId": 1002 }
              ]
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetCuratedListsAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CuratedLists, Has.Count.EqualTo(1));
        var list = result.CuratedLists[0];
        Assert.That(list.Uuid, Is.EqualTo("abc-123"));
        Assert.That(list.Name, Is.EqualTo("Top Tech"));
        Assert.That(list.Items, Has.Count.EqualTo(2));
        Assert.That(list.Items[0].InstrumentId, Is.EqualTo(1001));
    }

    // ─── GetMarketRecommendationsAsync ────────────────────────────────────────

    [Test]
    public async Task GetMarketRecommendationsAsync_CallsCorrectEndpointWithCount()
    {
        var json = """{ "responseType": "Instrument", "recommendations": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetMarketRecommendationsAsync(itemsCount: 5);

        Assert.That(handler.LastRequestUri, Does.Contain("market-recommendations/5"));
    }

    [Test]
    public async Task GetMarketRecommendationsAsync_DefaultCount10()
    {
        var json = """{ "responseType": "Instrument", "recommendations": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetMarketRecommendationsAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("market-recommendations/10"));
    }

    [Test]
    public async Task GetMarketRecommendationsAsync_Returns204AsNull()
    {
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.NoContent);

        var result = await CreateClient(handler).GetMarketRecommendationsAsync();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetMarketRecommendationsAsync_DeserializesResponse()
    {
        var json = """{ "responseType": "Instrument", "recommendations": [1001, 1002, 1003] }""";
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetMarketRecommendationsAsync(3);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ResponseType, Is.EqualTo("Instrument"));
        Assert.That(result.Recommendations, Is.EquivalentTo(new[] { 1001, 1002, 1003 }));
    }

    [Test]
    public void GetMarketRecommendationsAsync_CountAboveLimit_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetMarketRecommendationsAsync(EToroRequestLimits.MaxItemsCount + 1));
    }

    // ─── GetUserWatchlistsAsync ───────────────────────────────────────────────

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

    [Test]
    public async Task GetUserWatchlistsAsync_DeserializesWatchlists()
    {
        var json = """
        {
          "status": 200,
          "isSucceeded": true,
          "watchlists": [
            {
              "watchlistId": "wl-1",
              "name": "My Stocks",
              "Gcid": 999,
              "watchlistType": "Static",
              "totalItems": 2,
              "isDefault": false,
              "isUserSelectedDefault": true,
              "watchlistRank": 1,
              "dynamicUrl": null,
              "items": [],
              "relatedAssets": null
            }
          ],
          "meta": { "pageNumber": 0, "itemsPerPage": 100, "maxItemsInWatchlistLimit": 200, "maxWatchlistsLimit": 10 }
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetUserWatchlistsAsync();

        Assert.That(result.IsSucceeded, Is.True);
        Assert.That(result.Watchlists, Has.Count.EqualTo(1));
        var wl = result.Watchlists[0];
        Assert.That(wl.WatchlistId, Is.EqualTo("wl-1"));
        Assert.That(wl.Name, Is.EqualTo("My Stocks"));
        Assert.That(result.Meta, Is.Not.Null);
        Assert.That(result.Meta!.MaxWatchlistsLimit, Is.EqualTo(10));
    }

    [Test]
    public void GetUserWatchlistsAsync_ItemsPerPageAboveLimit_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetUserWatchlistsAsync(itemsPerPageForSingle: EToroRequestLimits.MaxItemsPerPage + 1));
    }

    // ─── GetDefaultWatchlistItemsAsync ────────────────────────────────────────

    [Test]
    public async Task GetDefaultWatchlistItemsAsync_CallsCorrectEndpoint()
    {
        var json = "[]";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetDefaultWatchlistItemsAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("watchlists/default-watchlists/items"));
    }

    [Test]
    public async Task GetDefaultWatchlistItemsAsync_PassesOptionalLimit()
    {
        var json = "[]";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetDefaultWatchlistItemsAsync(itemsLimit: 25);

        Assert.That(handler.LastRequestUri, Does.Contain("itemsLimit=25"));
    }

    [Test]
    public async Task GetDefaultWatchlistItemsAsync_DeserializesItems()
    {
        var json = """
        [
          {
            "itemId": 5001,
            "itemType": "Instrument",
            "itemRank": 1,
            "itemAddedReason": "Manual",
            "itemAddedDate": "2025-01-15T10:00:00Z",
            "market": {
              "id": "5001",
              "symbolName": "AAPL",
              "displayName": "Apple Inc",
              "assetTypeId": 4,
              "exchangeId": 1,
              "hasExpirationDate": false
            }
          }
        ]
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetDefaultWatchlistItemsAsync();

        Assert.That(result, Has.Count.EqualTo(1));
        var item = result[0];
        Assert.That(item.ItemId, Is.EqualTo(5001));
        Assert.That(item.ItemType, Is.EqualTo("Instrument"));
        Assert.That(item.Market, Is.Not.Null);
        Assert.That(item.Market!.SymbolName, Is.EqualTo("AAPL"));
    }

    [Test]
    public void GetDefaultWatchlistItemsAsync_ItemsLimitAboveLimit_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler("[]");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetDefaultWatchlistItemsAsync(itemsLimit: EToroRequestLimits.MaxItemsLimit + 1));
    }

    // ─── GetUsersPublicWatchlistsAsync ────────────────────────────────────────

    [Test]
    public async Task GetUsersPublicWatchlistsAsync_CallsCorrectEndpointWithUserId()
    {
        var json = """{ "status": 200, "isSucceeded": true, "watchlists": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetUsersPublicWatchlistsAsync(userId: 42);

        Assert.That(handler.LastRequestUri, Does.Contain("watchlists/public/42"));
    }

    [Test]
    public async Task GetUsersPublicWatchlistsAsync_PassesItemsPerPage()
    {
        var json = """{ "status": 200, "isSucceeded": true, "watchlists": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetUsersPublicWatchlistsAsync(userId: 42, itemsPerPageForSingle: 25);

        Assert.That(handler.LastRequestUri, Does.Contain("itemsPerPageForSingle=25"));
    }

    [Test]
    public void GetUsersPublicWatchlistsAsync_ZeroUserId_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetUsersPublicWatchlistsAsync(0));
    }

    // ─── GetSinglePublicWatchlistAsync ────────────────────────────────────────

    [Test]
    public async Task GetSinglePublicWatchlistAsync_CallsCorrectEndpoint()
    {
        var json = """
        {
          "status": 200,
          "isSucceeded": true,
          "watchlists": [{
            "watchlistId": "wl-pub-1",
            "name": "Public List",
            "Gcid": 100,
            "watchlistType": "Static",
            "totalItems": 0,
            "isDefault": false,
            "isUserSelectedDefault": false,
            "watchlistRank": 0,
            "items": []
          }]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetSinglePublicWatchlistAsync(userId: 7, watchlistId: "wl-pub-1");

        Assert.That(handler.LastRequestUri, Does.Contain("watchlists/public/7/wl-pub-1"));
    }

    [Test]
    public async Task GetSinglePublicWatchlistAsync_PassesPaginationParams()
    {
        var json = """
        {
          "status": 200,
          "isSucceeded": true,
          "watchlists": [{
            "watchlistId": "wl-x",
            "name": "X",
            "Gcid": 1,
            "watchlistType": "Static",
            "totalItems": 0,
            "isDefault": false,
            "isUserSelectedDefault": false,
            "watchlistRank": 0,
            "items": []
          }]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetSinglePublicWatchlistAsync(
            userId: 7, watchlistId: "wl-x", pageNumber: 2, itemsPerPage: 25);

        Assert.That(handler.LastRequestUri, Does.Contain("pageNumber=2"));
        Assert.That(handler.LastRequestUri, Does.Contain("itemsPerPage=25"));
    }

    [Test]
    public async Task GetSinglePublicWatchlistAsync_DeserializesWatchlist()
    {
        var json = """
        {
          "status": 200,
          "isSucceeded": true,
          "watchlists": [{
            "watchlistId": "pub-wl",
            "name": "Tech Watch",
            "Gcid": 55,
            "watchlistType": "Dynamic",
            "totalItems": 3,
            "isDefault": false,
            "isUserSelectedDefault": false,
            "watchlistRank": 2,
            "dynamicUrl": "https://example.com/dynamic",
            "items": [],
            "relatedAssets": [1001, 1002]
          }]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetSinglePublicWatchlistAsync(1, "pub-wl");

        Assert.That(result.WatchlistId, Is.EqualTo("pub-wl"));
        Assert.That(result.Name, Is.EqualTo("Tech Watch"));
        Assert.That(result.TotalItems, Is.EqualTo(3));
        Assert.That(result.RelatedAssets, Is.EquivalentTo(new[] { 1001, 1002 }));
    }

    [Test]
    public void GetSinglePublicWatchlistAsync_LongWatchlistId_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);
        var watchlistId = new string('x', EToroRequestLimits.MaxWatchlistIdLength + 1);

        Assert.ThrowsAsync<ArgumentException>(() => client.GetSinglePublicWatchlistAsync(1, watchlistId));
    }

    // ─── GetSingleWatchlistAsync ──────────────────────────────────────────────

    [Test]
    public async Task GetSingleWatchlistAsync_CallsCorrectEndpointWithId()
    {
        var json = """{ "status": 200, "isSucceeded": true, "watchlists": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetSingleWatchlistAsync(watchlistId: "wl-99");

        Assert.That(handler.LastRequestUri, Does.Contain("watchlists/wl-99"));
    }

    [Test]
    public async Task GetSingleWatchlistAsync_PassesPaginationParams()
    {
        var json = """{ "status": 200, "isSucceeded": true, "watchlists": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetSingleWatchlistAsync("wl-99", pageNumber: 1, itemsPerPage: 50);

        Assert.That(handler.LastRequestUri, Does.Contain("pageNumber=1"));
        Assert.That(handler.LastRequestUri, Does.Contain("itemsPerPage=50"));
    }

    [Test]
    public async Task GetSingleWatchlistAsync_DeserializesResponse()
    {
        var json = """
        {
          "status": 200,
          "isSucceeded": true,
          "watchlists": [
            {
              "watchlistId": "wl-99",
              "name": "My ETFs",
              "Gcid": 111,
              "watchlistType": "Static",
              "totalItems": 5,
              "isDefault": true,
              "isUserSelectedDefault": false,
              "watchlistRank": 0,
              "items": []
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetSingleWatchlistAsync("wl-99");

        Assert.That(result.IsSucceeded, Is.True);
        Assert.That(result.Watchlists, Has.Count.EqualTo(1));
        Assert.That(result.Watchlists[0].Name, Is.EqualTo("My ETFs"));
    }

    [Test]
    public void GetSingleWatchlistAsync_WatchlistIdWithControlCharacter_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler("{}");
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentException>(() => client.GetSingleWatchlistAsync("wl\n99"));
    }
}
