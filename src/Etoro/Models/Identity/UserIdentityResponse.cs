namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Identity;

public sealed record UserIdentityResponse(
    int Gcid,
    int RealCid,
    int DemoCid
);
