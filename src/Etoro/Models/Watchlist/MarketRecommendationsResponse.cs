using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Watchlist;

public record MarketRecommendationsResponse(
    [property: JsonPropertyName("responseType")] string ResponseType,
    [property: JsonPropertyName("recommendations")] IReadOnlyList<int> Recommendations
);
