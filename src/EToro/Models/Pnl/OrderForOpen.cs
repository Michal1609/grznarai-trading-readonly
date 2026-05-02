using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Pnl;

public record OrderForOpen(
    [property: JsonPropertyName("mirrorId")]             int MirrorId,
    [property: JsonPropertyName("amount")]               decimal Amount,
    [property: JsonPropertyName("totalExternalCosts")]   decimal TotalExternalCosts,
    [property: JsonPropertyName("orderId")]              long OrderId = 0,
    [property: JsonPropertyName("instrumentId")]         int InstrumentId = 0,
    [property: JsonPropertyName("cid")]                  int Cid = 0,
    [property: JsonPropertyName("statusId")]             int StatusId = 0,
    [property: JsonPropertyName("orderType")]            string? OrderType = null,
    [property: JsonPropertyName("amountInUnits")]        decimal AmountInUnits = 0,
    [property: JsonPropertyName("frozenAmount")]         decimal FrozenAmount = 0,
    [property: JsonPropertyName("lastUpdate")]           DateTimeOffset? LastUpdate = null
);
