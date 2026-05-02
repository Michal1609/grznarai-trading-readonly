using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Social;
using GrznarAi.Trading.ReadOnly.Models.UserInfo;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class QueryStringBuilderTests
{
    [TestCase("PopularInvestorPeriod.CurrMonth", "?period=CurrMonth")]
    [TestCase("PopularInvestorPeriod.LastTwoYears", "?period=LastTwoYears")]
    [TestCase("UserInfoPeriod.OneYearAgo", "?period=OneYearAgo")]
    [TestCase("UserDailyGainType.Period", "?type=Period")]
    public void Add_Enum_UsesExplicitApiToken(string valueName, string expected)
    {
        Enum value = valueName switch
        {
            "PopularInvestorPeriod.CurrMonth" => PopularInvestorPeriod.CurrMonth,
            "PopularInvestorPeriod.LastTwoYears" => PopularInvestorPeriod.LastTwoYears,
            "UserInfoPeriod.OneYearAgo" => UserInfoPeriod.OneYearAgo,
            "UserDailyGainType.Period" => UserDailyGainType.Period,
            _ => throw new ArgumentOutOfRangeException(nameof(valueName), valueName, null)
        };

        var queryString = new QueryStringBuilder().Add(expected.Contains("type=", StringComparison.Ordinal) ? "type" : "period", value);

        Assert.That(queryString.ToString(), Is.EqualTo(expected));
    }
}
