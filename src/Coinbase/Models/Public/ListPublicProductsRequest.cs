namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;

/// <summary>Request for <c>GET /api/v3/brokerage/market/products</c>.</summary>
public sealed class ListPublicProductsRequest
{
    /// <summary>Max products per page.</summary>
    public int? Limit { get; set; }

    /// <summary>Number of products to skip before returning.</summary>
    public int? Offset { get; set; }

    /// <summary>Filter by product type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductType"/>.</summary>
    public string? ProductType { get; set; }

    /// <summary>Filter to specific trading pairs (e.g. 'BTC-USD').</summary>
    public IEnumerable<string>? ProductIds { get; set; }

    /// <summary>For futures: filter by expiry type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ContractExpiryType"/>.</summary>
    public string? ContractExpiryType { get; set; }

    /// <summary>For futures: filter by expiry status — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Products.ExpiringContractStatus"/>.</summary>
    public string? ExpiringContractStatus { get; set; }

    /// <summary>Include expired futures contracts.</summary>
    public bool? GetAllProducts { get; set; }

    /// <summary>Sort order — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Products.ProductsSortOrder"/>.</summary>
    public string? ProductsSortOrder { get; set; }

    /// <summary>Base64-encoded pagination cursor.</summary>
    public string? Cursor { get; set; }

    /// <summary>Filter futures by underlying asset type.</summary>
    public string? FuturesUnderlyingType { get; set; }

    /// <summary>ISO 3166-1 alpha-2 country code for localized product names.</summary>
    public string? UserCountryCode { get; set; }
}
