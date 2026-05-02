namespace GrznarAi.Trading.ReadOnly.Models.Social;

public sealed record PopularInvestorsRequest
{
    public required PopularInvestorPeriod Period { get; init; }
    public bool? PopularInvestor { get; init; }
    public int? GainMax { get; init; }
    public int? MaxDailyRiskScoreMin { get; init; }
    public int? MaxDailyRiskScoreMax { get; init; }
    public int? MaxMonthlyRiskScoreMin { get; init; }
    public int? MaxMonthlyRiskScoreMax { get; init; }
    public int? InstrumentId { get; init; }
    public int? CountryId { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; } = 20;
    public string? Sort { get; init; }
}
