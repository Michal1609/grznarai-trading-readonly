using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Market;

public record LiveRatesResponse(
    [property: JsonPropertyName("rates")] IReadOnlyList<InstrumentRate> Rates
);

public record InstrumentRate(
    [property: JsonPropertyName("instrumentID")]              int InstrumentId,
    [property: JsonPropertyName("ask")]                       decimal Ask,
    [property: JsonPropertyName("bid")]                       decimal Bid,
    [property: JsonPropertyName("lastExecution")]             decimal LastExecution,
    [property: JsonPropertyName("conversionRateAsk")]         decimal ConversionRateAsk,
    [property: JsonPropertyName("conversionRateBid")]         decimal ConversionRateBid,
    [property: JsonPropertyName("date")]                      DateTimeOffset Date,
    [property: JsonPropertyName("priceRateID")]               decimal? PriceRateId,
    [property: JsonPropertyName("unitMargin")]                decimal? UnitMargin,
    [property: JsonPropertyName("unitMarginAsk")]             decimal? UnitMarginAsk,
    [property: JsonPropertyName("unitMarginBid")]             decimal? UnitMarginBid,
    [property: JsonPropertyName("bidDiscounted")]             decimal? BidDiscounted,
    [property: JsonPropertyName("askDiscounted")]             decimal? AskDiscounted,
    [property: JsonPropertyName("unitMarginBidDiscounted")]   decimal? UnitMarginBidDiscounted,
    [property: JsonPropertyName("unitMarginAskDiscounted")]   decimal? UnitMarginAskDiscounted
);
