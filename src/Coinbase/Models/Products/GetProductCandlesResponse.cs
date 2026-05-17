using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Response from <c>GET /api/v3/brokerage/products/{product_id}/candles</c>.</summary>
public sealed class GetProductCandlesResponse
{
    [JsonPropertyName("candles")] public List<Candle>? Candles { get; set; }
}
