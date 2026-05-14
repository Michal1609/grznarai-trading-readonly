namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Calculations;

/// <summary>
/// Portfolio allocation for one open instrument, derived from eToro PnL positions.
/// </summary>
/// <remarks>
/// <para>
/// The allocation uses open-position <c>Amount</c> values as invested principal.
/// It includes both manually opened positions and positions inside copied
/// people/portfolios returned in PnL <c>mirrors</c>.
/// </para>
/// <para>
/// It does not include cash, pending orders, realized PnL, unrealized PnL,
/// fees, spreads, dividends, or closed positions.
/// </para>
/// </remarks>
public sealed record PortfolioInstrumentAllocation(
    /// <summary>eToro instrument ID.</summary>
    int InstrumentId,
    /// <summary>Instrument symbol or fallback label.</summary>
    string Symbol,
    /// <summary>eToro asset-class ID when available.</summary>
    int? AssetClassId,
    /// <summary>Asset-class name when available; otherwise an Unknown fallback.</summary>
    string AssetClass,
    /// <summary>eToro stock industry ID when available.</summary>
    int? IndustryId,
    /// <summary>Industry name when available; otherwise an Unknown fallback.</summary>
    string Industry,
    /// <summary>Total invested open-position amount for the instrument.</summary>
    decimal InvestedAmount,
    /// <summary>Share of total invested open-position amount, in the 0..1 range.</summary>
    decimal Share,
    /// <summary>Invested amount from manually opened positions.</summary>
    decimal ManualAmount,
    /// <summary>Invested amount from copied people/portfolio mirror positions.</summary>
    decimal MirrorAmount,
    /// <summary>Number of open PnL positions included in the instrument bucket.</summary>
    int PositionCount);

/// <summary>
/// Portfolio allocation aggregated by a named group such as asset class or industry.
/// </summary>
public sealed record PortfolioGroupAllocation(
    /// <summary>Group ID when eToro metadata provides one.</summary>
    int? GroupId,
    /// <summary>Display name of the group.</summary>
    string GroupName,
    /// <summary>Total invested open-position amount in the group.</summary>
    decimal InvestedAmount,
    /// <summary>Share of total invested open-position amount, in the 0..1 range.</summary>
    decimal Share,
    /// <summary>Number of distinct instruments in the group.</summary>
    int InstrumentCount,
    /// <summary>Number of open PnL positions included in the group.</summary>
    int PositionCount);
