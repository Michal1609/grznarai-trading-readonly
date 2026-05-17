namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;

/// <summary>
/// Known string values for <see cref="Account.Platform"/> returned by the Coinbase API.
/// </summary>
public static class AccountPlatform
{
    public const string Unspecified = "ACCOUNT_PLATFORM_UNSPECIFIED";
    public const string Consumer = "ACCOUNT_PLATFORM_CONSUMER";
    public const string CfmConsumer = "ACCOUNT_PLATFORM_CFM_CONSUMER";
    public const string Intx = "ACCOUNT_PLATFORM_INTX";
}
