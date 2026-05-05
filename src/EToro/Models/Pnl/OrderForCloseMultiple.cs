using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Pnl;

public record OrderForCloseMultiple(
    [property: JsonPropertyName("orderId")]                 long OrderId,
    [property: JsonPropertyName("instrumentId")]            int InstrumentId = 0,
    [property: JsonPropertyName("cid")]                     int Cid = 0,
    [property: JsonPropertyName("orderType")]               int OrderType = 0,
    [property: JsonPropertyName("statusId")]                int StatusId = 0,
    [property: JsonPropertyName("unitsToDeduct")]           decimal UnitsToDeduct = 0,
    [property: JsonPropertyName("lotsToDeduct")]            decimal LotsToDeduct = 0,
    [property: JsonPropertyName("openDateTime")]            DateTimeOffset? OpenDateTime = null,
    [property: JsonPropertyName("lastUpdate")]              DateTimeOffset? LastUpdate = null,
    [property: JsonPropertyName("pendingClosePositionIds")] IReadOnlyList<long>? PendingClosePositionIds = null
);
