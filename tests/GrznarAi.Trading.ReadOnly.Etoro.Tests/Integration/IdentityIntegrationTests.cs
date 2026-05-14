using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// IntegraÄŤnĂ­ testy pro sekci IDENTITY.
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "Category=Integration"
/// NutnĂ©: nastavit EToroOptions__ApiKey + EToroOptions__UserKey nebo lokĂˇlnĂ­ appsettings.test.json
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class IdentityIntegrationTests
{
    private IEToroClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();
    }

    [Test]
    public async Task GetIdentityAsync_ReturnsResponse()
    {
        var result = await _client.GetIdentityAsync();

        Debug.WriteLine($"Identity: GCID={result.Gcid} RealCID={result.RealCid} DemoCID={result.DemoCid}");

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetIdentityAsync_GcidIsPositive()
    {
        var result = await _client.GetIdentityAsync();

        Assert.That(result.Gcid, Is.GreaterThan(0), "GCID must be a positive integer.");
    }

    [Test]
    public async Task GetIdentityAsync_RealAndDemoCidArePositive()
    {
        var result = await _client.GetIdentityAsync();

        Assert.That(result.RealCid, Is.GreaterThan(0), "RealCid must be a positive integer.");
        Assert.That(result.DemoCid, Is.GreaterThan(0), "DemoCid must be a positive integer.");
    }

    [Test]
    public async Task GetIdentityAsync_GcidDiffersFromRealAndDemoCid()
    {
        var result = await _client.GetIdentityAsync();

        Assert.That(result.Gcid, Is.Not.EqualTo(result.RealCid), "GCID should differ from RealCid.");
        Assert.That(result.Gcid, Is.Not.EqualTo(result.DemoCid), "GCID should differ from DemoCid.");
    }
}
