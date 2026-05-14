namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Market;

/// <summary>
/// Available field names for SearchInstrumentsAsync. API allows max 5 fields per request.
/// Use <see cref="All"/> to validate field values before sending.
/// </summary>
public static class InstrumentFields
{
    /// <summary>All known valid field names accepted by the search endpoint.</summary>
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        InstrumentId, DisplayName, InstrumentTypeId, InstrumentType, ExchangeId, Symbol,
        InternalAssetClassId, InternalInstrumentDisplayName, IsInternalInstrument, InternalSymbolFull,
        IsHiddenFromClient, InternalInstrumentId, InternalCryptoTypeId, InternalExchangeId,
        InternalExchangeName, InternalAssetClassName, InternalIndustryId, InternalStockIndustryName,
        InternalClosingPrice, IsOpen, IsDelisted, IsCurrentlyTradable, IsExchangeOpen, IsActiveInPlatform,
        IsBuyEnabled, CurrentRate, CvtBid, CvtAsk, CvtBidNoSpread, CvtAskNoSpread,
        DailyPriceChange, AbsDailyPriceChange, WeeklyPriceChange, MonthlyPriceChange,
        ThreeMonthPriceChange, SixMonthPriceChange, OneYearPriceChange, CurrMonthPriceChange,
        CurrQuarterPriceChange, CurrYearPriceChange, LastYearPriceChange, LastTwoYearsPriceChange,
        OneMonthAgoPriceChange, TwoMonthsAgoPriceChange, ThreeMonthsAgoPriceChange,
        SixMonthsAgoPriceChange, OneYearAgoPriceChange,
        PopularityUniques, PopularityUniques7Day, PopularityUniques14Day, PopularityUniques30Day,
        Traders7DayChange, Traders14DayChange, Traders30DayChange,
        HoldingPct, BuyHoldingPct, SellHoldingPct, BuyPctChange24Hours, AbsBuyPctChange24Hours,
        IndustryNameId, SectorNameId, Logo35x35, Logo50x50, Logo150x150
    };

    // Identification
    public const string InstrumentId                    = "instrumentId";
    public const string DisplayName                     = "displayname";
    public const string InstrumentTypeId                = "instrumentTypeID";
    public const string InstrumentType                  = "instrumentType";
    public const string ExchangeId                      = "exchangeID";
    public const string Symbol                          = "symbol";

    // Internal platform metadata
    public const string InternalAssetClassId            = "internalAssetClassId";
    public const string InternalInstrumentDisplayName   = "internalInstrumentDisplayName";
    public const string IsInternalInstrument            = "isInternalInstrument";
    public const string InternalSymbolFull              = "internalSymbolFull";
    public const string IsHiddenFromClient              = "isHiddenFromClient";
    public const string InternalInstrumentId            = "internalInstrumentId";
    public const string InternalCryptoTypeId            = "internalCryptoTypeId";
    public const string InternalExchangeId              = "internalExchangeId";
    public const string InternalExchangeName            = "internalExchangeName";
    public const string InternalAssetClassName          = "internalAssetClassName";
    public const string InternalIndustryId              = "internalIndustryId";
    public const string InternalStockIndustryName       = "internalStockIndustryName";
    public const string InternalClosingPrice            = "internalClosingPrice";

    // Trading status
    public const string IsOpen                          = "isOpen";
    public const string IsDelisted                      = "isDelisted";
    public const string IsCurrentlyTradable             = "isCurrentlyTradable";
    public const string IsExchangeOpen                  = "isExchangeOpen";
    public const string IsActiveInPlatform              = "isActiveInPlatform";
    public const string IsBuyEnabled                    = "isBuyEnabled";

    // Price
    public const string CurrentRate                     = "currentRate";
    public const string CvtBid                         = "cvtBid";
    public const string CvtAsk                         = "cvtAsk";
    public const string CvtBidNoSpread                 = "cvtBiNoSpread";
    public const string CvtAskNoSpread                 = "cvtAskNoSpread";

    // Price changes
    public const string DailyPriceChange                = "dailyPriceChange";
    public const string AbsDailyPriceChange             = "absDailyPriceChange";
    public const string WeeklyPriceChange               = "weeklyPriceChange";
    public const string MonthlyPriceChange              = "monthlyPriceChange";
    public const string ThreeMonthPriceChange           = "threeMonthPriceChange";
    public const string SixMonthPriceChange             = "sixMonthPriceChange";
    public const string OneYearPriceChange              = "oneYearPriceChange";
    public const string CurrMonthPriceChange            = "currMonthPriceChange";
    public const string CurrQuarterPriceChange          = "currQuarterPriceChange";
    public const string CurrYearPriceChange             = "currYearPriceChange";
    public const string LastYearPriceChange             = "lastYearPriceChange";
    public const string LastTwoYearsPriceChange         = "lastTwoYearsPriceChange";
    public const string OneMonthAgoPriceChange          = "oneMonthAgoPriceChange";
    public const string TwoMonthsAgoPriceChange         = "twoMonthsAgoPriceChange";
    public const string ThreeMonthsAgoPriceChange       = "threeMonthsAgoPriceChange";
    public const string SixMonthsAgoPriceChange         = "sixMonthsAgoPriceChange";
    public const string OneYearAgoPriceChange           = "oneYearAgoPriceChange";

    // Popularity and traders
    public const string PopularityUniques               = "popularityUniques";
    public const string PopularityUniques7Day           = "popularityUniques7Day";
    public const string PopularityUniques14Day          = "popularityUniques14Day";
    public const string PopularityUniques30Day          = "popularityUniques30Day";
    public const string Traders7DayChange               = "traders7DayChange";
    public const string Traders14DayChange              = "traders14DayChange";
    public const string Traders30DayChange              = "traders30DayChange";

    // Holdings and positions
    public const string HoldingPct                      = "holdingPct";
    public const string BuyHoldingPct                  = "buyHoldingPct";
    public const string SellHoldingPct                  = "sellHoldingPct";
    public const string BuyPctChange24Hours             = "buyPctChange24Hours";
    public const string AbsBuyPctChange24Hours          = "absBuyPctChange24Hours";

    // Industry
    public const string IndustryNameId                  = "industryNameId";
    public const string SectorNameId                    = "sectorNameId";

    // Logos
    public const string Logo35x35                       = "logo35x35";
    public const string Logo50x50                       = "logo50x50";
    public const string Logo150x150                     = "logo150x150";
}
