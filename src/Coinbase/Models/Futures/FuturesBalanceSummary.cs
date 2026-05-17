using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Aggregated balance and margin information for CFM futures trading.
/// </summary>
public sealed class FuturesBalanceSummary
{
    /// <summary>Available cash that can be used to open new CFM futures positions.</summary>
    [JsonPropertyName("futures_buying_power")]
    public FuturesAmount? FuturesBuyingPower { get; set; }

    /// <summary>Total USD balance aggregated across spot and futures accounts.</summary>
    [JsonPropertyName("total_usd_balance")]
    public FuturesAmount? TotalUsdBalance { get; set; }

    /// <summary>USD held in the Coinbase spot (CBI) account.</summary>
    [JsonPropertyName("cbi_usd_balance")]
    public FuturesAmount? CbiUsdBalance { get; set; }

    /// <summary>USD held in the CFTC-regulated CFM futures account.</summary>
    [JsonPropertyName("cfm_usd_balance")]
    public FuturesAmount? CfmUsdBalance { get; set; }

    /// <summary>Balance currently on hold for open futures orders.</summary>
    [JsonPropertyName("total_open_orders_hold_amount")]
    public FuturesAmount? TotalOpenOrdersHoldAmount { get; set; }

    /// <summary>Unrealized profit or loss across all open positions.</summary>
    [JsonPropertyName("unrealized_pnl")]
    public FuturesAmount? UnrealizedPnl { get; set; }

    /// <summary>Realized profit or loss since the start of the current trade date.</summary>
    [JsonPropertyName("daily_realized_pnl")]
    public FuturesAmount? DailyRealizedPnl { get; set; }

    /// <summary>Margin required to maintain all current positions.</summary>
    [JsonPropertyName("initial_margin")]
    public FuturesAmount? InitialMargin { get; set; }

    /// <summary>Funds available to satisfy margin requirements.</summary>
    [JsonPropertyName("available_margin")]
    public FuturesAmount? AvailableMargin { get; set; }

    /// <summary>Balance level at which positions are subject to liquidation.</summary>
    [JsonPropertyName("liquidation_threshold")]
    public FuturesAmount? LiquidationThreshold { get; set; }

    /// <summary>Funds exceeding the liquidation threshold (cushion before liquidation).</summary>
    [JsonPropertyName("liquidation_buffer_amount")]
    public FuturesAmount? LiquidationBufferAmount { get; set; }

    /// <summary>Liquidation buffer expressed as a percentage string.</summary>
    [JsonPropertyName("liquidation_buffer_percentage")]
    public string? LiquidationBufferPercentage { get; set; }

    /// <summary>Margin window measures for the intraday trading session.</summary>
    [JsonPropertyName("intraday_margin_window_measure")]
    public FcmMarginWindowMeasure? IntradayMarginWindowMeasure { get; set; }

    /// <summary>Margin window measures for the overnight trading session.</summary>
    [JsonPropertyName("overnight_margin_window_measure")]
    public FcmMarginWindowMeasure? OvernightMarginWindowMeasure { get; set; }

    /// <summary>Total amount of pending fund transfers.</summary>
    [JsonPropertyName("total_pending_transfers_amount")]
    public FuturesAmount? TotalPendingTransfersAmount { get; set; }

    /// <summary>Funding P&amp;L applicable to US Perpetuals Futures accounts.</summary>
    [JsonPropertyName("funding_pnl")]
    public FuturesAmount? FundingPnl { get; set; }
}
