namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Request parameters for <c>GET /api/v3/brokerage/orders/historical/batch</c>.</summary>
public sealed class ListOrdersRequest
{
    /// <summary>Filter by specific order IDs.</summary>
    public IReadOnlyList<string>? OrderIds { get; init; }

    /// <summary>Filter by product IDs (e.g. <c>BTC-USD</c>). Defaults to all products.</summary>
    public IReadOnlyList<string>? ProductIds { get; init; }

    /// <summary>
    /// Filter by product type. Use constants from
    /// <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductType"/>.
    /// </summary>
    public string? ProductType { get; init; }

    /// <summary>
    /// Filter by execution statuses. Use constants from <see cref="OrderStatus"/>.
    /// </summary>
    public IReadOnlyList<string>? OrderStatus { get; init; }

    /// <summary>
    /// Filter by time-in-force values. Use constants from <see cref="TimeInForce"/>.
    /// </summary>
    public IReadOnlyList<string>? TimeInForces { get; init; }

    /// <summary>
    /// Filter by order types. Use constants from <see cref="OrderType"/>.
    /// </summary>
    public IReadOnlyList<string>? OrderTypes { get; init; }

    /// <summary>
    /// Filter by order side. Use constants from <see cref="OrderSide"/>.
    /// </summary>
    public string? OrderSide { get; init; }

    /// <summary>Inclusive start date/time (RFC3339) for order creation.</summary>
    public DateTimeOffset? StartDate { get; init; }

    /// <summary>Exclusive end date/time (RFC3339) for order creation.</summary>
    public DateTimeOffset? EndDate { get; init; }

    /// <summary>
    /// Filter by placement source. Use constants from <see cref="OrderPlacementSource"/>.
    /// </summary>
    public string? OrderPlacementSource { get; init; }

    /// <summary>
    /// For futures orders only. Use constants from
    /// <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ContractExpiryType"/>.
    /// </summary>
    public string? ContractExpiryType { get; init; }

    /// <summary>Filter by base, quote, or underlying asset (e.g. <c>BTC</c>).</summary>
    public IReadOnlyList<string>? AssetFilters { get; init; }

    /// <summary>Deprecated by Coinbase. Retail portfolio ID filter.</summary>
    public string? RetailPortfolioId { get; init; }

    /// <summary>Number of orders per page.</summary>
    public int? Limit { get; init; }

    /// <summary>Pagination cursor from a previous response.</summary>
    public string? Cursor { get; init; }

    /// <summary>
    /// Sort field. Use constants from <see cref="OrderSortBy"/>.
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>Deprecated by Coinbase. Native currency for order values. Defaults to USD.</summary>
    public string? UserNativeCurrency { get; init; }

    /// <summary>Use simplified total value calculation. Defaults to <c>true</c>.</summary>
    public bool? UseSimplifiedTotalValueCalculation { get; init; }

    /// <summary>2FA proof token for EU SCA compliance.</summary>
    public string? ProofToken { get; init; }
}
