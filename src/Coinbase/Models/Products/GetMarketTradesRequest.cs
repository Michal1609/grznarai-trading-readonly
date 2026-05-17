namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Parameters for <c>GET /api/v3/brokerage/products/{product_id}/ticker</c>.</summary>
public sealed class GetMarketTradesRequest
{
    /// <summary>Trading pair (e.g. 'BTC-USD'). Required.</summary>
    public required string ProductId { get; set; }

    /// <summary>Number of trades to return. Required.</summary>
    public required int Limit { get; set; }

    /// <summary>UNIX timestamp (seconds) for the start of the time range. Optional.</summary>
    public string? Start { get; set; }

    /// <summary>UNIX timestamp (seconds) for the end of the time range. Optional.</summary>
    public string? End { get; set; }
}
