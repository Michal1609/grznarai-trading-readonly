using System.Text.Json;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

const string json = """
{
  "breakdown": {
    "portfolio": { "uuid": "aot-uuid", "name": "AOT" },
    "portfolio_balances": { "total_balance": { "value": "1.23", "currency": "USD" } },
    "spot_positions": [],
    "perp_positions": [],
    "futures_positions": []
  }
}
""";

var resp = JsonSerializer.Deserialize(json, CoinbaseJsonContext.Default.GetPortfolioBreakdownResponse);
if (resp?.Breakdown?.Portfolio?.Uuid != "aot-uuid")
{
    Console.Error.WriteLine("AOT smoke FAILED: unexpected payload.");
    return 1;
}

Console.WriteLine("AOT smoke OK.");
return 0;
