namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>status</c> values for orders returned by the Coinbase API.</summary>
public static class OrderStatus
{
    public const string Pending = "PENDING";
    public const string Open = "OPEN";
    public const string Filled = "FILLED";
    public const string Cancelled = "CANCELLED";
    public const string Expired = "EXPIRED";
    public const string Failed = "FAILED";
    public const string Unknown = "UNKNOWN_ORDER_STATUS";
    public const string Queued = "QUEUED";
    public const string CancelQueued = "CANCEL_QUEUED";
    public const string EditQueued = "EDIT_QUEUED";
}
