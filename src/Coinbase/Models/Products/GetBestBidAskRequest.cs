namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Optional parameters for <c>GET /api/v3/brokerage/best_bid_ask</c>.</summary>
public sealed class GetBestBidAskRequest
{
    /// <summary>Filter to specific trading pairs (e.g. "BTC-USD"). Omit for all products.</summary>
    public List<string>? ProductIds { get; set; }
}
