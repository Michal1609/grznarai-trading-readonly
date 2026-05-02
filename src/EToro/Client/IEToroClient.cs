namespace GrznarAi.Trading.ReadOnly.Client;

/// <summary>
/// Unified eToro API client facade combining all domain-specific client interfaces.
/// Use the individual interfaces (<see cref="IEToroTradingClient"/>, <see cref="IEToroMarketDataClient"/>,
/// <see cref="IEToroUserInfoClient"/>, <see cref="IEToroSocialClient"/>, <see cref="IEToroFeedClient"/>,
/// <see cref="IEToroWatchlistClient"/>) to reduce blast radius when only a subset of functionality is needed.
/// </summary>
public interface IEToroClient
    : IEToroTradingClient
    , IEToroMarketDataClient
    , IEToroUserInfoClient
    , IEToroSocialClient
    , IEToroFeedClient
    , IEToroWatchlistClient
{
}
