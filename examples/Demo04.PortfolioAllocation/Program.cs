/*
 * Demo 04 - Portfolio Allocation
 * ------------------------------
 * Fetches the current eToro PnL snapshot and prints how much money is
 * invested in each instrument. It includes both manually opened positions
 * and positions inside copied people/portfolios from PnL mirrors.
 *
 * Usage:
 *   dotnet run --project examples/Demo04.PortfolioAllocation
 *   dotnet run --project examples/Demo04.PortfolioAllocation -- --account real
 *
 * Credentials:
 *   This demo uses the same UserSecretsId as the integration tests:
 *   GrznarAi.Trading.ReadOnly.Tests.Integration
 *
 *   If you already configured secrets for the test project in Visual Studio,
 *   this demo will read the same values.
 *
 *   You can also use appsettings.json or environment variables:
 *   EToroOptions__ApiKey=...  EToroOptions__UserKey=...
 */

using System.Globalization;
using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Calculations;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var options = ParseArgs(args);
if (!options.IsValid)
{
    Console.Error.WriteLine(options.ErrorMessage);
    Console.Error.WriteLine();
    PrintUsage();
    return 2;
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var credentialProblem = ValidateCredentials(configuration);
if (credentialProblem is not null)
{
    Console.Error.WriteLine(credentialProblem);
    Console.Error.WriteLine();
    Console.Error.WriteLine("Set credentials with user secrets, appsettings.json, or environment variables before running the demo.");
    return 2;
}

var services = new ServiceCollection();
services.AddEToro(configuration);

await using var serviceProvider = services.BuildServiceProvider(
    new ServiceProviderOptions { ValidateOnBuild = true });

var calculations = serviceProvider.GetRequiredService<IEToroCalculationService>();

IReadOnlyList<PortfolioInstrumentAllocation> instruments;
try
{
    instruments = await calculations.GetPortfolioInstrumentAllocationAsync(options.Account);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error calculating {FormatAccount(options.Account)} allocation: {ex.Message}");
    return 1;
}

var assetClasses = calculations.CalculatePortfolioAssetClassAllocation(instruments);
var industries = calculations.CalculatePortfolioIndustryAllocation(instruments);

PrintReport(instruments, assetClasses, industries, options.Account);

return instruments.Count > 0 ? 0 : 1;

static CliOptions ParseArgs(string[] args)
{
    var account = EToroEnvironment.Real;

    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        if (arg.Equals("--account", StringComparison.OrdinalIgnoreCase))
        {
            if (++i >= args.Length)
                return CliOptions.Invalid("Missing value for --account. Expected demo or real.");

            if (args[i].Equals("real", StringComparison.OrdinalIgnoreCase))
                account = EToroEnvironment.Real;
            else if (args[i].Equals("demo", StringComparison.OrdinalIgnoreCase))
                account = EToroEnvironment.Demo;
            else
                return CliOptions.Invalid($"Invalid --account value '{args[i]}'. Expected demo or real.");
        }
        else if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                 arg.Equals("-h", StringComparison.OrdinalIgnoreCase))
        {
            PrintUsage();
            Environment.Exit(0);
        }
        else
        {
            return CliOptions.Invalid($"Unknown argument '{arg}'.");
        }
    }

    return new CliOptions(account);
}

static void PrintReport(
    IReadOnlyList<PortfolioInstrumentAllocation> instruments,
    IReadOnlyList<PortfolioGroupAllocation> assetClasses,
    IReadOnlyList<PortfolioGroupAllocation> industries,
    EToroEnvironment account)
{
    Console.WriteLine("Portfolio Allocation");
    Console.WriteLine($"Account: {FormatAccount(account)}");
    Console.WriteLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine();

    if (instruments.Count == 0)
    {
        Console.WriteLine("No invested open positions were found in PnL.");
        return;
    }

    Console.WriteLine($"Total invested in open instruments: {Usd(instruments.Sum(i => i.InvestedAmount))}");
    Console.WriteLine();
    Console.WriteLine("Allocation by symbol");
    Console.WriteLine($"{"Rank",4} {"Symbol",-18} {"Invested",14} {"Share",9} {"Manual",14} {"Mirrors",14} {"Positions",9}");

    for (var i = 0; i < instruments.Count; i++)
    {
        var row = instruments[i];
        Console.WriteLine(
            $"{i + 1,4} {Shorten(row.Symbol, 18),-18} {Usd(row.InvestedAmount),14} {Percent(row.Share),9} {Usd(row.ManualAmount),14} {Usd(row.MirrorAmount),14} {row.PositionCount,9}");
    }

    PrintGroupReport("Allocation by asset class", assetClasses);
    PrintGroupReport("Allocation by industry", industries);
}

static void PrintGroupReport(
    string title,
    IReadOnlyList<PortfolioGroupAllocation> groups)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine($"{"Rank",4} {"Group",-28} {"Invested",14} {"Share",9} {"Symbols",8} {"Positions",9}");

    for (var i = 0; i < groups.Count; i++)
    {
        var group = groups[i];
        Console.WriteLine(
            $"{i + 1,4} {Shorten(group.GroupName, 28),-28} {Usd(group.InvestedAmount),14} {Percent(group.Share),9} {group.InstrumentCount,8} {group.PositionCount,9}");
    }
}

static void PrintUsage()
{
    Console.WriteLine("Portfolio Allocation");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project examples/Demo04.PortfolioAllocation");
    Console.WriteLine("  dotnet run --project examples/Demo04.PortfolioAllocation -- --account real");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --account demo|real  Account endpoint to query. Default: real.");
}

static string? ValidateCredentials(IConfiguration configuration)
{
    var apiKey = configuration["EToroOptions:ApiKey"];
    var userKey = configuration["EToroOptions:UserKey"];

    if (string.IsNullOrWhiteSpace(apiKey) || IsPlaceholder(apiKey))
        return "Missing EToroOptions:ApiKey.";
    if (string.IsNullOrWhiteSpace(userKey) || IsPlaceholder(userKey))
        return "Missing EToroOptions:UserKey.";

    return null;
}

static bool IsPlaceholder(string value)
{
    return value.Contains("replace-me", StringComparison.OrdinalIgnoreCase)
           || value.Contains("dummy", StringComparison.OrdinalIgnoreCase)
           || value.Contains("your-", StringComparison.OrdinalIgnoreCase);
}

static string FormatAccount(EToroEnvironment account)
{
    return account == EToroEnvironment.Demo ? "demo" : "real";
}

static string Usd(decimal value)
{
    var culture = CultureInfo.GetCultureInfo("en-US");
    return value.ToString("C", culture);
}

static string Percent(decimal value)
{
    return (value * 100m).ToString("F2", CultureInfo.InvariantCulture) + "%";
}

static string Shorten(string value, int maxLength)
{
    if (value.Length <= maxLength)
        return value;

    return value[..Math.Max(0, maxLength - 3)] + "...";
}

internal sealed record CliOptions(EToroEnvironment Account)
{
    public bool IsValid => ErrorMessage is null;
    public string? ErrorMessage { get; init; }

    public static CliOptions Invalid(string message)
    {
        return new CliOptions(EToroEnvironment.Real) { ErrorMessage = message };
    }
}
