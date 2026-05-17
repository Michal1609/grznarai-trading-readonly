namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;

/// <summary>Request for <c>GET /api/v3/brokerage/market/products/{product_id}/candles</c>.</summary>
public sealed class GetPublicProductCandlesRequest
{
    /// <summary>Trading pair (e.g. 'BTC-USD'). Required.</summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>UNIX timestamp (seconds) for the start of the range. Required.</summary>
    public string Start { get; set; } = string.Empty;

    /// <summary>UNIX timestamp (seconds) for the end of the range. Required.</summary>
    public string End { get; set; } = string.Empty;

    /// <summary>Candle timeframe — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Products.Granularity"/>. Required.</summary>
    public string Granularity { get; set; } = string.Empty;

    /// <summary>Max candles to return (default and max: 350).</summary>
    public int? Limit { get; set; }
}
