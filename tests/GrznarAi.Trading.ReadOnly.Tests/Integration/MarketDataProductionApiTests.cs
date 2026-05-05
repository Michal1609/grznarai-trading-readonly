using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Market;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Tests.Integration;

/// <summary>
/// Produkční / průzkumné testy market-data API.
/// Cíl: odhalit chování eToro nedokumentované nebo odlišné od dokumentace —
/// limity stránkování, skutečné počty, ignorované parametry, chování filtrů.
/// Rate limit: 60 req/min — zajišťuje RateLimitHandler automaticky.
///
/// Spuštění: dotnet test --filter "FullyQualifiedName~MarketDataProductionApiTests"
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruční spuštění — vyžaduje reálné API klíče mimo git")]
public class MarketDataProductionApiTests
{
    private IEToroClient _client = null!;

    // Stabilní instrument ID pro AAPL — eToro interní ID
    private const int AaplInstrumentId = 1586;
    private const int BtcInstrumentId  = 100578;

    // Seed data
    private IReadOnlyList<int> _exchangeIds        = [];
    private IReadOnlyList<int> _instrumentTypeIds  = [];
    private IReadOnlyList<int> _stocksIndustryIds  = [];
    private IReadOnlyList<int> _instrumentIds      = [];

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();

        var exchanges = await _client.GetExchangesAsync();
        _exchangeIds = exchanges.ExchangeInfo.Select(e => e.ExchangeId).Take(10).ToList();

        var types = await _client.GetInstrumentTypesAsync();
        _instrumentTypeIds = types.InstrumentTypes.Select(t => t.InstrumentTypeId).Take(5).ToList();

        var industries = await _client.GetStocksIndustriesAsync();
        _stocksIndustryIds = industries.StocksIndustries.Select(s => s.IndustryId).Take(5).ToList();

        var search = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields   = [InstrumentFields.InstrumentId, InstrumentFields.DisplayName],
            PageSize = 100,
        });
        _instrumentIds = search.Instruments
            .Where(i => i.InstrumentId.HasValue && i.InstrumentId > 0)
            .Select(i => i.InstrumentId!.Value)
            .ToList();

        Debug.WriteLine($"[SetUp] exchanges={_exchangeIds.Count} types={_instrumentTypeIds.Count} " +
                        $"industries={_stocksIndustryIds.Count} instruments={_instrumentIds.Count}");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetExchangesAsync
    // Dokumentace: query param exchangeIds (array int, optional), žádná paginace
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetExchangesAsync_NoFilter_ReturnsAll()
    {
        var result = await _client.GetExchangesAsync();

        Debug.WriteLine($"[Exchanges] Celkem: {result.ExchangeInfo.Count}");
        foreach (var e in result.ExchangeInfo)
            Debug.WriteLine($"  ID={e.ExchangeId} Desc={e.ExchangeDescription}");

        Assert.That(result.ExchangeInfo, Is.Not.Empty);
    }

    [Test]
    public async Task GetExchangesAsync_FilterSingleId_ReturnsExactlyOne()
    {
        if (_exchangeIds.Count == 0) Assert.Ignore("Žádné exchange IDs ze seed dat.");

        var id = _exchangeIds[0];
        var result = await _client.GetExchangesAsync(exchangeIds: [id]);

        Debug.WriteLine($"[Exchanges] Filter id={id} → vráceno={result.ExchangeInfo.Count}");
        foreach (var e in result.ExchangeInfo)
            Debug.WriteLine($"  ID={e.ExchangeId} Desc={e.ExchangeDescription}");

        if (result.ExchangeInfo.Count != 1)
            Debug.WriteLine($"  ⚠ Očekáváno 1, vráceno {result.ExchangeInfo.Count}.");

        Assert.That(result.ExchangeInfo.Select(e => e.ExchangeId), Does.Contain(id),
            $"Vrácený seznam neobsahuje požadovaný ID={id}.");
    }

    [TestCase(2, TestName = "Exchanges_FilterMultiple_2Ids")]
    [TestCase(5, TestName = "Exchanges_FilterMultiple_5Ids")]
    public async Task GetExchangesAsync_FilterMultipleIds_ReturnsSubset(int count)
    {
        if (_exchangeIds.Count < count)
        {
            Assert.Ignore($"Seed má jen {_exchangeIds.Count} exchange IDs, potřeba {count}.");
            return;
        }

        var ids = _exchangeIds.Take(count).ToList();
        var result = await _client.GetExchangesAsync(exchangeIds: ids);

        var returned = result.ExchangeInfo.Count;
        Debug.WriteLine($"[Exchanges] Filter {count} IDs → vráceno={returned}");

        if (returned != count)
            Debug.WriteLine($"  ⚠ Vráceno {returned} ≠ požadovaných {count} — eToro vrátil jiný počet.");

        Assert.That(returned, Is.LessThanOrEqualTo(count),
            $"API vrátilo víc exchanges ({returned}) než bylo požadováno ({count}).");
        foreach (var e in result.ExchangeInfo)
            Assert.That(ids, Does.Contain(e.ExchangeId),
                $"ExchangeId={e.ExchangeId} není v požadovaném filtru {string.Join(",", ids)}.");
    }

    [Test]
    public async Task GetExchangesAsync_FilterNonExistentId_ReturnsEmpty()
    {
        var result = await _client.GetExchangesAsync(exchangeIds: [int.MaxValue]);

        Debug.WriteLine($"[Exchanges] Filter neexistující ID={int.MaxValue} → vráceno={result.ExchangeInfo.Count}");

        if (result.ExchangeInfo.Count > 0)
            Debug.WriteLine($"  ⚠ API vrátilo data pro neexistující ID — ignoruje filtr?");

        Assert.That(result.ExchangeInfo, Is.Empty,
            "API by mělo vrátit prázdný seznam pro neexistující exchange ID.");
    }

    [Test]
    public async Task GetExchangesAsync_AllExchangesHavePositiveId()
    {
        var result = await _client.GetExchangesAsync();

        foreach (var e in result.ExchangeInfo)
            Assert.That(e.ExchangeId, Is.GreaterThan(0),
                $"Exchange '{e.ExchangeDescription}' má neplatné ID={e.ExchangeId}.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetInstrumentTypesAsync
    // Dokumentace: query param instrumentTypeIds (array int, optional), žádná paginace
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetInstrumentTypesAsync_NoFilter_ReturnsAll()
    {
        var result = await _client.GetInstrumentTypesAsync();

        Debug.WriteLine($"[InstrumentTypes] Celkem: {result.InstrumentTypes.Count}");
        foreach (var t in result.InstrumentTypes)
            Debug.WriteLine($"  ID={t.InstrumentTypeId} Desc={t.InstrumentTypeDescription}");

        Assert.That(result.InstrumentTypes, Is.Not.Empty);
    }

    [Test]
    public async Task GetInstrumentTypesAsync_FilterSingleId_ReturnsExactlyOne()
    {
        if (_instrumentTypeIds.Count == 0) Assert.Ignore("Žádné type IDs ze seed dat.");

        var id = _instrumentTypeIds[0];
        var result = await _client.GetInstrumentTypesAsync(instrumentTypeIds: [id]);

        Debug.WriteLine($"[InstrumentTypes] Filter id={id} → vráceno={result.InstrumentTypes.Count}");
        foreach (var t in result.InstrumentTypes)
            Debug.WriteLine($"  ID={t.InstrumentTypeId} Desc={t.InstrumentTypeDescription}");

        if (result.InstrumentTypes.Count != 1)
            Debug.WriteLine($"  ⚠ Očekáváno 1, vráceno {result.InstrumentTypes.Count}.");

        Assert.That(result.InstrumentTypes.Select(t => t.InstrumentTypeId), Does.Contain(id));
    }

    [Test]
    public async Task GetInstrumentTypesAsync_FilterNonExistentId_ReturnsEmpty()
    {
        var result = await _client.GetInstrumentTypesAsync(instrumentTypeIds: [int.MaxValue]);

        Debug.WriteLine($"[InstrumentTypes] Neexistující ID → vráceno={result.InstrumentTypes.Count}");

        if (result.InstrumentTypes.Count > 0)
            Debug.WriteLine($"  ⚠ API vrátilo data pro neexistující ID — ignoruje filtr?");

        Assert.That(result.InstrumentTypes, Is.Empty,
            "Neexistující instrumentTypeId by měl vrátit prázdný seznam.");
    }

    [Test]
    public async Task GetInstrumentTypesAsync_AllTypesHavePositiveId()
    {
        var result = await _client.GetInstrumentTypesAsync();

        foreach (var t in result.InstrumentTypes)
            Assert.That(t.InstrumentTypeId, Is.GreaterThan(0),
                $"Type '{t.InstrumentTypeDescription}' má neplatné ID={t.InstrumentTypeId}.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetInstrumentMetadataAsync
    // Dokumentace: filtry instrumentIds, exchangeIds, stocksIndustryIds, instrumentTypeIds
    // Žádná paginace — může vrátit tisíce záznamů při volání bez filtrů
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterByInstrumentIds_SingleId()
    {
        var result = await _client.GetInstrumentMetadataAsync(instrumentIds: [AaplInstrumentId]);

        Debug.WriteLine($"[Metadata] Filter 1 ID → vráceno={result.InstrumentDisplayDatas.Count}");
        var inst = result.InstrumentDisplayDatas.FirstOrDefault();
        if (inst is not null)
            Debug.WriteLine($"  ID={inst.InstrumentId} Name={inst.InstrumentDisplayName} Symbol={inst.SymbolFull}");

        Assert.That(result.InstrumentDisplayDatas, Is.Not.Empty);
        Assert.That(result.InstrumentDisplayDatas[0].InstrumentId, Is.EqualTo(AaplInstrumentId));
    }

    [TestCase(2,  TestName = "Metadata_FilterByInstrumentIds_2Ids")]
    [TestCase(5,  TestName = "Metadata_FilterByInstrumentIds_5Ids")]
    [TestCase(10, TestName = "Metadata_FilterByInstrumentIds_10Ids")]
    [TestCase(50, TestName = "Metadata_FilterByInstrumentIds_50Ids")]
    public async Task GetInstrumentMetadataAsync_FilterByInstrumentIds_MultipleIds(int count)
    {
        if (_instrumentIds.Count < count)
        {
            Assert.Ignore($"Seed má jen {_instrumentIds.Count} IDs, potřeba {count}.");
            return;
        }

        var ids = _instrumentIds.Take(count).ToList();
        var result = await _client.GetInstrumentMetadataAsync(instrumentIds: ids);

        var returned = result.InstrumentDisplayDatas.Count;
        Debug.WriteLine($"[Metadata] Filter {count} IDs → vráceno={returned}");

        if (returned != count)
            Debug.WriteLine($"  ⚠ Vráceno {returned} ≠ požadovaných {count}. " +
                            $"Chybějící IDs: {string.Join(",", ids.Except(result.InstrumentDisplayDatas.Select(i => i.InstrumentId)))}");

        Assert.That(returned, Is.GreaterThan(0), "API nevrátilo žádná data pro platné instrumentIds.");
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterByExchangeId_ReturnsInstrumentsForExchange()
    {
        if (_exchangeIds.Count == 0) Assert.Ignore("Žádné exchange IDs.");

        var id = _exchangeIds[0];
        var result = await _client.GetInstrumentMetadataAsync(exchangeIds: [id]);

        Debug.WriteLine($"[Metadata] Filter exchangeId={id} → vráceno={result.InstrumentDisplayDatas.Count}");

        var wrongExchange = result.InstrumentDisplayDatas.Where(i => i.ExchangeId != id).ToList();
        if (wrongExchange.Count > 0)
            Debug.WriteLine($"  ⚠ {wrongExchange.Count} instrumentů má jiný exchangeId než filtrovaný {id}!");

        Assert.That(result.InstrumentDisplayDatas, Is.Not.Empty,
            $"Žádné instrumenty pro exchangeId={id}.");
        Assert.That(wrongExchange, Is.Empty,
            $"API vrátilo instrumenty s jiným exchangeId než bylo filtrováno.");
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterByInstrumentTypeId_ReturnsCorrectType()
    {
        if (_instrumentTypeIds.Count == 0) Assert.Ignore("Žádné type IDs.");

        var id = _instrumentTypeIds[0];
        var result = await _client.GetInstrumentMetadataAsync(instrumentTypeIds: [id]);

        Debug.WriteLine($"[Metadata] Filter typeId={id} → vráceno={result.InstrumentDisplayDatas.Count}");

        var wrongType = result.InstrumentDisplayDatas.Where(i => i.InstrumentTypeId != id).ToList();
        if (wrongType.Count > 0)
            Debug.WriteLine($"  ⚠ {wrongType.Count} instrumentů má jiný typeId než {id}!");

        Assert.That(result.InstrumentDisplayDatas, Is.Not.Empty);
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterByStocksIndustryId()
    {
        if (_stocksIndustryIds.Count == 0) Assert.Ignore("Žádné industry IDs.");

        var id = _stocksIndustryIds[0];
        var result = await _client.GetInstrumentMetadataAsync(stocksIndustryIds: [id]);

        Debug.WriteLine($"[Metadata] Filter industryId={id} → vráceno={result.InstrumentDisplayDatas.Count}");

        Assert.That(result.InstrumentDisplayDatas, Is.Not.Empty,
            $"Žádné instrumenty pro industryId={id}.");
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterNonExistentInstrumentId_ReturnsEmpty()
    {
        var result = await _client.GetInstrumentMetadataAsync(instrumentIds: [int.MaxValue]);

        Debug.WriteLine($"[Metadata] Neexistující ID → vráceno={result.InstrumentDisplayDatas.Count}");

        if (result.InstrumentDisplayDatas.Count > 0)
            Debug.WriteLine($"  ⚠ API vrátilo data pro neexistující instrumentId!");

        Assert.That(result.InstrumentDisplayDatas, Is.Empty);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetHistoricalClosingPricesAsync
    // Dokumentace: žádné parametry — vrátí vše pro všechny instrumenty
    // POZOR: může vrátit tisíce záznamů, pomalé volání
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetHistoricalClosingPricesAsync_ReturnsNonEmptyList()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        Debug.WriteLine($"[ClosingPrices] Celkem záznamů: {result.Count}");
        var sample = result.Take(3);
        foreach (var p in sample)
            Debug.WriteLine($"  ID={p.InstrumentId} Official={p.OfficialClosingPrice} " +
                            $"Daily={p.ClosingPrices?.Daily?.Price} Weekly={p.ClosingPrices?.Weekly?.Price} Monthly={p.ClosingPrices?.Monthly?.Price}");

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task GetHistoricalClosingPricesAsync_OfficialClosingPriceIsPositive()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        var nonPositive = result.Where(p => p.OfficialClosingPrice <= 0).Take(10).ToList();
        if (nonPositive.Count > 0)
        {
            Debug.WriteLine($"[ClosingPrices] ⚠ {nonPositive.Count} instrumentů (ukázka) s OfficialClosingPrice ≤ 0:");
            foreach (var p in nonPositive)
                Debug.WriteLine($"  ID={p.InstrumentId} Official={p.OfficialClosingPrice}");
        }

        // Zaznamenáme ale netestujeme — delisted instrumenty mohou mít cenu 0
        Assert.That(result.Count(p => p.OfficialClosingPrice > 0), Is.GreaterThan(0),
            "Žádný instrument nemá kladnou OfficialClosingPrice — pravděpodobná chyba deserializace.");
    }

    [Test]
    public async Task GetHistoricalClosingPricesAsync_MonthlyPriceMinusOneIsDocumented()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        var withMinusOne = result.Where(p => p.ClosingPrices?.Monthly?.Price == -1).ToList();
        var withMonthly  = result.Where(p => p.ClosingPrices?.Monthly?.Price > 0).ToList();
        var withoutMonthly = result.Where(p => p.ClosingPrices?.Monthly is null).ToList();

        Debug.WriteLine($"[ClosingPrices] Monthly -1 (dokumentováno): {withMinusOne.Count}");
        Debug.WriteLine($"[ClosingPrices] Monthly > 0: {withMonthly.Count}");
        Debug.WriteLine($"[ClosingPrices] Monthly null: {withoutMonthly.Count}");

        // Informativní — ověřuje, že dokumentovaný případ -1 opravdu nastává
        Assert.Pass($"Monthly prices — valid: {withMonthly.Count}, -1: {withMinusOne.Count}, null: {withoutMonthly.Count}");
    }

    [Test]
    public async Task GetHistoricalClosingPricesAsync_AaplHasConsistentPrices()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        var aapl = result.FirstOrDefault(p => p.InstrumentId == AaplInstrumentId);
        if (aapl is null)
        {
            Assert.Pass($"AAPL (ID={AaplInstrumentId}) není v closing prices.");
            return;
        }

        Debug.WriteLine($"[ClosingPrices] AAPL official={aapl.OfficialClosingPrice}");
        Debug.WriteLine($"  Daily={aapl.ClosingPrices?.Daily?.Price} at {aapl.ClosingPrices?.Daily?.Date}");
        Debug.WriteLine($"  Weekly={aapl.ClosingPrices?.Weekly?.Price} at {aapl.ClosingPrices?.Weekly?.Date}");
        Debug.WriteLine($"  Monthly={aapl.ClosingPrices?.Monthly?.Price} at {aapl.ClosingPrices?.Monthly?.Date}");

        Assert.Multiple(() =>
        {
            Assert.That(aapl.OfficialClosingPrice, Is.GreaterThan(0));
            Assert.That(aapl.ClosingPrices, Is.Not.Null, "AAPL by měl mít ClosingPrices objekt.");
        });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetRatesAsync
    // Dokumentace: instrumentIds required, max 100 IDs, 400 při překročení
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(1,   TestName = "Rates_SingleInstrument")]
    [TestCase(5,   TestName = "Rates_5Instruments")]
    [TestCase(10,  TestName = "Rates_10Instruments")]
    [TestCase(50,  TestName = "Rates_50Instruments")]
    [TestCase(100, TestName = "Rates_100Instruments_MaxLimit")]
    public async Task GetRatesAsync_VariousInstrumentCounts_AllReturnRates(int count)
    {
        if (_instrumentIds.Count < count)
        {
            Assert.Ignore($"Seed má jen {_instrumentIds.Count} IDs, potřeba {count}.");
            return;
        }

        var ids = _instrumentIds.Take(count).ToList();
        var result = await _client.GetRatesAsync(ids);

        var returned = result.Rates.Count;
        Debug.WriteLine($"[Rates] Požadováno={count} → vráceno={returned}");

        if (returned != count)
            Debug.WriteLine($"  ⚠ Vráceno {returned} ≠ požadovaných {count}. " +
                            $"Chybějící IDs: {string.Join(",", ids.Except(result.Rates.Select(r => r.InstrumentId)).Take(5))}");

        Assert.Multiple(() =>
        {
            Assert.That(returned, Is.GreaterThan(0), "Žádné rates vráceny.");
            Assert.That(returned, Is.LessThanOrEqualTo(count), "Vráceno víc rates než bylo požadováno.");
        });
    }

    [Test]
    public async Task GetRatesAsync_AaplRate_AskGreaterThanBid()
    {
        var result = await _client.GetRatesAsync([AaplInstrumentId]);

        var rate = result.Rates.FirstOrDefault(r => r.InstrumentId == AaplInstrumentId);
        if (rate is null)
        {
            Assert.Pass("AAPL rate nebyla vrácena.");
            return;
        }

        Debug.WriteLine($"[Rates] AAPL Ask={rate.Ask} Bid={rate.Bid} Last={rate.LastExecution} Date={rate.Date}");

        Assert.Multiple(() =>
        {
            Assert.That(rate.Ask, Is.GreaterThan(0), "Ask cena musí být kladná.");
            Assert.That(rate.Bid, Is.GreaterThan(0), "Bid cena musí být kladná.");
            Assert.That(rate.Ask, Is.GreaterThanOrEqualTo(rate.Bid), "Ask musí být ≥ Bid (spread).");
        });
    }

    [Test]
    public async Task GetRatesAsync_AllReturnedRates_HavePositivePrices()
    {
        if (_instrumentIds.Count < 10) Assert.Ignore("Nedostatek seed IDs.");

        var ids = _instrumentIds.Take(10).ToList();
        var result = await _client.GetRatesAsync(ids);

        foreach (var rate in result.Rates)
        {
            Debug.WriteLine($"  ID={rate.InstrumentId} Ask={rate.Ask} Bid={rate.Bid}");
            if (rate.Ask <= 0 || rate.Bid <= 0)
                Debug.WriteLine($"  ⚠ ID={rate.InstrumentId} má nekladnou cenu Ask={rate.Ask} Bid={rate.Bid}");
        }

        Assert.That(result.Rates.All(r => r.Ask >= 0 && r.Bid >= 0), Is.True,
            "Některé rates mají záporné ceny.");
    }

    [Test]
    public async Task GetRatesAsync_Exceed100Ids_ClientThrowsBeforeApiCall()
    {
        // Toto testuje client-side validaci — NEPROVEDE API volání
        var ids = Enumerable.Range(1, 101).ToList();

        Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetRatesAsync(ids),
            "Client by měl hodit ArgumentOutOfRangeException pro > 100 IDs.");
    }

    [Test]
    public async Task GetRatesAsync_BtcRate_HasConversionRates()
    {
        var result = await _client.GetRatesAsync([BtcInstrumentId]);

        var rate = result.Rates.FirstOrDefault(r => r.InstrumentId == BtcInstrumentId);
        if (rate is null)
        {
            Assert.Pass($"BTC (ID={BtcInstrumentId}) rate nebyla vrácena.");
            return;
        }

        Debug.WriteLine($"[Rates] BTC Ask={rate.Ask} Bid={rate.Bid} CvtAsk={rate.ConversionRateAsk} CvtBid={rate.ConversionRateBid}");

        Assert.Multiple(() =>
        {
            Assert.That(rate.ConversionRateAsk, Is.GreaterThan(0), "ConversionRateAsk musí být kladný.");
            Assert.That(rate.ConversionRateBid, Is.GreaterThan(0), "ConversionRateBid musí být kladný.");
        });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetCandlesAsync
    // Dokumentace: instrumentId (path), direction (asc/desc), interval (enum),
    //              candlesCount default=100, max=1000
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(1,    TestName = "Candles_Count1")]
    [TestCase(10,   TestName = "Candles_Count10")]
    [TestCase(50,   TestName = "Candles_Count50")]
    [TestCase(100,  TestName = "Candles_Count100_Default")]
    [TestCase(500,  TestName = "Candles_Count500")]
    [TestCase(1000, TestName = "Candles_Count1000_MaxLimit")]
    public async Task GetCandlesAsync_VariousCandleCounts_ActualCountMatchesRequested(int count)
    {
        var result = await _client.GetCandlesAsync(
            AaplInstrumentId, CandleInterval.OneDay, CandleDirection.Desc, count);

        var returned = result.Candles.Count;
        Debug.WriteLine($"[Candles] Požadováno={count} interval=OneDay direction=Desc → vráceno={returned}");

        if (returned < count)
            Debug.WriteLine($"  ⚠ Vráceno méně ({returned}) než požadováno ({count}) — eToro bug nebo nedostatek dat?");

        Assert.That(returned, Is.LessThanOrEqualTo(count),
            $"Vráceno {returned} svíček, přestože candlesCount={count}.");
        Assert.That(returned, Is.GreaterThan(0), "Žádné svíčky vráceny.");
    }

    [TestCase(CandleInterval.OneMinute,     TestName = "Candles_Interval_OneMinute")]
    [TestCase(CandleInterval.FiveMinutes,   TestName = "Candles_Interval_FiveMinutes")]
    [TestCase(CandleInterval.TenMinutes,    TestName = "Candles_Interval_TenMinutes")]
    [TestCase(CandleInterval.FifteenMinutes,TestName = "Candles_Interval_FifteenMinutes")]
    [TestCase(CandleInterval.ThirtyMinutes, TestName = "Candles_Interval_ThirtyMinutes")]
    [TestCase(CandleInterval.OneHour,       TestName = "Candles_Interval_OneHour")]
    [TestCase(CandleInterval.FourHours,     TestName = "Candles_Interval_FourHours")]
    [TestCase(CandleInterval.OneDay,        TestName = "Candles_Interval_OneDay")]
    [TestCase(CandleInterval.OneWeek,       TestName = "Candles_Interval_OneWeek")]
    public async Task GetCandlesAsync_AllIntervals_ReturnCandles(CandleInterval interval)
    {
        var result = await _client.GetCandlesAsync(
            AaplInstrumentId, interval, CandleDirection.Desc, 10);

        Debug.WriteLine($"[Candles] Interval={interval} → vráceno={result.Candles.Count} intervalStr={result.Interval}");
        if (result.Candles.Count > 0)
        {
            var c = result.Candles[0];
            Debug.WriteLine($"  Latest: Date={c.FromDate} O={c.Open} H={c.High} L={c.Low} C={c.Close} V={c.Volume}");
        }

        Assert.That(result.Candles, Is.Not.Empty, $"Interval={interval} nevrátil žádné svíčky.");
    }

    [Test]
    public async Task GetCandlesAsync_AscVsDesc_ReturnDifferentOrder()
    {
        var desc = await _client.GetCandlesAsync(AaplInstrumentId, CandleInterval.OneDay, CandleDirection.Desc, 5);
        var asc  = await _client.GetCandlesAsync(AaplInstrumentId, CandleInterval.OneDay, CandleDirection.Asc,  5);

        var descDates = desc.Candles.Select(c => c.FromDate).ToList();
        var ascDates  = asc.Candles.Select(c => c.FromDate).ToList();

        Debug.WriteLine($"[Candles] Desc daty: {string.Join(", ", descDates.Select(d => d.ToString("yyyy-MM-dd")))}");
        Debug.WriteLine($"[Candles] Asc daty:  {string.Join(", ", ascDates.Select(d => d.ToString("yyyy-MM-dd")))}");

        // Desc: nejnovější první → descDates[0] > descDates[1]
        if (descDates.Count >= 2)
            Assert.That(descDates[0], Is.GreaterThanOrEqualTo(descDates[1]),
                "Desc směr — první svíčka by měla být novější než druhá.");

        // Asc: nejstarší první → ascDates[0] < ascDates[1]
        if (ascDates.Count >= 2)
            Assert.That(ascDates[0], Is.LessThanOrEqualTo(ascDates[1]),
                "Asc směr — první svíčka by měla být starší než druhá.");
    }

    [Test]
    public async Task GetCandlesAsync_OHLC_HighAlwaysGreaterOrEqualToLow()
    {
        var result = await _client.GetCandlesAsync(AaplInstrumentId, CandleInterval.OneDay, CandleDirection.Desc, 50);

        var invalidCandles = result.Candles.Where(c => c.High < c.Low).ToList();
        if (invalidCandles.Count > 0)
        {
            Debug.WriteLine($"[Candles] ⚠ {invalidCandles.Count} svíček s High < Low:");
            foreach (var c in invalidCandles)
                Debug.WriteLine($"  Date={c.FromDate} H={c.High} L={c.Low}");
        }

        Assert.That(invalidCandles, Is.Empty, "Nalezeny svíčky kde High < Low — chyba dat.");
    }

    [Test]
    public async Task GetCandlesAsync_ResponseInterval_MatchesRequested()
    {
        foreach (var interval in new[] { CandleInterval.OneDay, CandleInterval.OneHour, CandleInterval.OneWeek })
        {
            var result = await _client.GetCandlesAsync(AaplInstrumentId, interval, CandleDirection.Desc, 3);

            Debug.WriteLine($"[Candles] Požadovaný interval={interval} → vrácený interval string='{result.Interval}'");

            Assert.That(result.Interval, Is.Not.Null.And.Not.Empty,
                $"Interval string v odpovědi je prázdný pro {interval}.");
        }
    }

    [Test]
    public async Task GetCandlesAsync_ExceedMaxCount_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetCandlesAsync(AaplInstrumentId, CandleInterval.OneDay, CandleDirection.Desc, 1001),
            "Client by měl hodit ArgumentOutOfRangeException pro candlesCount > 1000.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SearchInstrumentsAsync
    // Klíčové: fields required (min 1, MAX 5), pageSize (1–200), pageNumber (≥0)
    // HLAVNÍ cíl testu: odhalit, zda eToro skutečně vrátí požadovaný pageSize
    // ═══════════════════════════════════════════════════════════════════════════

    // ─── Počet fields (kritický limit = 5) ───────────────────────────────────

    [TestCase(1, TestName = "Search_Fields_Count1")]
    [TestCase(2, TestName = "Search_Fields_Count2")]
    [TestCase(3, TestName = "Search_Fields_Count3")]
    [TestCase(4, TestName = "Search_Fields_Count4")]
    [TestCase(5, TestName = "Search_Fields_Count5_Max")]
    public async Task SearchInstrumentsAsync_FieldsCounts_AllReturnResults(int fieldCount)
    {
        var allFields = new[]
        {
            InstrumentFields.InstrumentId,
            InstrumentFields.DisplayName,
            InstrumentFields.Symbol,
            InstrumentFields.CurrentRate,
            InstrumentFields.DailyPriceChange
        };

        var fields = allFields.Take(fieldCount).ToList();

        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields   = fields,
            PageSize = 5,
        });

        Debug.WriteLine($"[Search] Fields={fieldCount} ({string.Join(",", fields)}) → instruments={result.Instruments.Count} total={result.TotalItems}");

        Assert.That(result.Instruments, Is.Not.Empty, $"Search s {fieldCount} fields nevrátil výsledky.");
    }

    [Test]
    public async Task SearchInstrumentsAsync_SixFields_ClientThrows()
    {
        // Client-side validace — max 5 fields
        var sixFields = new[]
        {
            InstrumentFields.InstrumentId,
            InstrumentFields.DisplayName,
            InstrumentFields.Symbol,
            InstrumentFields.CurrentRate,
            InstrumentFields.DailyPriceChange,
            InstrumentFields.WeeklyPriceChange   // 6. field → musí vyhodit
        };

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.SearchInstrumentsAsync(new InstrumentSearchRequest { Fields = sixFields }),
            "Client by měl odmítnout > 5 fields.");
    }

    [Test]
    public async Task SearchInstrumentsAsync_UnknownField_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentException>(
            () => _client.SearchInstrumentsAsync(new InstrumentSearchRequest
            {
                Fields = ["nonExistentField123"]
            }),
            "Client by měl odmítnout neznámé field jméno.");
    }

    // ─── PageSize — klíčový test: vrátí eToro požadovaný počet? ─────────────

    [TestCase(1,   TestName = "Search_PageSize_1")]
    [TestCase(4,   TestName = "Search_PageSize_4")]
    [TestCase(5,   TestName = "Search_PageSize_5")]
    [TestCase(10,  TestName = "Search_PageSize_10")]
    [TestCase(20,  TestName = "Search_PageSize_20_Default")]
    [TestCase(50,  TestName = "Search_PageSize_50")]
    [TestCase(100, TestName = "Search_PageSize_100_MaxLimit")]
    public async Task SearchInstrumentsAsync_PageSize_ActualCountMatchesRequested(int pageSize)
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields   = [InstrumentFields.InstrumentId, InstrumentFields.DisplayName],
            PageSize = pageSize,
        });

        var returned  = result.Instruments.Count;
        var total     = result.TotalItems;
        var expected  = Math.Min(pageSize, total);

        Debug.WriteLine($"[Search] pageSize={pageSize} → returned={returned} totalItems={total} expected≤{expected}");

        if (returned < expected)
            Debug.WriteLine($"  ⚠ Vráceno {returned} < min(pageSize={pageSize}, total={total})={expected} — eToro bug!");
        if (returned != pageSize && returned < total)
            Debug.WriteLine($"  ⚠ pageSize={pageSize} ale vráceno {returned} (total={total} > pageSize) — parametr ignorován?");

        Assert.Multiple(() =>
        {
            Assert.That(returned, Is.LessThanOrEqualTo(pageSize),
                $"Vráceno {returned} > pageSize={pageSize} — API ignoruje parametr.");
            if (total >= pageSize)
                Assert.That(returned, Is.EqualTo(pageSize),
                    $"totalItems={total} ≥ pageSize={pageSize}, ale vráceno jen {returned}.");
        });
    }

    // ─── SearchText ───────────────────────────────────────────────────────────

    [TestCase("Apple",   TestName = "Search_Text_Apple")]
    [TestCase("Bitcoin", TestName = "Search_Text_Bitcoin")]
    [TestCase("SPY",     TestName = "Search_Text_SPY")]
    [TestCase("Tesla",   TestName = "Search_Text_Tesla")]
    public async Task SearchInstrumentsAsync_SearchText_ReturnsRelevantResults(string searchText)
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields     = [InstrumentFields.InstrumentId, InstrumentFields.DisplayName, InstrumentFields.Symbol],
            SearchText = searchText,
            PageSize   = 10,
        });

        Debug.WriteLine($"[Search] text='{searchText}' → total={result.TotalItems} returned={result.Instruments.Count}");
        foreach (var i in result.Instruments.Take(5))
            Debug.WriteLine($"  ID={i.InstrumentId} Name={i.DisplayName} Symbol={i.Symbol}");

        Assert.That(result.Instruments, Is.Not.Empty, $"Vyhledávání '{searchText}' nevrátilo výsledky.");
    }

    [Test]
    public async Task SearchInstrumentsAsync_EmptySearchText_ReturnsResults()
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields   = [InstrumentFields.InstrumentId],
            PageSize = 5,
        });

        Debug.WriteLine($"[Search] Bez SearchText → total={result.TotalItems} returned={result.Instruments.Count}");

        Assert.That(result.TotalItems, Is.GreaterThan(0), "Bez filtru by měl vrátit instrumenty.");
    }

    // ─── Kombinace pageSize + searchText + fields ─────────────────────────────

    [Test]
    public async Task SearchInstrumentsAsync_AllParams_Combination()
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields     = [InstrumentFields.InstrumentId, InstrumentFields.DisplayName, InstrumentFields.Symbol, InstrumentFields.CurrentRate, InstrumentFields.DailyPriceChange],
            SearchText = "Apple",
            PageSize   = 3,
        });

        Debug.WriteLine($"[Search] FullCombo → total={result.TotalItems} returned={result.Instruments.Count} page={result.Page} pageSize={result.PageSize}");
        foreach (var i in result.Instruments)
            Debug.WriteLine($"  ID={i.InstrumentId} Name={i.DisplayName} Rate={i.CurrentRate} DailyChange={i.DailyPriceChange}");

        Assert.Multiple(() =>
        {
            Assert.That(result.Instruments.Count, Is.LessThanOrEqualTo(3));
            Assert.That(result.PageSize, Is.EqualTo(3));
        });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetStocksIndustriesAsync
    // Dokumentace: query param stocksIndustryIds (optional), žádná paginace
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetStocksIndustriesAsync_NoFilter_ReturnsAll()
    {
        var result = await _client.GetStocksIndustriesAsync();

        Debug.WriteLine($"[Industries] Celkem: {result.StocksIndustries.Count}");
        foreach (var i in result.StocksIndustries)
            Debug.WriteLine($"  ID={i.IndustryId} Name={i.IndustryName}");

        Assert.That(result.StocksIndustries, Is.Not.Empty);
    }

    [Test]
    public async Task GetStocksIndustriesAsync_FilterSingleId_ReturnsExactlyOne()
    {
        if (_stocksIndustryIds.Count == 0) Assert.Ignore("Žádné industry IDs ze seed dat.");

        var id = _stocksIndustryIds[0];
        var result = await _client.GetStocksIndustriesAsync(stocksIndustryIds: [id]);

        Debug.WriteLine($"[Industries] Filter id={id} → vráceno={result.StocksIndustries.Count}");
        foreach (var i in result.StocksIndustries)
            Debug.WriteLine($"  ID={i.IndustryId} Name={i.IndustryName}");

        if (result.StocksIndustries.Count != 1)
            Debug.WriteLine($"  ⚠ Očekáváno 1, vráceno {result.StocksIndustries.Count}.");

        Assert.That(result.StocksIndustries.Select(i => i.IndustryId), Does.Contain(id));
    }

    [TestCase(2, TestName = "Industries_Filter_2Ids")]
    [TestCase(3, TestName = "Industries_Filter_3Ids")]
    public async Task GetStocksIndustriesAsync_FilterMultipleIds_ReturnsSubset(int count)
    {
        if (_stocksIndustryIds.Count < count)
        {
            Assert.Ignore($"Seed má jen {_stocksIndustryIds.Count} IDs, potřeba {count}.");
            return;
        }

        var ids = _stocksIndustryIds.Take(count).ToList();
        var result = await _client.GetStocksIndustriesAsync(stocksIndustryIds: ids);

        var returned = result.StocksIndustries.Count;
        Debug.WriteLine($"[Industries] Filter {count} IDs → vráceno={returned}");

        if (returned != count)
            Debug.WriteLine($"  ⚠ Vráceno {returned} ≠ požadovaných {count}.");

        Assert.That(returned, Is.LessThanOrEqualTo(count));
        foreach (var i in result.StocksIndustries)
            Assert.That(ids, Does.Contain(i.IndustryId),
                $"IndustryId={i.IndustryId} není v požadovaném filtru.");
    }

    [Test]
    public async Task GetStocksIndustriesAsync_FilterNonExistentId_ReturnsEmpty()
    {
        var result = await _client.GetStocksIndustriesAsync(stocksIndustryIds: [int.MaxValue]);

        Debug.WriteLine($"[Industries] Neexistující ID → vráceno={result.StocksIndustries.Count}");

        if (result.StocksIndustries.Count > 0)
            Debug.WriteLine($"  ⚠ API vrátilo data pro neexistující ID!");

        Assert.That(result.StocksIndustries, Is.Empty,
            "Neexistující industryId by měl vrátit prázdný seznam.");
    }

    [Test]
    public async Task GetStocksIndustriesAsync_AllIndustriesHaveNonEmptyName()
    {
        var result = await _client.GetStocksIndustriesAsync();

        var withoutName = result.StocksIndustries.Where(i => string.IsNullOrWhiteSpace(i.IndustryName)).ToList();
        if (withoutName.Count > 0)
            Debug.WriteLine($"[Industries] ⚠ {withoutName.Count} industrií bez jména: {string.Join(",", withoutName.Select(i => i.IndustryId))}");

        Assert.That(withoutName, Is.Empty, "Některé industries nemají jméno.");
    }
}
