namespace GrznarAi.Trading.ReadOnly.Models.Market;

public sealed record InstrumentSearchRequest
{
    public required IReadOnlyList<string> Fields { get; init; }
    public string? SearchText { get; init; }
    public string? InternalSymbolFull { get; init; }
    public int PageSize { get; init; } = 20;
}
