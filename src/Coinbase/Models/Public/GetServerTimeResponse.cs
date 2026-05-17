using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Json;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;

/// <summary>Response from <c>GET /api/v3/brokerage/time</c>.</summary>
public sealed class GetServerTimeResponse
{
    /// <summary>ISO-8601 representation of the server timestamp.</summary>
    [JsonPropertyName("iso")] public string? Iso { get; set; }

    /// <summary>Second-precision UNIX timestamp.</summary>
    [JsonPropertyName("epochSeconds")]
    [JsonConverter(typeof(NullableInt64StringConverter))]
    public long? EpochSeconds { get; set; }

    /// <summary>Millisecond-precision UNIX timestamp.</summary>
    [JsonPropertyName("epochMillis")]
    [JsonConverter(typeof(NullableInt64StringConverter))]
    public long? EpochMillis { get; set; }
}
