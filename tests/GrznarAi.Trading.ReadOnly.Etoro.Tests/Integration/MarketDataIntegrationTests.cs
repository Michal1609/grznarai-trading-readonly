using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Configuration;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Market;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// IntegraÄŤnĂ­ testy pro sekci MARKET DATA.
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "Category=Integration"
/// NutnĂ©: nastavit EToroOptions__ApiKey + EToroOptions__UserKey nebo lokĂˇlnĂ­ appsettings.test.json
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class MarketDataIntegrationTests
{
    private IEToroClient _client = null!;

    private const int AaplInstrumentId = 1586;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();
    }

    // â”€â”€â”€ GetExchangesAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetExchangesAsync_ReturnsExchanges()
    {
        var result = await _client.GetExchangesAsync();

        Debug.WriteLine($"Exchanges count: {result.ExchangeInfo.Count}");
        foreach (var e in result.ExchangeInfo.Take(5))
            Debug.WriteLine($"  ID={e.ExchangeId} Desc={e.ExchangeDescription}");

        Assert.That(result.ExchangeInfo, Is.Not.Null);
        Assert.That(result.ExchangeInfo, Is.Not.Empty);
    }

    [Test]
    public async Task GetExchangesAsync_FilterByIds_ReturnsSubset()
    {
        var all = await _client.GetExchangesAsync();
        if (all.ExchangeInfo.Count == 0) Assert.Pass("No exchanges returned.");

        var firstId = all.ExchangeInfo[0].ExchangeId;
        var filtered = await _client.GetExchangesAsync(exchangeIds: [firstId]);

        Debug.WriteLine($"Filtered exchange: ID={filtered.ExchangeInfo.FirstOrDefault()?.ExchangeId}");

        Assert.That(filtered.ExchangeInfo, Has.Count.EqualTo(1));
        Assert.That(filtered.ExchangeInfo[0].ExchangeId, Is.EqualTo(firstId));
    }

    [Test]
    public async Task GetExchangesAsync_ExchangeHasDescription()
    {
        var result = await _client.GetExchangesAsync();

        foreach (var e in result.ExchangeInfo.Take(10))
            Assert.That(e.ExchangeDescription, Is.Not.Null.And.Not.Empty,
                $"Exchange {e.ExchangeId} has no description.");
    }

    // â”€â”€â”€ GetInstrumentTypesAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetInstrumentTypesAsync_ReturnsTypes()
    {
        var result = await _client.GetInstrumentTypesAsync();

        Debug.WriteLine($"Instrument types count: {result.InstrumentTypes.Count}");
        foreach (var t in result.InstrumentTypes)
            Debug.WriteLine($"  ID={t.InstrumentTypeId} Desc={t.InstrumentTypeDescription}");

        Assert.That(result.InstrumentTypes, Is.Not.Null);
        Assert.That(result.InstrumentTypes, Is.Not.Empty);
    }

    [Test]
    public async Task GetInstrumentTypesAsync_FilterByIds_ReturnsSubset()
    {
        var all = await _client.GetInstrumentTypesAsync();
        if (all.InstrumentTypes.Count == 0) Assert.Pass("No instrument types returned.");

        var firstId = all.InstrumentTypes[0].InstrumentTypeId;
        var filtered = await _client.GetInstrumentTypesAsync(instrumentTypeIds: [firstId]);

        Assert.That(filtered.InstrumentTypes, Has.Count.EqualTo(1));
        Assert.That(filtered.InstrumentTypes[0].InstrumentTypeId, Is.EqualTo(firstId));
    }

    [Test]
    public async Task GetInstrumentTypesAsync_TypesHaveDescription()
    {
        var result = await _client.GetInstrumentTypesAsync();

        foreach (var t in result.InstrumentTypes)
            Assert.That(t.InstrumentTypeDescription, Is.Not.Null.And.Not.Empty,
                $"InstrumentType {t.InstrumentTypeId} has no description.");
    }

    // â”€â”€â”€ GetInstrumentMetadataAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetInstrumentMetadataAsync_FilterByInstrumentId_ReturnsData()
    {
        var result = await _client.GetInstrumentMetadataAsync(instrumentIds: [AaplInstrumentId]);

        Debug.WriteLine($"Instruments metadata count: {result.InstrumentDisplayDatas.Count}");
        foreach (var i in result.InstrumentDisplayDatas)
            Debug.WriteLine($"  ID={i.InstrumentId} Name={i.InstrumentDisplayName} Symbol={i.SymbolFull}");

        Assert.That(result.InstrumentDisplayDatas, Is.Not.Empty);
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_InstrumentHasDisplayName()
    {
        var result = await _client.GetInstrumentMetadataAsync(instrumentIds: [AaplInstrumentId]);

        if (result.InstrumentDisplayDatas.Count == 0) Assert.Pass("No data returned.");

        var inst = result.InstrumentDisplayDatas[0];
        Assert.That(inst.InstrumentDisplayName, Is.Not.Null.And.Not.Empty);
        Assert.That(inst.InstrumentId, Is.EqualTo(AaplInstrumentId));
    }

    [Test]
    public async Task GetInstrumentMetadataAsync_ImagesPresent()
    {
        var result = await _client.GetInstrumentMetadataAsync(instrumentIds: [AaplInstrumentId]);

        if (result.InstrumentDisplayDatas.Count == 0) Assert.Pass("No data returned.");

        var inst = result.InstrumentDisplayDatas[0];
        Debug.WriteLine($"Images count: {inst.Images?.Count ?? 0}");

        Assert.That(inst.Images, Is.Not.Null);
    }

    // â”€â”€â”€ GetHistoricalClosingPricesAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetHistoricalClosingPricesAsync_ReturnsData()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        Debug.WriteLine($"Closing prices count: {result.Count}");
        var first = result.FirstOrDefault();
        if (first is not null)
            Debug.WriteLine($"  ID={first.InstrumentId} OfficialClose={first.OfficialClosingPrice} Daily={first.ClosingPrices?.Daily?.Price}");

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task GetHistoricalClosingPricesAsync_AaplHasClosingPrices()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        var aapl = result.FirstOrDefault(i => i.InstrumentId == AaplInstrumentId);
        if (aapl is null)
        {
            Assert.Pass("AAPL not in closing prices response.");
            return;
        }

        Debug.WriteLine($"AAPL official close: {aapl.OfficialClosingPrice}");
        Debug.WriteLine($"AAPL daily: {aapl.ClosingPrices?.Daily?.Price} at {aapl.ClosingPrices?.Daily?.Date}");
        Debug.WriteLine($"AAPL weekly: {aapl.ClosingPrices?.Weekly?.Price}");
        Debug.WriteLine($"AAPL monthly: {aapl.ClosingPrices?.Monthly?.Price}");

        Assert.That(aapl.OfficialClosingPrice, Is.GreaterThan(0));
        Assert.That(aapl.ClosingPrices, Is.Not.Null);
    }

    [Test]
    public async Task GetHistoricalClosingPricesAsync_ClosingPricesHaveDailyInterval()
    {
        var result = await _client.GetHistoricalClosingPricesAsync();

        var withPrices = result
            .Where(i => i.ClosingPrices?.Daily is not null)
            .Take(5)
            .ToList();

        Assert.That(withPrices, Is.Not.Empty, "Expected at least some instruments with daily closing prices.");
        foreach (var inst in withPrices)
            Assert.That(inst.ClosingPrices!.Daily!.Price, Is.GreaterThanOrEqualTo(0));
    }

    // â”€â”€â”€ GetStocksIndustriesAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetStocksIndustriesAsync_ReturnsIndustries()
    {
        var result = await _client.GetStocksIndustriesAsync();

        Debug.WriteLine($"Industries count: {result.StocksIndustries.Count}");
        foreach (var i in result.StocksIndustries)
            Debug.WriteLine($"  ID={i.IndustryId} Name={i.IndustryName}");

        Assert.That(result.StocksIndustries, Is.Not.Null);
        Assert.That(result.StocksIndustries, Is.Not.Empty);
    }

    [Test]
    public async Task GetStocksIndustriesAsync_FilterByIds_ReturnsSubset()
    {
        var all = await _client.GetStocksIndustriesAsync();
        if (all.StocksIndustries.Count == 0) Assert.Pass("No industries returned.");

        var firstId = all.StocksIndustries[0].IndustryId;
        var filtered = await _client.GetStocksIndustriesAsync(stocksIndustryIds: [firstId]);

        Assert.That(filtered.StocksIndustries, Has.Count.EqualTo(1));
        Assert.That(filtered.StocksIndustries[0].IndustryId, Is.EqualTo(firstId));
    }

    [Test]
    public async Task GetStocksIndustriesAsync_IndustriesHaveNames()
    {
        var result = await _client.GetStocksIndustriesAsync();

        foreach (var i in result.StocksIndustries)
            Assert.That(i.IndustryName, Is.Not.Null.And.Not.Empty,
                $"Industry {i.IndustryId} has no name.");
    }

    // â”€â”€â”€ SearchInstrumentsAsync (existing) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task SearchInstrumentsAsync_ReturnsResults()
    {
        var result = await _client.SearchInstrumentsAsync(new InstrumentSearchRequest
        {
            Fields = [InstrumentFields.InstrumentId, InstrumentFields.DisplayName, InstrumentFields.Symbol],
            SearchText = "Apple",
            PageSize = 5
        });

        Debug.WriteLine($"Search results: {result.TotalItems} total, {result.Instruments.Count} returned");
        foreach (var i in result.Instruments)
            Debug.WriteLine($"  ID={i.InstrumentId} Name={i.DisplayName} Symbol={i.Symbol}");

        Assert.That(result.Instruments, Is.Not.Empty);
    }

    // â”€â”€â”€ GetRatesAsync (existing) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetRatesAsync_ReturnsAaplRate()
    {
        var result = await _client.GetRatesAsync([AaplInstrumentId]);

        Assert.That(result.Rates, Is.Not.Empty);
        var rate = result.Rates.FirstOrDefault(r => r.InstrumentId == AaplInstrumentId);
        Assert.That(rate, Is.Not.Null);
        Assert.That(rate!.Ask, Is.GreaterThan(0));
        Assert.That(rate.Bid, Is.GreaterThan(0));

        Debug.WriteLine($"AAPL Ask={rate.Ask} Bid={rate.Bid} LastExecution={rate.LastExecution}");
        Debug.WriteLine($"PriceRateId={rate.PriceRateId} UnitMargin={rate.UnitMargin}");
    }

    // â”€â”€â”€ GetCandlesAsync (existing) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Test]
    public async Task GetCandlesAsync_ReturnsAaplCandles()
    {
        var result = await _client.GetCandlesAsync(
            AaplInstrumentId, CandleInterval.OneDay, CandleDirection.Desc, 10);

        Assert.That(result.Candles, Is.Not.Empty);
        Assert.That(result.Candles.Count, Is.LessThanOrEqualTo(10));

        var candle = result.Candles[0];
        Debug.WriteLine($"Latest candle: O={candle.Open} H={candle.High} L={candle.Low} C={candle.Close} V={candle.Volume}");
        Debug.WriteLine($"Range: Open={candle.RangeOpen} Close={candle.RangeClose} High={candle.RangeHigh} Low={candle.RangeLow}");

        Assert.That(candle.High, Is.GreaterThanOrEqualTo(candle.Low));
    }
}
