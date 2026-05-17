using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Response from <c>GET /api/v3/brokerage/product_book</c>.</summary>
public sealed class GetProductBookResponse
{
    [JsonPropertyName("pricebook")] public PriceBook? Pricebook { get; set; }

    /// <summary>Price of the latest trade.</summary>
    [JsonPropertyName("last")] public string? Last { get; set; }

    /// <summary>Mid-market price (midpoint of best bid and best ask).</summary>
    [JsonPropertyName("mid_market")] public string? MidMarket { get; set; }

    /// <summary>Bid-ask spread expressed in basis points.</summary>
    [JsonPropertyName("spread_bps")] public string? SpreadBps { get; set; }

    /// <summary>Bid-ask spread expressed as an absolute price difference.</summary>
    [JsonPropertyName("spread_absolute")] public string? SpreadAbsolute { get; set; }
}
