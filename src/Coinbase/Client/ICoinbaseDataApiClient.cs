using GrznarAi.Trading.ReadOnly.Coinbase.Models.DataApi;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Read-only client for Coinbase Advanced Trade Data API endpoints.
/// </summary>
public interface ICoinbaseDataApiClient
{
    /// <summary>
    /// Get the permissions associated with the current API key.
    /// <para>
    /// Endpoint: <c>GET /api/v3/brokerage/key_permissions</c>
    /// </para>
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<GetApiKeyPermissionsResponse> GetApiKeyPermissionsAsync(CancellationToken ct = default);
}
