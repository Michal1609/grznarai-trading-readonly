using GrznarAi.Trading.ReadOnly.Coinbase.Models.DataApi;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    /// <inheritdoc cref="ICoinbaseDataApiClient.GetApiKeyPermissionsAsync"/>
    public Task<GetApiKeyPermissionsResponse> GetApiKeyPermissionsAsync(CancellationToken ct = default) =>
        GetFromJsonAsync<GetApiKeyPermissionsResponse>(
            "/api/v3/brokerage/key_permissions",
            "Empty get-api-key-permissions response.",
            ct);
}
