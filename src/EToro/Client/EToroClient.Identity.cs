using GrznarAi.Trading.ReadOnly.Models.Identity;

namespace GrznarAi.Trading.ReadOnly.Client;

public sealed partial class EToroClient
{
    public async Task<UserIdentityResponse> GetIdentityAsync(CancellationToken ct = default)
    {
        return await GetFromJsonAsync<UserIdentityResponse>(
            "me",
            "Empty identity response.",
            ct).ConfigureAwait(false);
    }
}
