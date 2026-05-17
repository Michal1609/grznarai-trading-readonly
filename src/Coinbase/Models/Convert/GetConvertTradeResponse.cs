using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

/// <summary>
/// Response from <c>GET /api/v3/brokerage/convert/trade/{trade_id}</c>.
/// </summary>
public sealed class GetConvertTradeResponse
{
    /// <summary>The convert trade details.</summary>
    [JsonPropertyName("trade")]
    public ConvertTrade? Trade { get; set; }
}
