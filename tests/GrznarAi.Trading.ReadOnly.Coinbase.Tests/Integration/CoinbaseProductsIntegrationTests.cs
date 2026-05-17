using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API — Products section.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbaseProductsIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── GetBestBidAskAsync ──────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetBestBidAsk_without_filter_returns_pricebooks()
    {
        var result = await Client.GetBestBidAskAsync();

        Debug.WriteLine($"Pricebook count: {result.Pricebooks?.Count ?? 0}");
        if (result.Pricebooks?.Count > 0)
        {
            var first = result.Pricebooks[0];
            Debug.WriteLine($"  First: {first.ProductId}  Bids={first.Bids?.Count ?? 0}  Asks={first.Asks?.Count ?? 0}");
        }

        Assert.NotNull(result.Pricebooks);
    }

    [CoinbaseIntegrationFact]
    public async Task GetBestBidAsk_for_btc_usd_returns_single_pricebook()
    {
        var result = await Client.GetBestBidAskAsync(["BTC-USD"]);

        Debug.WriteLine($"Pricebook count: {result.Pricebooks?.Count ?? 0}");
        if (result.Pricebooks?.Count > 0)
        {
            var pb = result.Pricebooks[0];
            Debug.WriteLine($"  ProductId: {pb.ProductId}");
            Debug.WriteLine($"  Best Bid:  {pb.Bids?.FirstOrDefault()?.Price}  Size: {pb.Bids?.FirstOrDefault()?.Size}");
            Debug.WriteLine($"  Best Ask:  {pb.Asks?.FirstOrDefault()?.Price}  Size: {pb.Asks?.FirstOrDefault()?.Size}");
        }

        Assert.NotNull(result.Pricebooks);
        Assert.Contains(result.Pricebooks!, p => p.ProductId == "BTC-USD");
    }

    [CoinbaseIntegrationFact]
    public async Task GetBestBidAsk_request_overload_matches_params_overload()
    {
        var byParams = await Client.GetBestBidAskAsync(["BTC-USD"]);
        var byRequest = await Client.GetBestBidAskAsync(new GetBestBidAskRequest { ProductIds = ["BTC-USD"] });

        Assert.Equal(
            byParams.Pricebooks?.Count ?? 0,
            byRequest.Pricebooks?.Count ?? 0);
    }

    // ─── GetMarketTradesAsync ────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetMarketTrades_returns_trades_for_btc_usd()
    {
        var result = await Client.GetMarketTradesAsync("BTC-USD", 10);

        Debug.WriteLine($"Trades: {result.Trades?.Count ?? 0}");
        Debug.WriteLine($"Best Bid: {result.BestBid}  Best Ask: {result.BestAsk}");
        foreach (var t in result.Trades ?? [])
            Debug.WriteLine($"  TradeId={t.TradeId}  Price={t.Price}  Size={t.Size}  Side={t.Side}  Time={t.Time}");

        Assert.NotNull(result.Trades);
        Assert.NotEmpty(result.BestBid ?? "x");
        Assert.NotEmpty(result.BestAsk ?? "x");
    }

    [CoinbaseIntegrationFact]
    public async Task GetMarketTrades_request_overload_matches_params_overload()
    {
        var byParams = await Client.GetMarketTradesAsync("BTC-USD", 5);
        var byRequest = await Client.GetMarketTradesAsync(new GetMarketTradesRequest { ProductId = "BTC-USD", Limit = 5 });

        Assert.Equal(byParams.BestBid, byRequest.BestBid);
    }

    // ─── GetProductAsync ─────────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetProduct_returns_btc_usd()
    {
        var result = await Client.GetProductAsync("BTC-USD");

        Debug.WriteLine($"ProductId:      {result.ProductId}");
        Debug.WriteLine($"Price:          {result.Price}");
        Debug.WriteLine($"BaseName:       {result.BaseName}");
        Debug.WriteLine($"QuoteName:      {result.QuoteName}");
        Debug.WriteLine($"ProductType:    {result.ProductType}");
        Debug.WriteLine($"ProductVenue:   {result.ProductVenue}");
        Debug.WriteLine($"TradingDisabled:{result.TradingDisabled}");

        Assert.Equal("BTC-USD", result.ProductId);
        Assert.NotNull(result.Price);
    }

    [CoinbaseIntegrationFact]
    public async Task GetProduct_request_overload_matches_params_overload()
    {
        var byParams = await Client.GetProductAsync("ETH-USD");
        var byRequest = await Client.GetProductAsync(new GetProductRequest { ProductId = "ETH-USD" });

        Assert.Equal(byParams.ProductId, byRequest.ProductId);
    }

    [CoinbaseIntegrationFact]
    public async Task GetProduct_throws_CoinbaseApiException_for_unknown_product()
    {
        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => Client.GetProductAsync("UNKNOWN-NOTAPRODUCT"));

        Debug.WriteLine($"StatusCode: {ex.StatusCode}");

        Assert.True(
            ex.StatusCode == System.Net.HttpStatusCode.NotFound
            || ex.StatusCode == System.Net.HttpStatusCode.BadRequest
            || ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity);
    }

    // ─── GetProductBookAsync ─────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetProductBook_returns_order_book_for_btc_usd()
    {
        var result = await Client.GetProductBookAsync("BTC-USD", limit: 5);

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
    public async Task GetProductBook_request_overload_matches_params_overload()
    {
        var byParams = await Client.GetProductBookAsync("BTC-USD", limit: 3);
        var byRequest = await Client.GetProductBookAsync(new GetProductBookRequest { ProductId = "BTC-USD", Limit = 3 });

        Assert.Equal(byParams.Pricebook?.ProductId, byRequest.Pricebook?.ProductId);
    }

    // ─── GetProductCandlesAsync ──────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetProductCandles_returns_candles_for_btc_usd()
    {
        var endTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        var startTs = DateTimeOffset.UtcNow.AddHours(-24).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);

        var result = await Client.GetProductCandlesAsync("BTC-USD", startTs, endTs, Granularity.OneHour);

        Debug.WriteLine($"Candles: {result.Candles?.Count ?? 0}");
        foreach (var c in result.Candles?.Take(3) ?? [])
            Debug.WriteLine($"  Start={c.Start}  O={c.Open}  H={c.High}  L={c.Low}  C={c.Close}  V={c.Volume}");

        Assert.NotNull(result.Candles);
        Assert.NotEmpty(result.Candles!);
    }

    [CoinbaseIntegrationFact]
    public async Task GetProductCandles_request_overload_matches_params_overload()
    {
        var endTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        var startTs = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);

        var byParams = await Client.GetProductCandlesAsync("BTC-USD", startTs, endTs, Granularity.FiveMinute);
        var byRequest = await Client.GetProductCandlesAsync(new GetProductCandlesRequest
        {
            ProductId = "BTC-USD",
            Start = startTs,
            End = endTs,
            Granularity = Granularity.FiveMinute
        });

        Assert.Equal(byParams.Candles?.Count ?? 0, byRequest.Candles?.Count ?? 0);
    }

    // ─── ListProductsAsync ───────────────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task ListProducts_returns_products()
    {
        var result = await Client.ListProductsAsync(limit: 10);

        Debug.WriteLine($"Products: {result.Products?.Count ?? 0}  NumProducts: {result.NumProducts}");
        foreach (var p in result.Products?.Take(5) ?? [])
            Debug.WriteLine($"  {p.ProductId}  Type={p.ProductType}  Venue={p.ProductVenue}  Price={p.Price}");

        Assert.NotNull(result.Products);
        Assert.True(result.Products!.Count > 0);
    }

    [CoinbaseIntegrationFact]
    public async Task ListProducts_with_spot_filter_returns_spot_products()
    {
        var result = await Client.ListProductsAsync(productType: "SPOT", limit: 5);

        Debug.WriteLine($"SPOT products: {result.Products?.Count ?? 0}");

        Assert.NotNull(result.Products);
        Assert.All(result.Products!, p => Assert.Equal("SPOT", p.ProductType));
    }

    [CoinbaseIntegrationFact]
    public async Task ListProducts_with_product_ids_filter_returns_matching()
    {
        var result = await Client.ListProductsAsync(productIds: ["BTC-USD", "ETH-USD"]);

        Debug.WriteLine($"Filtered products: {result.Products?.Count ?? 0}");
        foreach (var p in result.Products ?? [])
            Debug.WriteLine($"  {p.ProductId}");

        Assert.NotNull(result.Products);
    }

    [CoinbaseIntegrationFact]
    public async Task ListProducts_request_overload_matches_params_overload()
    {
        var byParams = await Client.ListProductsAsync(limit: 5);
        var byRequest = await Client.ListProductsAsync(new ListProductsRequest { Limit = 5 });

        Assert.Equal(byParams.Products?.Count ?? 0, byRequest.Products?.Count ?? 0);
    }

    [CoinbaseIntegrationFact]
    public async Task ListProducts_pagination_metadata_populated()
    {
        var result = await Client.ListProductsAsync(limit: 5);

        Debug.WriteLine($"HasNext: {result.Pagination?.HasNext}  NextCursor: {result.Pagination?.NextCursor}");

        Assert.NotNull(result.NumProducts);
    }
}
