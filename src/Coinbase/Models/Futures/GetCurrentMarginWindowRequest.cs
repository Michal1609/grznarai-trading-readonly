namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Optional parameters for <c>GET /api/v3/brokerage/cfm/intraday/current_margin_window</c>.
/// </summary>
public sealed record GetCurrentMarginWindowRequest
{
    /// <summary>
    /// Filter to a specific margin profile type.
    /// Use constants from <see cref="MarginProfileType"/>.
    /// </summary>
    public string? MarginProfileType { get; init; }
}
