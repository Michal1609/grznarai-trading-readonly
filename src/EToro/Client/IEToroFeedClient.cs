using GrznarAi.Trading.ReadOnly.Models.Feed;

namespace GrznarAi.Trading.ReadOnly.Client;

public interface IEToroFeedClient
{
    /// <summary>
    /// Retrieves feed posts associated with a specific financial instrument, including discussions, analyses, and other content.
    /// <br/>CZ: Vrátí příspěvky z feedu pro konkrétní finanční nástroj, včetně diskuzí, analýz a dalšího obsahu.
    /// </summary>
    Task<DiscussionsResponse> GetInstrumentFeedPostsAsync(
        int marketId,
        FeedPostsRequest? request = null,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves feed posts associated with a specific user, including their discussions, analyses, and posted content.
    /// <br/>CZ: Vrátí příspěvky z feedu konkrétního uživatele, včetně jeho diskuzí, analýz a zveřejněného obsahu.
    /// </summary>
    Task<DiscussionsResponse> GetUserFeedPostsAsync(
        int userId,
        FeedPostsRequest? request = null,
        CancellationToken ct = default);
}
