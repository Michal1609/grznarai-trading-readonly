using GrznarAi.Trading.ReadOnly.Etoro.Models.Market;
using GrznarAi.Trading.ReadOnly.Etoro.Models.PiData;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

public interface IEToroMarketDataClient
{
    /// <summary>
    /// Retrieves a list of exchanges supported by the platform along with basic descriptive data.
    /// <br/>CZ: VrĂˇtĂ­ seznam burz podporovanĂ˝ch platformou spolu se zĂˇkladnĂ­mi popisnĂ˝mi daty.
    /// </summary>
    Task<ExchangesResponse> GetExchangesAsync(
        IEnumerable<int>? exchangeIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Fetch available instrument types (asset classes) such as stocks, ETFs, commodities, etc.
    /// <br/>CZ: VrĂˇtĂ­ dostupnĂ© typy nĂˇstrojĹŻ (tĹ™Ă­dy aktiv) jako jsou akcie, ETF, komodity apod.
    /// </summary>
    Task<InstrumentTypesResponse> GetInstrumentTypesAsync(
        IEnumerable<int>? instrumentTypeIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves metadata for specified instruments, including display names, exchange IDs, and classification.
    /// <br/>CZ: VrĂˇtĂ­ metadata pro zadanĂ© nĂˇstroje, vÄŤetnÄ› zobrazovanĂ˝ch nĂˇzvĹŻ, ID burz a klasifikace.
    /// </summary>
    Task<InstrumentMetadataResponse> GetInstrumentMetadataAsync(
        IEnumerable<int>? instrumentIds = null,
        IEnumerable<int>? exchangeIds = null,
        IEnumerable<int>? stocksIndustryIds = null,
        IEnumerable<int>? instrumentTypeIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get historical closing prices for all instruments at daily, weekly, and monthly intervals.
    /// <br/>CZ: VrĂˇtĂ­ historickĂ© zĂˇvÄ›reÄŤnĂ© ceny pro vĹˇechny nĂˇstroje v dennĂ­m, tĂ˝dennĂ­m a mÄ›sĂ­ÄŤnĂ­m intervalu.
    /// </summary>
    Task<IReadOnlyList<InstrumentClosingPrice>> GetHistoricalClosingPricesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets data on available stocks industries.
    /// <br/>CZ: VrĂˇtĂ­ data o dostupnĂ˝ch odvÄ›tvĂ­ch akciĂ­.
    /// </summary>
    Task<StocksIndustriesResponse> GetStocksIndustriesAsync(
        IEnumerable<int>? stocksIndustryIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Search for instruments by fields and optional text filter. PageSize is capped at 100.
    /// Note: sort and pageNumber parameters are not supported by the eToro API and have been removed.
    /// <br/>CZ: VyhledĂˇ nĂˇstroje podle polĂ­ a volitelnĂ©ho textovĂ©ho filtru. PageSize je omezen na 100.
    /// Parametry sort a pageNumber nejsou eToro API podporovĂˇny a byly odstranÄ›ny.
    /// </summary>
    Task<InstrumentSearchResponse> SearchInstrumentsAsync(
        InstrumentSearchRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieve current market rates and pricing information for specified instruments.
    /// <br/>CZ: VrĂˇtĂ­ aktuĂˇlnĂ­ trĹľnĂ­ kurzy a cenovĂ© informace pro zadanĂ© nĂˇstroje.
    /// </summary>
    Task<LiveRatesResponse> GetRatesAsync(
        IEnumerable<int> instrumentIds,
        CancellationToken ct = default);

    /// <summary>
    /// Get historical candles data for an instrument.
    /// <br/>CZ: VrĂˇtĂ­ historickĂˇ OHLCV svĂ­ÄŤkovĂˇ data pro danĂ˝ nĂˇstroj.
    /// </summary>
    Task<CandleResponse> GetCandlesAsync(
        int instrumentId,
        CandleInterval interval = CandleInterval.OneDay,
        CandleDirection direction = CandleDirection.Desc,
        int candlesCount = 100,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a list of users copying your portfolio, with demographic and financial info.
    /// <br/>CZ: VrĂˇtĂ­ seznam uĹľivatelĹŻ kopĂ­rujĂ­cĂ­ch vaĹˇe portfolio spolu s demografickĂ˝mi a finanÄŤnĂ­mi informacemi.
    /// </summary>
    Task<CopiersResponse> GetCopiersPublicInfoAsync(CancellationToken ct = default);
}
