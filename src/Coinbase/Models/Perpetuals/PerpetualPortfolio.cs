using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Perpetuals portfolio returned by the Get Perpetuals Portfolio Summary endpoint.
/// </summary>
public sealed class PerpetualPortfolio
{
    [JsonPropertyName("portfolio_uuid")]
    public string? PortfolioUuid { get; set; }

    /// <summary>Total USDC collateral value.</summary>
    [JsonPropertyName("collateral")]
    public string? Collateral { get; set; }

    /// <summary>Total position notional in USDC.</summary>
    [JsonPropertyName("position_notional")]
    public string? PositionNotional { get; set; }

    /// <summary>Position notional for open orders in USDC.</summary>
    [JsonPropertyName("open_position_notional")]
    public string? OpenPositionNotional { get; set; }

    /// <summary>Pending fees in USDC.</summary>
    [JsonPropertyName("pending_fees")]
    public string? PendingFees { get; set; }

    /// <summary>Total USDC borrowed.</summary>
    [JsonPropertyName("borrow")]
    public string? Borrow { get; set; }

    [JsonPropertyName("accrued_interest")]
    public string? AccruedInterest { get; set; }

    [JsonPropertyName("rolling_debt")]
    public string? RollingDebt { get; set; }

    [JsonPropertyName("portfolio_initial_margin")]
    public string? PortfolioInitialMargin { get; set; }

    [JsonPropertyName("portfolio_im_notional")]
    public PerpetualAmount? PortfolioImNotional { get; set; }

    [JsonPropertyName("portfolio_maintenance_margin")]
    public string? PortfolioMaintenanceMargin { get; set; }

    [JsonPropertyName("portfolio_mm_notional")]
    public PerpetualAmount? PortfolioMmNotional { get; set; }

    [JsonPropertyName("liquidation_percentage")]
    public string? LiquidationPercentage { get; set; }

    [JsonPropertyName("liquidation_buffer")]
    public string? LiquidationBuffer { get; set; }

    [JsonPropertyName("margin_type")]
    public MarginType MarginType { get; set; }

    [JsonPropertyName("margin_flags")]
    public PortfolioMarginFlags MarginFlags { get; set; }

    [JsonPropertyName("liquidation_status")]
    public PortfolioLiquidationStatus LiquidationStatus { get; set; }

    [JsonPropertyName("unrealized_pnl")]
    public PerpetualAmount? UnrealizedPnl { get; set; }

    [JsonPropertyName("total_balance")]
    public PerpetualAmount? TotalBalance { get; set; }
}
