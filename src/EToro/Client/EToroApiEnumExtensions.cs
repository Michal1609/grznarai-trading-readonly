using GrznarAi.Trading.ReadOnly.Models.Market;
using GrznarAi.Trading.ReadOnly.Models.Social;
using GrznarAi.Trading.ReadOnly.Models.UserInfo;
using GrznarAi.Trading.ReadOnly.Models.Watchlist;

namespace GrznarAi.Trading.ReadOnly.Client;

internal static class EToroApiEnumExtensions
{
    public static string ToApiString(this Enum value) =>
        value switch
        {
            CandleInterval.OneMinute => "OneMinute",
            CandleInterval.FiveMinutes => "FiveMinutes",
            CandleInterval.TenMinutes => "TenMinutes",
            CandleInterval.FifteenMinutes => "FifteenMinutes",
            CandleInterval.ThirtyMinutes => "ThirtyMinutes",
            CandleInterval.OneHour => "OneHour",
            CandleInterval.FourHours => "FourHours",
            CandleInterval.OneDay => "OneDay",
            CandleInterval.OneWeek => "OneWeek",
            CandleDirection.Asc => "Asc",
            CandleDirection.Desc => "Desc",
            PopularInvestorPeriod.CurrMonth => "CurrMonth",
            PopularInvestorPeriod.CurrQuarter => "CurrQuarter",
            PopularInvestorPeriod.CurrYear => "CurrYear",
            PopularInvestorPeriod.LastYear => "LastYear",
            PopularInvestorPeriod.LastTwoYears => "LastTwoYears",
            PopularInvestorPeriod.OneMonthAgo => "OneMonthAgo",
            PopularInvestorPeriod.TwoMonthsAgo => "TwoMonthsAgo",
            PopularInvestorPeriod.ThreeMonthsAgo => "ThreeMonthsAgo",
            PopularInvestorPeriod.SixMonthsAgo => "SixMonthsAgo",
            PopularInvestorPeriod.OneYearAgo => "OneYearAgo",
            UserInfoPeriod.CurrMonth => "CurrMonth",
            UserInfoPeriod.CurrQuarter => "CurrQuarter",
            UserInfoPeriod.CurrYear => "CurrYear",
            UserInfoPeriod.LastYear => "LastYear",
            UserInfoPeriod.LastTwoYears => "LastTwoYears",
            UserInfoPeriod.OneMonthAgo => "OneMonthAgo",
            UserInfoPeriod.TwoMonthsAgo => "TwoMonthsAgo",
            UserInfoPeriod.ThreeMonthsAgo => "ThreeMonthsAgo",
            UserInfoPeriod.SixMonthsAgo => "SixMonthsAgo",
            UserInfoPeriod.OneYearAgo => "OneYearAgo",
            UserDailyGainType.Daily => "Daily",
            UserDailyGainType.Period => "Period",
            WatchlistType.Static => "Static",
            WatchlistType.Dynamic => "Dynamic",
            WatchlistType.RecentlyInvested => "RecentlyInvested",
            WatchlistType.Default => "Default",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Enum value is not mapped to an eToro API token.")
        };
}
