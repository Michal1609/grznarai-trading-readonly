namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.DataApi;

/// <summary>
/// Known string values for <see cref="ApiKeyPermissions.PortfolioType"/> returned by the Coinbase API.
/// </summary>
public static class PortfolioType
{
    public const string Undefined = "UNDEFINED";
    public const string Default = "DEFAULT";
    public const string Consumer = "CONSUMER";
    public const string Intx = "INTX";
}
