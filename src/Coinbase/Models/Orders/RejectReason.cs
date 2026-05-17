namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>reject_reason</c> values returned by the Coinbase API.</summary>
public static class RejectReason
{
    public const string Unspecified = "REJECT_REASON_UNSPECIFIED";
    public const string HoldFailure = "HOLD_FAILURE";
    public const string TooManyOpenOrders = "TOO_MANY_OPEN_ORDERS";
    public const string InsufficientFunds = "REJECT_REASON_INSUFFICIENT_FUNDS";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
}
