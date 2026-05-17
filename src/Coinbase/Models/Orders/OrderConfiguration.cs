using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>
/// Describes how an order is configured. Exactly one nested property is populated
/// depending on the order's execution strategy and time-in-force.
/// </summary>
public sealed class OrderConfiguration
{
    [JsonPropertyName("market_market_ioc")] public MarketMarketIoc? MarketIoc { get; set; }
    [JsonPropertyName("sor_limit_ioc")] public SorLimitIoc? SorLimit { get; set; }
    [JsonPropertyName("limit_limit_gtc")] public LimitLimitGtc? LimitGtc { get; set; }
    [JsonPropertyName("limit_limit_gtd")] public LimitLimitGtd? LimitGtd { get; set; }
    [JsonPropertyName("limit_limit_fok")] public LimitLimitFok? LimitFok { get; set; }
    [JsonPropertyName("stop_limit_stop_limit_gtc")] public StopLimitStopLimitGtc? StopLimitGtc { get; set; }
    [JsonPropertyName("stop_limit_stop_limit_gtd")] public StopLimitStopLimitGtd? StopLimitGtd { get; set; }
    [JsonPropertyName("trigger_bracket_gtc")] public TriggerBracketGtc? BracketGtc { get; set; }
    [JsonPropertyName("trigger_bracket_gtd")] public TriggerBracketGtd? BracketGtd { get; set; }

    /// <summary>Market order with immediate-or-cancel time-in-force.</summary>
    public sealed class MarketMarketIoc
    {
        [JsonPropertyName("quote_size")] public string? QuoteSize { get; set; }
        [JsonPropertyName("base_size")] public string? BaseSize { get; set; }
    }

    /// <summary>Smart-order-routing limit order with immediate-or-cancel time-in-force.</summary>
    public sealed class SorLimitIoc
    {
        [JsonPropertyName("base_size")] public string? BaseSize { get; set; }
        [JsonPropertyName("limit_price")] public string? LimitPrice { get; set; }
    }

    /// <summary>Limit order with good-till-cancelled time-in-force.</summary>
    public sealed class LimitLimitGtc
    {
        [JsonPropertyName("base_size")] public string? BaseSize { get; set; }
        [JsonPropertyName("limit_price")] public string? LimitPrice { get; set; }
        [JsonPropertyName("post_only")] public bool? PostOnly { get; set; }
    }

    /// <summary>Limit order with good-till-date time-in-force.</summary>
    public sealed class LimitLimitGtd
    {
        [JsonPropertyName("base_size")] public string? BaseSize { get; set; }
        [JsonPropertyName("limit_price")] public string? LimitPrice { get; set; }
        [JsonPropertyName("end_time")] public DateTimeOffset? EndTime { get; set; }
        [JsonPropertyName("post_only")] public bool? PostOnly { get; set; }
    }

    /// <summary>Limit order with fill-or-kill time-in-force.</summary>
    public sealed class LimitLimitFok
    {
        [JsonPropertyName("base_size")] public string? BaseSize { get; set; }
        [JsonPropertyName("limit_price")] public string? LimitPrice { get; set; }
    }

    /// <summary>Stop-limit order with good-till-cancelled time-in-force.</summary>
    public sealed class StopLimitStopLimitGtc
    {
        [JsonPropertyName("base_size")] public string? BaseSize { get; set; }
        [JsonPropertyName("limit_price")] public string? LimitPrice { get; set; }
        [JsonPropertyName("stop_price")] public string? StopPrice { get; set; }
        [JsonPropertyName("stop_direction")] public string? StopDirection { get; set; }
    }

    /// <summary>Stop-limit order with good-till-date time-in-force.</summary>
    public sealed class StopLimitStopLimitGtd
    {
        [JsonPropertyName("base_size")] public string? BaseSize { get; set; }
        [JsonPropertyName("limit_price")] public string? LimitPrice { get; set; }
        [JsonPropertyName("start_time")] public DateTimeOffset? StartTime { get; set; }
        [JsonPropertyName("stop_price")] public string? StopPrice { get; set; }
        [JsonPropertyName("stop_direction")] public string? StopDirection { get; set; }
        [JsonPropertyName("end_time")] public DateTimeOffset? EndTime { get; set; }
    }

    /// <summary>Bracket (take-profit / stop-loss) order with good-till-cancelled time-in-force.</summary>
    public sealed class TriggerBracketGtc
    {
        [JsonPropertyName("base_size")] public string? BaseSize { get; set; }
        [JsonPropertyName("limit_price")] public string? LimitPrice { get; set; }
        [JsonPropertyName("stop_trigger_price")] public string? StopTriggerPrice { get; set; }
    }

    /// <summary>Bracket (take-profit / stop-loss) order with good-till-date time-in-force.</summary>
    public sealed class TriggerBracketGtd
    {
        [JsonPropertyName("base_size")] public string? BaseSize { get; set; }
        [JsonPropertyName("limit_price")] public string? LimitPrice { get; set; }
        [JsonPropertyName("stop_trigger_price")] public string? StopTriggerPrice { get; set; }
        [JsonPropertyName("end_time")] public DateTimeOffset? EndTime { get; set; }
    }
}
