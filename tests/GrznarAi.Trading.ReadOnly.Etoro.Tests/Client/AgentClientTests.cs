using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Tests.Helpers;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Client;

[TestFixture]
public class AgentClientTests
{
    private const string BaseUrl = "https://public-api.etoro.com/api/v1/";

    private static EToroClient CreateClient(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        return new EToroClient(httpClient);
    }

    [Test]
    public async Task GetAgentPortfoliosAsync_CallsCorrectEndpoint()
    {
        var json = """{ "agentPortfolios": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetAgentPortfoliosAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("agent-portfolios"));
    }

    [Test]
    public async Task GetAgentPortfoliosAsync_EmptyList_ReturnsEmptyCollection()
    {
        var json = """{ "agentPortfolios": [] }""";
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetAgentPortfoliosAsync();

        Assert.That(result.AgentPortfolios, Is.Empty);
    }

    [Test]
    public async Task GetAgentPortfoliosAsync_DeserializesPortfolio()
    {
        var json = """
        {
          "agentPortfolios": [
            {
              "agentPortfolioId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
              "agentPortfolioName": "MyPort1",
              "agentPortfolioGcid": 12345678,
              "agentPortfolioVirtualBalance": 10000.00,
              "mirrorId": 12345,
              "createdAt": "2026-03-01T10:30:00+00:00",
              "userTokens": []
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetAgentPortfoliosAsync();

        Assert.That(result.AgentPortfolios, Has.Count.EqualTo(1));
        var portfolio = result.AgentPortfolios[0];
        Assert.That(portfolio.AgentPortfolioId, Is.EqualTo(Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890")));
        Assert.That(portfolio.AgentPortfolioName, Is.EqualTo("MyPort1"));
        Assert.That(portfolio.AgentPortfolioGcid, Is.EqualTo(12345678));
        Assert.That(portfolio.AgentPortfolioVirtualBalance, Is.EqualTo(10000m));
        Assert.That(portfolio.MirrorId, Is.EqualTo(12345));
        Assert.That(portfolio.UserTokens, Is.Empty);
    }

    [Test]
    public async Task GetAgentPortfoliosAsync_DeserializesUserTokens()
    {
        var json = """
        {
          "agentPortfolios": [
            {
              "agentPortfolioId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
              "agentPortfolioName": "MyPort1",
              "agentPortfolioGcid": 12345678,
              "agentPortfolioVirtualBalance": 10000.00,
              "mirrorId": 12345,
              "createdAt": "2026-03-01T10:30:00+00:00",
              "userTokens": [
                {
                  "userTokenId": "00000000-0000-0000-0000-000000000001",
                  "userTokenName": "my-trading-token",
                  "clientId": "c1d2e3f4-a5b6-7890-cdef-123456789abc",
                  "externalApplicationName": "Trading Bot v2",
                  "ipsWhitelist": ["192.168.1.1"],
                  "expiresAt": "2026-12-31T23:59:59Z",
                  "scopeIds": [200, 202],
                  "createdAt": "2026-03-01T10:30:00Z"
                }
              ]
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetAgentPortfoliosAsync();

        var token = result.AgentPortfolios[0].UserTokens[0];
        Assert.That(token.UserTokenId, Is.EqualTo(Guid.Parse("00000000-0000-0000-0000-000000000001")));
        Assert.That(token.UserTokenName, Is.EqualTo("my-trading-token"));
        Assert.That(token.ExternalApplicationName, Is.EqualTo("Trading Bot v2"));
        Assert.That(token.IpsWhitelist, Contains.Item("192.168.1.1"));
        Assert.That(token.ScopeIds, Is.EquivalentTo(new[] { 200, 202 }));
    }

    [Test]
    public async Task GetAgentPortfoliosAsync_MultiplePortfolios_DeserializesAll()
    {
        var json = """
        {
          "agentPortfolios": [
            {
              "agentPortfolioId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
              "agentPortfolioName": "Port1",
              "agentPortfolioGcid": 111,
              "agentPortfolioVirtualBalance": 5000,
              "mirrorId": 1,
              "createdAt": "2026-01-01T00:00:00Z",
              "userTokens": []
            },
            {
              "agentPortfolioId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
              "agentPortfolioName": "Port2",
              "agentPortfolioGcid": 222,
              "agentPortfolioVirtualBalance": 20000,
              "mirrorId": 2,
              "createdAt": "2026-02-01T00:00:00Z",
              "userTokens": []
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetAgentPortfoliosAsync();

        Assert.That(result.AgentPortfolios, Has.Count.EqualTo(2));
        Assert.That(result.AgentPortfolios[0].AgentPortfolioName, Is.EqualTo("Port1"));
        Assert.That(result.AgentPortfolios[1].AgentPortfolioName, Is.EqualTo("Port2"));
    }
}
