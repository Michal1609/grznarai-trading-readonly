using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// A pending or processing fund sweep between the user's Coinbase spot account
/// and their CFTC-regulated CFM futures account.
/// </summary>
public sealed class FuturesSweep
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Amount requested to sweep.</summary>
    [JsonPropertyName("requested_amount")]
    public FuturesAmount? RequestedAmount { get; set; }

    /// <summary>When <c>true</c> the sweep transfers all available funds.</summary>
    [JsonPropertyName("should_sweep_all")]
    public bool? ShouldSweepAll { get; set; }

    /// <summary>Sweep lifecycle status — see <see cref="FuturesSweepStatus"/>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>UTC timestamp when the sweep was submitted.</summary>
    [JsonPropertyName("scheduled_time")]
    public DateTimeOffset? ScheduledTime { get; set; }
}
