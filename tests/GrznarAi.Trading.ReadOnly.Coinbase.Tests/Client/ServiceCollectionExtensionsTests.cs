using GrznarAi.Trading.ReadOnly.Coinbase;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Client;

public class ServiceCollectionExtensionsTests
{
    private static ServiceProvider BuildProvider()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Coinbase:KeyName"] = "organizations/x/apiKeys/y",
                ["Coinbase:PrivateKeyPem"] = "-----BEGIN EC PRIVATE KEY-----\nfake\n-----END EC PRIVATE KEY-----\n",
                ["Coinbase:BaseUrl"] = "https://api.coinbase.com"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddCoinbaseClient(config);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Facade_resolves()
    {
        using var sp = BuildProvider();
        var client = sp.GetRequiredService<ICoinbaseClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void Domain_interfaces_resolve()
    {
        using var sp = BuildProvider();
        Assert.NotNull(sp.GetRequiredService<ICoinbaseAccountsClient>());
        Assert.NotNull(sp.GetRequiredService<ICoinbasePortfoliosClient>());
    }

    [Fact]
    public void Domain_interfaces_return_CoinbaseClient()
    {
        using var sp = BuildProvider();
        var portfolios = sp.GetRequiredService<ICoinbasePortfoliosClient>();
        var accounts = sp.GetRequiredService<ICoinbaseAccountsClient>();
        Assert.IsType<CoinbaseClient>(portfolios);
        Assert.IsType<CoinbaseClient>(accounts);
    }
}
