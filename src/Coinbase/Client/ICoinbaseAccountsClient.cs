using GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public interface ICoinbaseAccountsClient
{
    /// <summary>
    /// List the authenticated user's brokerage accounts.
    /// <br/>CZ: VrĂˇtĂ­ brokerage ĂşÄŤty pĹ™ihlĂˇĹˇenĂ©ho uĹľivatele.
    /// </summary>
    Task<ListAccountsResponse> ListAccountsAsync(int? limit = null, string? cursor = null, CancellationToken ct = default);

    /// <summary>
    /// Get a single brokerage account by UUID.
    /// <br/>CZ: VrĂˇtĂ­ jeden brokerage ĂşÄŤet podle UUID.
    /// </summary>
    Task<GetAccountResponse> GetAccountAsync(string accountUuid, CancellationToken ct = default);
}
