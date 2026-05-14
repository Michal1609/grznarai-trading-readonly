using GrznarAi.Trading.ReadOnly.Etoro.Models.Identity;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

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
