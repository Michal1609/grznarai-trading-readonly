using GrznarAi.Trading.ReadOnly.Etoro.Models.Feed;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

public interface IEToroFeedClient
{
    /// <summary>
    /// Retrieves feed posts associated with a specific financial instrument, including discussions, analyses, and other content.
    /// <br/>CZ: VrĂˇtĂ­ pĹ™Ă­spÄ›vky z feedu pro konkrĂ©tnĂ­ finanÄŤnĂ­ nĂˇstroj, vÄŤetnÄ› diskuzĂ­, analĂ˝z a dalĹˇĂ­ho obsahu.
    /// </summary>
    Task<DiscussionsResponse> GetInstrumentFeedPostsAsync(
        int marketId,
        FeedPostsRequest? request = null,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves feed posts associated with a specific user, including their discussions, analyses, and posted content.
    /// <br/>CZ: VrĂˇtĂ­ pĹ™Ă­spÄ›vky z feedu konkrĂ©tnĂ­ho uĹľivatele, vÄŤetnÄ› jeho diskuzĂ­, analĂ˝z a zveĹ™ejnÄ›nĂ©ho obsahu.
    /// </summary>
    Task<DiscussionsResponse> GetUserFeedPostsAsync(
        int userId,
        FeedPostsRequest? request = null,
        CancellationToken ct = default);
}
