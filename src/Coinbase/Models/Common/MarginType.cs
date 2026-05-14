using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverter<MarginType>))]
public enum MarginType
{
    [JsonStringEnumMemberName("MARGIN_TYPE_UNSPECIFIED")] Unspecified = 0,
    [JsonStringEnumMemberName("MARGIN_TYPE_CROSS")] Cross = 1,
    [JsonStringEnumMemberName("MARGIN_TYPE_ISOLATED")] Isolated = 2
}
