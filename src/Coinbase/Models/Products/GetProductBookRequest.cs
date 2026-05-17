namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Parameters for <c>GET /api/v3/brokerage/product_book</c>.</summary>
public sealed class GetProductBookRequest
{
    /// <summary>Trading pair (e.g. 'BTC-USD'). Required.</summary>
    public required string ProductId { get; set; }

    /// <summary>Number of bid/ask levels to return.</summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Minimum price interval at which orders are grouped.
    /// When set, adjacent levels within this increment are aggregated into one.
    /// </summary>
    public string? AggregationPriceIncrement { get; set; }
}
