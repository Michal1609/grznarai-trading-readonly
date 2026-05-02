using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Market;

public record InstrumentMetadataResponse(
    [property: JsonPropertyName("instrumentDisplayDatas")] IReadOnlyList<InstrumentDisplayData> InstrumentDisplayDatas
);

public record InstrumentDisplayData(
    [property: JsonPropertyName("instrumentID")]          int InstrumentId,
    [property: JsonPropertyName("instrumentDisplayName")] string? InstrumentDisplayName,
    [property: JsonPropertyName("instrumentTypeID")]      int? InstrumentTypeId,
    [property: JsonPropertyName("exchangeID")]            int? ExchangeId,
    [property: JsonPropertyName("symbolFull")]            string? SymbolFull,
    [property: JsonPropertyName("stocksIndustryId")]      int? StocksIndustryId,
    [property: JsonPropertyName("priceSource")]           string? PriceSource,
    [property: JsonPropertyName("hasExpirationDate")]     bool? HasExpirationDate,
    [property: JsonPropertyName("isInternalInstrument")]  bool? IsInternalInstrument,
    [property: JsonPropertyName("images")]                IReadOnlyList<InstrumentImageAsset>? Images
);

public record InstrumentImageAsset(
    [property: JsonPropertyName("url")]    string? Url,
    [property: JsonPropertyName("width")]  decimal? Width,
    [property: JsonPropertyName("height")] decimal? Height,
    [property: JsonPropertyName("theme")]  string? Theme
);
