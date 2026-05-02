using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Tests.Helpers;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class IdentityClientTests
{
    private const string BaseUrl = "https://public-api.etoro.com/api/v1/";

    private static EToroClient CreateClient(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        return new EToroClient(httpClient);
    }

    [Test]
    public async Task GetIdentityAsync_CallsCorrectEndpoint()
    {
        var json = """{ "gcid": 1, "realCid": 2, "demoCid": 3 }""";
        var handler = new MockHttpMessageHandler(json);

        await CreateClient(handler).GetIdentityAsync();

        Assert.That(handler.LastRequestUri, Does.EndWith("me"));
    }

    [Test]
    public async Task GetIdentityAsync_DeserializesAllFields()
    {
        var json = """{ "gcid": 123456, "realCid": 789012, "demoCid": 345678 }""";
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetIdentityAsync();

        Assert.That(result.Gcid, Is.EqualTo(123456));
        Assert.That(result.RealCid, Is.EqualTo(789012));
        Assert.That(result.DemoCid, Is.EqualTo(345678));
    }

    [Test]
    public async Task GetIdentityAsync_ZeroIds_Deserializes()
    {
        var json = """{ "gcid": 0, "realCid": 0, "demoCid": 0 }""";
        var handler = new MockHttpMessageHandler(json);

        var result = await CreateClient(handler).GetIdentityAsync();

        Assert.That(result.Gcid, Is.EqualTo(0));
        Assert.That(result.RealCid, Is.EqualTo(0));
        Assert.That(result.DemoCid, Is.EqualTo(0));
    }

    [Test]
    public void GetIdentityAsync_ServerError_Throws()
    {
        var handler = new MockHttpMessageHandler("", System.Net.HttpStatusCode.Unauthorized);

        var exception = Assert.ThrowsAsync<EToroApiException>(
            () => CreateClient(handler).GetIdentityAsync());

        Assert.That(exception!.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
        Assert.That(exception.Endpoint, Does.Contain("/me"));
    }
}
