namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>displayed_order_config</c> values returned by the Coinbase API.</summary>
public static class DisplayedOrderConfig
{
    public const string Unknown = "UNKNOWN_DISPLAYED_ORDER_CONFIG";
    public const string InstantGfd = "INSTANT_GFD";
    public const string LimitGfd = "LIMIT_GFD";
    public const string LimitGtc = "LIMIT_GTC";
}
