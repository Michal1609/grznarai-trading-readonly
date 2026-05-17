using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverter<PositionSide>))]
public enum PositionSide
{
#pragma warning disable CA1069
    [JsonStringEnumMemberName("POSITION_SIDE_UNSPECIFIED")] Unspecified = 0,
    [JsonStringEnumMemberName("POSITION_SIDE_UNKNOWN")] Unknown = 0,
#pragma warning restore CA1069
    [JsonStringEnumMemberName("POSITION_SIDE_LONG")] Long = 1,
    [JsonStringEnumMemberName("POSITION_SIDE_SHORT")] Short = 2,
    [JsonStringEnumMemberName("FUTURES_POSITION_SIDE_LONG")] FuturesLong = 3,
    [JsonStringEnumMemberName("FUTURES_POSITION_SIDE_SHORT")] FuturesShort = 4
}
