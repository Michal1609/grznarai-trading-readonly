using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Market;

public record StocksIndustriesResponse(
    [property: JsonPropertyName("stocksIndustries")] IReadOnlyList<StocksIndustry> StocksIndustries
);

public record StocksIndustry(
    [property: JsonPropertyName("industryID")]   int IndustryId,
    [property: JsonPropertyName("industryName")] string? IndustryName
);
