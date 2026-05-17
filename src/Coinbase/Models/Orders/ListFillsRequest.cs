namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Request parameters for <c>GET /api/v3/brokerage/orders/historical/fills</c>.</summary>
public sealed class ListFillsRequest
{
    /// <summary>Filter by specific order IDs.</summary>
    public IReadOnlyList<string>? OrderIds { get; init; }

    /// <summary>Filter by specific trade/fill IDs.</summary>
    public IReadOnlyList<string>? TradeIds { get; init; }

    /// <summary>Filter by product IDs (e.g. <c>BTC-USD</c>).</summary>
    public IReadOnlyList<string>? ProductIds { get; init; }

    /// <summary>Return only fills with trade time after this timestamp (RFC3339).</summary>
    public DateTimeOffset? StartSequenceTimestamp { get; init; }

    /// <summary>Return only fills with trade time before this timestamp (RFC3339).</summary>
    public DateTimeOffset? EndSequenceTimestamp { get; init; }

    /// <summary>Deprecated by Coinbase. Portfolio ID filter.</summary>
    public string? RetailPortfolioId { get; init; }

    /// <summary>Number of fills returned per page. Default 100.</summary>
    public int? Limit { get; init; }

    /// <summary>Pagination cursor from a previous response.</summary>
    public string? Cursor { get; init; }

    /// <summary>Sort field. Use constants from <see cref="FillSortBy"/>.</summary>
    public string? SortBy { get; init; }

    /// <summary>Filter by asset (e.g. <c>BTC</c>).</summary>
    public IReadOnlyList<string>? AssetFilters { get; init; }

    /// <summary>
    /// Filter by order types. Use constants from <see cref="OrderType"/>.
    /// </summary>
    public IReadOnlyList<string>? OrderTypes { get; init; }

    /// <summary>Filter by order side. Use constants from <see cref="OrderSide"/>.</summary>
    public string? OrderSide { get; init; }

    /// <summary>
    /// Filter by product types. Use constants from
    /// <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductType"/>.
    /// </summary>
    public IReadOnlyList<string>? ProductTypes { get; init; }

    /// <summary>2FA proof token for EU SCA compliance.</summary>
    public string? ProofToken { get; init; }
}
