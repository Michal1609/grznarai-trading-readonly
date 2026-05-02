using GrznarAi.Trading.ReadOnly.Models.PiData;

namespace GrznarAi.Trading.ReadOnly.Client;

public sealed partial class EToroClient
{
    public async Task<CopiersResponse> GetCopiersPublicInfoAsync(CancellationToken ct = default)
    {
        return await GetFromJsonAsync<CopiersResponse>(
            "pi-data/copiers",
            "Empty copiers response.",
            ct).ConfigureAwait(false);
    }
}
