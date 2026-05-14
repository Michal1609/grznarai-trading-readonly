using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Tests.Helpers;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Etoro.Tests.Client;

[TestFixture]
public class PiDataClientTests
{
    private const string BaseUrl = "https://public-api.etoro.com/api/v1/";

    private static EToroClient CreateClient(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        return new EToroClient(httpClient);
    }

    [Test]
    public async Task GetCopiersPublicInfoAsync_CallsCorrectEndpoint()
    {
        var json = """{ "copiers": [] }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetCopiersPublicInfoAsync();

        Assert.That(handler.LastRequestUri, Does.Contain("pi-data/copiers"));
    }

    [Test]
    public async Task GetCopiersPublicInfoAsync_EmptyList_ReturnsEmptyCollection()
    {
        var json = """{ "copiers": [] }""";
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetCopiersPublicInfoAsync();

        Assert.That(result.Copiers, Is.Empty);
    }

    [Test]
    public async Task GetCopiersPublicInfoAsync_DeserializesAllFields()
    {
        var json = """
        {
          "copiers": [
            {
              "Gender": "M",
              "Club": "Gold",
              "Country": "Germany",
              "CopyStartedAtCategory": "less than 1 month",
              "AmountCategory": "1000-5000",
              "AgeCategory": "30-44",
              "CopyRealizedEquity_pnl": "1589.2",
              "AvailableCopyBalance": "55.2"
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetCopiersPublicInfoAsync();

        Assert.That(result.Copiers, Has.Count.EqualTo(1));
        var copier = result.Copiers[0];
        Assert.That(copier.Gender, Is.EqualTo("M"));
        Assert.That(copier.Club, Is.EqualTo("Gold"));
        Assert.That(copier.Country, Is.EqualTo("Germany"));
        Assert.That(copier.CopyStartedAtCategory, Is.EqualTo("less than 1 month"));
        Assert.That(copier.AmountCategory, Is.EqualTo("1000-5000"));
        Assert.That(copier.AgeCategory, Is.EqualTo("30-44"));
        Assert.That(copier.CopyRealizedEquity_pnl, Is.EqualTo("1589.2"));
        Assert.That(copier.AvailableCopyBalance, Is.EqualTo("55.2"));
    }

    [Test]
    public async Task GetCopiersPublicInfoAsync_MultipleCopiers_DeserializesAll()
    {
        var json = """
        {
          "copiers": [
            {
              "Gender": "M",
              "Club": "Gold",
              "Country": "Germany",
              "CopyStartedAtCategory": "less than 1 year",
              "AmountCategory": ">5000",
              "AgeCategory": "45-59",
              "CopyRealizedEquity_pnl": "200.5",
              "AvailableCopyBalance": "10.0"
            },
            {
              "Gender": "F",
              "Club": "Silver",
              "Country": "France",
              "CopyStartedAtCategory": "more than 1 year",
              "AmountCategory": "100-500",
              "AgeCategory": "18-29",
              "CopyRealizedEquity_pnl": "-50.0",
              "AvailableCopyBalance": "300.0"
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetCopiersPublicInfoAsync();

        Assert.That(result.Copiers, Has.Count.EqualTo(2));
        Assert.That(result.Copiers[0].Country, Is.EqualTo("Germany"));
        Assert.That(result.Copiers[1].Country, Is.EqualTo("France"));
    }

    [Test]
    public async Task GetCopiersPublicInfoAsync_NullableFields_DeserializesNulls()
    {
        var json = """
        {
          "copiers": [
            {
              "Gender": null,
              "Club": null,
              "Country": null,
              "CopyStartedAtCategory": null,
              "AmountCategory": null,
              "AgeCategory": null,
              "CopyRealizedEquity_pnl": null,
              "AvailableCopyBalance": null
            }
          ]
        }
        """;
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetCopiersPublicInfoAsync();

        Assert.That(result.Copiers, Has.Count.EqualTo(1));
        var copier = result.Copiers[0];
        Assert.That(copier.Gender, Is.Null);
        Assert.That(copier.Country, Is.Null);
    }

    [Test]
    public void GetCopiersPublicInfoAsync_ServerError_Throws()
    {
        var handler = new MockHttpMessageHandler("", System.Net.HttpStatusCode.Unauthorized);

        var exception = Assert.ThrowsAsync<EToroApiException>(
            () => CreateClient(handler).GetCopiersPublicInfoAsync());

        Assert.That(exception!.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
        Assert.That(exception.Endpoint, Does.Contain("pi-data/copiers"));
    }
}
