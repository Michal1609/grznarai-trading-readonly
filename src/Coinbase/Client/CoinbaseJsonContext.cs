using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.DataApi;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.PaymentMethods;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Public;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, MaxDepth = 32)]
[JsonSerializable(typeof(ListAccountsResponse))]
[JsonSerializable(typeof(GetAccountResponse))]
[JsonSerializable(typeof(GetConvertTradeResponse))]
[JsonSerializable(typeof(GetApiKeyPermissionsResponse))]
[JsonSerializable(typeof(GetTransactionSummaryResponse))]
[JsonSerializable(typeof(GetCurrentMarginWindowResponse))]
[JsonSerializable(typeof(GetFuturesBalanceSummaryResponse))]
[JsonSerializable(typeof(GetFuturesPositionResponse))]
[JsonSerializable(typeof(GetIntradayMarginSettingResponse))]
[JsonSerializable(typeof(ListFuturesPositionsResponse))]
[JsonSerializable(typeof(ListFuturesSweepsResponse))]
[JsonSerializable(typeof(GetOrderResponse))]
[JsonSerializable(typeof(ListOrdersResponse))]
[JsonSerializable(typeof(ListFillsResponse))]
[JsonSerializable(typeof(ListPaymentMethodsResponse))]
[JsonSerializable(typeof(GetPaymentMethodResponse))]
[JsonSerializable(typeof(GetPerpetualPortfolioSummaryResponse))]
[JsonSerializable(typeof(GetPerpetualPositionResponse))]
[JsonSerializable(typeof(GetPortfolioBalancesResponse))]
[JsonSerializable(typeof(ListPerpetualPositionsResponse))]
[JsonSerializable(typeof(ListPortfoliosResponse))]
[JsonSerializable(typeof(GetPortfolioBreakdownResponse))]
[JsonSerializable(typeof(GetBestBidAskResponse))]
[JsonSerializable(typeof(GetMarketTradesResponse))]
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(GetProductBookResponse))]
[JsonSerializable(typeof(GetProductCandlesResponse))]
[JsonSerializable(typeof(ListProductsResponse))]
[JsonSerializable(typeof(GetServerTimeResponse))]
internal sealed partial class CoinbaseJsonContext : JsonSerializerContext { }
