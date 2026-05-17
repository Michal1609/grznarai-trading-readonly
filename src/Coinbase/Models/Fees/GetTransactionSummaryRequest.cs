using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;

/// <summary>
/// Optional filter parameters for <c>GET /api/v3/brokerage/transaction_summary</c>.
/// </summary>
public sealed record GetTransactionSummaryRequest
{
    /// <summary>
    /// Filter by product type.
    /// See <see cref="ProductType"/> for known values.
    /// Defaults to <see cref="ProductType.Unknown"/> (all types).
    /// </summary>
    public string? ProductType { get; init; }

    /// <summary>
    /// Filter by contract expiry type.
    /// See <see cref="ContractExpiryType"/> for known values.
    /// Only applicable when <see cref="ProductType"/> is <see cref="Common.ProductType.Future"/>.
    /// </summary>
    public string? ContractExpiryType { get; init; }

    /// <summary>
    /// Filter by product venue.
    /// See <see cref="ProductVenue"/> for known values.
    /// Defaults to <see cref="ProductVenue.Unknown"/> (all venues).
    /// </summary>
    public string? ProductVenue { get; init; }
}
