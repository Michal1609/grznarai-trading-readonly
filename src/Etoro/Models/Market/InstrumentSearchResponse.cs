using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Market;

public record InstrumentSearchResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<Instrument> Instruments,
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize
);
