using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Market;

public record InstrumentClosingPrice(
    [property: JsonPropertyName("instrumentId")]         int InstrumentId,
    [property: JsonPropertyName("officialClosingPrice")] decimal OfficialClosingPrice,
    [property: JsonPropertyName("isMarketOpen")]         bool? IsMarketOpen,
    [property: JsonPropertyName("closingPrices")]        ClosingPriceIntervals? ClosingPrices
);

public record ClosingPriceIntervals(
    [property: JsonPropertyName("daily")]   ClosingPriceEntry? Daily,
    [property: JsonPropertyName("weekly")]  ClosingPriceEntry? Weekly,
    [property: JsonPropertyName("monthly")] ClosingPriceEntry? Monthly
);

/// <summary>Price and date for a single closing interval.</summary>
/// <remarks>Date uses non-standard format "yyyy-MM-dd HH:mm:ssZ" (space instead of T).</remarks>
public record ClosingPriceEntry(
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("date")]  string? Date
);
