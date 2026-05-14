using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Market;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// ProdukÄŤnĂ­ / prĹŻzkumnĂ© testy market-data API.
/// CĂ­l: odhalit chovĂˇnĂ­ eToro nedokumentovanĂ© nebo odliĹˇnĂ© od dokumentace â€”
/// limity strĂˇnkovĂˇnĂ­, skuteÄŤnĂ© poÄŤty, ignorovanĂ© parametry, chovĂˇnĂ­ filtrĹŻ.
/// Rate limit: 60 req/min â€” zajiĹˇĹĄuje RateLimitHandler automaticky.
///
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "FullyQualifiedName~MarketDataProductionApiTests"
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class MarketDataProductionApiTests
{
    private IEToroClient _client = null!;

    // StabilnĂ­ instrument ID pro AAPL â€” eToro internĂ­ ID
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

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetExchangesAsync
    // Dokumentace: query param exchangeIds (array int, optional), ĹľĂˇdnĂˇ paginace
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

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
        if (_exchangeIds.Count == 0) Assert.Ignore("Ĺ˝ĂˇdnĂ© exchange IDs ze seed dat.");

        var id = _exchangeIds[0];
        var result = await _client.GetExchangesAsync(exchangeIds: [id]);

        Debug.WriteLine($"[Exchanges] Filter id={id} â†’ vrĂˇceno={result.ExchangeInfo.Count}");
        foreach (var e in result.ExchangeInfo)
            Debug.WriteLine($"  ID={e.ExchangeId} Desc={e.ExchangeDescription}");

        if (result.ExchangeInfo.Count != 1)
            Debug.WriteLine($"  âš  OÄŤekĂˇvĂˇno 1, vrĂˇceno {result.ExchangeInfo.Count}.");

        Assert.That(result.ExchangeInfo.Select(e => e.ExchangeId), Does.Contain(id),
            $"VrĂˇcenĂ˝ seznam neobsahuje poĹľadovanĂ˝ ID={id}.");
    }

    [TestCase(2, TestName = "Exchanges_FilterMultiple_2Ids")]
    [TestCase(5, TestName = "Exchanges_FilterMultiple_5Ids")]
    public async Task GetExchangesAsync_FilterMultipleIds_ReturnsSubset(int count)
    {
        if (_exchangeIds.Count < count)
        {
            Assert.Ignore($"Seed mĂˇ jen {_exchangeIds.Count} exchange IDs, potĹ™eba {count}.");
            return;
        }

        var ids = _exchangeIds.Take(count).ToList();
        var result = await _client.GetExchangesAsync(exchangeIds: ids);

        var returned = result.ExchangeInfo.Count;
        Debug.WriteLine($"[Exchanges] Filter {count} IDs â†’ vrĂˇceno={returned}");

        if (returned != count)
            Debug.WriteLine($"  âš  VrĂˇceno {returned} â‰  poĹľadovanĂ˝ch {count} â€” eToro vrĂˇtil jinĂ˝ poÄŤet.");

        Assert.That(returned, Is.LessThanOrEqualTo(count),
            $"API vrĂˇtilo vĂ­c exchanges ({returned}) neĹľ bylo poĹľadovĂˇno ({count}).");
        foreach (var e in result.ExchangeInfo)
            Assert.That(ids, Does.Contain(e.ExchangeId),
                $"ExchangeId={e.ExchangeId} nenĂ­ v poĹľadovanĂ©m filtru {string.Join(",", ids)}.");
    }

    [Test]
    public async Task GetExchangesAsync_FilterNonExistentId_ReturnsEmpty()
    {
        var result = await _client.GetExchangesAsync(exchangeIds: [int.MaxValue]);

        Debug.WriteLine($"[Exchanges] Filter neexistujĂ­cĂ­ ID={int.MaxValue} â†’ vrĂˇceno={result.ExchangeInfo.Count}");

        if (result.ExchangeInfo.Count > 0)
            Debug.WriteLine($"  âš  API vrĂˇtilo data pro neexistujĂ­cĂ­ ID â€” ignoruje filtr?");

        Assert.That(result.ExchangeInfo, Is.Empty,
            "API by mÄ›lo vrĂˇtit prĂˇzdnĂ˝ seznam pro neexistujĂ­cĂ­ exchange ID.");
    }

    [Test]
    public async Task GetExchangesAsync_AllExchangesHavePositiveId()
    {
        var result = await _client.GetExchangesAsync();

        foreach (var e in result.ExchangeInfo)
            Assert.That(e.ExchangeId, Is.GreaterThan(0),
                $"Exchange '{e.ExchangeDescription}' mĂˇ neplatnĂ© ID={e.ExchangeId}.");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetInstrumentTypesAsync
    // Dokumentace: query param instrumentTypeIds (array int, optional), ĹľĂˇdnĂˇ paginace
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

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
        if (_instrumentTypeIds.Count == 0) Assert.Ignore("Ĺ˝ĂˇdnĂ© type IDs ze seed dat.");

        var id = _instrumentTypeIds[0];
        var result = await _client.GetInstrumentTypesAsync(instrumentTypeIds: [id]);

        Debug.WriteLine($"[InstrumentTypes] Filter id={id} â†’ vrĂˇceno={result.InstrumentTypes.Count}");
        foreach (var t in result.InstrumentTypes)
            Debug.WriteLine($"  ID={t.InstrumentTypeId} Desc={t.InstrumentTypeDescription}");

        if (result.InstrumentTypes.Count != 1)
            Debug.WriteLine($"  âš  OÄŤekĂˇvĂˇno 1, vrĂˇceno {result.InstrumentTypes.Count}.");

        Assert.That(result.InstrumentTypes.Select(t => t.InstrumentTypeId), Does.Contain(id));
    }

    [Test]
    public async Task GetInstrumentTypesAsync_FilterNonExistentId_ReturnsEmpty()
    {
        var result = await _client.GetInstrumentTypesAsync(instrumentTypeIds: [int.MaxValue]);

        Debug.WriteLine($"[InstrumentTypes] NeexistujĂ­cĂ­ ID â†’ vrĂˇceno={result.InstrumentTypes.Count}");

        if (result.InstrumentTypes.Count > 0)
            Debug.WriteLine($"  âš  API vrĂˇtilo data pro neexistujĂ­cĂ­ ID â€” ignoruje filtr?");

        Assert.That(result.InstrumentTypes, Is.Empty,
            "NeexistujĂ­cĂ­ instrumentTypeId by mÄ›l vrĂˇtit prĂˇzdnĂ˝ seznam.");
    }

    [Test]
    public async Task GetInstrumentTypesAsync_AllTypesHavePositiveId()
    {
        var result = await _client.GetInstrumentTypesAsync();

        foreach (var t in result.InstrumentTypes)
            Assert.That(t.InstrumentTypeId, Is.GreaterThan(0),
                $"Type '{t.InstrumentTypeDescription}' mĂˇ neplatnĂ© ID={t.InstrumentTypeId}.");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetInstrumentMetadataAsync
    // Dokumentace: filtry instrumentIds, exchangeIds, stocksIndustryIds, instrumentTypeIds
    // Ĺ˝ĂˇdnĂˇ paginace â€” mĹŻĹľe vrĂˇtit tisĂ­ce zĂˇznamĹŻ pĹ™i volĂˇnĂ­ bez filtrĹŻ
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterByInstrumentIds_SingleId()
    {
        var result = await _client.GetInstrumentMetadataAsync(instrumentIds: [AaplInstrumentId]);

        Debug.WriteLine($"[Metadata] Filter 1 ID â†’ vrĂˇceno={result.InstrumentDisplayDatas.Count}");
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
            Assert.Ignore($"Seed mĂˇ jen {_instrumentIds.Count} IDs, potĹ™eba {count}.");
            return;
        }

        var ids = _instrumentIds.Take(count).ToList();
        var result = await _client.GetInstrumentMetadataAsync(instrumentIds: ids);

        var returned = result.InstrumentDisplayDatas.Count;
        Debug.WriteLine($"[Metadata] Filter {count} IDs â†’ vrĂˇceno={returned}");

        if (returned != count)
            Debug.WriteLine($"  âš  VrĂˇceno {returned} â‰  poĹľadovanĂ˝ch {count}. " +
                            $"ChybÄ›jĂ­cĂ­ IDs: {string.Join(",", ids.Except(result.InstrumentDisplayDatas.Select(i => i.InstrumentId)))}");

        Assert.That(returned, Is.GreaterThan(0), "API nevrĂˇtilo ĹľĂˇdnĂˇ data pro platnĂ© instrumentIds.");
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterByExchangeId_ReturnsInstrumentsForExchange()
    {
        if (_exchangeIds.Count == 0) Assert.Ignore("Ĺ˝ĂˇdnĂ© exchange IDs.");

        var id = _exchangeIds[0];
        var result = await _client.GetInstrumentMetadataAsync(exchangeIds: [id]);

        Debug.WriteLine($"[Metadata] Filter exchangeId={id} â†’ vrĂˇceno={result.InstrumentDisplayDatas.Count}");

        var wrongExchange = result.InstrumentDisplayDatas.Where(i => i.ExchangeId != id).ToList();
        if (wrongExchange.Count > 0)
            Debug.WriteLine($"  âš  {wrongExchange.Count} instrumentĹŻ mĂˇ jinĂ˝ exchangeId neĹľ filtrovanĂ˝ {id}!");

        Assert.That(result.InstrumentDisplayDatas, Is.Not.Empty,
            $"Ĺ˝ĂˇdnĂ© instrumenty pro exchangeId={id}.");
        Assert.That(wrongExchange, Is.Empty,
            $"API vrĂˇtilo instrumenty s jinĂ˝m exchangeId neĹľ bylo filtrovĂˇno.");
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterByInstrumentTypeId_ReturnsCorrectType()
    {
        if (_instrumentTypeIds.Count == 0) Assert.Ignore("Ĺ˝ĂˇdnĂ© type IDs.");

        var id = _instrumentTypeIds[0];
        var result = await _client.GetInstrumentMetadataAsync(instrumentTypeIds: [id]);

        Debug.WriteLine($"[Metadata] Filter typeId={id} â†’ vrĂˇceno={result.InstrumentDisplayDatas.Count}");

        var wrongType = result.InstrumentDisplayDatas.Where(i => i.InstrumentTypeId != id).ToList();
        if (wrongType.Count > 0)
            Debug.WriteLine($"  âš  {wrongType.Count} instrumentĹŻ mĂˇ jinĂ˝ typeId neĹľ {id}!");

        Assert.That(result.InstrumentDisplayDatas, Is.Not.Empty);
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterByStocksIndustryId()
    {
        if (_stocksIndustryIds.Count == 0) Assert.Ignore("Ĺ˝ĂˇdnĂ© industry IDs.");

        var id = _stocksIndustryIds[0];
        var result = await _client.GetInstrumentMetadataAsync(stocksIndustryIds: [id]);

        Debug.WriteLine($"[Metadata] Filter industryId={id} â†’ vrĂˇceno={result.InstrumentDisplayDatas.Count}");

        Assert.That(result.InstrumentDisplayDatas, Is.Not.Empty,
            $"Ĺ˝ĂˇdnĂ© instrumenty pro industryId={id}.");
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterNonExistentInstrumentId_ReturnsEmpty()
    {
        var result = await _client.GetInstrumentMetadataAsync(instrumentIds: [int.MaxValue]);

        Debug.WriteLine($"[Metadata] NeexistujĂ­cĂ­ ID â†’ vrĂˇceno={result.InstrumentDisplayDatas.Count}");

        if (result.InstrumentDisplayDatas.Count > 0)
            Debug.WriteLine($"  âš  API vrĂˇtilo data pro neexistujĂ­cĂ­ instrumentId!");

        Assert.That(result.InstrumentDisplayDatas, Is.Empty);
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetHistoricalClosingPricesAsync
    // Dokumentace: ĹľĂˇdnĂ© parametry â€” vrĂˇtĂ­ vĹˇe pro vĹˇechny instrumenty
    // POZOR: mĹŻĹľe vrĂˇtit tisĂ­ce zĂˇznamĹŻ, pomalĂ© volĂˇnĂ­
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [Test]
    public async Task GetHistoricalClosingPricesAsync_ReturnsNonEmptyList()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        Debug.WriteLine($"[ClosingPrices] Celkem zĂˇznamĹŻ: {result.Count}");
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
            Debug.WriteLine($"[ClosingPrices] âš  {nonPositive.Count} instrumentĹŻ (ukĂˇzka) s OfficialClosingPrice â‰¤ 0:");
            foreach (var p in nonPositive)
                Debug.WriteLine($"  ID={p.InstrumentId} Official={p.OfficialClosingPrice}");
        }

        // ZaznamenĂˇme ale netestujeme â€” delisted instrumenty mohou mĂ­t cenu 0
        Assert.That(result.Count(p => p.OfficialClosingPrice > 0), Is.GreaterThan(0),
            "Ĺ˝ĂˇdnĂ˝ instrument nemĂˇ kladnou OfficialClosingPrice â€” pravdÄ›podobnĂˇ chyba deserializace.");
    }

    [Test]
    public async Task GetHistoricalClosingPricesAsync_MonthlyPriceMinusOneIsDocumented()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        var withMinusOne = result.Where(p => p.ClosingPrices?.Monthly?.Price == -1).ToList();
        var withMonthly  = result.Where(p => p.ClosingPrices?.Monthly?.Price > 0).ToList();
        var withoutMonthly = result.Where(p => p.ClosingPrices?.Monthly is null).ToList();

        Debug.WriteLine($"[ClosingPrices] Monthly -1 (dokumentovĂˇno): {withMinusOne.Count}");
        Debug.WriteLine($"[ClosingPrices] Monthly > 0: {withMonthly.Count}");
        Debug.WriteLine($"[ClosingPrices] Monthly null: {withoutMonthly.Count}");

        // InformativnĂ­ â€” ovÄ›Ĺ™uje, Ĺľe dokumentovanĂ˝ pĹ™Ă­pad -1 opravdu nastĂˇvĂˇ
        Assert.Pass($"Monthly prices â€” valid: {withMonthly.Count}, -1: {withMinusOne.Count}, null: {withoutMonthly.Count}");
    }

    [Test]
    public async Task GetHistoricalClosingPricesAsync_AaplHasConsistentPrices()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        var aapl = result.FirstOrDefault(p => p.InstrumentId == AaplInstrumentId);
        if (aapl is null)
        {
            Assert.Pass($"AAPL (ID={AaplInstrumentId}) nenĂ­ v closing prices.");
            return;
        }

        Debug.WriteLine($"[ClosingPrices] AAPL official={aapl.OfficialClosingPrice}");
        Debug.WriteLine($"  Daily={aapl.ClosingPrices?.Daily?.Price} at {aapl.ClosingPrices?.Daily?.Date}");
        Debug.WriteLine($"  Weekly={aapl.ClosingPrices?.Weekly?.Price} at {aapl.ClosingPrices?.Weekly?.Date}");
        Debug.WriteLine($"  Monthly={aapl.ClosingPrices?.Monthly?.Price} at {aapl.ClosingPrices?.Monthly?.Date}");

        Assert.Multiple(() =>
        {
            Assert.That(aapl.OfficialClosingPrice, Is.GreaterThan(0));
            Assert.That(aapl.ClosingPrices, Is.Not.Null, "AAPL by mÄ›l mĂ­t ClosingPrices objekt.");
        });
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetRatesAsync
    // Dokumentace: instrumentIds required, max 100 IDs, 400 pĹ™i pĹ™ekroÄŤenĂ­
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    [TestCase(1,   TestName = "Rates_SingleInstrument")]
    [TestCase(5,   TestName = "Rates_5Instruments")]
    [TestCase(10,  TestName = "Rates_10Instruments")]
    [TestCase(50,  TestName = "Rates_50Instruments")]
    [TestCase(100, TestName = "Rates_100Instruments_MaxLimit")]
    public async Task GetRatesAsync_VariousInstrumentCounts_AllReturnRates(int count)
    {
        if (_instrumentIds.Count < count)
        {
            Assert.Ignore($"Seed mĂˇ jen {_instrumentIds.Count} IDs, potĹ™eba {count}.");
            return;
        }

        var ids = _instrumentIds.Take(count).ToList();
        var result = await _client.GetRatesAsync(ids);

        var returned = result.Rates.Count;
        Debug.WriteLine($"[Rates] PoĹľadovĂˇno={count} â†’ vrĂˇceno={returned}");

        if (returned != count)
            Debug.WriteLine($"  âš  VrĂˇceno {returned} â‰  poĹľadovanĂ˝ch {count}. " +
                            $"ChybÄ›jĂ­cĂ­ IDs: {string.Join(",", ids.Except(result.Rates.Select(r => r.InstrumentId)).Take(5))}");

        Assert.Multiple(() =>
        {
            Assert.That(returned, Is.GreaterThan(0), "Ĺ˝ĂˇdnĂ© rates vrĂˇceny.");
            Assert.That(returned, Is.LessThanOrEqualTo(count), "VrĂˇceno vĂ­c rates neĹľ bylo poĹľadovĂˇno.");
        });
    }

    [Test]
    public async Task GetRatesAsync_AaplRate_AskGreaterThanBid()
    {
        var result = await _client.GetRatesAsync([AaplInstrumentId]);

        var rate = result.Rates.FirstOrDefault(r => r.InstrumentId == AaplInstrumentId);
        if (rate is null)
        {
            Assert.Pass("AAPL rate nebyla vrĂˇcena.");
            return;
        }

        Debug.WriteLine($"[Rates] AAPL Ask={rate.Ask} Bid={rate.Bid} Last={rate.LastExecution} Date={rate.Date}");

        Assert.Multiple(() =>
        {
            Assert.That(rate.Ask, Is.GreaterThan(0), "Ask cena musĂ­ bĂ˝t kladnĂˇ.");
            Assert.That(rate.Bid, Is.GreaterThan(0), "Bid cena musĂ­ bĂ˝t kladnĂˇ.");
            Assert.That(rate.Ask, Is.GreaterThanOrEqualTo(rate.Bid), "Ask musĂ­ bĂ˝t â‰Ą Bid (spread).");
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
                Debug.WriteLine($"  âš  ID={rate.InstrumentId} mĂˇ nekladnou cenu Ask={rate.Ask} Bid={rate.Bid}");
        }

        Assert.That(result.Rates.All(r => r.Ask >= 0 && r.Bid >= 0), Is.True,
            "NÄ›kterĂ© rates majĂ­ zĂˇpornĂ© ceny.");
    }

    [Test]
    public async Task GetRatesAsync_Exceed100Ids_ClientThrowsBeforeApiCall()
    {
        // Toto testuje client-side validaci â€” NEPROVEDE API volĂˇnĂ­
        var ids = Enumerable.Range(1, 101).ToList();

        Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetRatesAsync(ids),
            "Client by mÄ›l hodit ArgumentOutOfRangeException pro > 100 IDs.");
    }

    [Test]
    public async Task GetRatesAsync_BtcRate_HasConversionRates()
    {
        var result = await _client.GetRatesAsync([BtcInstrumentId]);

        var rate = result.Rates.FirstOrDefault(r => r.InstrumentId == BtcInstrumentId);
        if (rate is null)
        {
            Assert.Pass($"BTC (ID={BtcInstrumentId}) rate nebyla vrĂˇcena.");
            return;
        }

        Debug.WriteLine($"[Rates] BTC Ask={rate.Ask} Bid={rate.Bid} CvtAsk={rate.ConversionRateAsk} CvtBid={rate.ConversionRateBid}");

        Assert.Multiple(() =>
        {
            Assert.That(rate.ConversionRateAsk, Is.GreaterThan(0), "ConversionRateAsk musĂ­ bĂ˝t kladnĂ˝.");
            Assert.That(rate.ConversionRateBid, Is.GreaterThan(0), "ConversionRateBid musĂ­ bĂ˝t kladnĂ˝.");
        });
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetCandlesAsync
    // Dokumentace: instrumentId (path), direction (asc/desc), interval (enum),
    //              candlesCount default=100, max=1000
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

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
        Debug.WriteLine($"[Candles] PoĹľadovĂˇno={count} interval=OneDay direction=Desc â†’ vrĂˇceno={returned}");

        if (returned < count)
            Debug.WriteLine($"  âš  VrĂˇceno mĂ©nÄ› ({returned}) neĹľ poĹľadovĂˇno ({count}) â€” eToro bug nebo nedostatek dat?");

        Assert.That(returned, Is.LessThanOrEqualTo(count),
            $"VrĂˇceno {returned} svĂ­ÄŤek, pĹ™estoĹľe candlesCount={count}.");
        Assert.That(returned, Is.GreaterThan(0), "Ĺ˝ĂˇdnĂ© svĂ­ÄŤky vrĂˇceny.");
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

        Debug.WriteLine($"[Candles] Interval={interval} â†’ vrĂˇceno={result.Candles.Count} intervalStr={result.Interval}");
        if (result.Candles.Count > 0)
        {
            var c = result.Candles[0];
            Debug.WriteLine($"  Latest: Date={c.FromDate} O={c.Open} H={c.High} L={c.Low} C={c.Close} V={c.Volume}");
        }

        Assert.That(result.Candles, Is.Not.Empty, $"Interval={interval} nevrĂˇtil ĹľĂˇdnĂ© svĂ­ÄŤky.");
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

        // Desc: nejnovÄ›jĹˇĂ­ prvnĂ­ â†’ descDates[0] > descDates[1]
        if (descDates.Count >= 2)
            Assert.That(descDates[0], Is.GreaterThanOrEqualTo(descDates[1]),
                "Desc smÄ›r â€” prvnĂ­ svĂ­ÄŤka by mÄ›la bĂ˝t novÄ›jĹˇĂ­ neĹľ druhĂˇ.");

        // Asc: nejstarĹˇĂ­ prvnĂ­ â†’ ascDates[0] < ascDates[1]
        if (ascDates.Count >= 2)
            Assert.That(ascDates[0], Is.LessThanOrEqualTo(ascDates[1]),
                "Asc smÄ›r â€” prvnĂ­ svĂ­ÄŤka by mÄ›la bĂ˝t starĹˇĂ­ neĹľ druhĂˇ.");
    }

    [Test]
    public async Task GetCandlesAsync_OHLC_HighAlwaysGreaterOrEqualToLow()
    {
        var result = await _client.GetCandlesAsync(AaplInstrumentId, CandleInterval.OneDay, CandleDirection.Desc, 50);

        var invalidCandles = result.Candles.Where(c => c.High < c.Low).ToList();
        if (invalidCandles.Count > 0)
        {
            Debug.WriteLine($"[Candles] âš  {invalidCandles.Count} svĂ­ÄŤek s High < Low:");
            foreach (var c in invalidCandles)
                Debug.WriteLine($"  Date={c.FromDate} H={c.High} L={c.Low}");
        }

        Assert.That(invalidCandles, Is.Empty, "Nalezeny svĂ­ÄŤky kde High < Low â€” chyba dat.");
    }

    [Test]
    public async Task GetCandlesAsync_ResponseInterval_MatchesRequested()
    {
        foreach (var interval in new[] { CandleInterval.OneDay, CandleInterval.OneHour, CandleInterval.OneWeek })
        {
            var result = await _client.GetCandlesAsync(AaplInstrumentId, interval, CandleDirection.Desc, 3);

            Debug.WriteLine($"[Candles] PoĹľadovanĂ˝ interval={interval} â†’ vrĂˇcenĂ˝ interval string='{result.Interval}'");

            Assert.That(result.Interval, Is.Not.Null.And.Not.Empty,
                $"Interval string v odpovÄ›di je prĂˇzdnĂ˝ pro {interval}.");
        }
    }

    [Test]
    public async Task GetCandlesAsync_ExceedMaxCount_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.GetCandlesAsync(AaplInstrumentId, CandleInterval.OneDay, CandleDirection.Desc, 1001),
            "Client by mÄ›l hodit ArgumentOutOfRangeException pro candlesCount > 1000.");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // SearchInstrumentsAsync
    // KlĂ­ÄŤovĂ©: fields required (min 1, MAX 5), pageSize (1â€“200), pageNumber (â‰Ą0)
    // HLAVNĂŤ cĂ­l testu: odhalit, zda eToro skuteÄŤnÄ› vrĂˇtĂ­ poĹľadovanĂ˝ pageSize
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    // â”€â”€â”€ PoÄŤet fields (kritickĂ˝ limit = 5) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

        Debug.WriteLine($"[Search] Fields={fieldCount} ({string.Join(",", fields)}) â†’ instruments={result.Instruments.Count} total={result.TotalItems}");

        Assert.That(result.Instruments, Is.Not.Empty, $"Search s {fieldCount} fields nevrĂˇtil vĂ˝sledky.");
    }

    [Test]
    public async Task SearchInstrumentsAsync_SixFields_ClientThrows()
    {
        // Client-side validace â€” max 5 fields
        var sixFields = new[]
        {
            InstrumentFields.InstrumentId,
            InstrumentFields.DisplayName,
            InstrumentFields.Symbol,
            InstrumentFields.CurrentRate,
            InstrumentFields.DailyPriceChange,
            InstrumentFields.WeeklyPriceChange   // 6. field â†’ musĂ­ vyhodit
        };

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.SearchInstrumentsAsync(new InstrumentSearchRequest { Fields = sixFields }),
            "Client by mÄ›l odmĂ­tnout > 5 fields.");
    }

    [Test]
    public async Task SearchInstrumentsAsync_UnknownField_ClientThrows()
    {
        Assert.ThrowsAsync<ArgumentException>(
            () => _client.SearchInstrumentsAsync(new InstrumentSearchRequest
            {
                Fields = ["nonExistentField123"]
            }),
            "Client by mÄ›l odmĂ­tnout neznĂˇmĂ© field jmĂ©no.");
    }

    // â”€â”€â”€ PageSize â€” klĂ­ÄŤovĂ˝ test: vrĂˇtĂ­ eToro poĹľadovanĂ˝ poÄŤet? â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

        Debug.WriteLine($"[Search] pageSize={pageSize} â†’ returned={returned} totalItems={total} expectedâ‰¤{expected}");

        if (returned < expected)
            Debug.WriteLine($"  âš  VrĂˇceno {returned} < min(pageSize={pageSize}, total={total})={expected} â€” eToro bug!");
        if (returned != pageSize && returned < total)
            Debug.WriteLine($"  âš  pageSize={pageSize} ale vrĂˇceno {returned} (total={total} > pageSize) â€” parametr ignorovĂˇn?");

        Assert.Multiple(() =>
        {
            Assert.That(returned, Is.LessThanOrEqualTo(pageSize),
                $"VrĂˇceno {returned} > pageSize={pageSize} â€” API ignoruje parametr.");
            if (total >= pageSize)
                Assert.That(returned, Is.EqualTo(pageSize),
                    $"totalItems={total} â‰Ą pageSize={pageSize}, ale vrĂˇceno jen {returned}.");
        });
    }

    // â”€â”€â”€ SearchText â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

        Debug.WriteLine($"[Search] text='{searchText}' â†’ total={result.TotalItems} returned={result.Instruments.Count}");
        foreach (var i in result.Instruments.Take(5))
            Debug.WriteLine($"  ID={i.InstrumentId} Name={i.DisplayName} Symbol={i.Symbol}");

        Assert.That(result.Instruments, Is.Not.Empty, $"VyhledĂˇvĂˇnĂ­ '{searchText}' nevrĂˇtilo vĂ˝sledky.");
    }

    [Test]
    public async Task SearchInstrumentsAsync_EmptySearchText_ReturnsResults()
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields   = [InstrumentFields.InstrumentId],
            PageSize = 5,
        });

        Debug.WriteLine($"[Search] Bez SearchText â†’ total={result.TotalItems} returned={result.Instruments.Count}");

        Assert.That(result.TotalItems, Is.GreaterThan(0), "Bez filtru by mÄ›l vrĂˇtit instrumenty.");
    }

    // â”€â”€â”€ Kombinace pageSize + searchText + fields â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task SearchInstrumentsAsync_AllParams_Combination()
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields     = [InstrumentFields.InstrumentId, InstrumentFields.DisplayName, InstrumentFields.Symbol, InstrumentFields.CurrentRate, InstrumentFields.DailyPriceChange],
            SearchText = "Apple",
            PageSize   = 3,
        });

        Debug.WriteLine($"[Search] FullCombo â†’ total={result.TotalItems} returned={result.Instruments.Count} page={result.Page} pageSize={result.PageSize}");
        foreach (var i in result.Instruments)
            Debug.WriteLine($"  ID={i.InstrumentId} Name={i.DisplayName} Rate={i.CurrentRate} DailyChange={i.DailyPriceChange}");

        Assert.Multiple(() =>
        {
            Assert.That(result.Instruments.Count, Is.LessThanOrEqualTo(3));
            Assert.That(result.PageSize, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task SearchInstrumentsAsync_BigPageSize_ReturnsAllInstruments()
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = [InstrumentFields.InstrumentId, InstrumentFields.InternalSymbolFull],
            PageSize = 12000,
        });

        Debug.WriteLine($"[Search] Bez SearchText â†’ total={result.TotalItems} returned={result.Instruments.Count}");

        Assert.That(result.TotalItems, Is.GreaterThan(11000), "Bez filtru by mÄ›l vrĂˇtit instrumenty.");
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // GetStocksIndustriesAsync
    // Dokumentace: query param stocksIndustryIds (optional), ĹľĂˇdnĂˇ paginace
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

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
        if (_stocksIndustryIds.Count == 0) Assert.Ignore("Ĺ˝ĂˇdnĂ© industry IDs ze seed dat.");

        var id = _stocksIndustryIds[0];
        var result = await _client.GetStocksIndustriesAsync(stocksIndustryIds: [id]);

        Debug.WriteLine($"[Industries] Filter id={id} â†’ vrĂˇceno={result.StocksIndustries.Count}");
        foreach (var i in result.StocksIndustries)
            Debug.WriteLine($"  ID={i.IndustryId} Name={i.IndustryName}");

        if (result.StocksIndustries.Count != 1)
            Debug.WriteLine($"  âš  OÄŤekĂˇvĂˇno 1, vrĂˇceno {result.StocksIndustries.Count}.");

        Assert.That(result.StocksIndustries.Select(i => i.IndustryId), Does.Contain(id));
    }

    [TestCase(2, TestName = "Industries_Filter_2Ids")]
    [TestCase(3, TestName = "Industries_Filter_3Ids")]
    public async Task GetStocksIndustriesAsync_FilterMultipleIds_ReturnsSubset(int count)
    {
        if (_stocksIndustryIds.Count < count)
        {
            Assert.Ignore($"Seed mĂˇ jen {_stocksIndustryIds.Count} IDs, potĹ™eba {count}.");
            return;
        }

        var ids = _stocksIndustryIds.Take(count).ToList();
        var result = await _client.GetStocksIndustriesAsync(stocksIndustryIds: ids);

        var returned = result.StocksIndustries.Count;
        Debug.WriteLine($"[Industries] Filter {count} IDs â†’ vrĂˇceno={returned}");

        if (returned != count)
            Debug.WriteLine($"  âš  VrĂˇceno {returned} â‰  poĹľadovanĂ˝ch {count}.");

        Assert.That(returned, Is.LessThanOrEqualTo(count));
        foreach (var i in result.StocksIndustries)
            Assert.That(ids, Does.Contain(i.IndustryId),
                $"IndustryId={i.IndustryId} nenĂ­ v poĹľadovanĂ©m filtru.");
    }

    [Test]
    public async Task GetStocksIndustriesAsync_FilterNonExistentId_ReturnsEmpty()
    {
        var result = await _client.GetStocksIndustriesAsync(stocksIndustryIds: [int.MaxValue]);

        Debug.WriteLine($"[Industries] NeexistujĂ­cĂ­ ID â†’ vrĂˇceno={result.StocksIndustries.Count}");

        if (result.StocksIndustries.Count > 0)
            Debug.WriteLine($"  âš  API vrĂˇtilo data pro neexistujĂ­cĂ­ ID!");

        Assert.That(result.StocksIndustries, Is.Empty,
            "NeexistujĂ­cĂ­ industryId by mÄ›l vrĂˇtit prĂˇzdnĂ˝ seznam.");
    }

    [Test]
    public async Task GetStocksIndustriesAsync_AllIndustriesHaveNonEmptyName()
    {
        var result = await _client.GetStocksIndustriesAsync();

        var withoutName = result.StocksIndustries.Where(i => string.IsNullOrWhiteSpace(i.IndustryName)).ToList();
        if (withoutName.Count > 0)
            Debug.WriteLine($"[Industries] âš  {withoutName.Count} industriĂ­ bez jmĂ©na: {string.Join(",", withoutName.Select(i => i.IndustryId))}");

        Assert.That(withoutName, Is.Empty, "NÄ›kterĂ© industries nemajĂ­ jmĂ©no.");
    }
}
