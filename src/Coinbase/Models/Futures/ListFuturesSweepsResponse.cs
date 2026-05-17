using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Response from <c>GET /api/v3/brokerage/cfm/sweeps</c>.
/// </summary>
public sealed class ListFuturesSweepsResponse
{
    [JsonPropertyName("sweeps")]
    public List<FuturesSweep>? Sweeps { get; set; }
}
