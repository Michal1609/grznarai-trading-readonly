using System.Net;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class CoinbaseClientPerpetualTests : IDisposable
{
    private readonly MockHttpMessageHandler _mock = new();
    private readonly CoinbaseClient _client;

    private const string PortfolioUuid = "portfolio-uuid-123";
    private const string Symbol = "BTC-PERP-INTX";

    public CoinbaseClientPerpetualTests()
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

    // ─── GetPerpetualPortfolioSummaryAsync ───────────────────────────────────

    [Fact]
    public async Task GetPerpetualPortfolioSummary_builds_correct_path()
    {
        _mock.Expect(HttpMethod.Get,
                $"https://api.coinbase.com/api/v3/brokerage/intx/portfolio/{PortfolioUuid}")
            .Respond("application/json", MinimalPortfolioSummaryJson());

        var result = await _client.GetPerpetualPortfolioSummaryAsync(PortfolioUuid);

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPerpetualPortfolioSummary_throws_on_blank_portfolio_uuid(string uuid)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPerpetualPortfolioSummaryAsync(uuid));
    }

    [Fact]
    public async Task GetPerpetualPortfolioSummary_url_encodes_uuid()
    {
        _mock.Expect(HttpMethod.Get,
                "https://api.coinbase.com/api/v3/brokerage/intx/portfolio/uuid%2Fwith%2Fslash")
            .Respond("application/json", MinimalPortfolioSummaryJson());

        await _client.GetPerpetualPortfolioSummaryAsync("uuid/with/slash");

        _mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPerpetualPortfolioSummary_deserializes_full_response()
    {
        const string json = """
            {
              "portfolios": [
                {
                  "portfolio_uuid": "portfolio-uuid-123",
                  "collateral": "10000.00",
                  "position_notional": "5000.00",
                  "open_position_notional": "1000.00",
                  "pending_fees": "2.50",
                  "borrow": "0.00",
                  "accrued_interest": "0.10",
                  "rolling_debt": "0.00",
                  "portfolio_initial_margin": "500.00",
                  "portfolio_im_notional": { "value": "500.00", "currency": "USD" },
                  "portfolio_maintenance_margin": "250.00",
                  "portfolio_mm_notional": { "value": "250.00", "currency": "USD" },
                  "liquidation_percentage": "0.05",
                  "liquidation_buffer": "0.10",
                  "margin_type": "MARGIN_TYPE_CROSS",
                  "margin_flags": "PORTFOLIO_MARGIN_FLAGS_UNSPECIFIED",
                  "liquidation_status": "PORTFOLIO_LIQUIDATION_STATUS_NOT_LIQUIDATING",
                  "unrealized_pnl": { "value": "150.00", "currency": "USD" },
                  "total_balance": { "value": "10150.00", "currency": "USD" }
                }
              ],
              "summary": {
                "unrealized_pnl": { "value": "150.00", "currency": "USD" },
                "buying_power": { "value": "9500.00", "currency": "USD" },
                "total_balance": { "value": "10150.00", "currency": "USD" },
                "max_withdrawal_amount": { "value": "8000.00", "currency": "USD" }
              }
            }
            """;

        _mock.When("*/intx/portfolio/*").Respond("application/json", json);

        var result = await _client.GetPerpetualPortfolioSummaryAsync(PortfolioUuid);

        var portfolio = Assert.Single(result.Portfolios!);
        Assert.Equal("portfolio-uuid-123", portfolio.PortfolioUuid);
        Assert.Equal("10000.00", portfolio.Collateral);
        Assert.Equal("5000.00", portfolio.PositionNotional);
        Assert.Equal("2.50", portfolio.PendingFees);
        Assert.Equal("500.00", portfolio.PortfolioImNotional!.Value);
        Assert.Equal("USD", portfolio.PortfolioImNotional.Currency);
        Assert.Equal(MarginType.Cross, portfolio.MarginType);
        Assert.Equal(PortfolioMarginFlags.Unspecified, portfolio.MarginFlags);
        Assert.Equal(PortfolioLiquidationStatus.NotLiquidating, portfolio.LiquidationStatus);
        Assert.Equal("150.00", portfolio.UnrealizedPnl!.Value);

        var summary = result.Summary!;
        Assert.Equal("9500.00", summary.BuyingPower!.Value);
        Assert.Equal("8000.00", summary.MaxWithdrawalAmount!.Value);
    }

    [Fact]
    public async Task GetPerpetualPortfolioSummary_throws_on_401()
    {
        _mock.When("*/intx/portfolio/*")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetPerpetualPortfolioSummaryAsync(PortfolioUuid));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    // ─── GetPerpetualPositionAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetPerpetualPosition_builds_correct_path()
    {
        _mock.Expect(HttpMethod.Get,
                $"https://api.coinbase.com/api/v3/brokerage/intx/positions/{PortfolioUuid}/{Symbol}")
            .Respond("application/json", """{"position":{}}""");

        var result = await _client.GetPerpetualPositionAsync(PortfolioUuid, Symbol);

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPerpetualPosition_throws_on_blank_portfolio_uuid(string uuid)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPerpetualPositionAsync(uuid, Symbol));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPerpetualPosition_throws_on_blank_symbol(string symbol)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPerpetualPositionAsync(PortfolioUuid, symbol));
    }

    [Fact]
    public async Task GetPerpetualPosition_deserializes_full_response()
    {
        const string json = """
            {
              "position": {
                "product_id": "BTC-PERP-INTX",
                "product_uuid": "product-uuid-abc",
                "portfolio_uuid": "portfolio-uuid-123",
                "symbol": "BTC-PERP-INTX",
                "vwap": { "value": "65000.00", "currency": "USD" },
                "entry_vwap": { "value": "64000.00", "currency": "USD" },
                "position_side": "POSITION_SIDE_LONG",
                "margin_type": "MARGIN_TYPE_CROSS",
                "net_size": "0.5",
                "buy_order_size": "0.0",
                "sell_order_size": "0.0",
                "im_contribution": "3250.00",
                "unrealized_pnl": { "value": "500.00", "currency": "USD" },
                "mark_price": { "value": "65000.00", "currency": "USD" },
                "liquidation_price": { "value": "55000.00", "currency": "USD" },
                "leverage": "10",
                "im_notional": { "value": "3250.00", "currency": "USD" },
                "mm_notional": { "value": "1625.00", "currency": "USD" },
                "position_notional": { "value": "32500.00", "currency": "USD" },
                "aggregated_pnl": { "value": "500.00", "currency": "USD" }
              }
            }
            """;

        _mock.When("*/intx/positions/*/*").Respond("application/json", json);

        var result = await _client.GetPerpetualPositionAsync(PortfolioUuid, Symbol);

        var pos = result.Position!;
        Assert.Equal("BTC-PERP-INTX", pos.ProductId);
        Assert.Equal("BTC-PERP-INTX", pos.Symbol);
        Assert.Equal("portfolio-uuid-123", pos.PortfolioUuid);
        Assert.Equal("65000.00", pos.Vwap!.Value);
        Assert.Equal("64000.00", pos.EntryVwap!.Value);
        Assert.Equal(PositionSide.Long, pos.PositionSide);
        Assert.Equal(MarginType.Cross, pos.MarginType);
        Assert.Equal("0.5", pos.NetSize);
        Assert.Equal("500.00", pos.UnrealizedPnl!.Value);
        Assert.Equal("55000.00", pos.LiquidationPrice!.Value);
        Assert.Equal("10", pos.Leverage);
        Assert.Equal("32500.00", pos.PositionNotional!.Value);
    }

    [Fact]
    public async Task GetPerpetualPosition_throws_on_404()
    {
        _mock.When("*/intx/positions/*/*")
            .Respond(HttpStatusCode.NotFound, "application/json", """{"error":"NOT_FOUND"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetPerpetualPositionAsync(PortfolioUuid, "UNKNOWN-SYMBOL"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    // ─── GetPortfolioBalancesAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetPortfolioBalances_builds_correct_path()
    {
        _mock.Expect(HttpMethod.Get,
                $"https://api.coinbase.com/api/v3/brokerage/intx/balances/{PortfolioUuid}")
            .Respond("application/json", """{"portfolio_balances":[]}""");

        var result = await _client.GetPortfolioBalancesAsync(PortfolioUuid);

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPortfolioBalances_throws_on_blank_portfolio_uuid(string uuid)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetPortfolioBalancesAsync(uuid));
    }

    [Fact]
    public async Task GetPortfolioBalances_deserializes_full_response()
    {
        const string json = """
            {
              "portfolio_balances": [
                {
                  "portfolio_uuid": "portfolio-uuid-123",
                  "is_margin_limit_reached": false,
                  "balances": [
                    {
                      "asset": {
                        "asset_id": "asset-id-1",
                        "asset_uuid": "asset-uuid-1",
                        "asset_name": "USD Coin",
                        "status": "ACTIVE",
                        "collateral_weight": "1.0",
                        "account_collateral_limit": "100000.00",
                        "ecosystem_collateral_limit_breached": false,
                        "asset_icon_url": "https://example.com/usdc.png",
                        "supported_networks_enabled": true
                      },
                      "quantity": "10000.00",
                      "hold": "500.00",
                      "transfer_hold": "0.00",
                      "collateral_value": "10000.00",
                      "collateral_weight": "1.0",
                      "max_withdraw_amount": "9500.00",
                      "loan": "0.00",
                      "loan_collateral_requirement_usd": "0.00",
                      "pledged_quantity": "0.00",
                      "max_portfolio_transfer_amount": "9000.00"
                    }
                  ]
                }
              ]
            }
            """;

        _mock.When("*/intx/balances/*").Respond("application/json", json);

        var result = await _client.GetPortfolioBalancesAsync(PortfolioUuid);

        var portfolioBalance = Assert.Single(result.PortfolioBalances!);
        Assert.Equal("portfolio-uuid-123", portfolioBalance.PortfolioUuid);
        Assert.False(portfolioBalance.IsMarginLimitReached);

        var balance = Assert.Single(portfolioBalance.Balances!);
        Assert.Equal("USD Coin", balance.Asset!.AssetName);
        Assert.Equal("asset-uuid-1", balance.Asset.AssetUuid);
        Assert.False(balance.Asset.EcosystemCollateralLimitBreached);
        Assert.True(balance.Asset.SupportedNetworksEnabled);
        Assert.Equal("10000.00", balance.Quantity);
        Assert.Equal("500.00", balance.Hold);
        Assert.Equal("9500.00", balance.MaxWithdrawAmount);
        Assert.Equal("9000.00", balance.MaxPortfolioTransferAmount);
    }

    [Fact]
    public async Task GetPortfolioBalances_throws_on_403()
    {
        _mock.When("*/intx/balances/*")
            .Respond(HttpStatusCode.Forbidden, "application/json", """{"error":"FORBIDDEN"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.GetPortfolioBalancesAsync(PortfolioUuid));

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
    }

    // ─── ListPerpetualPositionsAsync ─────────────────────────────────────────

    [Fact]
    public async Task ListPerpetualPositions_builds_correct_path()
    {
        _mock.Expect(HttpMethod.Get,
                $"https://api.coinbase.com/api/v3/brokerage/intx/positions/{PortfolioUuid}")
            .Respond("application/json", """{"positions":[],"summary":{}}""");

        var result = await _client.ListPerpetualPositionsAsync(PortfolioUuid);

        Assert.NotNull(result);
        _mock.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ListPerpetualPositions_throws_on_blank_portfolio_uuid(string uuid)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.ListPerpetualPositionsAsync(uuid));
    }

    [Fact]
    public async Task ListPerpetualPositions_deserializes_positions_and_summary()
    {
        const string json = """
            {
              "positions": [
                {
                  "product_id": "BTC-PERP-INTX",
                  "symbol": "BTC-PERP-INTX",
                  "position_side": "POSITION_SIDE_LONG",
                  "margin_type": "MARGIN_TYPE_CROSS",
                  "net_size": "0.5",
                  "leverage": "10",
                  "unrealized_pnl": { "value": "500.00", "currency": "USD" },
                  "aggregated_pnl": { "value": "500.00", "currency": "USD" }
                },
                {
                  "product_id": "ETH-PERP-INTX",
                  "symbol": "ETH-PERP-INTX",
                  "position_side": "POSITION_SIDE_SHORT",
                  "margin_type": "MARGIN_TYPE_CROSS",
                  "net_size": "-2.0",
                  "leverage": "5",
                  "unrealized_pnl": { "value": "-100.00", "currency": "USD" },
                  "aggregated_pnl": { "value": "-100.00", "currency": "USD" }
                }
              ],
              "summary": {
                "aggregated_pnl": { "value": "400.00", "currency": "USD" }
              }
            }
            """;

        _mock.When("*/intx/positions/*").Respond("application/json", json);

        var result = await _client.ListPerpetualPositionsAsync(PortfolioUuid);

        Assert.Equal(2, result.Positions!.Count);

        var btc = result.Positions[0];
        Assert.Equal("BTC-PERP-INTX", btc.ProductId);
        Assert.Equal(PositionSide.Long, btc.PositionSide);
        Assert.Equal("0.5", btc.NetSize);
        Assert.Equal("500.00", btc.UnrealizedPnl!.Value);

        var eth = result.Positions[1];
        Assert.Equal("ETH-PERP-INTX", eth.ProductId);
        Assert.Equal(PositionSide.Short, eth.PositionSide);
        Assert.Equal("-2.0", eth.NetSize);

        Assert.Equal("400.00", result.Summary!.AggregatedPnl!.Value);
    }

    [Fact]
    public async Task ListPerpetualPositions_returns_empty_list()
    {
        _mock.When("*/intx/positions/*")
            .Respond("application/json", """{"positions":[],"summary":{"aggregated_pnl":{"value":"0.00","currency":"USD"}}}""");

        var result = await _client.ListPerpetualPositionsAsync(PortfolioUuid);

        Assert.NotNull(result.Positions);
        Assert.Empty(result.Positions);
        Assert.Equal("0.00", result.Summary!.AggregatedPnl!.Value);
    }

    [Fact]
    public async Task ListPerpetualPositions_throws_on_401()
    {
        _mock.When("*/intx/positions/*")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"UNAUTHORIZED"}""");

        var ex = await Assert.ThrowsAsync<CoinbaseApiException>(
            () => _client.ListPerpetualPositionsAsync(PortfolioUuid));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string MinimalPortfolioSummaryJson() =>
        """{"portfolios":[],"summary":{"unrealized_pnl":{"value":"0.00","currency":"USD"}}}""";
}
