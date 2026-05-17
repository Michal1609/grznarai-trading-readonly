using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientFuturesTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    public CoinbaseClientFuturesTests()
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

    // ─── GetCurrentMarginWindowAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetCurrentMarginWindow_no_params_calls_bare_path()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/cfm/intraday/current_margin_window")
            .Respond("application/json", MinimalMarginWindowJson());

        var result = await _client.GetCurrentMarginWindowAsync();

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetCurrentMarginWindow_margin_profile_type_adds_query_param()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/cfm/intraday/current_margin_window" +
                "?margin_profile_type=MARGIN_PROFILE_TYPE_RETAIL_INTRADAY_MARGIN_1")
            .Respond("application/json", MinimalMarginWindowJson());

        await _client.GetCurrentMarginWindowAsync(
            marginProfileType: MarginProfileType.RetailIntradayMargin1);

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetCurrentMarginWindow_request_overload_delegates_correctly()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/cfm/intraday/current_margin_window" +
                "?margin_profile_type=MARGIN_PROFILE_TYPE_RETAIL_REGULAR")
            .Respond("application/json", MinimalMarginWindowJson());

        await _client.GetCurrentMarginWindowAsync(new GetCurrentMarginWindowRequest
        {
            MarginProfileType = MarginProfileType.RetailRegular
        });

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetCurrentMarginWindow_request_overload_throws_on_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.GetCurrentMarginWindowAsync((GetCurrentMarginWindowRequest)null!));
    }

    [Fact]
    public async Task GetCurrentMarginWindow_deserializes_full_response()
    {
        const string json = """
            {
              "margin_window": {
                "margin_window_type": "MARGIN_WINDOW_TYPE_INTRADAY",
                "end_time": "2024-01-15T21:00:00Z"
              },
              "is_intraday_margin_killswitch_enabled": false,
              "is_intraday_margin_enrollment_killswitch_enabled": true
            }
            """;

        _mock.When("*/current_margin_window").Respond("application/json", json);

        var result = await _client.GetCurrentMarginWindowAsync();

        Assert.Equal(MarginWindowType.Intraday, result.MarginWindow!.MarginWindowType);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 21, 0, 0, TimeSpan.Zero), result.MarginWindow.EndTime);
        Assert.False(result.IsIntradayMarginKillswitchEnabled);
        Assert.True(result.IsIntradayMarginEnrollmentKillswitchEnabled);
    }

    [Fact]
    public async Task GetCurrentMarginWindow_throws_on_401()
    {
        _mock.When("*/current_margin_window")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetCurrentMarginWindowAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    // ─── GetFuturesBalanceSummaryAsync ────────────────────────────────────────

    [Fact]
    public async Task GetFuturesBalanceSummary_calls_correct_path()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/cfm/balance_summary")
            .Respond("application/json", """{"balance_summary":{}}""");

        var result = await _client.GetFuturesBalanceSummaryAsync();

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetFuturesBalanceSummary_deserializes_full_response()
    {
        const string json = """
            {
              "balance_summary": {
                "futures_buying_power": { "value": "5000.00", "currency": "USD" },
                "total_usd_balance": { "value": "10000.00", "currency": "USD" },
                "unrealized_pnl": { "value": "-250.50", "currency": "USD" },
                "daily_realized_pnl": { "value": "100.00", "currency": "USD" },
                "liquidation_buffer_percentage": "25.5",
                "intraday_margin_window_measure": {
                  "margin_window_type": "FCM_MARGIN_WINDOW_TYPE_INTRADAY",
                  "margin_level": "MARGIN_LEVEL_TYPE_BASE",
                  "initial_margin": "1000.00",
                  "futures_buying_power": "5000.00"
                },
                "overnight_margin_window_measure": {
                  "margin_window_type": "FCM_MARGIN_WINDOW_TYPE_OVERNIGHT",
                  "margin_level": "MARGIN_LEVEL_TYPE_WARNING"
                }
              }
            }
            """;

        _mock.When("*/balance_summary").Respond("application/json", json);

        var result = await _client.GetFuturesBalanceSummaryAsync();

        var summary = result.BalanceSummary!;
        Assert.Equal("5000.00", summary.FuturesBuyingPower!.Value);
        Assert.Equal("USD", summary.FuturesBuyingPower.Currency);
        Assert.Equal("10000.00", summary.TotalUsdBalance!.Value);
        Assert.Equal("-250.50", summary.UnrealizedPnl!.Value);
        Assert.Equal("100.00", summary.DailyRealizedPnl!.Value);
        Assert.Equal("25.5", summary.LiquidationBufferPercentage);
        Assert.Equal(FcmMarginWindowType.Intraday, summary.IntradayMarginWindowMeasure!.MarginWindowType);
        Assert.Equal(MarginLevelType.Base, summary.IntradayMarginWindowMeasure.MarginLevel);
        Assert.Equal("1000.00", summary.IntradayMarginWindowMeasure.InitialMargin);
        Assert.Equal(FcmMarginWindowType.Overnight, summary.OvernightMarginWindowMeasure!.MarginWindowType);
        Assert.Equal(MarginLevelType.Warning, summary.OvernightMarginWindowMeasure.MarginLevel);
    }

    [Fact]
    public async Task GetFuturesBalanceSummary_deserializes_cbrn_field()
    {
        const string json = """
            {
              "balance_summary": {
                "futures_buying_power": {
                  "value": "1000.00",
                  "currency": "USD",
                  "cbrn": "crn:v0:mainnet:asset:abc123"
                }
              }
            }
            """;

        _mock.When("*/balance_summary").Respond("application/json", json);

        var result = await _client.GetFuturesBalanceSummaryAsync();

        Assert.Equal("crn:v0:mainnet:asset:abc123",
            result.BalanceSummary!.FuturesBuyingPower!.Cbrn);
    }

    // ─── GetFuturesPositionAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetFuturesPosition_calls_correct_path()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/cfm/positions/BIT-28JUL23-CDE")
            .Respond("application/json", """{"position":{}}""");

        var result = await _client.GetFuturesPositionAsync("BIT-28JUL23-CDE");

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetFuturesPosition_throws_on_empty_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetFuturesPositionAsync(""));
    }

    [Fact]
    public async Task GetFuturesPosition_throws_on_whitespace_product_id()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetFuturesPositionAsync("   "));
    }

    [Fact]
    public async Task GetFuturesPosition_deserializes_full_response()
    {
        const string json = """
            {
              "position": {
                "product_id": "BIT-28JUL23-CDE",
                "expiration_time": "2023-07-28T00:00:00Z",
                "side": "LONG",
                "number_of_contracts": "5",
                "current_price": "29500.00",
                "avg_entry_price": "29000.00",
                "unrealized_pnl": "2500.00",
                "daily_realized_pnl": "500.00"
              }
            }
            """;

        _mock.When("*/cfm/positions/*").Respond("application/json", json);

        var result = await _client.GetFuturesPositionAsync("BIT-28JUL23-CDE");

        var pos = result.Position!;
        Assert.Equal("BIT-28JUL23-CDE", pos.ProductId);
        Assert.Equal(new DateTimeOffset(2023, 7, 28, 0, 0, 0, TimeSpan.Zero), pos.ExpirationTime);
        Assert.Equal(FuturesPositionSide.Long, pos.Side);
        Assert.Equal("5", pos.NumberOfContracts);
        Assert.Equal("29500.00", pos.CurrentPrice);
        Assert.Equal("29000.00", pos.AvgEntryPrice);
        Assert.Equal("2500.00", pos.UnrealizedPnl);
        Assert.Equal("500.00", pos.DailyRealizedPnl);
    }

    [Fact]
    public async Task GetFuturesPosition_throws_on_404()
    {
        _mock.When("*/cfm/positions/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"NOT_FOUND"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetFuturesPositionAsync("UNKNOWN-PRODUCT"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    // ─── GetIntradayMarginSettingAsync ────────────────────────────────────────

    [Fact]
    public async Task GetIntradayMarginSetting_calls_correct_path()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/cfm/intraday/margin_setting")
            .Respond("application/json", """{"setting":"INTRADAY_MARGIN_SETTING_STANDARD"}""");

        var result = await _client.GetIntradayMarginSettingAsync();

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetIntradayMarginSetting_deserializes_setting()
    {
        _mock.When("*/margin_setting")
            .Respond("application/json", """{"setting":"INTRADAY_MARGIN_SETTING_INTRADAY"}""");

        var result = await _client.GetIntradayMarginSettingAsync();

        Assert.Equal(IntradayMarginSetting.Intraday, result.Setting);
    }

    // ─── ListFuturesPositionsAsync ────────────────────────────────────────────

    [Fact]
    public async Task ListFuturesPositions_calls_correct_path()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/cfm/positions")
            .Respond("application/json", """{"positions":[]}""");

        var result = await _client.ListFuturesPositionsAsync();

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListFuturesPositions_deserializes_positions()
    {
        const string json = """
            {
              "positions": [
                {
                  "product_id": "BIT-28JUL23-CDE",
                  "side": "SHORT",
                  "number_of_contracts": "2",
                  "unrealized_pnl": "-100.00"
                },
                {
                  "product_id": "ETH-29SEP23-CDE",
                  "side": "LONG",
                  "number_of_contracts": "10",
                  "unrealized_pnl": "500.00"
                }
              ]
            }
            """;

        _mock.When("*/cfm/positions").Respond("application/json", json);

        var result = await _client.ListFuturesPositionsAsync();

        Assert.Equal(2, result.Positions!.Count);
        Assert.Equal("BIT-28JUL23-CDE", result.Positions[0].ProductId);
        Assert.Equal(FuturesPositionSide.Short, result.Positions[0].Side);
        Assert.Equal("ETH-29SEP23-CDE", result.Positions[1].ProductId);
        Assert.Equal(FuturesPositionSide.Long, result.Positions[1].Side);
    }

    // ─── ListFuturesSweepsAsync ───────────────────────────────────────────────

    [Fact]
    public async Task ListFuturesSweeps_calls_correct_path()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/cfm/sweeps")
            .Respond("application/json", """{"sweeps":[]}""");

        var result = await _client.ListFuturesSweepsAsync();

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ListFuturesSweeps_deserializes_sweeps()
    {
        const string json = """
            {
              "sweeps": [
                {
                  "id": "sweep-123",
                  "requested_amount": { "value": "500.00", "currency": "USD" },
                  "should_sweep_all": false,
                  "status": "PENDING",
                  "scheduled_time": "2024-01-15T10:00:00Z"
                }
              ]
            }
            """;

        _mock.When("*/cfm/sweeps").Respond("application/json", json);

        var result = await _client.ListFuturesSweepsAsync();

        var sweep = Assert.Single(result.Sweeps!);
        Assert.Equal("sweep-123", sweep.Id);
        Assert.Equal("500.00", sweep.RequestedAmount!.Value);
        Assert.Equal("USD", sweep.RequestedAmount.Currency);
        Assert.False(sweep.ShouldSweepAll);
        Assert.Equal(FuturesSweepStatus.Pending, sweep.Status);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero), sweep.ScheduledTime);
    }

    [Fact]
    public async Task ListFuturesSweeps_deserializes_processing_status()
    {
        const string json = """
            {
              "sweeps": [
                {
                  "id": "sweep-456",
                  "should_sweep_all": true,
                  "status": "PROCESSING"
                }
              ]
            }
            """;

        _mock.When("*/cfm/sweeps").Respond("application/json", json);

        var result = await _client.ListFuturesSweepsAsync();

        var sweep = Assert.Single(result.Sweeps!);
        Assert.Equal(FuturesSweepStatus.Processing, sweep.Status);
        Assert.True(sweep.ShouldSweepAll);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string MinimalMarginWindowJson() =>
        """{"margin_window":{"margin_window_type":"MARGIN_WINDOW_TYPE_INTRADAY"},"is_intraday_margin_killswitch_enabled":false,"is_intraday_margin_enrollment_killswitch_enabled":false}""";
}
