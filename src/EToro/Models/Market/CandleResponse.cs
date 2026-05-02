using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Market;

public record CandleResponse(
    [property: JsonPropertyName("interval")] string Interval,
    [property: JsonPropertyName("candles")] IReadOnlyList<Candle> Candles
);

public record Candle(
    [property: JsonPropertyName("instrumentID")] int InstrumentId,
    [property: JsonPropertyName("fromDate")]     DateTimeOffset FromDate,
    [property: JsonPropertyName("open")]         decimal Open,
    [property: JsonPropertyName("high")]         decimal High,
    [property: JsonPropertyName("low")]          decimal Low,
    [property: JsonPropertyName("close")]        decimal Close,
    [property: JsonPropertyName("volume")]       decimal Volume,
    [property: JsonPropertyName("rangeOpen")]    decimal? RangeOpen,
    [property: JsonPropertyName("rangeClose")]   decimal? RangeClose,
    [property: JsonPropertyName("rangeHigh")]    decimal? RangeHigh,
    [property: JsonPropertyName("rangeLow")]     decimal? RangeLow
);
