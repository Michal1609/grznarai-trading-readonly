namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;

/// <summary>Request for <c>GET /api/v3/brokerage/market/products/{product_id}</c>.</summary>
public sealed class GetPublicProductRequest
{
    /// <summary>Trading pair (e.g. 'BTC-USD'). Required.</summary>
    public string ProductId { get; set; } = string.Empty;
}
