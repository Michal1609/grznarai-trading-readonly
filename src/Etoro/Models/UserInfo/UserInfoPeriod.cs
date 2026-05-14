namespace GrznarAi.Trading.ReadOnly.Etoro.Models.UserInfo;

public enum UserInfoPeriod
{
    CurrMonth,
    CurrQuarter,
    CurrYear,
    LastYear,
    LastTwoYears,
    OneMonthAgo,
    TwoMonthsAgo,
    ThreeMonthsAgo,
    SixMonthsAgo,
    OneYearAgo
}

public enum UserDailyGainType
{
    Daily,
    Period
}
