using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientPortfoliosTests
{
    private static (CoinbaseClient client, MockHttpMessageHandler mock) CreateClient()
    {
        var mock = new MockHttpMessageHandler();
        var http = mock.ToHttpClient();
        http.BaseAddress = new Uri("https://api.coinbase.com");
        var opts = Options.Create(new CoinbaseOptions
        {
            KeyName = "test",
            PrivateKeyPem = "test",
            BaseUrl = "https://api.coinbase.com"
        });
        return (new CoinbaseClient(http, opts), mock);
    }

    [Fact]
    public async Task GetPortfolioBreakdown_builds_correct_path_and_query()
    {
        var (client, mock) = CreateClient();
        const string uuid = "abc-123";
        mock.Expect(HttpMethod.Get, $"https://api.coinbase.com/api/v3/brokerage/portfolios/{uuid}?currency=USD")
            .Respond("application/json", """{"breakdown":{"portfolio":{"uuid":"abc-123"}}}""");

        var result = await client.GetPortfolioBreakdownAsync(uuid, "USD");

        Assert.Equal("abc-123", result.Breakdown!.Portfolio!.Uuid);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPortfolioBreakdown_without_currency_omits_query()
    {
        var (client, mock) = CreateClient();
        mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/portfolios/uuid-1")
            .Respond("application/json", """{"breakdown":{"portfolio":{"uuid":"uuid-1"}}}""");

        await client.GetPortfolioBreakdownAsync("uuid-1");

        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPortfolioBreakdown_throws_on_404()
    {
        var (client, mock) = CreateClient();
        mock.When("*/portfolios/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => client.GetPortfolioBreakdownAsync("missing"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("/portfolios/{uuid}", ex.Endpoint);
    }

    [Fact]
    public async Task GetPortfolioBreakdown_rejects_empty_uuid()
    {
        var (client, _) = CreateClient();
        await Assert.ThrowsAsync<ArgumentException>(
            () => client.GetPortfolioBreakdownAsync(""));
    }

    [Fact]
    public async Task GetPortfolioBreakdown_request_overload_delegates()
    {
        var (client, mock) = CreateClient();
        mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/portfolios/x?currency=EUR")
            .Respond("application/json", """{"breakdown":{"portfolio":{"uuid":"x"}}}""");

        await client.GetPortfolioBreakdownAsync(new GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios.GetPortfolioBreakdownRequest
        {
            PortfolioUuid = "x",
            Currency = "EUR"
        });

        mock.VerifyNoOutstandingExpectation();
    }
}
