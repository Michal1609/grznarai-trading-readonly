namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Known <c>expiring_contract_status</c> filter values for <c>list-products</c>.</summary>
public static class ExpiringContractStatus
{
    public const string Unknown = "UNKNOWN_EXPIRING_CONTRACT_STATUS";
    public const string Unexpired = "STATUS_UNEXPIRED";
    public const string Expired = "STATUS_EXPIRED";
    public const string All = "STATUS_ALL";
}
