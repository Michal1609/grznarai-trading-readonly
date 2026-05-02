namespace GrznarAi.Trading.ReadOnly.Models.PiData;

public sealed record CopiersResponse(
    IReadOnlyList<CopierInfo> Copiers
);
