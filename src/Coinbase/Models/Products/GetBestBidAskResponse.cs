using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Response from <c>GET /api/v3/brokerage/best_bid_ask</c>.</summary>
public sealed class GetBestBidAskResponse
{
    [JsonPropertyName("pricebooks")] public List<PriceBook>? Pricebooks { get; set; }
}
