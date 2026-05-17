using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// A single CFM (CFTC-regulated futures) position held by the authenticated user.
/// Numeric fields are string-encoded to preserve precision.
/// </summary>
public sealed class FcmPosition
{
    /// <summary>Ticker symbol, e.g. <c>BIT-28JUL23-CDE</c>.</summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    /// <summary>UTC timestamp when the futures contract expires.</summary>
    [JsonPropertyName("expiration_time")]
    public DateTimeOffset? ExpirationTime { get; set; }

    /// <summary>Position direction — see <see cref="FuturesPositionSide"/>.</summary>
    [JsonPropertyName("side")]
    public string? Side { get; set; }

    /// <summary>Position size expressed as number of contracts.</summary>
    [JsonPropertyName("number_of_contracts")]
    public string? NumberOfContracts { get; set; }

    /// <summary>Current mark price of the product.</summary>
    [JsonPropertyName("current_price")]
    public string? CurrentPrice { get; set; }

    /// <summary>Volume-weighted average price at which the position was entered.</summary>
    [JsonPropertyName("avg_entry_price")]
    public string? AvgEntryPrice { get; set; }

    /// <summary>Unrealized profit or loss at the current mark price.</summary>
    [JsonPropertyName("unrealized_pnl")]
    public string? UnrealizedPnl { get; set; }

    /// <summary>Realized profit or loss since the start of the current trade date.</summary>
    [JsonPropertyName("daily_realized_pnl")]
    public string? DailyRealizedPnl { get; set; }
}
