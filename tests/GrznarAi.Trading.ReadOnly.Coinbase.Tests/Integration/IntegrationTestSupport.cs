using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

internal static class IntegrationTestSupport
{
    /// <summary>
    /// Returns true when Coinbase API credentials are available in the environment.
    /// </summary>
    public static bool HasCredentials()
    {
        var opts = TryLoadOptions();
        return opts is not null
            && !string.IsNullOrWhiteSpace(opts.KeyName)
            && !string.IsNullOrWhiteSpace(opts.PrivateKeyPem)
            && !IsPlaceholder(opts.KeyName)
            && !IsPlaceholder(opts.PrivateKeyPem);
    }

    /// <summary>
    /// Builds a real <see cref="ICoinbaseClient"/> from appsettings.test.json or environment variables.
    /// </summary>
    public static ICoinbaseClient CreateClient()
    {
        var config = BuildConfiguration();
        var services = new ServiceCollection();
        services.AddCoinbaseClient(config);
        var sp = services.BuildServiceProvider();
        return sp.GetRequiredService<ICoinbaseClient>();
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddUserSecrets<CoinbaseIntegrationFactAttribute>(optional: true)
            .AddEnvironmentVariables()
            .Build();

    private static CoinbaseOptions? TryLoadOptions()
    {
        var config = BuildConfiguration();
        return config.GetSection(CoinbaseOptions.SectionName).Get<CoinbaseOptions>();
    }

    private static bool IsPlaceholder(string value) =>
        value.Contains("replace-me", StringComparison.OrdinalIgnoreCase)
        || value.Contains("your-", StringComparison.OrdinalIgnoreCase)
        || value.Contains("dummy", StringComparison.OrdinalIgnoreCase);
}
