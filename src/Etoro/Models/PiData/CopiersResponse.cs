namespace GrznarAi.Trading.ReadOnly.Etoro.Models.PiData;

public sealed record CopiersResponse(
    IReadOnlyList<CopierInfo> Copiers
);
