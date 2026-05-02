using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Models.Agent;
using GrznarAi.Trading.ReadOnly.Models.Feed;
using GrznarAi.Trading.ReadOnly.Models.Identity;
using GrznarAi.Trading.ReadOnly.Models.Market;
using GrznarAi.Trading.ReadOnly.Models.PiData;
using GrznarAi.Trading.ReadOnly.Models.Pnl;
using GrznarAi.Trading.ReadOnly.Models.Portfolio;
using GrznarAi.Trading.ReadOnly.Models.Social;
using GrznarAi.Trading.ReadOnly.Models.Trades;
using GrznarAi.Trading.ReadOnly.Models.UserInfo;
using GrznarAi.Trading.ReadOnly.Models.Watchlist;

namespace GrznarAi.Trading.ReadOnly.Client;

// Enum converters stay on concrete enum types with JsonStringEnumConverter<T>.
// The open JsonStringEnumConverter is reflection-based and not NativeAOT-safe.
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, MaxDepth = 32)]
[JsonSerializable(typeof(AgentPortfolioResponse))]
[JsonSerializable(typeof(DiscussionsResponse))]
[JsonSerializable(typeof(UserIdentityResponse))]
[JsonSerializable(typeof(ExchangesResponse))]
[JsonSerializable(typeof(InstrumentTypesResponse))]
[JsonSerializable(typeof(InstrumentMetadataResponse))]
[JsonSerializable(typeof(IReadOnlyList<InstrumentClosingPrice>))]
[JsonSerializable(typeof(StocksIndustriesResponse))]
[JsonSerializable(typeof(InstrumentSearchResponse))]
[JsonSerializable(typeof(LiveRatesResponse))]
[JsonSerializable(typeof(CandleResponse))]
[JsonSerializable(typeof(CopiersResponse))]
[JsonSerializable(typeof(PopularInvestorsResponse))]
[JsonSerializable(typeof(PnlApiResponse))]
[JsonSerializable(typeof(PortfolioApiResponse))]
[JsonSerializable(typeof(List<ClosedTrade>))]
[JsonSerializable(typeof(OrderForOpenInfoResponse))]
[JsonSerializable(typeof(UserProfileResponse))]
[JsonSerializable(typeof(UserSearchResponse))]
[JsonSerializable(typeof(UserGainResponse))]
[JsonSerializable(typeof(UserDailyGainResponse))]
[JsonSerializable(typeof(UserLivePortfolioResponse))]
[JsonSerializable(typeof(UserTradeInfoResponse))]
[JsonSerializable(typeof(CuratedListsResponse))]
[JsonSerializable(typeof(MarketRecommendationsResponse))]
[JsonSerializable(typeof(WatchlistsResponse))]
[JsonSerializable(typeof(IReadOnlyList<WatchlistItemDto>))]
[JsonSerializable(typeof(WatchlistDto))]
internal sealed partial class EToroJsonContext : JsonSerializerContext { }
