using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Response from <c>GET /api/v3/brokerage/products</c>.</summary>
public sealed class ListProductsResponse
{
    [JsonPropertyName("products")] public List<Product>? Products { get; set; }

    /// <summary>Number of products returned in this page.</summary>
    [JsonPropertyName("num_products")] public int? NumProducts { get; set; }

    [JsonPropertyName("pagination")] public PaginationMetadata? Pagination { get; set; }
}
