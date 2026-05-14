using GrznarAi.Trading.ReadOnly.Etoro.Models.Feed;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

public sealed partial class EToroClient
{
    public async Task<DiscussionsResponse> GetInstrumentFeedPostsAsync(
        int marketId,
        FeedPostsRequest? request = null,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(marketId);
        request ??= new FeedPostsRequest();
        ValidateFeedPostsRequest(request);

        var qs = BuildFeedPostsQueryString(request);
        return await GetFromJsonAsync<DiscussionsResponse>(
            $"feeds/instrument/{marketId}{qs}",
            "Empty instrument feed response.",
            ct).ConfigureAwait(false);
    }

    public async Task<DiscussionsResponse> GetUserFeedPostsAsync(
        int userId,
        FeedPostsRequest? request = null,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        request ??= new FeedPostsRequest();
        ValidateFeedPostsRequest(request);

        var qs = BuildFeedPostsQueryString(request);
        return await GetFromJsonAsync<DiscussionsResponse>(
            $"feeds/user/{userId}{qs}",
            "Empty user feed response.",
            ct).ConfigureAwait(false);
    }

    private static void ValidateFeedPostsRequest(FeedPostsRequest request)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(request.Take, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(request.Take, EToroRequestLimits.MaxTake);
        ArgumentOutOfRangeException.ThrowIfNegative(request.Offset);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(request.ReactionsPageSize, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(request.ReactionsPageSize, EToroRequestLimits.MaxReactionsPageSize);
        EToroInputValidator.ValidateOptionalString(request.RequesterUserId, nameof(request.RequesterUserId), EToroRequestLimits.MaxRequesterUserIdLength);
    }

    private static string BuildFeedPostsQueryString(FeedPostsRequest request) =>
        new QueryStringBuilder()
            .Add("take", request.Take)
            .Add("offset", request.Offset)
            .Add("badgesExperimentIsEnabled", request.BadgesExperimentIsEnabled)
            .Add("reactionsPageSize", request.ReactionsPageSize)
            .Add("requesterUserId", request.RequesterUserId)
            .ToString();
}
