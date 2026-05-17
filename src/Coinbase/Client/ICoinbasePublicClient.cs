using GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Read-only client for Coinbase Advanced Trade Public endpoints (no authentication required).
/// </summary>
public interface ICoinbasePublicClient
{
    /// <summary>
    /// Get recent public market trades for a product.
    /// </summary>
    /// <param name="productId">Trading pair (e.g. 'BTC-USD'). Required.</param>
    /// <param name="limit">Number of trades to return. Required.</param>
    /// <param name="start">UNIX timestamp (seconds) for the start of the range.</param>
    /// <param name="end">UNIX timestamp (seconds) for the end of the range.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetMarketTradesResponse> GetPublicMarketTradesAsync(
        string productId,
        int limit,
        string? start = null,
        string? end = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get recent public market trades using a request object.
    /// </summary>
    Task<GetMarketTradesResponse> GetPublicMarketTradesAsync(GetPublicMarketTradesRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get a single public product by ID.
    /// </summary>
    /// <param name="productId">Trading pair (e.g. 'BTC-USD'). Required.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Product> GetPublicProductAsync(string productId, CancellationToken ct = default);

    /// <summary>
    /// Get a single public product using a request object.
    /// </summary>
    Task<Product> GetPublicProductAsync(GetPublicProductRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get the public order book (level 2 depth) for a product.
    /// </summary>
    /// <param name="productId">Trading pair (e.g. 'BTC-USD'). Required.</param>
    /// <param name="limit">Number of bid/ask levels to return.</param>
    /// <param name="aggregationPriceIncrement">Minimum price interval for grouping levels.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetProductBookResponse> GetPublicProductBookAsync(
        string productId,
        int? limit = null,
        string? aggregationPriceIncrement = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get the public order book using a request object.
    /// </summary>
    Task<GetProductBookResponse> GetPublicProductBookAsync(GetPublicProductBookRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get public historical OHLCV candles for a product.
    /// </summary>
    /// <param name="productId">Trading pair (e.g. 'BTC-USD'). Required.</param>
    /// <param name="start">UNIX timestamp (seconds) for the start of the range. Required.</param>
    /// <param name="end">UNIX timestamp (seconds) for the end of the range. Required.</param>
    /// <param name="granularity">Candle timeframe — see <see cref="Granularity"/>. Required.</param>
    /// <param name="limit">Max candles to return (default and max: 350).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GetProductCandlesResponse> GetPublicProductCandlesAsync(
        string productId,
        string start,
        string end,
        string granularity,
        int? limit = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get public historical OHLCV candles using a request object.
    /// </summary>
    Task<GetProductCandlesResponse> GetPublicProductCandlesAsync(GetPublicProductCandlesRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get the current Coinbase Advanced Trade API server time.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<GetServerTimeResponse> GetServerTimeAsync(CancellationToken ct = default);

    /// <summary>
    /// List all available public products with optional filtering and pagination.
    /// </summary>
    /// <param name="limit">Max products per page.</param>
    /// <param name="offset">Number of products to skip.</param>
    /// <param name="productType">Filter by product type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductType"/>.</param>
    /// <param name="productIds">Filter to specific trading pairs.</param>
    /// <param name="contractExpiryType">For futures: filter by expiry type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ContractExpiryType"/>.</param>
    /// <param name="expiringContractStatus">For futures: filter by expiry status — see <see cref="ExpiringContractStatus"/>.</param>
    /// <param name="getAllProducts">Include expired futures contracts.</param>
    /// <param name="productsSortOrder">Sort order — see <see cref="ProductsSortOrder"/>.</param>
    /// <param name="cursor">Base64-encoded pagination cursor.</param>
    /// <param name="futuresUnderlyingType">Filter futures by underlying asset type.</param>
    /// <param name="userCountryCode">ISO 3166-1 alpha-2 country code.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ListProductsResponse> ListPublicProductsAsync(
        int? limit = null,
        int? offset = null,
        string? productType = null,
        IEnumerable<string>? productIds = null,
        string? contractExpiryType = null,
        string? expiringContractStatus = null,
        bool? getAllProducts = null,
        string? productsSortOrder = null,
        string? cursor = null,
        string? futuresUnderlyingType = null,
        string? userCountryCode = null,
        CancellationToken ct = default);

    /// <summary>
    /// List public products using a request object.
    /// </summary>
    Task<ListProductsResponse> ListPublicProductsAsync(ListPublicProductsRequest request, CancellationToken ct = default);
}
