using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Market;

public record ExchangesResponse(
    [property: JsonPropertyName("exchangeInfo")] IReadOnlyList<ExchangeInfo> ExchangeInfo
);

public record ExchangeInfo(
    [property: JsonPropertyName("exchangeID")]          int ExchangeId,
    [property: JsonPropertyName("exchangeDescription")] string? ExchangeDescription
);
