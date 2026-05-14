namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Calculations;

/// <summary>
/// Aggregated account metrics derived from PnL and closed-trade history endpoints.
/// </summary>
/// <remarks>
/// <para>
/// All values are estimates derived from eToro's PnL and trade-history API endpoints.
/// They are <b>not</b> authoritative account statements. Withdrawals, deposits, bonuses,
/// rebates, dividends, corporate actions, and currency-conversion fees may not be
/// fully reflected.
/// </para>
/// <para>
/// In particular, <see cref="EstimatedNetDeposits"/>, <see cref="TotalReturn"/>, and
/// <see cref="TotalReturnPct"/> are heuristic approximations, not real deposit-ledger values.
/// </para>
/// </remarks>
public record AccountMetrics(
    decimal AvailableCash,
    decimal InvestedPrincipal,
    decimal UnrealizedPnL,
    decimal RealizedPnL,
    decimal Equity,
    /// <summary>
    /// Estimated net deposits, computed as <c>Equity - TotalReturn</c>.
    /// This is an approximation â€” it does not reflect a real deposit ledger.
    /// Withdrawals, bonuses, dividends, and other cash flows may not be included.
    /// </summary>
    decimal EstimatedNetDeposits,
    decimal TotalReturn,
    decimal TotalReturnPct
)
{
    public decimal TotalInvested => InvestedPrincipal;
    public decimal RealizedProfit => RealizedPnL;
    public decimal TotalAccountPerformance => TotalReturn;
}
