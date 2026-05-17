namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Parameters for <c>GET /api/v3/brokerage/products/{product_id}</c>.</summary>
public sealed class GetProductRequest
{
    /// <summary>Trading pair (e.g. 'BTC-USD'). Required.</summary>
    public required string ProductId { get; set; }

    /// <summary>
    /// When <c>true</c>, populates <see cref="Product.ViewOnly"/> with the tradability status.
    /// Only applicable to SPOT products.
    /// </summary>
    public bool? GetTradabilityStatus { get; set; }
}
