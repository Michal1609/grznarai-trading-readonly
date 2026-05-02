using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GrznarAi.Trading.ReadOnly.Tests.Integration;

internal static class IntegrationTestSupport
{
    private const string MissingCredentialsMessage =
        "Integration tests require EToroOptions:ApiKey and EToroOptions:UserKey. " +
        "Set EToroOptions__ApiKey and EToroOptions__UserKey environment variables, " +
        "or use user-secrets, or create tests/GrznarAi.Trading.ReadOnly.Tests/appsettings.test.json.";

    public static EToroOptions LoadOptionsOrIgnore()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddUserSecrets<IntegrationTestMarker>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = config.GetSection("EToroOptions").Get<EToroOptions>();
        if (options is null ||
            string.IsNullOrWhiteSpace(options.ApiKey) ||
            string.IsNullOrWhiteSpace(options.UserKey) ||
            IsPlaceholder(options.ApiKey) ||
            IsPlaceholder(options.UserKey))
        {
            Assert.Ignore(MissingCredentialsMessage);
        }

        return options;
    }

    public static IEToroClient CreateClient()
    {
        return CreateClient(LoadOptionsOrIgnore());
    }

    public static IEToroClient CreateClient(EToroOptions options)
    {
        var optionsWrapper = Options.Create(options);
        var authHandler = new EToroAuthHandler(optionsWrapper);
        var rateLimitHandler = new RateLimitHandler(
            optionsWrapper,
            new EToroRateLimiter(optionsWrapper))
        {
            InnerHandler = new HttpClientHandler()
        };
        authHandler.InnerHandler = rateLimitHandler;

        var httpClient = new HttpClient(authHandler)
        {
            BaseAddress = new Uri("https://public-api.etoro.com/api/v1/")
        };

        return new EToroClient(httpClient);
    }

    private static bool IsPlaceholder(string value)
    {
        return value.Contains("replace-me", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("dummy", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("your-", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class IntegrationTestMarker;
}
