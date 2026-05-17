namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Known string values for <see cref="ConvertTrade.Status"/> returned by the Coinbase Convert API.
/// </summary>
public static class TradeStatus
{
    public const string Unspecified = "TRADE_STATUS_UNSPECIFIED";
    public const string Created = "TRADE_STATUS_CREATED";
    public const string Started = "TRADE_STATUS_STARTED";
    public const string Completed = "TRADE_STATUS_COMPLETED";
    public const string Canceled = "TRADE_STATUS_CANCELED";
}
