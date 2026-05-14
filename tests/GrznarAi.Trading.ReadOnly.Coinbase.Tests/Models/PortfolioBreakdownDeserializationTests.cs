using System.Text.Json;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Models;

public class PortfolioBreakdownDeserializationTests
{
    private const string SampleJson = """
    {
      "breakdown": {
        "portfolio": {
          "name": "Default",
          "uuid": "11111111-1111-1111-1111-111111111111",
          "type": "DEFAULT",
          "deleted": false
        },
        "portfolio_balances": {
          "total_balance": { "value": "12345.67", "currency": "USD" },
          "total_futures_balance": { "value": "0", "currency": "USD" },
          "total_cash_equivalent_balance": { "value": "100.50", "currency": "USD" },
          "total_crypto_balance": { "value": "12245.17", "currency": "USD" },
          "futures_unrealized_pnl": { "value": "0", "currency": "USD" },
          "perp_unrealized_pnl": { "value": "0", "currency": "USD" }
        },
        "spot_positions": [
          {
            "asset": "BTC",
            "account_uuid": "22222222-2222-2222-2222-222222222222",
            "total_balance_fiat": "10000.00",
            "total_balance_crypto": "0.25",
            "available_to_trade_fiat": "10000.00",
            "allocation": "0.81",
            "one_day_change": "1.23",
            "cost_basis": { "value": "9000.00", "currency": "USD" },
            "asset_img_url": "https://x/btc.png",
            "is_cash": false,
            "average_entry_price": { "value": "36000.00", "currency": "USD" },
            "asset_uuid": "33333333-3333-3333-3333-333333333333",
            "available_to_trade_crypto": "0.25",
            "unrealized_pnl": "1000.00",
            "available_to_transfer": "0.25",
            "available_to_transfer_fiat": "10000.00",
            "available_to_transfer_crypto": "0.25"
          }
        ],
        "perp_positions": [
          {
            "product_id": "BTC-PERP",
            "product_uuid": "44444444-4444-4444-4444-444444444444",
            "symbol": "BTC-PERP",
            "asset_image_url": "https://x/btc.png",
            "vwap": { "value": "40000.00", "currency": "USD" },
            "position_side": "POSITION_SIDE_LONG",
            "net_size": "0.5",
            "buy_order_size": "0",
            "sell_order_size": "0",
            "im_contribution": "0.1",
            "unrealized_pnl": { "value": "500.00", "currency": "USD" },
            "mark_price": { "value": "41000.00", "currency": "USD" },
            "liquidation_price": { "value": "30000.00", "currency": "USD" },
            "leverage": "5",
            "im_notional": { "value": "4100.00", "currency": "USD" },
            "mm_notional": { "value": "1025.00", "currency": "USD" },
            "position_notional": { "value": "20500.00", "currency": "USD" },
            "margin_type": "MARGIN_TYPE_CROSS",
            "liquidation_buffer": "0.30",
            "liquidation_percentage": "30"
          }
        ],
        "futures_positions": [
          {
            "product_id": "BIT-26JUL24-CDE",
            "contract_size": "0.01",
            "side": "FUTURES_POSITION_SIDE_LONG",
            "amount": "1",
            "avg_entry_price": "65000.00",
            "current_price": "66000.00",
            "unrealized_pnl": "10.00",
            "expiry": "2024-07-26T16:00:00Z",
            "underlying_asset_id": "BTC",
            "asset_img_url": "https://x/btc.png",
            "product_name": "BTC 26JUL24",
            "venue": "CDE",
            "notional_value": "660.00"
          }
        ]
      }
    }
    """;

    [Fact]
    public void Deserialises_full_breakdown()
    {
        var result = JsonSerializer.Deserialize<GetPortfolioBreakdownResponse>(SampleJson);

        Assert.NotNull(result);
        Assert.NotNull(result!.Breakdown);
        var b = result.Breakdown!;

        Assert.Equal("Default", b.Portfolio!.Name);
        Assert.Equal("11111111-1111-1111-1111-111111111111", b.Portfolio.Uuid);

        Assert.Equal(12345.67m, b.PortfolioBalances!.TotalBalance!.Value);
        Assert.Equal("USD", b.PortfolioBalances.TotalBalance.Currency);

        var spot = Assert.Single(b.SpotPositions!);
        Assert.Equal("BTC", spot.Asset);
        Assert.Equal(0.25m, spot.TotalBalanceCrypto);
        Assert.Equal(1000.00m, spot.UnrealizedPnl);
        Assert.False(spot.IsCash);

        var perp = Assert.Single(b.PerpPositions!);
        Assert.Equal(GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.PositionSide.Long, perp.PositionSide);
        Assert.Equal(GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.MarginType.Cross, perp.MarginType);
        Assert.Equal(0.5m, perp.NetSize);
        Assert.Equal(500.00m, perp.UnrealizedPnl!.Value);

        var fut = Assert.Single(b.FuturesPositions!);
        Assert.Equal(GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.PositionSide.FuturesLong, fut.Side);
        Assert.Equal(65000.00m, fut.AvgEntryPrice);
        Assert.Equal(new DateTimeOffset(2024, 7, 26, 16, 0, 0, TimeSpan.Zero), fut.Expiry);
    }

    [Fact]
    public void Handles_missing_optional_arrays()
    {
        var json = """{"breakdown":{"portfolio":{"uuid":"x"}}}""";
        var result = JsonSerializer.Deserialize<GetPortfolioBreakdownResponse>(json);

        Assert.NotNull(result);
        var breakdown = result!.Breakdown;
        Assert.NotNull(breakdown);
        Assert.Null(breakdown!.SpotPositions);
        Assert.Null(breakdown.PerpPositions);
        Assert.Null(breakdown.FuturesPositions);
    }
}
