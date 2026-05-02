using System.Net;
using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Feed;
using GrznarAi.Trading.ReadOnly.Tests.Helpers;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class FeedClientTests
{
    private const string BaseUrl = "https://public-api.etoro.com/api/v1/";

    private static EToroClient CreateClient(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        return new EToroClient(httpClient);
    }

    private static string EmptyDiscussionsJson => """{ "discussions": [], "paging": null, "metadata": null }""";

    // ─── GetInstrumentFeedPostsAsync ──────────────────────────────────────────

    [Test]
    public async Task GetInstrumentFeedPostsAsync_CallsCorrectEndpoint()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);

        await CreateClient(handler).GetInstrumentFeedPostsAsync(marketId: 1234);

        Assert.That(handler.LastRequestUri, Does.Contain("feeds/instrument/1234"));
    }

    [Test]
    public async Task GetInstrumentFeedPostsAsync_PassesDefaultQueryParams()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);

        await CreateClient(handler).GetInstrumentFeedPostsAsync(marketId: 1);

        Assert.That(handler.LastRequestUri, Does.Contain("take=20"));
        Assert.That(handler.LastRequestUri, Does.Contain("offset=0"));
        Assert.That(handler.LastRequestUri, Does.Contain("reactionsPageSize=10"));
        Assert.That(handler.LastRequestUri, Does.Contain("badgesExperimentIsEnabled=false"));
    }

    [Test]
    public async Task GetInstrumentFeedPostsAsync_PassesCustomQueryParams()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);

        await CreateClient(handler).GetInstrumentFeedPostsAsync(
            marketId: 1,
            new FeedPostsRequest
            {
                Take = 50,
                Offset = 20,
                BadgesExperimentIsEnabled = true,
                ReactionsPageSize = 25,
                RequesterUserId = "user-99"
            });

        Assert.That(handler.LastRequestUri, Does.Contain("take=50"));
        Assert.That(handler.LastRequestUri, Does.Contain("offset=20"));
        Assert.That(handler.LastRequestUri, Does.Contain("badgesExperimentIsEnabled=true"));
        Assert.That(handler.LastRequestUri, Does.Contain("reactionsPageSize=25"));
        Assert.That(handler.LastRequestUri, Does.Contain("requesterUserId=user-99"));
    }

    [Test]
    public async Task GetInstrumentFeedPostsAsync_OmitsRequesterUserIdWhenNull()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);

        await CreateClient(handler).GetInstrumentFeedPostsAsync(
            marketId: 1,
            new FeedPostsRequest { RequesterUserId = null });

        Assert.That(handler.LastRequestUri, Does.Not.Contain("requesterUserId"));
    }

    [Test]
    public async Task GetInstrumentFeedPostsAsync_DeserializesDiscussions()
    {
        var json = """
        {
          "discussions": [
            {
              "id": "disc-1",
              "post": {
                "id": "post-1",
                "owner": { "id": "42", "username": "trader_joe", "fullname": "Joe Trader" },
                "message": { "text": "AAPL looks bullish!", "languageCode": "en" },
                "created": "2025-03-01T10:00:00Z",
                "type": "regular",
                "isDeleted": false,
                "isSpam": false,
                "attachments": [],
                "tags": [],
                "mentions": []
              },
              "commentsData": { "totalCount": 3, "comments": [] },
              "emotionsData": { "total": 10, "requesterEmotion": null }
            }
          ],
          "paging": { "next": null, "offSet": 0, "take": 20, "version": "v1" },
          "metadata": { "streamType": "Instrument" }
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetInstrumentFeedPostsAsync(1234);

        Assert.That(result.Discussions, Has.Count.EqualTo(1));
        var disc = result.Discussions[0];
        Assert.That(disc.Id, Is.EqualTo("disc-1"));
        Assert.That(disc.Post, Is.Not.Null);
        Assert.That(disc.Post!.Owner!.Username, Is.EqualTo("trader_joe"));
        Assert.That(disc.Post.Message!.Text, Is.EqualTo("AAPL looks bullish!"));
        Assert.That(disc.CommentsData!.TotalCount, Is.EqualTo(3));
        Assert.That(result.Paging!.Take, Is.EqualTo(20));
        Assert.That(result.Metadata!.StreamType, Is.EqualTo("Instrument"));
    }

    [Test]
    public async Task GetInstrumentFeedPostsAsync_ThrowsOnError()
    {
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.Unauthorized);

        var exception = Assert.ThrowsAsync<EToroApiException>(
            () => CreateClient(handler).GetInstrumentFeedPostsAsync(1));

        Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(exception.Endpoint, Does.Contain("feeds/instrument/1"));
    }

    [Test]
    public void GetInstrumentFeedPostsAsync_ZeroMarketId_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetInstrumentFeedPostsAsync(0));
    }

    [Test]
    public void GetInstrumentFeedPostsAsync_TakeAboveLimit_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetInstrumentFeedPostsAsync(1, new FeedPostsRequest { Take = EToroRequestLimits.MaxTake + 1 }));
    }

    [Test]
    public void GetInstrumentFeedPostsAsync_RequesterUserIdWithControlCharacter_ThrowsArgumentException()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentException>(() =>
            client.GetInstrumentFeedPostsAsync(1, new FeedPostsRequest { RequesterUserId = "user\n1" }));
    }

    // ─── GetUserFeedPostsAsync ────────────────────────────────────────────────

    [Test]
    public async Task GetUserFeedPostsAsync_CallsCorrectEndpoint()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);

        await CreateClient(handler).GetUserFeedPostsAsync(userId: 5678);

        Assert.That(handler.LastRequestUri, Does.Contain("feeds/user/5678"));
    }

    [Test]
    public async Task GetUserFeedPostsAsync_PassesDefaultQueryParams()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);

        await CreateClient(handler).GetUserFeedPostsAsync(userId: 1);

        Assert.That(handler.LastRequestUri, Does.Contain("take=20"));
        Assert.That(handler.LastRequestUri, Does.Contain("offset=0"));
        Assert.That(handler.LastRequestUri, Does.Contain("reactionsPageSize=10"));
        Assert.That(handler.LastRequestUri, Does.Contain("badgesExperimentIsEnabled=false"));
    }

    [Test]
    public async Task GetUserFeedPostsAsync_PassesCustomQueryParams()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);

        await CreateClient(handler).GetUserFeedPostsAsync(
            userId: 1,
            new FeedPostsRequest
            {
                Take = 100,
                Offset = 40,
                BadgesExperimentIsEnabled = true,
                ReactionsPageSize = 50,
                RequesterUserId = "req-user"
            });

        Assert.That(handler.LastRequestUri, Does.Contain("take=100"));
        Assert.That(handler.LastRequestUri, Does.Contain("offset=40"));
        Assert.That(handler.LastRequestUri, Does.Contain("badgesExperimentIsEnabled=true"));
        Assert.That(handler.LastRequestUri, Does.Contain("reactionsPageSize=50"));
        Assert.That(handler.LastRequestUri, Does.Contain("requesterUserId=req-user"));
    }

    [Test]
    public async Task GetUserFeedPostsAsync_OmitsRequesterUserIdWhenNull()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);

        await CreateClient(handler).GetUserFeedPostsAsync(
            userId: 1,
            new FeedPostsRequest { RequesterUserId = null });

        Assert.That(handler.LastRequestUri, Does.Not.Contain("requesterUserId"));
    }

    [Test]
    public async Task GetUserFeedPostsAsync_DeserializesAttachments()
    {
        var json = """
        {
          "discussions": [
            {
              "id": "disc-2",
              "post": {
                "id": "post-2",
                "owner": { "id": "7", "username": "investora" },
                "message": { "text": "Check this chart", "languageCode": "en" },
                "created": "2025-04-01T08:00:00Z",
                "type": "regular",
                "isDeleted": false,
                "isSpam": false,
                "attachments": [
                  {
                    "type": "image",
                    "url": "https://example.com/chart.png",
                    "thumbnailUrl": "https://example.com/chart-thumb.png",
                    "mediaType": "image/png",
                    "metadata": { "width": 800, "height": 600 }
                  }
                ],
                "tags": [],
                "mentions": []
              }
            }
          ],
          "paging": null,
          "metadata": null
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetUserFeedPostsAsync(7);

        var attachment = result.Discussions[0].Post!.Attachments![0];
        Assert.That(attachment.Type, Is.EqualTo("image"));
        Assert.That(attachment.Url, Is.EqualTo("https://example.com/chart.png"));
        Assert.That(attachment.Metadata!.Width, Is.EqualTo(800));
        Assert.That(attachment.Metadata.Height, Is.EqualTo(600));
    }

    [Test]
    public async Task GetUserFeedPostsAsync_ThrowsOnError()
    {
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.Forbidden);

        var exception = Assert.ThrowsAsync<EToroApiException>(
            () => CreateClient(handler).GetUserFeedPostsAsync(1));

        Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(exception.Endpoint, Does.Contain("feeds/user/1"));
    }

    [Test]
    public void GetUserFeedPostsAsync_ZeroUserId_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetUserFeedPostsAsync(0));
    }

    [Test]
    public void GetUserFeedPostsAsync_ReactionsPageSizeAboveLimit_ThrowsArgumentOutOfRangeException()
    {
        var handler = new MockHttpMessageHandler(EmptyDiscussionsJson);
        var client = CreateClient(handler);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetUserFeedPostsAsync(1, new FeedPostsRequest
            {
                ReactionsPageSize = EToroRequestLimits.MaxReactionsPageSize + 1
            }));
    }
}
