namespace GrznarAi.Trading.ReadOnly.Client;

/// <summary>
/// Conservative upper bounds for API request parameters.
/// Values outside these ranges risk excessive URL length, server rejections, or unintended API load.
/// </summary>
public static class EToroRequestLimits
{
    public const int MaxPageSize = 200;
    public const int MaxSearchPageSize = 100;
    public const int MaxItemsPerPage = 1000;
    public const int MaxItemsCount = 500;
    public const int MaxItemsLimit = 10_000;
    public const int MaxCandlesCount = 1_000;
    public const int MaxTake = 200;
    public const int MaxReactionsPageSize = 100;
    public const int MaxCsvIds = 100;
    public const int MaxSearchTextLength = 200;
    public const int MaxSortLength = 100;
    public const int MaxUsernameLength = 50;
    public const int MaxWatchlistIdLength = 100;
    public const int MaxRequesterUserIdLength = 50;
}
