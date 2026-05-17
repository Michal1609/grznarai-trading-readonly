namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>order_type</c> values returned by the Coinbase API.</summary>
public static class OrderType
{
    public const string Unknown = "UNKNOWN_ORDER_TYPE";
    public const string Market = "MARKET";
    public const string Limit = "LIMIT";
    public const string Stop = "STOP";
    public const string StopLimit = "STOP_LIMIT";
    public const string Bracket = "BRACKET";
    public const string Twap = "TWAP";
    public const string RollOpen = "ROLL_OPEN";
    public const string RollClose = "ROLL_CLOSE";
    public const string Liquidation = "LIQUIDATION";
    public const string Scaled = "SCALED";
}
