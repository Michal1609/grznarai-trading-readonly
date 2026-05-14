using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, MaxDepth = 32)]
[JsonSerializable(typeof(ListAccountsResponse))]
[JsonSerializable(typeof(GetAccountResponse))]
[JsonSerializable(typeof(ListPortfoliosResponse))]
[JsonSerializable(typeof(GetPortfolioBreakdownResponse))]
internal sealed partial class CoinbaseJsonContext : JsonSerializerContext { }
