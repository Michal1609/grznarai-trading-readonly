using GrznarAi.Trading.ReadOnly.Models.UserInfo;

namespace GrznarAi.Trading.ReadOnly.Client;

public sealed partial class EToroClient
{
    public async Task<UserProfileResponse> GetUserProfilesAsync(
        IEnumerable<string>? usernames = null,
        IEnumerable<int>? cidList = null,
        CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .AddCsv("usernames", usernames)
            .AddCsv("cidList", cidList);

        return await GetFromJsonAsync<UserProfileResponse>(
            $"user-info/people{qs}",
            "Empty user profiles response.",
            ct).ConfigureAwait(false);
    }

    public async Task<UserSearchResponse> SearchUsersAsync(
        UserSearchRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Page.HasValue)
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(request.Page.Value, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(request.PageSize, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(request.PageSize, EToroRequestLimits.MaxPageSize);
        EToroInputValidator.ValidateOptionalString(request.Sort, nameof(request.Sort), EToroRequestLimits.MaxSortLength);

        var qs = new QueryStringBuilder()
            .Add("period", request.Period)
            .AddIfHasValue("isTestAccount", request.IsTestAccount)
            .AddIfHasValue("optIn", request.OptIn)
            .AddIfHasValue("blocked", request.Blocked)
            .AddIfHasValue("gainMax", request.GainMax)
            .AddIfHasValue("maxDailyRiskScoreMin", request.MaxDailyRiskScoreMin)
            .AddIfHasValue("maxDailyRiskScoreMax", request.MaxDailyRiskScoreMax)
            .AddIfHasValue("maxMonthlyRiskScoreMin", request.MaxMonthlyRiskScoreMin)
            .AddIfHasValue("maxMonthlyRiskScoreMax", request.MaxMonthlyRiskScoreMax)
            .AddIfHasValue("weeksSinceRegistrationMin", request.WeeksSinceRegistrationMin)
            .AddIfHasValue("countryId", request.CountryId)
            .AddIfHasValue("instrumentId", request.InstrumentId)
            .AddIfHasValue("instrumentPctMin", request.InstrumentPctMin)
            .AddIfHasValue("instrumentPctMax", request.InstrumentPctMax)
            .AddIfHasValue("page", request.Page)
            .Add("pageSize", request.PageSize)
            .Add("sort", request.Sort);

        return await GetFromJsonAsync<UserSearchResponse>(
            $"user-info/people/search{qs}",
            "Empty user search response.",
            ct).ConfigureAwait(false);
    }

    public async Task<UserDailyGainResponse> GetUserDailyGainAsync(
        string username,
        DateOnly minDate,
        DateOnly maxDate,
        UserDailyGainType type = UserDailyGainType.Daily,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        EToroInputValidator.ValidateRequiredString(username, nameof(username), EToroRequestLimits.MaxUsernameLength);
        if (maxDate < minDate)
            throw new ArgumentException("Max date must be greater than or equal to min date.", nameof(maxDate));

        var qs = new QueryStringBuilder()
            .AddDate("minDate", minDate)
            .AddDate("maxDate", maxDate)
            .Add("type", type);

        return await GetFromJsonAsync<UserDailyGainResponse>(
            $"user-info/people/{QueryStringBuilder.EscapePathSegment(username)}/daily-gain{qs}",
            "Empty user daily gain response.",
            ct).ConfigureAwait(false);
    }

    public async Task<UserGainResponse> GetUserGainAsync(
        string username,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        EToroInputValidator.ValidateRequiredString(username, nameof(username), EToroRequestLimits.MaxUsernameLength);

        return await GetFromJsonAsync<UserGainResponse>(
            $"user-info/people/{QueryStringBuilder.EscapePathSegment(username)}/gain",
            "Empty user gain response.",
            ct).ConfigureAwait(false);
    }

    public async Task<UserLivePortfolioResponse> GetUserLivePortfolioAsync(
        string username,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        EToroInputValidator.ValidateRequiredString(username, nameof(username), EToroRequestLimits.MaxUsernameLength);

        return await GetFromJsonAsync<UserLivePortfolioResponse>(
            $"user-info/people/{QueryStringBuilder.EscapePathSegment(username)}/portfolio/live",
            "Empty user live portfolio response.",
            ct).ConfigureAwait(false);
    }

    public async Task<UserTradeInfoResponse> GetUserTradeInfoAsync(
        string username,
        UserInfoPeriod period,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        EToroInputValidator.ValidateRequiredString(username, nameof(username), EToroRequestLimits.MaxUsernameLength);

        var qs = new QueryStringBuilder().Add("period", period);
        return await GetFromJsonAsync<UserTradeInfoResponse>(
            $"user-info/people/{QueryStringBuilder.EscapePathSegment(username)}/tradeinfo{qs}",
            "Empty user trade info response.",
            ct).ConfigureAwait(false);
    }
}
