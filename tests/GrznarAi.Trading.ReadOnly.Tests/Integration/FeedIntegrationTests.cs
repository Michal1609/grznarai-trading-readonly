using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Models.Feed;
using GrznarAi.Trading.ReadOnly.Models.Market;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Tests.Integration;

/// <summary>
/// Integrační testy pro sekci FEED.
/// Spuštění: dotnet test --filter "Category=Integration"
/// Nutné: nastavit EToroOptions__ApiKey + EToroOptions__UserKey nebo lokální appsettings.test.json
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruční spuštění — vyžaduje reálné API klíče mimo git")]
public class FeedIntegrationTests
{
    private IEToroClient _client = null!;

    // AAPL instrument ID on eToro
    private const int AaplMarketId = 1586;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();
    }

    // ─── GetInstrumentFeedPostsAsync ──────────────────────────────────────────

    [Test]
    public async Task GetInstrumentFeedPostsAsync_ReturnsDiscussions()
    {
        var result = await _client.GetInstrumentFeedPostsAsync(AaplMarketId, new FeedPostsRequest { Take = 5 });

        Debug.WriteLine($"Discussions count: {result.Discussions.Count}");
        foreach (var d in result.Discussions)
            Debug.WriteLine($"  ID={d.Id} Owner={d.Post?.Owner?.Username} Text={d.Post?.Message?.Text?[..Math.Min(60, d.Post.Message.Text.Length)]}");

        Assert.That(result.Discussions, Is.Not.Null);
    }

    [Test]
    public async Task GetInstrumentFeedPostsAsync_RespectsTakeParam()
    {
        var result = await _client.GetInstrumentFeedPostsAsync(AaplMarketId, new FeedPostsRequest { Take = 3 });

        Assert.That(result.Discussions.Count, Is.LessThanOrEqualTo(3));
    }

    [Test]
    public async Task GetInstrumentFeedPostsAsync_PagingIsPopulated()
    {
        var result = await _client.GetInstrumentFeedPostsAsync(AaplMarketId, new FeedPostsRequest { Take = 5 });

        Debug.WriteLine($"Paging: offset={result.Paging?.Offset} take={result.Paging?.Take} next={result.Paging?.Next}");

        Assert.That(result.Paging, Is.Not.Null);
        Assert.That(result.Paging!.Take, Is.EqualTo(5));
    }

    [Test]
    public async Task GetInstrumentFeedPostsAsync_DiscussionsHaveOwners()
    {
        var result = await _client.GetInstrumentFeedPostsAsync(AaplMarketId, new FeedPostsRequest { Take = 10 });

        foreach (var d in result.Discussions)
        {
            Assert.That(d.Post, Is.Not.Null, $"Discussion {d.Id} has no post.");
            Assert.That(d.Post!.Owner, Is.Not.Null, $"Post {d.Post.Id} has no owner.");
            Assert.That(d.Post.Owner!.Username, Is.Not.Null.And.Not.Empty,
                $"Post {d.Post.Id} owner has empty username.");
        }
    }

    [Test]
    public async Task GetInstrumentFeedPostsAsync_OffsetPaginationWorks()
    {
        var page1 = await _client.GetInstrumentFeedPostsAsync(AaplMarketId, new FeedPostsRequest { Take = 3, Offset = 0 });
        var page2 = await _client.GetInstrumentFeedPostsAsync(AaplMarketId, new FeedPostsRequest { Take = 3, Offset = 3 });

        var ids1 = page1.Discussions.Select(d => d.Id).ToHashSet();
        var ids2 = page2.Discussions.Select(d => d.Id).ToHashSet();

        Debug.WriteLine($"Page1 IDs: {string.Join(", ", ids1)}");
        Debug.WriteLine($"Page2 IDs: {string.Join(", ", ids2)}");

        Assert.That(ids1.Intersect(ids2), Is.Empty, "Paginated pages should not overlap.");
    }

    // ─── GetUserFeedPostsAsync ────────────────────────────────────────────────

    [Test]
    public async Task GetUserFeedPostsAsync_ReturnsDiscussions()
    {
        var instruments = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = [InstrumentFields.InstrumentId, InstrumentFields.Symbol],
            PageSize = 5
        });
        var firstDiscussion = await _client.GetInstrumentFeedPostsAsync(
            instruments.Instruments[4].InstrumentId ?? AaplMarketId,
            new FeedPostsRequest { Take = 1 });

        if (firstDiscussion.Discussions.Count == 0)
        {
            Assert.Pass("No discussions available to get a userId from.");
            return;
        }

        var ownerIdStr = firstDiscussion.Discussions[0].Post?.Owner?.Id;
        if (ownerIdStr is null || !int.TryParse(ownerIdStr, out var userId))
        {
            Assert.Pass("Post has no parseable owner id.");
            return;
        }

        var result = await _client.GetUserFeedPostsAsync(userId, new FeedPostsRequest { Take = 5 });

        Debug.WriteLine($"User {userId} feed discussions: {result.Discussions.Count}");
        foreach (var d in result.Discussions)
            Debug.WriteLine($"  ID={d.Id} Text={d.Post?.Message?.Text?[..Math.Min(60, d.Post.Message.Text.Length)]}");

        Assert.That(result.Discussions, Is.Not.Null);
    }

    [Test]
    public async Task GetUserFeedPostsAsync_RespectsTakeParam()
    {
        var instFeed = await _client.GetInstrumentFeedPostsAsync(AaplMarketId, new FeedPostsRequest { Take = 1 });
        if (instFeed.Discussions.Count == 0) Assert.Pass("No feed posts to get userId.");

        var ownerIdStr1 = instFeed.Discussions[0].Post?.Owner?.Id;
        if (!int.TryParse(ownerIdStr1, out var userId1)) Assert.Pass("No parseable owner id.");

        var result = await _client.GetUserFeedPostsAsync(userId1, new FeedPostsRequest { Take = 2 });

        Assert.That(result.Discussions.Count, Is.LessThanOrEqualTo(2));
    }

    [Test]
    public async Task GetUserFeedPostsAsync_PagingIsPopulated()
    {
        var instFeed = await _client.GetInstrumentFeedPostsAsync(AaplMarketId, new FeedPostsRequest { Take = 1 });
        if (instFeed.Discussions.Count == 0) Assert.Pass("No feed posts to get userId.");

        var ownerIdStr2 = instFeed.Discussions[0].Post?.Owner?.Id;
        if (!int.TryParse(ownerIdStr2, out var userId2)) Assert.Pass("No parseable owner id.");

        var result = await _client.GetUserFeedPostsAsync(userId2, new FeedPostsRequest { Take = 5 });

        Debug.WriteLine($"Paging: offset={result.Paging?.Offset} take={result.Paging?.Take}");

        Assert.That(result.Paging, Is.Not.Null);
    }
}
