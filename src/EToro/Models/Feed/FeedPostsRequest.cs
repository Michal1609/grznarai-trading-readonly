namespace GrznarAi.Trading.ReadOnly.Models.Feed;

public sealed record FeedPostsRequest
{
    public string? RequesterUserId { get; init; }
    public int Take { get; init; } = 20;
    public int Offset { get; init; }
    public bool BadgesExperimentIsEnabled { get; init; }
    public int ReactionsPageSize { get; init; } = 10;
}
