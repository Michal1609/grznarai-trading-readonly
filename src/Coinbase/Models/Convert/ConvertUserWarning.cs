using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// User-facing warning attached to a convert trade.
/// </summary>
public sealed class ConvertUserWarning
{
    /// <summary>Unique identifier of the warning.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Machine-readable warning code.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>Localised message shown to the user.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
