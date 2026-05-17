using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API — Public section.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbasePublicIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── GetPublicMarketTradesAsync ──────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetPublicMarketTrades_returns_trades_for_btc_usd()
    {
        var result = await Client.GetPublicMarketTradesAsync("BTC-USD", 10);

        Debug.WriteLine($"Trades: {result.Trades?.Count ?? 0}");
        Debug.WriteLine($"Best Bid: {result.BestBid}  Best Ask: {result.BestAsk}");
        foreach (var t in result.Trades?.Take(3) ?? [])
            Debug.WriteLine($"  TradeId={t.TradeId}  Price={t.Price}  Size={t.Size}  Side={t.Side}  Time={t.Time}");

        Assert.NotNull(result.Trades);
        Assert.NotEmpty(result.BestBid ?? "x");
        Assert.NotEmpty(result.BestAsk ?? "x");
    }

    [CoinbaseIntegrationFact]
    public async Task GetPublicMarketTrades_request_overload_matches_params_overload()
    {
        var byParams = await Client.GetPublicMarketTradesAsync("BTC-USD", 5);
        var byRequest = await Client.GetPublicMarketTradesAsync(new GetPublicMarketTradesRequest { ProductId = "BTC-USD", Limit = 5 });

        Assert.Equal(byParams.BestBid, byRequest.BestBid);
    }

    // ─── GetPublicProductAsync ───────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetPublicProduct_returns_btc_usd()
    {
        var result = await Client.GetPublicProductAsync("BTC-USD");

        Debug.WriteLine($"ProductId:   {result.ProductId}");
        Debug.WriteLine($"Price:       {result.Price}");
        Debug.WriteLine($"ProductType: {result.ProductType}");
        Debug.WriteLine($"BaseName:    {result.BaseName}");
        Debug.WriteLine($"QuoteName:   {result.QuoteName}");

        Assert.Equal("BTC-USD", result.ProductId);
        Assert.NotNull(result.Price);
    }

    [CoinbaseIntegrationFact]
    public async Task GetPublicProduct_request_overload_matches_params_overload()
    {
        var byParams = await Client.GetPublicProductAsync("ETH-USD");
        var byRequest = await Client.GetPublicProductAsync(new GetPublicProductRequest { ProductId = "ETH-USD" });

        Assert.Equal(byParams.ProductId, byRequest.ProductId);
    }

    [CoinbaseIntegrationFact]
    public async Task GetPublicProduct_throws_CoinbaseApiException_for_unknown_product()
    {
        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => Client.GetPublicProductAsync("UNKNOWN-NOTAPRODUCT"));

        Debug.WriteLine($"StatusCode: {ex.StatusCode}");

        Assert.True(
            ex.StatusCode == System.Net.HttpStatusCode.NotFound
            || ex.StatusCode == System.Net.HttpStatusCode.BadRequest
            || ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity);
    }

    // ─── GetPublicProductBookAsync ───────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetPublicProductBook_returns_order_book_for_btc_usd()
    {
        var result = await Client.GetPublicProductBookAsync("BTC-USD", limit: 5);

        Debug.WriteLine($"ProductId:       {result.Pricebook?.ProductId}");
        Debug.WriteLine($"Bids:            {result.Pricebook?.Bids?.Count ?? 0}");
        Debug.WriteLine($"Asks:            {result.Pricebook?.Asks?.Count ?? 0}");
        Debug.WriteLine($"Mid Market:      {result.MidMarket}");
        Debug.WriteLine($"Spread BPS:      {result.SpreadBps}");
        Debug.WriteLine($"Spread Absolute: {result.SpreadAbsolute}");

        Assert.NotNull(result.Pricebook);
        Assert.Equal("BTC-USD", result.Pricebook!.ProductId);
    }

    [CoinbaseIntegrationFact]
    public async Task GetPublicProductBook_request_overload_matches_params_overload()
    {
        var byParams = await Client.GetPublicProductBookAsync("BTC-USD", limit: 3);
        var byRequest = await Client.GetPublicProductBookAsync(new GetPublicProductBookRequest { ProductId = "BTC-USD", Limit = 3 });

        Assert.Equal(byParams.Pricebook?.ProductId, byRequest.Pricebook?.ProductId);
    }

    // ─── GetPublicProductCandlesAsync ────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetPublicProductCandles_returns_candles_for_btc_usd()
    {
        var endTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        var startTs = DateTimeOffset.UtcNow.AddHours(-24).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);

        var result = await Client.GetPublicProductCandlesAsync("BTC-USD", startTs, endTs, Granularity.OneHour);

        Debug.WriteLine($"Candles: {result.Candles?.Count ?? 0}");
        foreach (var c in result.Candles?.Take(3) ?? [])
            Debug.WriteLine($"  Start={c.Start}  O={c.Open}  H={c.High}  L={c.Low}  C={c.Close}  V={c.Volume}");

        Assert.NotNull(result.Candles);
        Assert.NotEmpty(result.Candles!);
    }

    [CoinbaseIntegrationFact]
    public async Task GetPublicProductCandles_request_overload_matches_params_overload()
    {
        var endTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        var startTs = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);

        var byParams = await Client.GetPublicProductCandlesAsync("BTC-USD", startTs, endTs, Granularity.FiveMinute);
        var byRequest = await Client.GetPublicProductCandlesAsync(new GetPublicProductCandlesRequest
        {
            ProductId = "BTC-USD",
            Start = startTs,
            End = endTs,
            Granularity = Granularity.FiveMinute
        });

        Assert.Equal(byParams.Candles?.Count ?? 0, byRequest.Candles?.Count ?? 0);
    }

    // ─── GetServerTimeAsync ──────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetServerTime_returns_valid_timestamp()
    {
        var result = await Client.GetServerTimeAsync();

        Debug.WriteLine($"ISO:          {result.Iso}");
        Debug.WriteLine($"EpochSeconds: {result.EpochSeconds}");
        Debug.WriteLine($"EpochMillis:  {result.EpochMillis}");

        Assert.NotNull(result.Iso);
        Assert.NotNull(result.EpochSeconds);
        Assert.NotNull(result.EpochMillis);
        Assert.True(result.EpochSeconds > 0);
        Assert.True(result.EpochMillis > result.EpochSeconds);
    }

    // ─── ListPublicProductsAsync ─────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListPublicProducts_returns_products()
    {
        var result = await Client.ListPublicProductsAsync(limit: 10);

        Debug.WriteLine($"Products: {result.Products?.Count ?? 0}  NumProducts: {result.NumProducts}");
        foreach (var p in result.Products?.Take(5) ?? [])
            Debug.WriteLine($"  {p.ProductId}  Type={p.ProductType}  Price={p.Price}");

        Assert.NotNull(result.Products);
        Assert.True(result.Products!.Count > 0);
    }

    [CoinbaseIntegrationFact]
    public async Task ListPublicProducts_with_spot_filter_returns_spot_products()
    {
        var result = await Client.ListPublicProductsAsync(productType: "SPOT", limit: 5);

        Debug.WriteLine($"SPOT products: {result.Products?.Count ?? 0}");

        Assert.NotNull(result.Products);
        Assert.All(result.Products!, p => Assert.Equal("SPOT", p.ProductType));
    }

    [CoinbaseIntegrationFact]
    public async Task ListPublicProducts_with_product_ids_filter_returns_matching()
    {
        var result = await Client.ListPublicProductsAsync(productIds: ["BTC-USD", "ETH-USD"]);

        Debug.WriteLine($"Filtered products: {result.Products?.Count ?? 0}");
        foreach (var p in result.Products ?? [])
            Debug.WriteLine($"  {p.ProductId}");

        Assert.NotNull(result.Products);
    }

    [CoinbaseIntegrationFact]
    public async Task ListPublicProducts_request_overload_matches_params_overload()
    {
        var byParams = await Client.ListPublicProductsAsync(limit: 5);
        var byRequest = await Client.ListPublicProductsAsync(new ListPublicProductsRequest { Limit = 5 });

        Assert.Equal(byParams.Products?.Count ?? 0, byRequest.Products?.Count ?? 0);
    }

    [CoinbaseIntegrationFact]
    public async Task ListPublicProducts_pagination_metadata_populated()
    {
        var result = await Client.ListPublicProductsAsync(limit: 5);

        Debug.WriteLine($"NumProducts: {result.NumProducts}");
        Debug.WriteLine($"HasNext: {result.Pagination?.HasNext}  NextCursor: {result.Pagination?.NextCursor}");

        Assert.NotNull(result.NumProducts);
    }
}
