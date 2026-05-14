using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Integration;

/// <summary>
/// IntegraÄŤnĂ­ testy pro sekci PI DATA.
/// SpuĹˇtÄ›nĂ­: dotnet test --filter "Category=Integration"
/// NutnĂ©: nastavit EToroOptions__ApiKey + EToroOptions__UserKey nebo lokĂˇlnĂ­ appsettings.test.json
/// </summary>
[TestFixture]
[Category("Integration")]
[Explicit("Pouze ruÄŤnĂ­ spuĹˇtÄ›nĂ­ â€” vyĹľaduje reĂˇlnĂ© API klĂ­ÄŤe mimo git")]
public class PiDataIntegrationTests
{
    private IEToroClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = IntegrationTestSupport.CreateClient();
    }

    [Test]
    public async Task GetCopiersPublicInfoAsync_ReturnsResponse()
    {
        var result = await _client.GetCopiersPublicInfoAsync();

        Debug.WriteLine($"Copiers count: {result.Copiers.Count}");

        foreach (var c in result.Copiers)
            Debug.WriteLine($"  Gender={c.Gender} Club={c.Club} Country={c.Country} " +
                            $"Age={c.AgeCategory} Amount={c.AmountCategory} " +
                            $"PnL={c.CopyRealizedEquity_pnl} Balance={c.AvailableCopyBalance}");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Copiers, Is.Not.Null);
    }

    [Test]
    public async Task GetCopiersPublicInfoAsync_AllCopiersHaveKnownAmountCategory()
    {
        var validAmounts = new[] { "<100", "100-500", "500-1000", "1000-5000", ">5000" };

        var result = await _client.GetCopiersPublicInfoAsync();

        foreach (var c in result.Copiers.Where(x => x.AmountCategory is not null))
            Assert.That(validAmounts, Contains.Item(c.AmountCategory),
                $"Unknown AmountCategory: '{c.AmountCategory}'");
    }

    [Test]
    public async Task GetCopiersPublicInfoAsync_AllCopiersHaveKnownAgeCategory()
    {
        var validAges = new[] { "Under 18", "18-29", "30-44", "45-59", "60+" };

        var result = await _client.GetCopiersPublicInfoAsync();

        foreach (var c in result.Copiers.Where(x => x.AgeCategory is not null))
            Assert.That(validAges, Contains.Item(c.AgeCategory),
                $"Unknown AgeCategory: '{c.AgeCategory}'");
    }

    [Test]
    public async Task GetCopiersPublicInfoAsync_AllCopiersHaveKnownCopyStartedCategory()
    {
        var validCategories = new[]
        {
            "less than 1 day",
            "less than 1 week",
            "less than 1 month",
            "less than 1 year",
            "more than 1 year"
        };

        var result = await _client.GetCopiersPublicInfoAsync();

        foreach (var c in result.Copiers.Where(x => x.CopyStartedAtCategory is not null))
            Assert.That(validCategories, Contains.Item(c.CopyStartedAtCategory),
                $"Unknown CopyStartedAtCategory: '{c.CopyStartedAtCategory}'");
    }
}
