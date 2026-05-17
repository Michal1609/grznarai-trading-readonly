namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Parameters for <c>GET /api/v3/brokerage/products/{product_id}/candles</c>.</summary>
public sealed class GetProductCandlesRequest
{
    /// <summary>Trading pair (e.g. 'BTC-USD'). Required.</summary>
    public required string ProductId { get; set; }

    /// <summary>UNIX timestamp (seconds) for the start of the range. Required.</summary>
    public required string Start { get; set; }

    /// <summary>UNIX timestamp (seconds) for the end of the range. Required.</summary>
    public required string End { get; set; }

    /// <summary>
    /// Candle timeframe — see <see cref="Granularity"/>. Required.
    /// </summary>
    public required string Granularity { get; set; }

    /// <summary>Max candles to return (default and max: 350).</summary>
    public int? Limit { get; set; }
}
