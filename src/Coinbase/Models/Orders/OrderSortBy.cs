namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>sort_by</c> values for the List Orders endpoint.</summary>
public static class OrderSortBy
{
    public const string Unknown = "UNKNOWN_SORT_BY";
    public const string LimitPrice = "LIMIT_PRICE";
    public const string LastFillTime = "LAST_FILL_TIME";
    public const string LastUpdateTime = "LAST_UPDATE_TIME";
}
