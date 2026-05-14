using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Watchlist;

public record CuratedListItem(
    [property: JsonPropertyName("instrumentId")] int InstrumentId
);

public record CuratedList(
    [property: JsonPropertyName("uuid")] string Uuid,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("listImageUrl")] string? ListImageUrl,
    [property: JsonPropertyName("items")] IReadOnlyList<CuratedListItem> Items
);

public record CuratedListsResponse(
    [property: JsonPropertyName("curatedLists")] IReadOnlyList<CuratedList> CuratedLists
);
