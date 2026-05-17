namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;

/// <summary>Request for <c>GET /api/v3/brokerage/market/products/{product_id}/ticker</c>.</summary>
public sealed class GetPublicMarketTradesRequest
{
    /// <summary>Trading pair (e.g. 'BTC-USD'). Required.</summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>Number of trades to return. Required.</summary>
    public int Limit { get; set; }

    /// <summary>UNIX timestamp (seconds) for the start of the range.</summary>
    public string? Start { get; set; }

    /// <summary>UNIX timestamp (seconds) for the end of the range.</summary>
    public string? End { get; set; }
}
