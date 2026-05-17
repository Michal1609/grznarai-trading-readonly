using GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    // ─── GetPublicMarketTradesAsync ──────────────────────────────────────────

    /// <inheritdoc cref="ICoinbasePublicClient.GetPublicMarketTradesAsync(string,int,string?,string?,CancellationToken)"/>
    public async Task<GetMarketTradesResponse> GetPublicMarketTradesAsync(
        string productId,
        int limit,
        string? start = null,
        string? end = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var qs = new QueryStringBuilder()
            .Add("limit", limit)
            .Add("start", start)
            .Add("end", end);

        return await GetFromJsonAsync<GetMarketTradesResponse>(
            $"/api/v3/brokerage/market/products/{QueryStringBuilder.EscapePathSegment(productId)}/ticker{qs}",
            "Empty public market-trades response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbasePublicClient.GetPublicMarketTradesAsync(GetPublicMarketTradesRequest,CancellationToken)"/>
    public Task<GetMarketTradesResponse> GetPublicMarketTradesAsync(GetPublicMarketTradesRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetPublicMarketTradesAsync(request.ProductId, request.Limit, request.Start, request.End, ct);
    }

    // ─── GetPublicProductAsync ───────────────────────────────────────────────

    /// <inheritdoc cref="ICoinbasePublicClient.GetPublicProductAsync(string,CancellationToken)"/>
    public async Task<Product> GetPublicProductAsync(string productId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);

        return await GetFromJsonAsync<Product>(
            $"/api/v3/brokerage/market/products/{QueryStringBuilder.EscapePathSegment(productId)}",
            "Empty public get-product response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbasePublicClient.GetPublicProductAsync(GetPublicProductRequest,CancellationToken)"/>
    public Task<Product> GetPublicProductAsync(GetPublicProductRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetPublicProductAsync(request.ProductId, ct);
    }

    // ─── GetPublicProductBookAsync ───────────────────────────────────────────

    /// <inheritdoc cref="ICoinbasePublicClient.GetPublicProductBookAsync(string,int?,string?,CancellationToken)"/>
    public async Task<GetProductBookResponse> GetPublicProductBookAsync(
        string productId,
        int? limit = null,
        string? aggregationPriceIncrement = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);

        var qs = new QueryStringBuilder()
            .Add("product_id", productId)
            .AddIfHasValue("limit", limit)
            .Add("aggregation_price_increment", aggregationPriceIncrement);

        return await GetFromJsonAsync<GetProductBookResponse>(
            $"/api/v3/brokerage/market/product_book{qs}",
            "Empty public product-book response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbasePublicClient.GetPublicProductBookAsync(GetPublicProductBookRequest,CancellationToken)"/>
    public Task<GetProductBookResponse> GetPublicProductBookAsync(GetPublicProductBookRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetPublicProductBookAsync(request.ProductId, request.Limit, request.AggregationPriceIncrement, ct);
    }

    // ─── GetPublicProductCandlesAsync ────────────────────────────────────────

    /// <inheritdoc cref="ICoinbasePublicClient.GetPublicProductCandlesAsync(string,string,string,string,int?,CancellationToken)"/>
    public async Task<GetProductCandlesResponse> GetPublicProductCandlesAsync(
        string productId,
        string start,
        string end,
        string granularity,
        int? limit = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        ArgumentException.ThrowIfNullOrWhiteSpace(start);
        ArgumentException.ThrowIfNullOrWhiteSpace(end);
        ArgumentException.ThrowIfNullOrWhiteSpace(granularity);

        var qs = new QueryStringBuilder()
            .Add("start", start)
            .Add("end", end)
            .Add("granularity", granularity)
            .AddIfHasValue("limit", limit);

        return await GetFromJsonAsync<GetProductCandlesResponse>(
            $"/api/v3/brokerage/market/products/{QueryStringBuilder.EscapePathSegment(productId)}/candles{qs}",
            "Empty public product-candles response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbasePublicClient.GetPublicProductCandlesAsync(GetPublicProductCandlesRequest,CancellationToken)"/>
    public Task<GetProductCandlesResponse> GetPublicProductCandlesAsync(GetPublicProductCandlesRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetPublicProductCandlesAsync(request.ProductId, request.Start, request.End, request.Granularity, request.Limit, ct);
    }

    // ─── GetServerTimeAsync ──────────────────────────────────────────────────

    /// <inheritdoc cref="ICoinbasePublicClient.GetServerTimeAsync"/>
    public async Task<GetServerTimeResponse> GetServerTimeAsync(CancellationToken ct = default)
    {
        return await GetFromJsonAsync<GetServerTimeResponse>(
            "/api/v3/brokerage/time",
            "Empty server-time response.",
            ct).ConfigureAwait(false);
    }

    // ─── ListPublicProductsAsync ─────────────────────────────────────────────

    /// <inheritdoc cref="ICoinbasePublicClient.ListPublicProductsAsync(int?,int?,string?,IEnumerable{string}?,string?,string?,bool?,string?,string?,string?,string?,CancellationToken)"/>
    public async Task<ListProductsResponse> ListPublicProductsAsync(
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
        CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .AddIfHasValue("limit", limit)
            .AddIfHasValue("offset", offset)
            .Add("product_type", productType)
            .AddRepeated("product_ids", productIds)
            .Add("contract_expiry_type", contractExpiryType)
            .Add("expiring_contract_status", expiringContractStatus)
            .AddIfHasValue("get_all_products", getAllProducts)
            .Add("products_sort_order", productsSortOrder)
            .Add("cursor", cursor)
            .Add("futures_underlying_type", futuresUnderlyingType)
            .Add("user_country_code", userCountryCode);

        return await GetFromJsonAsync<ListProductsResponse>(
            $"/api/v3/brokerage/market/products{qs}",
            "Empty list-public-products response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbasePublicClient.ListPublicProductsAsync(ListPublicProductsRequest,CancellationToken)"/>
    public Task<ListProductsResponse> ListPublicProductsAsync(ListPublicProductsRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ListPublicProductsAsync(
            request.Limit,
            request.Offset,
            request.ProductType,
            request.ProductIds,
            request.ContractExpiryType,
            request.ExpiringContractStatus,
            request.GetAllProducts,
            request.ProductsSortOrder,
            request.Cursor,
            request.FuturesUnderlyingType,
            request.UserCountryCode,
            ct);
    }
}
