using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.DataApi;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientDataApiTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientDataApiTests()
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

    // ─── GetApiKeyPermissionsAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetApiKeyPermissions_calls_correct_endpoint()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/key_permissions")
            .Respond("application/json", FullPermissionsJson());

        var result = await _client.GetApiKeyPermissionsAsync();

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetApiKeyPermissions_deserializes_all_fields()
    {
        _mock.When("*/key_permissions").Respond("application/json", FullPermissionsJson());

        var result = await _client.GetApiKeyPermissionsAsync();

        Assert.True(result.CanView);
        Assert.False(result.CanTrade);
        Assert.False(result.CanTransfer);
        Assert.True(result.CanReceive);
        Assert.Equal("portfolio-uuid-123", result.PortfolioUuid);
        Assert.Equal(PortfolioType.Consumer, result.PortfolioType);
    }

    [Fact]
    public async Task GetApiKeyPermissions_handles_undefined_portfolio_type()
    {
        const string json = """
            {
              "can_view": true,
              "can_trade": false,
              "can_transfer": false,
              "can_receive": false,
              "portfolio_uuid": "",
              "portfolio_type": "UNDEFINED"
            }
            """;

        _mock.When("*/key_permissions").Respond("application/json", json);

        var result = await _client.GetApiKeyPermissionsAsync();

        Assert.Equal(PortfolioType.Undefined, result.PortfolioType);
    }

    [Fact]
    public async Task GetApiKeyPermissions_handles_intx_portfolio_type()
    {
        const string json = """
            {
              "can_view": true,
              "can_trade": true,
              "can_transfer": true,
              "can_receive": false,
              "portfolio_uuid": "intx-portfolio-456",
              "portfolio_type": "INTX"
            }
            """;

        _mock.When("*/key_permissions").Respond("application/json", json);

        var result = await _client.GetApiKeyPermissionsAsync();

        Assert.True(result.CanTrade);
        Assert.True(result.CanTransfer);
        Assert.Equal(PortfolioType.Intx, result.PortfolioType);
        Assert.Equal("intx-portfolio-456", result.PortfolioUuid);
    }

    [Fact]
    public async Task GetApiKeyPermissions_throws_on_401()
    {
        _mock.When("*/key_permissions")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetApiKeyPermissionsAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task GetApiKeyPermissions_throws_on_403()
    {
        _mock.When("*/key_permissions")
            .Respond(HttpStatusCode.Forbidden, "application/json", """{"error":"FORBIDDEN"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetApiKeyPermissionsAsync());

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string FullPermissionsJson() =>
        """
        {
          "can_view": true,
          "can_trade": false,
          "can_transfer": false,
          "can_receive": true,
          "portfolio_uuid": "portfolio-uuid-123",
          "portfolio_type": "CONSUMER"
        }
        """;
}
