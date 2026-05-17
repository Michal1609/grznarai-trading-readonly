using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientPortfoliosTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientPortfoliosTests()
    {
        var http = _mock.ToHttpClient();
        http.BaseAddress = new Uri("https://api.coinbase.com");
        var opts = Options.Create(new CoinbaseOptions
        {
            KeyName = "test",
            PrivateKeyPem = "test",
            BaseUrl = "https://api.coinbase.com"
        });
        _client = new CoinbaseClient(http, opts);
    }

    public void Dispose()
    {
        _mock.Dispose();
        GC.SuppressFinalize(this);
    }

    // ─── ListPortfoliosAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task ListPortfolios_without_filter_calls_correct_url()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/portfolios")
            .Respond("application/json", """{"portfolios":[{"uuid":"p1","name":"Default","type":"DEFAULT"}]}""");

        var result = await _client.ListPortfoliosAsync();

        Assert.NotNull(result.Portfolios);
        Assert.Single(result.Portfolios!);
        Assert.Equal("p1", result.Portfolios![0].Uuid);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListPortfolios_with_type_filter_appends_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/portfolios?portfolio_type=CONSUMER")
            .Respond("application/json", """{"portfolios":[]}""");

        var result = await _client.ListPortfoliosAsync(portfolioType: "CONSUMER");

        Assert.NotNull(result.Portfolios);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListPortfolios_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/portfolios?portfolio_type=INTX")
            .Respond("application/json", """{"portfolios":[]}""");

        await _client.ListPortfoliosAsync(new ListPortfoliosRequest { PortfolioType = "INTX" });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListPortfolios_request_overload_rejects_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.ListPortfoliosAsync((ListPortfoliosRequest)null!));
    }

    // ─── GetPortfolioBreakdownAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetPortfolioBreakdown_builds_correct_path_and_query()
    {
        const string uuid = "abc-123";
        _mock.Expect(HttpMethod.Get, $"https://api.coinbase.com/api/v3/brokerage/portfolios/{uuid}?currency=USD")
            .Respond("application/json", """{"breakdown":{"portfolio":{"uuid":"abc-123"}}}""");

        var result = await _client.GetPortfolioBreakdownAsync(uuid, "USD");

        Assert.Equal("abc-123", result.Breakdown!.Portfolio!.Uuid);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPortfolioBreakdown_without_currency_omits_query()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/portfolios/uuid-1")
            .Respond("application/json", """{"breakdown":{"portfolio":{"uuid":"uuid-1"}}}""");

        await _client.GetPortfolioBreakdownAsync("uuid-1");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPortfolioBreakdown_throws_on_404()
    {
        _mock.When("*/portfolios/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetPortfolioBreakdownAsync("missing"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("/portfolios/{uuid}", ex.Endpoint);
    }

    [Fact]
    public async Task GetPortfolioBreakdown_rejects_empty_uuid()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPortfolioBreakdownAsync(""));
    }

    [Fact]
    public async Task GetPortfolioBreakdown_request_overload_delegates()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/portfolios/x?currency=EUR")
            .Respond("application/json", """{"breakdown":{"portfolio":{"uuid":"x"}}}""");

        await _client.GetPortfolioBreakdownAsync(new GetPortfolioBreakdownRequest
        {
            PortfolioUuid = "x",
            Currency = "EUR"
        });

        _mock.VerifyNoOutstandingExpectation();
    }
}
