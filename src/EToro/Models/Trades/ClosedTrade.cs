using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Trades;

public record ClosedTrade(
    [property: JsonPropertyName("positionId")] long PositionId,
    [property: JsonPropertyName("instrumentId")] int InstrumentId,
    [property: JsonPropertyName("isBuy")] bool IsBuy,
    [property: JsonPropertyName("leverage")] int Leverage,
    [property: JsonPropertyName("investment")] decimal Investment,
    [property: JsonPropertyName("initialInvestment")] decimal InitialInvestment,
    [property: JsonPropertyName("openRate")] decimal OpenRate,
    [property: JsonPropertyName("closeRate")] decimal CloseRate,
    [property: JsonPropertyName("openTimestamp")] DateTimeOffset OpenTimestamp,
    [property: JsonPropertyName("closeTimestamp")] DateTimeOffset CloseTimestamp,
    [property: JsonPropertyName("netProfit")] decimal NetProfit,
    [property: JsonPropertyName("fees")] decimal Fees,
    [property: JsonPropertyName("units")] decimal Units,
    [property: JsonPropertyName("stopLossRate")] decimal StopLossRate,
    [property: JsonPropertyName("takeProfitRate")] decimal TakeProfitRate,
    [property: JsonPropertyName("trailingStopLoss")] bool TrailingStopLoss,
    [property: JsonPropertyName("orderId")] long OrderId,
    [property: JsonPropertyName("socialTradeId")] long SocialTradeId,
    [property: JsonPropertyName("parentPositionId")] long ParentPositionId
);
