using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Portfolio;

public record PortfolioPosition(
    [property: JsonPropertyName("positionID")] long PositionId,
    [property: JsonPropertyName("instrumentID")] int InstrumentId,
    [property: JsonPropertyName("isBuy")] bool IsBuy,
    [property: JsonPropertyName("leverage")] int Leverage,
    [property: JsonPropertyName("investedAmount")] decimal InvestedAmount,
    [property: JsonPropertyName("units")] decimal Units,
    [property: JsonPropertyName("openRate")] decimal OpenRate,
    [property: JsonPropertyName("openDateTime")] DateTimeOffset OpenDateTime,
    [property: JsonPropertyName("currentRate")] decimal CurrentRate,
    [property: JsonPropertyName("netProfit")] decimal NetProfit,
    [property: JsonPropertyName("mirrorID")] int MirrorId
);
