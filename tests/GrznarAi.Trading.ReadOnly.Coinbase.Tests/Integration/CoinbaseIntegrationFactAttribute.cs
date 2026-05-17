using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// xUnit [Fact] that auto-skips when Coinbase API credentials are not configured.
/// Set CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
/// or create tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/appsettings.test.json.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class CoinbaseIntegrationFactAttribute : FactAttribute
{
    public CoinbaseIntegrationFactAttribute()
    {
        if (!IntegrationTestSupport.HasCredentials())
            Skip = "Run manually: requires CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem";
    }
}
