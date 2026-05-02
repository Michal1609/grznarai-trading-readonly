using GrznarAi.Trading.ReadOnly.Models.Market;
using GrznarAi.Trading.ReadOnly.Models.PiData;

namespace GrznarAi.Trading.ReadOnly.Client;

public interface IEToroMarketDataClient
{
    /// <summary>
    /// Retrieves a list of exchanges supported by the platform along with basic descriptive data.
    /// <br/>CZ: Vrátí seznam burz podporovaných platformou spolu se základními popisnými daty.
    /// </summary>
    Task<ExchangesResponse> GetExchangesAsync(
        IEnumerable<int>? exchangeIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Fetch available instrument types (asset classes) such as stocks, ETFs, commodities, etc.
    /// <br/>CZ: Vrátí dostupné typy nástrojů (třídy aktiv) jako jsou akcie, ETF, komodity apod.
    /// </summary>
    Task<InstrumentTypesResponse> GetInstrumentTypesAsync(
        IEnumerable<int>? instrumentTypeIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves metadata for specified instruments, including display names, exchange IDs, and classification.
    /// <br/>CZ: Vrátí metadata pro zadané nástroje, včetně zobrazovaných názvů, ID burz a klasifikace.
    /// </summary>
    Task<InstrumentMetadataResponse> GetInstrumentMetadataAsync(
        IEnumerable<int>? instrumentIds = null,
        IEnumerable<int>? exchangeIds = null,
        IEnumerable<int>? stocksIndustryIds = null,
        IEnumerable<int>? instrumentTypeIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get historical closing prices for all instruments at daily, weekly, and monthly intervals.
    /// <br/>CZ: Vrátí historické závěrečné ceny pro všechny nástroje v denním, týdenním a měsíčním intervalu.
    /// </summary>
    Task<IReadOnlyList<InstrumentClosingPrice>> GetHistoricalClosingPricesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets data on available stocks industries.
    /// <br/>CZ: Vrátí data o dostupných odvětvích akcií.
    /// </summary>
    Task<StocksIndustriesResponse> GetStocksIndustriesAsync(
        IEnumerable<int>? stocksIndustryIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Search for instruments by fields, text and pagination.
    /// <br/>CZ: Vyhledá nástroje podle polí, textu a stránkování.
    /// </summary>
    Task<InstrumentSearchResponse> SearchInstrumentsAsync(
        InstrumentSearchRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieve current market rates and pricing information for specified instruments.
    /// <br/>CZ: Vrátí aktuální tržní kurzy a cenové informace pro zadané nástroje.
    /// </summary>
    Task<LiveRatesResponse> GetRatesAsync(
        IEnumerable<int> instrumentIds,
        CancellationToken ct = default);

    /// <summary>
    /// Get historical candles data for an instrument.
    /// <br/>CZ: Vrátí historická OHLCV svíčková data pro daný nástroj.
    /// </summary>
    Task<CandleResponse> GetCandlesAsync(
        int instrumentId,
        CandleInterval interval = CandleInterval.OneDay,
        CandleDirection direction = CandleDirection.Desc,
        int candlesCount = 100,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a list of users copying your portfolio, with demographic and financial info.
    /// <br/>CZ: Vrátí seznam uživatelů kopírujících vaše portfolio spolu s demografickými a finančními informacemi.
    /// </summary>
    Task<CopiersResponse> GetCopiersPublicInfoAsync(CancellationToken ct = default);
}
