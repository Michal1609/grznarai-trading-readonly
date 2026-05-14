namespace GrznarAi.Trading.ReadOnly.Etoro.Models.PiData;

public sealed record CopierInfo(
    string? Gender,
    string? Club,
    string? Country,
    string? CopyStartedAtCategory,
    string? AmountCategory,
    string? AgeCategory,
    string? CopyRealizedEquity_pnl,
    string? AvailableCopyBalance
);
