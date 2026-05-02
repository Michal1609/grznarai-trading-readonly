namespace GrznarAi.Trading.ReadOnly.Models.Market;

public sealed record InstrumentSearchRequest
{
    public required IReadOnlyList<string> Fields { get; init; }
    public string? SearchText { get; init; }
    public int PageSize { get; init; } = 20;
    public int PageNumber { get; init; }
    public string? Sort { get; init; }
}
