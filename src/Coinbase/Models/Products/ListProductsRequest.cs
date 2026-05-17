namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Optional filter parameters for <c>GET /api/v3/brokerage/products</c>.</summary>
public sealed class ListProductsRequest
{
    /// <summary>Max number of products to return.</summary>
    public int? Limit { get; set; }

    /// <summary>Number of products to skip before returning results.</summary>
    public int? Offset { get; set; }

    /// <summary>
    /// Filter by product type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductType"/>.
    /// </summary>
    public string? ProductType { get; set; }

    /// <summary>Filter to specific trading pairs (e.g. "BTC-USD").</summary>
    public List<string>? ProductIds { get; set; }

    /// <summary>
    /// For futures: filter by expiry type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ContractExpiryType"/>.
    /// </summary>
    public string? ContractExpiryType { get; set; }

    /// <summary>
    /// For futures: filter by expiry status — see <see cref="ExpiringContractStatus"/>.
    /// </summary>
    public string? ExpiringContractStatus { get; set; }

    /// <summary>When <c>true</c>, populates <c>view_only</c> with tradability status.</summary>
    public bool? GetTradabilityStatus { get; set; }

    /// <summary>When <c>true</c>, returns all products including expired futures.</summary>
    public bool? GetAllProducts { get; set; }

    /// <summary>Sort order — see <see cref="ProductsSortOrder"/>.</summary>
    public string? ProductsSortOrder { get; set; }

    /// <summary>Base64-encoded cursor for pagination.</summary>
    public string? Cursor { get; set; }

    /// <summary>Filter futures by underlying asset type (e.g. "SPOT", "INDEX").</summary>
    public string? FuturesUnderlyingType { get; set; }

    /// <summary>ISO 3166-1 alpha-2 country code of the user.</summary>
    public string? UserCountryCode { get; set; }
}
