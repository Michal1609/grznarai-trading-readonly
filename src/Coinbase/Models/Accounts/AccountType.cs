namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;

/// <summary>
/// Known string values for <see cref="Account.Type"/> returned by the Coinbase API.
/// </summary>
public static class AccountType
{
    public const string Unspecified = "ACCOUNT_TYPE_UNSPECIFIED";
    public const string Crypto = "ACCOUNT_TYPE_CRYPTO";
    public const string Fiat = "ACCOUNT_TYPE_FIAT";
    public const string Vault = "ACCOUNT_TYPE_VAULT";
    public const string PerpFutures = "ACCOUNT_TYPE_PERP_FUTURES";
}
