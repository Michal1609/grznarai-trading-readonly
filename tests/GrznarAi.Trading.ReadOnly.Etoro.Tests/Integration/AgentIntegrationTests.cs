using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// IntegraÄŤnĂ­ testy pro sekci AGENT.
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "Category=Integration"
/// NutnĂ©: nastavit EToroOptions__ApiKey + EToroOptions__UserKey nebo lokĂˇlnĂ­ appsettings.test.json
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class AgentIntegrationTests
{
    private IEToroClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();
    }

    [Test]
    public async Task GetAgentPortfoliosAsync_ReturnsResponse()
    {
        var result = await _client.GetAgentPortfoliosAsync();

        Debug.WriteLine($"Agent portfolios count: {result.AgentPortfolios.Count}");

        foreach (var p in result.AgentPortfolios)
        {
            Debug.WriteLine($"  ID={p.AgentPortfolioId} Name='{p.AgentPortfolioName}' GCID={p.AgentPortfolioGcid} " +
                            $"Balance={p.AgentPortfolioVirtualBalance:F2} MirrorId={p.MirrorId} Created={p.CreatedAt:yyyy-MM-dd}");

            foreach (var t in p.UserTokens)
                Debug.WriteLine($"    Token: ID={t.UserTokenId} Name='{t.UserTokenName}' App='{t.ExternalApplicationName}' " +
                                $"Expires={t.ExpiresAt:yyyy-MM-dd} Scopes=[{string.Join(",", t.ScopeIds)}]");
        }

        Assert.That(result, Is.Not.Null);
        Assert.That(result.AgentPortfolios, Is.Not.Null);
    }

    [Test]
    public async Task GetAgentPortfoliosAsync_AllPortfoliosHaveValidIds()
    {
        var result = await _client.GetAgentPortfoliosAsync();

        foreach (var p in result.AgentPortfolios)
        {
            Assert.That(p.AgentPortfolioId, Is.Not.EqualTo(Guid.Empty),
                $"Portfolio '{p.AgentPortfolioName}' has empty GUID.");
            Assert.That(p.AgentPortfolioName, Is.Not.Null.And.Not.Empty,
                $"Portfolio {p.AgentPortfolioId} has empty name.");
        }
    }

    [Test]
    public async Task GetAgentPortfoliosAsync_UserTokensHaveValidScopes()
    {
        var result = await _client.GetAgentPortfoliosAsync();

        var validScopes = new[] { 200, 201, 202, 203 };

        foreach (var p in result.AgentPortfolios)
        foreach (var t in p.UserTokens)
        {
            Assert.That(t.ScopeIds, Is.Not.Null,
                $"Token '{t.UserTokenName}' has null ScopeIds.");
            foreach (var scope in t.ScopeIds)
                Assert.That(validScopes, Contains.Item(scope),
                    $"Token '{t.UserTokenName}' has unknown scope {scope}.");
        }
    }
}
