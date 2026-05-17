namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>time_in_force</c> values for orders returned by the Coinbase API.</summary>
public static class TimeInForce
{
    public const string Unknown = "UNKNOWN_TIME_IN_FORCE";
    public const string GoodUntilDateTime = "GOOD_UNTIL_DATE_TIME";
    public const string GoodUntilCancelled = "GOOD_UNTIL_CANCELLED";
    public const string ImmediateOrCancel = "IMMEDIATE_OR_CANCEL";
    public const string FillOrKill = "FILL_OR_KILL";
}
