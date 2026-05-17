using GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    // ─── GetBestBidAskAsync ──────────────────────────────────────────────────

    /// <inheritdoc cref="ICoinbaseProductsClient.GetBestBidAskAsync(IEnumerable{string}?,CancellationToken)"/>
    public async Task<GetBestBidAskResponse> GetBestBidAskAsync(
        IEnumerable<string>? productIds = null,
        CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .AddRepeated("product_ids", productIds);

        return await GetFromJsonAsync<GetBestBidAskResponse>(
            $"/api/v3/brokerage/best_bid_ask{qs}",
            "Empty best-bid-ask response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseProductsClient.GetBestBidAskAsync(GetBestBidAskRequest,CancellationToken)"/>
    public Task<GetBestBidAskResponse> GetBestBidAskAsync(GetBestBidAskRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetBestBidAskAsync(request.ProductIds, ct);
    }

    // ─── GetMarketTradesAsync ────────────────────────────────────────────────

    /// <inheritdoc cref="ICoinbaseProductsClient.GetMarketTradesAsync(string,int,string?,string?,CancellationToken)"/>
    public async Task<GetMarketTradesResponse> GetMarketTradesAsync(
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
            $"/api/v3/brokerage/products/{QueryStringBuilder.EscapePathSegment(productId)}/ticker{qs}",
            "Empty market-trades response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseProductsClient.GetMarketTradesAsync(GetMarketTradesRequest,CancellationToken)"/>
    public Task<GetMarketTradesResponse> GetMarketTradesAsync(GetMarketTradesRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetMarketTradesAsync(request.ProductId, request.Limit, request.Start, request.End, ct);
    }

    // ─── GetProductAsync ─────────────────────────────────────────────────────

    /// <inheritdoc cref="ICoinbaseProductsClient.GetProductAsync(string,bool?,CancellationToken)"/>
    public async Task<Product> GetProductAsync(
        string productId,
        bool? getTradabilityStatus = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);

        var qs = new QueryStringBuilder()
            .AddIfHasValue("get_tradability_status", getTradabilityStatus);

        return await GetFromJsonAsync<Product>(
            $"/api/v3/brokerage/products/{QueryStringBuilder.EscapePathSegment(productId)}{qs}",
            "Empty get-product response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseProductsClient.GetProductAsync(GetProductRequest,CancellationToken)"/>
    public Task<Product> GetProductAsync(GetProductRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetProductAsync(request.ProductId, request.GetTradabilityStatus, ct);
    }

    // ─── GetProductBookAsync ─────────────────────────────────────────────────

    /// <inheritdoc cref="ICoinbaseProductsClient.GetProductBookAsync(string,int?,string?,CancellationToken)"/>
    public async Task<GetProductBookResponse> GetProductBookAsync(
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
            $"/api/v3/brokerage/product_book{qs}",
            "Empty product-book response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseProductsClient.GetProductBookAsync(GetProductBookRequest,CancellationToken)"/>
    public Task<GetProductBookResponse> GetProductBookAsync(GetProductBookRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetProductBookAsync(request.ProductId, request.Limit, request.AggregationPriceIncrement, ct);
    }

    // ─── GetProductCandlesAsync ──────────────────────────────────────────────

    /// <inheritdoc cref="ICoinbaseProductsClient.GetProductCandlesAsync(string,string,string,string,int?,CancellationToken)"/>
    public async Task<GetProductCandlesResponse> GetProductCandlesAsync(
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
            $"/api/v3/brokerage/products/{QueryStringBuilder.EscapePathSegment(productId)}/candles{qs}",
            "Empty product-candles response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseProductsClient.GetProductCandlesAsync(GetProductCandlesRequest,CancellationToken)"/>
    public Task<GetProductCandlesResponse> GetProductCandlesAsync(GetProductCandlesRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetProductCandlesAsync(request.ProductId, request.Start, request.End, request.Granularity, request.Limit, ct);
    }

    // ─── ListProductsAsync ───────────────────────────────────────────────────

    /// <inheritdoc cref="ICoinbaseProductsClient.ListProductsAsync(int?,int?,string?,IEnumerable{string}?,string?,string?,bool?,bool?,string?,string?,string?,string?,CancellationToken)"/>
    public async Task<ListProductsResponse> ListProductsAsync(
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
        CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .AddIfHasValue("limit", limit)
            .AddIfHasValue("offset", offset)
            .Add("product_type", productType)
            .AddRepeated("product_ids", productIds)
            .Add("contract_expiry_type", contractExpiryType)
            .Add("expiring_contract_status", expiringContractStatus)
            .AddIfHasValue("get_tradability_status", getTradabilityStatus)
            .AddIfHasValue("get_all_products", getAllProducts)
            .Add("products_sort_order", productsSortOrder)
            .Add("cursor", cursor)
            .Add("futures_underlying_type", futuresUnderlyingType)
            .Add("user_country_code", userCountryCode);

        return await GetFromJsonAsync<ListProductsResponse>(
            $"/api/v3/brokerage/products{qs}",
            "Empty list-products response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseProductsClient.ListProductsAsync(ListProductsRequest,CancellationToken)"/>
    public Task<ListProductsResponse> ListProductsAsync(ListProductsRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ListProductsAsync(
            request.Limit,
            request.Offset,
            request.ProductType,
            request.ProductIds,
            request.ContractExpiryType,
            request.ExpiringContractStatus,
            request.GetTradabilityStatus,
            request.GetAllProducts,
            request.ProductsSortOrder,
            request.Cursor,
            request.FuturesUnderlyingType,
            request.UserCountryCode,
            ct);
    }
}
