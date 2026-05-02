using GrznarAi.Trading.ReadOnly.Models.Social;

namespace GrznarAi.Trading.ReadOnly.Client;

public sealed partial class EToroClient
{
    public async Task<PopularInvestorsResponse> GetPopularInvestorsAsync(
        PopularInvestorsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentOutOfRangeException.ThrowIfNegative(request.Page);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(request.PageSize, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(request.PageSize, EToroRequestLimits.MaxPageSize);
        EToroInputValidator.ValidateOptionalString(request.Sort, nameof(request.Sort), EToroRequestLimits.MaxSortLength);

        var qs = new QueryStringBuilder()
            .Add("period", request.Period)
            .AddIfHasValue("popularInvestor", request.PopularInvestor)
            .AddIfHasValue("gainMax", request.GainMax)
            .AddIfHasValue("maxDailyRiskScoreMin", request.MaxDailyRiskScoreMin)
            .AddIfHasValue("maxDailyRiskScoreMax", request.MaxDailyRiskScoreMax)
            .AddIfHasValue("maxMonthlyRiskScoreMin", request.MaxMonthlyRiskScoreMin)
            .AddIfHasValue("maxMonthlyRiskScoreMax", request.MaxMonthlyRiskScoreMax)
            .AddIfHasValue("instrumentId", request.InstrumentId)
            .AddIfHasValue("countryId", request.CountryId)
            .Add("page", request.Page)
            .Add("pageSize", request.PageSize)
            .Add("sort", request.Sort);

        return await GetFromJsonAsync<PopularInvestorsResponse>(
            $"user-info/people/search{qs}",
            "Empty popular investors response.",
            ct).ConfigureAwait(false);
    }
}
