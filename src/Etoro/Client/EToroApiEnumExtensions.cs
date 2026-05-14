using GrznarAi.Trading.ReadOnly.Etoro.Models.Market;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Social;
using GrznarAi.Trading.ReadOnly.Etoro.Models.UserInfo;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Watchlist;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

internal static class EToroApiEnumExtensions
{
    public static string ToApiString(this Enum value) => value switch
    {
        CandleInterval ci => ci switch
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
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Enum value is not mapped to an eToro API token.")
        },
        CandleDirection cd => cd switch
        {
            CandleDirection.Asc => "Asc",
            CandleDirection.Desc => "Desc",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Enum value is not mapped to an eToro API token.")
        },
        PopularInvestorPeriod pip => pip switch
        {
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
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Enum value is not mapped to an eToro API token.")
        },
        UserInfoPeriod uip => uip switch
        {
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
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Enum value is not mapped to an eToro API token.")
        },
        UserDailyGainType udgt => udgt switch
        {
            UserDailyGainType.Daily => "Daily",
            UserDailyGainType.Period => "Period",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Enum value is not mapped to an eToro API token.")
        },
        WatchlistType wt => wt switch
        {
            WatchlistType.Static => "Static",
            WatchlistType.Dynamic => "Dynamic",
            WatchlistType.RecentlyInvested => "RecentlyInvested",
            WatchlistType.Default => "Default",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Enum value is not mapped to an eToro API token.")
        },
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Enum value is not mapped to an eToro API token.")
    };
}
