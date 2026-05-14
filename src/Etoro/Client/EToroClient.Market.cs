using GrznarAi.Trading.ReadOnly.Etoro.Models.Market;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

public sealed partial class EToroClient
{
    public async Task<ExchangesResponse> GetExchangesAsync(
        IEnumerable<int>? exchangeIds = null,
        CancellationToken ct = default)
    {
        var url = "market-data/exchanges" + new QueryStringBuilder().AddCsv("exchangeIds", exchangeIds);
        return await GetFromJsonAsync<ExchangesResponse>(url, "Empty exchanges response.", ct).ConfigureAwait(false);
    }

    public async Task<InstrumentTypesResponse> GetInstrumentTypesAsync(
        IEnumerable<int>? instrumentTypeIds = null,
        CancellationToken ct = default)
    {
        var url = "market-data/instrument-types" + new QueryStringBuilder().AddCsv("instrumentTypeIds", instrumentTypeIds);
        return await GetFromJsonAsync<InstrumentTypesResponse>(url, "Empty instrument types response.", ct).ConfigureAwait(false);
    }

    public async Task<InstrumentMetadataResponse> GetInstrumentMetadataAsync(
        IEnumerable<int>? instrumentIds = null,
        IEnumerable<int>? exchangeIds = null,
        IEnumerable<int>? stocksIndustryIds = null,
        IEnumerable<int>? instrumentTypeIds = null,
        CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .AddCsv("instrumentIds", instrumentIds)
            .AddCsv("exchangeIds", exchangeIds)
            .AddCsv("stocksIndustryIds", stocksIndustryIds)
            .AddCsv("instrumentTypeIds", instrumentTypeIds);

        return await GetFromJsonAsync<InstrumentMetadataResponse>(
            $"market-data/instruments{qs}",
            "Empty instrument metadata response.",
            ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<InstrumentClosingPrice>> GetHistoricalClosingPricesAsync(
        CancellationToken ct = default)
    {
        return await GetFromJsonAsync<IReadOnlyList<InstrumentClosingPrice>>(
            "market-data/instruments/history/closing-price",
            "Empty closing prices response.",
            ct).ConfigureAwait(false);
    }

    public async Task<StocksIndustriesResponse> GetStocksIndustriesAsync(
        IEnumerable<int>? stocksIndustryIds = null,
        CancellationToken ct = default)
    {
        var url = "market-data/stocks-industries" + new QueryStringBuilder().AddCsv("stocksIndustryIds", stocksIndustryIds);
        return await GetFromJsonAsync<StocksIndustriesResponse>(url, "Empty stocks industries response.", ct).ConfigureAwait(false);
    }

    public async Task<InstrumentSearchResponse> SearchInstrumentsAsync(
        InstrumentSearchRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var fieldList = request.Fields.ToList();
        if (fieldList.Count == 0)
            throw new ArgumentException("At least one field is required.", nameof(request));
        if (fieldList.Count > 5)
            throw new ArgumentOutOfRangeException(nameof(request), "API allows max 5 fields per request.");

        var unknownFields = fieldList.Where(f => !InstrumentFields.All.Contains(f)).ToList();
        if (unknownFields.Count > 0)
            throw new ArgumentException($"Unknown field(s): {string.Join(", ", unknownFields)}. Use constants from InstrumentFields.", nameof(request));

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(request.PageSize, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(request.PageSize, EToroRequestLimits.MaxSearchPageSize);
        EToroInputValidator.ValidateOptionalString(request.SearchText, nameof(request.SearchText), EToroRequestLimits.MaxSearchTextLength);
        EToroInputValidator.ValidateOptionalString(request.InternalSymbolFull, nameof(request.InternalSymbolFull), EToroRequestLimits.MaxSearchTextLength);

        var qs = new QueryStringBuilder()
            .AddCsv("fields", fieldList)
            .Add("internalSymbolFull", request.InternalSymbolFull)
            .Add("searchText", request.SearchText)
            .Add("pageSize", request.PageSize);

        return await GetFromJsonAsync<InstrumentSearchResponse>(
            $"market-data/search{qs}",
            "Empty instruments response.",
            ct).ConfigureAwait(false);
    }

    public async Task<LiveRatesResponse> GetRatesAsync(
        IEnumerable<int> instrumentIds, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(instrumentIds);
        var idList = instrumentIds.ToList();
        if (idList.Count == 0)
            throw new ArgumentException("At least one instrument id is required.", nameof(instrumentIds));
        if (idList.Count > EToroRequestLimits.MaxCsvIds)
            throw new ArgumentException($"Too many instrument ids; max {EToroRequestLimits.MaxCsvIds} per request.", nameof(instrumentIds));

        var qs = new QueryStringBuilder().AddCsv("instrumentIds", idList);
        return await GetFromJsonAsync<LiveRatesResponse>(
            $"market-data/instruments/rates{qs}",
            "Empty rates response.",
            ct).ConfigureAwait(false);
    }

    public async Task<CandleResponse> GetCandlesAsync(
        int instrumentId,
        CandleInterval interval = CandleInterval.OneDay,
        CandleDirection direction = CandleDirection.Desc,
        int candlesCount = 100,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(instrumentId);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(candlesCount, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(candlesCount, EToroRequestLimits.MaxCandlesCount);

        var dir = direction == CandleDirection.Asc ? "asc" : "desc";
        return await GetFromJsonAsync<CandleResponse>(
            $"market-data/instruments/{instrumentId}/history/candles/{dir}/{interval}/{candlesCount}",
            "Empty candles response.",
            ct).ConfigureAwait(false);
    }
}
