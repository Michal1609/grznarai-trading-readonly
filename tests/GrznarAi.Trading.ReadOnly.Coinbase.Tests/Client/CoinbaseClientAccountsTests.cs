using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientAccountsTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientAccountsTests()
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

    // ─── ListAccountsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ListAccounts_no_params_builds_bare_path()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/accounts")
            .Respond("application/json", """{"accounts":[],"has_next":false,"cursor":"","size":0}""");

        var result = await _client.ListAccountsAsync();

        Assert.NotNull(result.Accounts);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListAccounts_with_limit_adds_query_param()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/accounts?limit=10")
            .Respond("application/json", """{"accounts":[],"has_next":false,"size":0}""");

        await _client.ListAccountsAsync(limit: 10);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListAccounts_with_cursor_adds_query_param()
    {
        _mock.Expect(HttpMethod.Get, "https://api.coinbase.com/api/v3/brokerage/accounts?cursor=abc")
            .Respond("application/json", """{"accounts":[],"has_next":false,"size":0}""");

        await _client.ListAccountsAsync(cursor: "abc");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListAccounts_with_retail_portfolio_id_adds_query_param()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/accounts?retail_portfolio_id=pid-1")
            .Respond("application/json", """{"accounts":[],"has_next":false,"size":0}""");

        await _client.ListAccountsAsync(retailPortfolioId: "pid-1");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListAccounts_with_all_params_builds_full_query()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/accounts?limit=50&cursor=tok&retail_portfolio_id=pid-2")
            .Respond("application/json", """{"accounts":[],"has_next":true,"cursor":"next","size":50}""");

        var result = await _client.ListAccountsAsync(limit: 50, cursor: "tok", retailPortfolioId: "pid-2");

        Assert.Equal(true, result.HasNext);
        Assert.Equal("next", result.Cursor);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListAccounts_request_overload_delegates_correctly()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/accounts?limit=5&cursor=c1&retail_portfolio_id=p1")
            .Respond("application/json", """{"accounts":[],"has_next":false,"size":0}""");

        await _client.ListAccountsAsync(new ListAccountsRequest
        {
            Limit = 5,
            Cursor = "c1",
            RetailPortfolioId = "p1"
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task ListAccounts_rejects_non_positive_limit(int limit)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _client.ListAccountsAsync(limit: limit));
    }

    [Fact]
    public async Task ListAccounts_request_overload_throws_on_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.ListAccountsAsync((ListAccountsRequest)null!));
    }

    [Fact]
    public async Task ListAccounts_deserializes_accounts_correctly()
    {
        const string json = """
            {
              "accounts": [
                {
                  "uuid": "uuid-1",
                  "name": "BTC Wallet",
                  "currency": "BTC",
                  "available_balance": { "value": "0.5", "currency": "BTC" },
                  "default": true,
                  "active": true,
                  "type": "ACCOUNT_TYPE_CRYPTO",
                  "ready": true,
                  "hold": { "value": "0.0", "currency": "BTC" },
                  "retail_portfolio_id": "port-1",
                  "platform": "ACCOUNT_PLATFORM_CONSUMER"
                }
              ],
              "has_next": false,
              "cursor": "",
              "size": 1
            }
            """;

        _mock.When("*/accounts").Respond("application/json", json);

        var result = await _client.ListAccountsAsync();

        Assert.Equal(1, result.Size);
        var acc = Assert.Single(result.Accounts!);
        Assert.Equal("uuid-1", acc.Uuid);
        Assert.Equal("BTC Wallet", acc.Name);
        Assert.Equal("BTC", acc.Currency);
        Assert.Equal(0.5m, acc.AvailableBalance!.Value);
        Assert.Equal("BTC", acc.AvailableBalance.Currency);
        Assert.True(acc.Default);
        Assert.True(acc.Active);
        Assert.Equal(AccountType.Crypto, acc.Type);
        Assert.True(acc.Ready);
        Assert.Equal(0.0m, acc.Hold!.Value);
        Assert.Equal("port-1", acc.RetailPortfolioId);
        Assert.Equal(AccountPlatform.Consumer, acc.Platform);
    }

    [Fact]
    public async Task ListAccounts_throws_on_401()
    {
        _mock.When("*/accounts")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.ListAccountsAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    // ─── GetAccountAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAccount_builds_correct_path()
    {
        const string uuid = "acct-123";
        _mock.Expect(HttpMethod.Get, $"https://api.coinbase.com/api/v3/brokerage/accounts/{uuid}")
            .Respond("application/json", """{"account":{"uuid":"acct-123","name":"USD Wallet","currency":"USD"}}""");

        var result = await _client.GetAccountAsync(uuid);

        Assert.Equal("acct-123", result.Account!.Uuid);
        Assert.Equal("USD Wallet", result.Account.Name);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetAccount_url_encodes_uuid()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/accounts/uuid%2Fwith%2Fslashes")
            .Respond("application/json", """{"account":{"uuid":"uuid/with/slashes"}}""");

        await _client.GetAccountAsync("uuid/with/slashes");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetAccount_sanitizes_uuid_in_exception_endpoint()
    {
        _mock.When("*/accounts/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"not_found"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetAccountAsync("missing-uuid"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("/accounts/{uuid}", ex.Endpoint);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAccount_rejects_blank_uuid(string uuid)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetAccountAsync(uuid));
    }

    [Fact]
    public async Task GetAccount_deserializes_full_response()
    {
        const string json = """
            {
              "account": {
                "uuid": "full-uuid",
                "name": "ETH Wallet",
                "currency": "ETH",
                "available_balance": { "value": "2.5", "currency": "ETH" },
                "default": false,
                "active": true,
                "created_at": "2024-01-15T10:00:00Z",
                "updated_at": "2024-06-01T08:30:00Z",
                "type": "ACCOUNT_TYPE_CRYPTO",
                "ready": true,
                "hold": { "value": "0.1", "currency": "ETH" },
                "retail_portfolio_id": "portfolio-x",
                "platform": "ACCOUNT_PLATFORM_CFM_CONSUMER"
              }
            }
            """;

        _mock.When("*/accounts/*").Respond("application/json", json);

        var result = await _client.GetAccountAsync("full-uuid");
        var acc = result.Account!;

        Assert.Equal("full-uuid", acc.Uuid);
        Assert.Equal("ETH", acc.Currency);
        Assert.Equal(2.5m, acc.AvailableBalance!.Value);
        Assert.False(acc.Default);
        Assert.True(acc.Active);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero), acc.CreatedAt);
        Assert.Equal(AccountType.Crypto, acc.Type);
        Assert.Equal(0.1m, acc.Hold!.Value);
        Assert.Equal(AccountPlatform.CfmConsumer, acc.Platform);
    }
}
