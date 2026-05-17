using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Additional product-specific details attached to an order.</summary>
public sealed class ProductDetails
{
    [JsonPropertyName("equity_details")] public EquityDetails? EquityDetails { get; set; }
}

/// <summary>Equity-specific details for an order on an equity product.</summary>
public sealed class EquityDetails
{
    [JsonPropertyName("base_cbrn")] public string? BaseCbrn { get; set; }
    [JsonPropertyName("ticker")] public string? Ticker { get; set; }
    [JsonPropertyName("quote_id")] public string? QuoteId { get; set; }
}
