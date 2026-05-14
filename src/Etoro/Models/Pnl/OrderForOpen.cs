using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Pnl;

public record OrderForOpen(
    [property: JsonPropertyName("mirrorId")]              int MirrorId,
    [property: JsonPropertyName("amount")]                decimal Amount,
    [property: JsonPropertyName("totalExternalCosts")]    decimal TotalExternalCosts,
    [property: JsonPropertyName("orderId")]               long OrderId = 0,
    [property: JsonPropertyName("instrumentId")]          int InstrumentId = 0,
    [property: JsonPropertyName("cid")]                   int Cid = 0,
    [property: JsonPropertyName("statusId")]              int StatusId = 0,
    [property: JsonPropertyName("orderType")]             int OrderType = 0,
    [property: JsonPropertyName("amountInUnits")]         decimal AmountInUnits = 0,
    [property: JsonPropertyName("frozenAmount")]          decimal FrozenAmount = 0,
    [property: JsonPropertyName("lastUpdate")]            DateTimeOffset? LastUpdate = null,
    [property: JsonPropertyName("openDateTime")]          DateTimeOffset? OpenDateTime = null,
    [property: JsonPropertyName("isBuy")]                 bool IsBuy = false,
    [property: JsonPropertyName("leverage")]              int Leverage = 1,
    [property: JsonPropertyName("stopLossRate")]          decimal StopLossRate = 0,
    [property: JsonPropertyName("takeProfitRate")]        decimal TakeProfitRate = 0,
    [property: JsonPropertyName("isTslEnabled")]          bool IsTslEnabled = false,
    [property: JsonPropertyName("isDiscounted")]          bool IsDiscounted = false,
    [property: JsonPropertyName("isNoTakeProfit")]        bool IsNoTakeProfit = false,
    [property: JsonPropertyName("isNoStopLoss")]          bool IsNoStopLoss = false,
    [property: JsonPropertyName("lotCount")]              decimal LotCount = 0,
    [property: JsonPropertyName("openPositionActionType")]int OpenPositionActionType = 0,
    [property: JsonPropertyName("externalOperation")]     JsonElement? ExternalOperation = null
);
