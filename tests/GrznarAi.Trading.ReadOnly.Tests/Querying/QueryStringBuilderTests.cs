using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Tests.Querying;

[TestFixture]
public class QueryStringBuilderTests
{
    [Test]
    public void ToString_EmptyBuilder_ReturnsEmptyString()
    {
        var builder = new QueryStringBuilder();

        Assert.That(builder.ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Add_String_SkipsNullEmptyOrWhitespaceValues()
    {
        var builder = new QueryStringBuilder()
            .Add("null", (string?)null)
            .Add("empty", string.Empty)
            .Add("blank", "   ");

        Assert.That(builder.ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Add_String_EscapesNameAndValue()
    {
        var builder = new QueryStringBuilder().Add("search term", "a b&c");

        Assert.That(builder.ToString(), Is.EqualTo("?search%20term=a%20b%26c"));
    }

    [Test]
    public void Add_MultipleParts_JoinsWithAmpersand()
    {
        var builder = new QueryStringBuilder()
            .Add("currency", "USD")
            .Add("limit", 50)
            .Add("active", true);

        Assert.That(builder.ToString(), Is.EqualTo("?currency=USD&limit=50&active=true"));
    }

    [Test]
    public void AddIfHasValue_SkipsNullAndWritesPresentValues()
    {
        var builder = new QueryStringBuilder()
            .AddIfHasValue("limit", (int?)null)
            .AddIfHasValue("active", (bool?)null)
            .AddIfHasValue("take", 25)
            .AddIfHasValue("enabled", false);

        Assert.That(builder.ToString(), Is.EqualTo("?take=25&enabled=false"));
    }

    [Test]
    public void AddDate_UsesIsoDateFormat()
    {
        var builder = new QueryStringBuilder().AddDate("from", new DateOnly(2026, 5, 12));

        Assert.That(builder.ToString(), Is.EqualTo("?from=2026-05-12"));
    }

    [Test]
    public void AddCsv_IntValues_JoinsWithCommas()
    {
        var builder = new QueryStringBuilder().AddCsv("ids", [1, 2, 3]);

        Assert.That(builder.ToString(), Is.EqualTo("?ids=1,2,3"));
    }

    [Test]
    public void AddCsv_StringValues_TrimsSkipsEmptyAndEscapesItems()
    {
        var builder = new QueryStringBuilder().AddCsv("symbols", [" AAPL ", "", "BRK B"]);

        Assert.That(builder.ToString(), Is.EqualTo("?symbols=AAPL,BRK%20B"));
    }

    [Test]
    public void AddCsv_NullOrEmptyValues_SkipsParameter()
    {
        var builder = new QueryStringBuilder()
            .AddCsv("strings", (IEnumerable<string>?)null)
            .AddCsv("emptyStrings", Array.Empty<string>())
            .AddCsv("ints", (IEnumerable<int>?)null)
            .AddCsv("emptyInts", Array.Empty<int>());

        Assert.That(builder.ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void EscapePathSegment_EscapesSpecialCharacters()
    {
        var escaped = QueryStringBuilder.EscapePathSegment("abc/def?x=1");

        Assert.That(escaped, Is.EqualTo("abc%2Fdef%3Fx%3D1"));
    }
}

