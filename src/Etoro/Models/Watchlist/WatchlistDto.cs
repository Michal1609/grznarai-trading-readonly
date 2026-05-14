using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Watchlist;

public record WatchlistDto(
    [property: JsonPropertyName("watchlistId")] string WatchlistId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("Gcid")] int Gcid,
    [property: JsonPropertyName("watchlistType")] WatchlistType WatchlistType,
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("isDefault")] bool IsDefault,
    [property: JsonPropertyName("isUserSelectedDefault")] bool IsUserSelectedDefault,
    [property: JsonPropertyName("watchlistRank")] int WatchlistRank,
    [property: JsonPropertyName("dynamicUrl")] string? DynamicUrl,
    [property: JsonPropertyName("items")] IReadOnlyList<WatchlistItemDto> Items,
    [property: JsonPropertyName("relatedAssets")] IReadOnlyList<int>? RelatedAssets
);

public record WatchlistMetaDto(
    [property: JsonPropertyName("pageNumber")] int PageNumber,
    [property: JsonPropertyName("itemsPerPage")] int ItemsPerPage,
    [property: JsonPropertyName("maxItemsInWatchlistLimit")] int MaxItemsInWatchlistLimit,
    [property: JsonPropertyName("maxWatchlistsLimit")] int MaxWatchlistsLimit
);

public record WatchlistExceptionDto(
    [property: JsonPropertyName("invalidItems")] IReadOnlyList<string>? InvalidItems,
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("message")] string? Message
);

public record WatchlistsResponse(
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("isSucceeded")] bool IsSucceeded,
    [property: JsonPropertyName("watchlists")] IReadOnlyList<WatchlistDto> Watchlists,
    [property: JsonPropertyName("exception")] WatchlistExceptionDto? Exception,
    [property: JsonPropertyName("meta")] WatchlistMetaDto? Meta
);
