namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Known <c>margin_profile_type</c> values for the Get Current Margin Window endpoint.
/// </summary>
public static class MarginProfileType
{
    public const string Unspecified = "MARGIN_PROFILE_TYPE_UNSPECIFIED";
    public const string RetailRegular = "MARGIN_PROFILE_TYPE_RETAIL_REGULAR";
    public const string RetailIntradayMargin1 = "MARGIN_PROFILE_TYPE_RETAIL_INTRADAY_MARGIN_1";
}
