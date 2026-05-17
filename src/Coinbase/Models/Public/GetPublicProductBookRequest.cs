namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;

/// <summary>Request for <c>GET /api/v3/brokerage/market/product_book</c>.</summary>
public sealed class GetPublicProductBookRequest
{
    /// <summary>Trading pair (e.g. 'BTC-USD'). Required.</summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>Number of bid/ask levels to return.</summary>
    public int? Limit { get; set; }

    /// <summary>Minimum price interval for grouping order book levels.</summary>
    public string? AggregationPriceIncrement { get; set; }
}
