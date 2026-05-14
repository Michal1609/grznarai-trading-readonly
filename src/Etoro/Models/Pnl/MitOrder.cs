using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Pnl;

public record MitOrder(
    [property: JsonPropertyName("orderId")]        long OrderId,
    [property: JsonPropertyName("amount")]         decimal Amount,
    [property: JsonPropertyName("instrumentId")]   int InstrumentId = 0,
    [property: JsonPropertyName("cid")]            int Cid = 0,
    [property: JsonPropertyName("isBuy")]          bool IsBuy = false,
    [property: JsonPropertyName("rate")]           decimal Rate = 0,
    [property: JsonPropertyName("leverage")]       int Leverage = 1,
    [property: JsonPropertyName("units")]          decimal Units = 0,
    [property: JsonPropertyName("takeProfitRate")] decimal TakeProfitRate = 0,
    [property: JsonPropertyName("stopLossRate")]   decimal StopLossRate = 0,
    [property: JsonPropertyName("isTslEnabled")]   bool IsTslEnabled = false,
    [property: JsonPropertyName("openDateTime")]   DateTimeOffset? OpenDateTime = null,
    [property: JsonPropertyName("executionType")]  int ExecutionType = 0,
    [property: JsonPropertyName("isDiscounted")]   bool IsDiscounted = false,
    [property: JsonPropertyName("isNoTakeProfit")] bool IsNoTakeProfit = false,
    [property: JsonPropertyName("isNoStopLoss")]   bool IsNoStopLoss = false
);
