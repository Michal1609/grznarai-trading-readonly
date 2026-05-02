using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Market;

public record Instrument(
    // Identification
    [property: JsonPropertyName("instrumentId")]                  int? InstrumentId,
    [property: JsonPropertyName("displayname")]                   string? DisplayName,
    [property: JsonPropertyName("instrumentTypeID")]              int? InstrumentTypeId,
    [property: JsonPropertyName("instrumentType")]                string? InstrumentType,
    [property: JsonPropertyName("exchangeID")]                    int? ExchangeId,
    [property: JsonPropertyName("symbol")]                        string? Symbol,

    // Internal platform metadata
    [property: JsonPropertyName("internalAssetClassId")]          int? InternalAssetClassId,
    [property: JsonPropertyName("internalInstrumentDisplayName")] string? InternalInstrumentDisplayName,
    [property: JsonPropertyName("isInternalInstrument")]          bool? IsInternalInstrument,
    [property: JsonPropertyName("internalSymbolFull")]            string? InternalSymbolFull,
    [property: JsonPropertyName("isHiddenFromClient")]            bool? IsHiddenFromClient,
    [property: JsonPropertyName("internalInstrumentId")]          int? InternalInstrumentId,
    [property: JsonPropertyName("internalCryptoTypeId")]          int? InternalCryptoTypeId,
    [property: JsonPropertyName("internalExchangeId")]            int? InternalExchangeId,
    [property: JsonPropertyName("internalExchangeName")]          string? InternalExchangeName,
    [property: JsonPropertyName("internalAssetClassName")]        string? InternalAssetClassName,
    [property: JsonPropertyName("internalIndustryId")]            int? InternalIndustryId,
    [property: JsonPropertyName("internalStockIndustryName")]     string? InternalStockIndustryName,
    [property: JsonPropertyName("internalClosingPrice")]          decimal? InternalClosingPrice,

    // Trading status
    [property: JsonPropertyName("isOpen")]                        bool? IsOpen,
    [property: JsonPropertyName("isDelisted")]                    bool? IsDelisted,
    [property: JsonPropertyName("isCurrentlyTradable")]           bool? IsCurrentlyTradable,
    [property: JsonPropertyName("isExchangeOpen")]                bool? IsExchangeOpen,
    [property: JsonPropertyName("isActiveInPlatform")]            bool? IsActiveInPlatform,
    [property: JsonPropertyName("isBuyEnabled")]                  bool? IsBuyEnabled,

    // Price
    [property: JsonPropertyName("currentRate")]                   decimal? CurrentRate,
    [property: JsonPropertyName("cvtBid")]                        decimal? CvtBid,
    [property: JsonPropertyName("cvtAsk")]                        decimal? CvtAsk,
    [property: JsonPropertyName("cvtBiNoSpread")]                 decimal? CvtBidNoSpread,
    [property: JsonPropertyName("cvtAskNoSpread")]                decimal? CvtAskNoSpread,

    // Price changes
    [property: JsonPropertyName("dailyPriceChange")]              decimal? DailyPriceChange,
    [property: JsonPropertyName("absDailyPriceChange")]           decimal? AbsDailyPriceChange,
    [property: JsonPropertyName("weeklyPriceChange")]             decimal? WeeklyPriceChange,
    [property: JsonPropertyName("monthlyPriceChange")]            decimal? MonthlyPriceChange,
    [property: JsonPropertyName("threeMonthPriceChange")]         decimal? ThreeMonthPriceChange,
    [property: JsonPropertyName("sixMonthPriceChange")]           decimal? SixMonthPriceChange,
    [property: JsonPropertyName("oneYearPriceChange")]            decimal? OneYearPriceChange,
    [property: JsonPropertyName("currMonthPriceChange")]          decimal? CurrMonthPriceChange,
    [property: JsonPropertyName("currQuarterPriceChange")]        decimal? CurrQuarterPriceChange,
    [property: JsonPropertyName("currYearPriceChange")]           decimal? CurrYearPriceChange,
    [property: JsonPropertyName("lastYearPriceChange")]           decimal? LastYearPriceChange,
    [property: JsonPropertyName("lastTwoYearsPriceChange")]       decimal? LastTwoYearsPriceChange,
    [property: JsonPropertyName("oneMonthAgoPriceChange")]        decimal? OneMonthAgoPriceChange,
    [property: JsonPropertyName("twoMonthsAgoPriceChange")]       decimal? TwoMonthsAgoPriceChange,
    [property: JsonPropertyName("threeMonthsAgoPriceChange")]     decimal? ThreeMonthsAgoPriceChange,
    [property: JsonPropertyName("sixMonthsAgoPriceChange")]       decimal? SixMonthsAgoPriceChange,
    [property: JsonPropertyName("oneYearAgoPriceChange")]         decimal? OneYearAgoPriceChange,

    // Popularity and traders
    [property: JsonPropertyName("popularityUniques")]             int? PopularityUniques,
    [property: JsonPropertyName("popularityUniques7Day")]         int? PopularityUniques7Day,
    [property: JsonPropertyName("popularityUniques14Day")]        int? PopularityUniques14Day,
    [property: JsonPropertyName("popularityUniques30Day")]        int? PopularityUniques30Day,
    [property: JsonPropertyName("traders7DayChange")]             decimal? Traders7DayChange,
    [property: JsonPropertyName("traders14DayChange")]            decimal? Traders14DayChange,
    [property: JsonPropertyName("traders30DayChange")]            decimal? Traders30DayChange,

    // Holdings and positions
    [property: JsonPropertyName("holdingPct")]                    decimal? HoldingPct,
    [property: JsonPropertyName("buyHoldingPct")]                 decimal? BuyHoldingPct,
    [property: JsonPropertyName("sellHoldingPct")]                decimal? SellHoldingPct,
    [property: JsonPropertyName("buyPctChange24Hours")]           decimal? BuyPctChange24Hours,
    [property: JsonPropertyName("absBuyPctChange24Hours")]        decimal? AbsBuyPctChange24Hours,

    // Industry
    [property: JsonPropertyName("industryNameId")]                int? IndustryNameId,
    [property: JsonPropertyName("sectorNameId")]                  int? SectorNameId,

    // Logos
    [property: JsonPropertyName("logo35x35")]                     string? Logo35x35,
    [property: JsonPropertyName("logo50x50")]                     string? Logo50x50,
    [property: JsonPropertyName("logo150x150")]                   string? Logo150x150
);
