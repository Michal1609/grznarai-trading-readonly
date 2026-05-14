using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Trades;

public record OrderForOpenInfoResponse(
    [property: JsonPropertyName("orderID")]         long OrderId,
    [property: JsonPropertyName("CID")]             long Cid,
    [property: JsonPropertyName("statusID")]        int StatusId,
    [property: JsonPropertyName("orderType")]       int OrderType,
    [property: JsonPropertyName("instrumentID")]    int InstrumentId,
    [property: JsonPropertyName("requestOccurred")] DateTimeOffset RequestOccurred,
    [property: JsonPropertyName("token")]           string? Token = null,
    [property: JsonPropertyName("referenceID")]     string? ReferenceId = null,
    [property: JsonPropertyName("openActionType")]  int OpenActionType = 0,
    [property: JsonPropertyName("errorCode")]       int? ErrorCode = null,
    [property: JsonPropertyName("errorMessage")]    string? ErrorMessage = null,
    [property: JsonPropertyName("amount")]          decimal Amount = 0,
    [property: JsonPropertyName("units")]           decimal Units = 0,
    [property: JsonPropertyName("positions")]       IReadOnlyList<OrderPositionInfo>? Positions = null
);

public record OrderPositionInfo(
    [property: JsonPropertyName("positionID")]     long PositionId,
    [property: JsonPropertyName("orderType")]      int OrderType,
    [property: JsonPropertyName("occurred")]       DateTimeOffset Occurred,
    [property: JsonPropertyName("rate")]           decimal Rate,
    [property: JsonPropertyName("units")]          decimal Units,
    [property: JsonPropertyName("amount")]         decimal Amount,
    [property: JsonPropertyName("isOpen")]         bool IsOpen,
    [property: JsonPropertyName("conversionRate")] decimal ConversionRate = 0
);
