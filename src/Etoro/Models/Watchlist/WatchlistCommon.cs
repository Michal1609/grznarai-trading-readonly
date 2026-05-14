using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Watchlist;

public record SvgAvatarDto(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("backgroundColor")] string? BackgroundColor,
    [property: JsonPropertyName("textColor")] string? TextColor
);

public record AvatarDto(
    [property: JsonPropertyName("small")] string? Small,
    [property: JsonPropertyName("medium")] string? Medium,
    [property: JsonPropertyName("large")] string? Large,
    [property: JsonPropertyName("svg")] SvgAvatarDto? Svg
);

public record MarketDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("symbolName")] string SymbolName,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("assetTypeId")] int AssetTypeId,
    [property: JsonPropertyName("assetTypeSubCategoryId")] int? AssetTypeSubCategoryId,
    [property: JsonPropertyName("exchangeId")] int ExchangeId,
    [property: JsonPropertyName("hasExpirationDate")] bool HasExpirationDate,
    [property: JsonPropertyName("avatar")] AvatarDto? Avatar
);

public record WatchlistItemDto(
    [property: JsonPropertyName("itemId")] int ItemId,
    [property: JsonPropertyName("itemType")] string ItemType,
    [property: JsonPropertyName("itemRank")] int ItemRank,
    [property: JsonPropertyName("itemAddedReason")] string? ItemAddedReason,
    [property: JsonPropertyName("itemAddedDate")] DateTimeOffset? ItemAddedDate,
    [property: JsonPropertyName("market")] MarketDto? Market
);
