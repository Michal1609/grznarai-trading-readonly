using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Pnl;

public record MirrorPosition(
    [property: JsonPropertyName("positionID")]    long PositionId,
    [property: JsonPropertyName("instrumentID")]  int InstrumentId,
    [property: JsonPropertyName("amount")]         decimal Amount,
    [property: JsonPropertyName("unrealizedPnL")]  PositionUnrealizedPnL? UnrealizedPnL,
    [property: JsonPropertyName("isBuy")]          bool IsBuy = false,
    [property: JsonPropertyName("leverage")]       int Leverage = 1,
    [property: JsonPropertyName("openRate")]       decimal OpenRate = 0,
    [property: JsonPropertyName("units")]          decimal Units = 0,
    [property: JsonPropertyName("totalFees")]      decimal TotalFees = 0,
    [property: JsonPropertyName("CID")]            int Cid = 0,
    [property: JsonPropertyName("openDateTime")]   DateTimeOffset? OpenDateTime = null
)
{
    public decimal PnL => UnrealizedPnL?.PnL ?? 0m;
}
