using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>
/// A single order returned by the Coinbase Advanced Trade API.
/// String-encoded numeric fields (e.g. <c>completion_percentage</c>) preserve full precision.
/// </summary>
public sealed class Order
{
    [JsonPropertyName("order_id")] public string? OrderId { get; set; }
    [JsonPropertyName("product_id")] public string? ProductId { get; set; }
    [JsonPropertyName("user_id")] public string? UserId { get; set; }
    [JsonPropertyName("order_configuration")] public OrderConfiguration? OrderConfiguration { get; set; }

    /// <summary>Order direction — see <see cref="OrderSide"/>.</summary>
    [JsonPropertyName("side")] public string? Side { get; set; }

    [JsonPropertyName("client_order_id")] public string? ClientOrderId { get; set; }

    /// <summary>Execution status — see <see cref="OrderStatus"/>.</summary>
    [JsonPropertyName("status")] public string? Status { get; set; }

    /// <summary>Time-in-force setting — see <see cref="TimeInForce"/>.</summary>
    [JsonPropertyName("time_in_force")] public string? TimeInForce { get; set; }

    [JsonPropertyName("created_time")] public DateTimeOffset? CreatedTime { get; set; }
    [JsonPropertyName("completion_percentage")] public string? CompletionPercentage { get; set; }
    [JsonPropertyName("filled_size")] public string? FilledSize { get; set; }
    [JsonPropertyName("average_filled_price")] public string? AverageFilledPrice { get; set; }
    [JsonPropertyName("number_of_fills")] public string? NumberOfFills { get; set; }
    [JsonPropertyName("filled_value")] public string? FilledValue { get; set; }
    [JsonPropertyName("pending_cancel")] public bool? PendingCancel { get; set; }
    [JsonPropertyName("size_in_quote")] public bool? SizeInQuote { get; set; }
    [JsonPropertyName("total_fees")] public string? TotalFees { get; set; }
    [JsonPropertyName("size_inclusive_of_fees")] public bool? SizeInclusiveOfFees { get; set; }
    [JsonPropertyName("total_value_after_fees")] public string? TotalValueAfterFees { get; set; }

    /// <summary>Stop trigger status — see <see cref="TriggerStatus"/>.</summary>
    [JsonPropertyName("trigger_status")] public string? TriggerStatus { get; set; }

    /// <summary>Order type — see <see cref="OrderType"/>.</summary>
    [JsonPropertyName("order_type")] public string? OrderType { get; set; }

    /// <summary>Rejection reason if the order was rejected — see <see cref="RejectReason"/>.</summary>
    [JsonPropertyName("reject_reason")] public string? RejectReason { get; set; }

    [JsonPropertyName("settled")] public bool? Settled { get; set; }

    /// <summary>Product type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductType"/>.</summary>
    [JsonPropertyName("product_type")] public string? ProductType { get; set; }

    [JsonPropertyName("reject_message")] public string? RejectMessage { get; set; }
    [JsonPropertyName("cancel_message")] public string? CancelMessage { get; set; }

    /// <summary>Placement source — see <see cref="OrderPlacementSource"/>.</summary>
    [JsonPropertyName("order_placement_source")] public string? OrderPlacementSource { get; set; }

    [JsonPropertyName("outstanding_hold_amount")] public string? OutstandingHoldAmount { get; set; }
    [JsonPropertyName("is_liquidation")] public bool? IsLiquidation { get; set; }
    [JsonPropertyName("last_fill_time")] public DateTimeOffset? LastFillTime { get; set; }
    [JsonPropertyName("edit_history")] public List<OrderEditHistory>? EditHistory { get; set; }
    [JsonPropertyName("leverage")] public string? Leverage { get; set; }
    [JsonPropertyName("margin_type")] public string? MarginType { get; set; }
    [JsonPropertyName("retail_portfolio_id")] public string? RetailPortfolioId { get; set; }
    [JsonPropertyName("originating_order_id")] public string? OriginatingOrderId { get; set; }
    [JsonPropertyName("attached_order_id")] public string? AttachedOrderId { get; set; }
    [JsonPropertyName("attached_order_configuration")] public OrderConfiguration? AttachedOrderConfiguration { get; set; }
    [JsonPropertyName("current_pending_replace")] public OrderEditHistory? CurrentPendingReplace { get; set; }
    [JsonPropertyName("commission_detail_total")] public CommissionDetailTotal? CommissionDetailTotal { get; set; }
    [JsonPropertyName("workable_size")] public string? WorkableSize { get; set; }
    [JsonPropertyName("workable_size_completion_pct")] public string? WorkableSizeCompletionPct { get; set; }
    [JsonPropertyName("product_details")] public ProductDetails? ProductDetails { get; set; }

    /// <summary>Cost basis method — see <see cref="CostBasisMethod"/>.</summary>
    [JsonPropertyName("cost_basis_method")] public string? CostBasisMethod { get; set; }

    /// <summary>Displayed order config type — see <see cref="DisplayedOrderConfig"/>.</summary>
    [JsonPropertyName("displayed_order_config")] public string? DisplayedOrderConfig { get; set; }

    /// <summary>Equity trading session — see <see cref="EquityTradingSession"/>.</summary>
    [JsonPropertyName("equity_trading_session")] public string? EquityTradingSession { get; set; }

    /// <summary>Prediction market side — see <see cref="PredictionSide"/>.</summary>
    [JsonPropertyName("prediction_side")] public string? PredictionSide { get; set; }

    [JsonPropertyName("last_update_time")] public DateTimeOffset? LastUpdateTime { get; set; }

    /// <summary>Deprecated by Coinbase. Fee charged for the order.</summary>
    [JsonPropertyName("fee")] public string? Fee { get; set; }
}
