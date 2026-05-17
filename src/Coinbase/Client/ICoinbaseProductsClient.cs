using GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Read-only client for Coinbase Advanced Trade Products endpoints.
/// </summary>
public interface ICoinbaseProductsClient
{
    /// <summary>
    /// Get best bid and ask prices for one or more products.
    /// </summary>
    /// <param name="productIds">Trading pairs to query. Omit for all products.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetBestBidAskResponse> GetBestBidAskAsync(
        IEnumerable<string>? productIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get best bid and ask prices using a request object.
    /// </summary>
    Task<GetBestBidAskResponse> GetBestBidAskAsync(GetBestBidAskRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get recent trades for a product (market ticker).
    /// </summary>
    /// <param name="productId">Trading pair (e.g. 'BTC-USD'). Required.</param>
    /// <param name="limit">Number of trades to return. Required.</param>
    /// <param name="start">UNIX timestamp (seconds) for the start of the range.</param>
    /// <param name="end">UNIX timestamp (seconds) for the end of the range.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetMarketTradesResponse> GetMarketTradesAsync(
        string productId,
        int limit,
        string? start = null,
        string? end = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get recent trades for a product using a request object.
    /// </summary>
    Task<GetMarketTradesResponse> GetMarketTradesAsync(GetMarketTradesRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get a single product by ID.
    /// </summary>
    /// <param name="productId">Trading pair (e.g. 'BTC-USD'). Required.</param>
    /// <param name="getTradabilityStatus">Populate <c>view_only</c> with tradability status (SPOT only).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Product> GetProductAsync(
        string productId,
        bool? getTradabilityStatus = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get a single product using a request object.
    /// </summary>
    Task<Product> GetProductAsync(GetProductRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get the order book (level 2 depth) for a product.
    /// </summary>
    /// <param name="productId">Trading pair (e.g. 'BTC-USD'). Required.</param>
    /// <param name="limit">Number of bid/ask levels to return.</param>
    /// <param name="aggregationPriceIncrement">Minimum price interval for grouping levels.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetProductBookResponse> GetProductBookAsync(
        string productId,
        int? limit = null,
        string? aggregationPriceIncrement = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get the order book using a request object.
    /// </summary>
    Task<GetProductBookResponse> GetProductBookAsync(GetProductBookRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get historical OHLCV candles for a product.
    /// </summary>
    /// <param name="productId">Trading pair (e.g. 'BTC-USD'). Required.</param>
    /// <param name="start">UNIX timestamp (seconds) for the start of the range. Required.</param>
    /// <param name="end">UNIX timestamp (seconds) for the end of the range. Required.</param>
    /// <param name="granularity">Candle timeframe — see <see cref="Granularity"/>. Required.</param>
    /// <param name="limit">Max candles to return (default and max: 350).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetProductCandlesResponse> GetProductCandlesAsync(
        string productId,
        string start,
        string end,
        string granularity,
        int? limit = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get historical OHLCV candles using a request object.
    /// </summary>
    Task<GetProductCandlesResponse> GetProductCandlesAsync(GetProductCandlesRequest request, CancellationToken ct = default);

    /// <summary>
    /// List all available products with optional filtering and pagination.
    /// </summary>
    /// <param name="limit">Max products per page.</param>
    /// <param name="offset">Number of products to skip.</param>
    /// <param name="productType">Filter by product type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductType"/>.</param>
    /// <param name="productIds">Filter to specific trading pairs.</param>
    /// <param name="contractExpiryType">For futures: filter by expiry type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ContractExpiryType"/>.</param>
    /// <param name="expiringContractStatus">For futures: filter by expiry status — see <see cref="ExpiringContractStatus"/>.</param>
    /// <param name="getTradabilityStatus">Populate <c>view_only</c> with tradability status.</param>
    /// <param name="getAllProducts">Include expired futures contracts.</param>
    /// <param name="productsSortOrder">Sort order — see <see cref="ProductsSortOrder"/>.</param>
    /// <param name="cursor">Base64-encoded pagination cursor.</param>
    /// <param name="futuresUnderlyingType">Filter futures by underlying asset type.</param>
    /// <param name="userCountryCode">ISO 3166-1 alpha-2 country code.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ListProductsResponse> ListProductsAsync(
        int? limit = null,
        int? offset = null,
        string? productType = null,
        IEnumerable<string>? productIds = null,
        string? contractExpiryType = null,
        string? expiringContractStatus = null,
        bool? getTradabilityStatus = null,
        bool? getAllProducts = null,
        string? productsSortOrder = null,
        string? cursor = null,
        string? futuresUnderlyingType = null,
        string? userCountryCode = null,
        CancellationToken ct = default);

    /// <summary>
    /// List products using a request object.
    /// </summary>
    Task<ListProductsResponse> ListProductsAsync(ListProductsRequest request, CancellationToken ct = default);
}
