namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

/// <summary>
/// Known <c>contract_expiry_type</c> values used across Coinbase Advanced Trade API endpoints.
/// Only applicable when <c>product_type</c> is <see cref="ProductType.Future"/>.
/// </summary>
public static class ContractExpiryType
{
    public const string Unknown = "UNKNOWN_CONTRACT_EXPIRY_TYPE";
    public const string Expiring = "EXPIRING";
    public const string Perpetual = "PERPETUAL";
}
