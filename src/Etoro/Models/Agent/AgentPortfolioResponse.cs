namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Agent;

public record AgentPortfolioResponse(
    IReadOnlyList<AgentPortfolio> AgentPortfolios
);

public record AgentPortfolio(
    Guid AgentPortfolioId,
    string AgentPortfolioName,
    int AgentPortfolioGcid,
    decimal AgentPortfolioVirtualBalance,
    int MirrorId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<AgentUserToken> UserTokens
);

public record AgentUserToken(
    Guid UserTokenId,
    string UserTokenName,
    Guid ClientId,
    string ExternalApplicationName,
    IReadOnlyList<string> IpsWhitelist,
    DateTimeOffset ExpiresAt,
    IReadOnlyList<int> ScopeIds,
    DateTimeOffset CreatedAt
);
