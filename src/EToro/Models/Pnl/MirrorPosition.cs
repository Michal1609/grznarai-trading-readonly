using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Pnl;

public record MirrorPosition(
    [property: JsonPropertyName("positionID")]            long PositionId,
    [property: JsonPropertyName("instrumentID")]          int InstrumentId,
    [property: JsonPropertyName("amount")]                decimal Amount,
    [property: JsonPropertyName("unrealizedPnL")]         PositionUnrealizedPnL? UnrealizedPnL,
    [property: JsonPropertyName("isBuy")]                 bool IsBuy = false,
    [property: JsonPropertyName("leverage")]              int Leverage = 1,
    [property: JsonPropertyName("openRate")]              decimal OpenRate = 0,
    [property: JsonPropertyName("units")]                 decimal Units = 0,
    [property: JsonPropertyName("totalFees")]             decimal TotalFees = 0,
    [property: JsonPropertyName("CID")]                   int Cid = 0,
    [property: JsonPropertyName("mirrorID")]              int MirrorId = 0,
    [property: JsonPropertyName("openDateTime")]          DateTimeOffset? OpenDateTime = null,
    [property: JsonPropertyName("takeProfitRate")]        decimal TakeProfitRate = 0,
    [property: JsonPropertyName("stopLossRate")]          decimal StopLossRate = 0,
    [property: JsonPropertyName("orderID")]               long OrderId = 0,
    [property: JsonPropertyName("orderType")]             int OrderType = 0,
    [property: JsonPropertyName("parentPositionID")]      long ParentPositionId = 0,
    [property: JsonPropertyName("initialAmountInDollars")]decimal InitialAmountInDollars = 0,
    [property: JsonPropertyName("isTslEnabled")]          bool IsTslEnabled = false,
    [property: JsonPropertyName("isPartiallyAltered")]    bool IsPartiallyAltered = false,
    [property: JsonPropertyName("unitsBaseValueDollars")] decimal UnitsBaseValueDollars = 0,
    [property: JsonPropertyName("isDiscounted")]          bool IsDiscounted = false,
    [property: JsonPropertyName("openPositionActionType")]int OpenPositionActionType = 0,
    [property: JsonPropertyName("settlementTypeID")]      int SettlementTypeId = 0,
    [property: JsonPropertyName("isDetached")]            bool IsDetached = false,
    [property: JsonPropertyName("openConversionRate")]    decimal OpenConversionRate = 0,
    [property: JsonPropertyName("totalExternalFees")]     decimal TotalExternalFees = 0,
    [property: JsonPropertyName("totalExternalTaxes")]    decimal TotalExternalTaxes = 0,
    [property: JsonPropertyName("isNoTakeProfit")]        bool IsNoTakeProfit = false,
    [property: JsonPropertyName("isNoStopLoss")]          bool IsNoStopLoss = false,
    [property: JsonPropertyName("lotCount")]              decimal LotCount = 0,
    [property: JsonPropertyName("initialUnits")]          decimal InitialUnits = 0,
    [property: JsonPropertyName("stopLossVersion")]       int StopLossVersion = 0,
    [property: JsonPropertyName("isSettled")]             bool IsSettled = false,
    [property: JsonPropertyName("redeemStatusID")]        int RedeemStatusId = 0,
    [property: JsonPropertyName("pnlVersion")]            int PnlVersion = 0,
    [property: JsonPropertyName("pnL")]                   decimal? DirectPnL = null,
    [property: JsonPropertyName("closeRate")]             decimal? CloseRate = null,
    [property: JsonPropertyName("closeConversionRate")]   decimal? CloseConversionRate = null,
    [property: JsonPropertyName("timestamp")]             DateTimeOffset? Timestamp = null,
    [property: JsonPropertyName("externalOperation")]     JsonElement? ExternalOperation = null
)
{
    [JsonIgnore]
    public decimal PnL => DirectPnL ?? UnrealizedPnL?.PnL ?? 0m;
}
