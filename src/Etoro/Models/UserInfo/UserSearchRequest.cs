namespace GrznarAi.Trading.ReadOnly.Etoro.Models.UserInfo;

public sealed record UserSearchRequest
{
    public required UserInfoPeriod Period { get; init; }
    public bool? IsTestAccount { get; init; }
    public bool? OptIn { get; init; }
    public bool? Blocked { get; init; }
    public int? GainMax { get; init; }
    public int? MaxDailyRiskScoreMin { get; init; }
    public int? MaxDailyRiskScoreMax { get; init; }
    public int? MaxMonthlyRiskScoreMin { get; init; }
    public int? MaxMonthlyRiskScoreMax { get; init; }
    public int? WeeksSinceRegistrationMin { get; init; }
    public int? CountryId { get; init; }
    public int? InstrumentId { get; init; }
    public int? InstrumentPctMin { get; init; }
    public int? InstrumentPctMax { get; init; }
    public int? Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Sort { get; init; }
}
