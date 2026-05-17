namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>order_placement_source</c> values returned by the Coinbase API.</summary>
public static class OrderPlacementSource
{
    public const string Unknown = "UNKNOWN_PLACEMENT_SOURCE";
    public const string RetailSimple = "RETAIL_SIMPLE";
    public const string RetailAdvanced = "RETAIL_ADVANCED";
}
