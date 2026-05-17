using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Response from <c>GET /api/v3/brokerage/products/{product_id}/ticker</c>.</summary>
public sealed class GetMarketTradesResponse
{
    [JsonPropertyName("trades")] public List<MarketTrade>? Trades { get; set; }

    /// <summary>Best bid for the product, in quote currency.</summary>
    [JsonPropertyName("best_bid")] public string? BestBid { get; set; }

    /// <summary>Best ask for the product, in quote currency.</summary>
    [JsonPropertyName("best_ask")] public string? BestAsk { get; set; }
}
