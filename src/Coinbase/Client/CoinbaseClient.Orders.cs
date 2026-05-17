using System.Globalization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    /// <inheritdoc cref="ICoinbaseOrdersClient.GetOrderAsync(string,CancellationToken)"/>
    public async Task<GetOrderResponse> GetOrderAsync(string orderId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);

        return await GetFromJsonAsync<GetOrderResponse>(
            $"/api/v3/brokerage/orders/historical/{QueryStringBuilder.EscapePathSegment(orderId)}",
            "Empty get-order response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseOrdersClient.GetOrderAsync(GetOrderRequest,CancellationToken)"/>
    public async Task<GetOrderResponse> GetOrderAsync(GetOrderRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OrderId);

        var qs = new QueryStringBuilder()
            .Add("client_order_id", request.ClientOrderId)
            .Add("user_native_currency", request.UserNativeCurrency);

        return await GetFromJsonAsync<GetOrderResponse>(
            $"/api/v3/brokerage/orders/historical/{QueryStringBuilder.EscapePathSegment(request.OrderId)}{qs}",
            "Empty get-order response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseOrdersClient.ListOrdersAsync(CancellationToken)"/>
    public Task<ListOrdersResponse> ListOrdersAsync(CancellationToken ct = default)
        => GetFromJsonAsync<ListOrdersResponse>(
            "/api/v3/brokerage/orders/historical/batch",
            "Empty list-orders response.",
            ct);

    /// <inheritdoc cref="ICoinbaseOrdersClient.ListOrdersAsync(ListOrdersRequest,CancellationToken)"/>
    public async Task<ListOrdersResponse> ListOrdersAsync(ListOrdersRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Limit.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.Limit.Value);

        var qs = new QueryStringBuilder()
            .AddCsv("order_ids", request.OrderIds)
            .AddCsv("product_ids", request.ProductIds)
            .Add("product_type", request.ProductType)
            .AddCsv("order_status", request.OrderStatus)
            .AddCsv("time_in_forces", request.TimeInForces)
            .AddCsv("order_types", request.OrderTypes)
            .Add("order_side", request.OrderSide)
            .Add("start_date", request.StartDate?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture))
            .Add("end_date", request.EndDate?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture))
            .Add("order_placement_source", request.OrderPlacementSource)
            .Add("contract_expiry_type", request.ContractExpiryType)
            .AddCsv("asset_filters", request.AssetFilters)
            .Add("retail_portfolio_id", request.RetailPortfolioId)
            .AddIfHasValue("limit", request.Limit)
            .Add("cursor", request.Cursor)
            .Add("sort_by", request.SortBy)
            .Add("user_native_currency", request.UserNativeCurrency)
            .AddIfHasValue("use_simplified_total_value_calculation", request.UseSimplifiedTotalValueCalculation)
            .Add("proof_token", request.ProofToken);

        return await GetFromJsonAsync<ListOrdersResponse>(
            $"/api/v3/brokerage/orders/historical/batch{qs}",
            "Empty list-orders response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseOrdersClient.ListFillsAsync(CancellationToken)"/>
    public Task<ListFillsResponse> ListFillsAsync(CancellationToken ct = default)
        => GetFromJsonAsync<ListFillsResponse>(
            "/api/v3/brokerage/orders/historical/fills",
            "Empty list-fills response.",
            ct);

    /// <inheritdoc cref="ICoinbaseOrdersClient.ListFillsAsync(ListFillsRequest,CancellationToken)"/>
    public async Task<ListFillsResponse> ListFillsAsync(ListFillsRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Limit.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.Limit.Value);

        var qs = new QueryStringBuilder()
            .AddCsv("order_ids", request.OrderIds)
            .AddCsv("trade_ids", request.TradeIds)
            .AddCsv("product_ids", request.ProductIds)
            .Add("start_sequence_timestamp", request.StartSequenceTimestamp?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture))
            .Add("end_sequence_timestamp", request.EndSequenceTimestamp?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture))
            .Add("retail_portfolio_id", request.RetailPortfolioId)
            .AddIfHasValue("limit", request.Limit)
            .Add("cursor", request.Cursor)
            .Add("sort_by", request.SortBy)
            .AddCsv("asset_filters", request.AssetFilters)
            .AddCsv("order_types", request.OrderTypes)
            .Add("order_side", request.OrderSide)
            .AddCsv("product_types", request.ProductTypes)
            .Add("proof_token", request.ProofToken);

        return await GetFromJsonAsync<ListFillsResponse>(
            $"/api/v3/brokerage/orders/historical/fills{qs}",
            "Empty list-fills response.",
            ct).ConfigureAwait(false);
    }
}
