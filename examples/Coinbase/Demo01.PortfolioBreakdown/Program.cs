/*
 * Demo 01 â€” Portfolio Breakdown
 * â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
 * Lists Coinbase portfolios for the authenticated CDP key, picks the first one,
 * fetches its full breakdown (balances + spot/perp/futures positions) and prints
 * a concise summary plus the top spot holdings.
 *
 * Usage:
 *   dotnet run
 *
 * Credentials â€” choose one:
 *   A) User Secrets (recommended â€” keys stay out of source control):
 *        dotnet user-secrets set "Coinbase:KeyName"       "organizations/{org}/apiKeys/{id}"
 *        dotnet user-secrets set "Coinbase:PrivateKeyPem" "-----BEGIN EC PRIVATE KEY-----\n...\n-----END EC PRIVATE KEY-----\n"
 *   B) Environment variables:
 *        Coinbase__KeyName=... Coinbase__PrivateKeyPem=...
 *   C) appsettings.json â€” edit KeyName / PrivateKeyPem directly (not safe for real keys)
 */

using GrznarAi.Trading.ReadOnly.Coinbase;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;
using GrznarAi.Trading.ReadOnly.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    PrintCredentialDiagnostics(configuration);
    Console.Error.WriteLine();
    Console.Error.WriteLine("Set credentials with user secrets or environment variables before running the demo.");
    Console.Error.WriteLine("See SECRETS.md in this folder for ready-to-paste commands.");
    return 2;
}

var services = new ServiceCollection();
services.AddCoinbaseClient(configuration);

await using var serviceProvider = services.BuildServiceProvider(
    new ServiceProviderOptions { ValidateOnBuild = true });

var portfolios = serviceProvider.GetRequiredService<ICoinbasePortfoliosClient>();

// â”€â”€ 1. Pick a portfolio â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Console.WriteLine("Fetching portfoliosâ€¦");

ListPortfoliosResponse list;
try
{
    list = await portfolios.ListPortfoliosAsync();
}
catch (OperationCanceledException ex)
{
    Console.Error.WriteLine($"Listing portfolios was canceled: {ex.Message}");
    return 1;
}
catch (TradingApiException ex)
{
    Console.Error.WriteLine($"Error listing portfolios: {ex.Message}");
    return 1;
}

var portfolio = list.Portfolios?.FirstOrDefault(p => p.Deleted != true);
if (portfolio?.Uuid is null)
{
    Console.Error.WriteLine("No portfolio available for this account.");
    return 1;
}

Console.WriteLine($"Using portfolio: {portfolio.Name} ({portfolio.Type}) â€” {portfolio.Uuid}");
Console.WriteLine();

// â”€â”€ 2. Fetch the breakdown in USD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
GetPortfolioBreakdownResponse response;
try
{
    response = await portfolios.GetPortfolioBreakdownAsync(portfolio.Uuid, currency: "USD");
}
catch (OperationCanceledException ex)
{
    Console.Error.WriteLine($"Fetching breakdown was canceled: {ex.Message}");
    return 1;
}
catch (TradingApiException ex)
{
    Console.Error.WriteLine($"Error fetching breakdown: {ex.Message}");
    return 1;
}

var breakdown = response.Breakdown
    ?? throw new InvalidOperationException("Empty breakdown payload.");

var balances = breakdown.PortfolioBalances;
var spot     = breakdown.SpotPositions ?? new List<SpotPosition>();
var perp     = breakdown.PerpPositions ?? new List<PerpPosition>();
var futures  = breakdown.FuturesPositions ?? new List<FuturesPosition>();

// â”€â”€ 3. Summary â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Console.WriteLine("Portfolio Breakdown");
Console.WriteLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine();

Console.WriteLine($"{"Total balance:",-28} {Money(balances?.TotalBalance?.Value),16} {balances?.TotalBalance?.Currency}");
Console.WriteLine($"{"  Crypto balance:",-28} {Money(balances?.TotalCryptoBalance?.Value),16} {balances?.TotalCryptoBalance?.Currency}");
Console.WriteLine($"{"  Cash equivalent:",-28} {Money(balances?.TotalCashEquivalentBalance?.Value),16} {balances?.TotalCashEquivalentBalance?.Currency}");
Console.WriteLine($"{"  Futures balance:",-28} {Money(balances?.TotalFuturesBalance?.Value),16} {balances?.TotalFuturesBalance?.Currency}");
Console.WriteLine($"{"Futures unrealized PnL:",-28} {Money(balances?.FuturesUnrealizedPnl?.Value),16} {balances?.FuturesUnrealizedPnl?.Currency}");
Console.WriteLine($"{"Perp unrealized PnL:",-28} {Money(balances?.PerpUnrealizedPnl?.Value),16} {balances?.PerpUnrealizedPnl?.Currency}");

Console.WriteLine();
Console.WriteLine($"Spot positions:    {spot.Count}");
Console.WriteLine($"Perp positions:    {perp.Count}");
Console.WriteLine($"Futures positions: {futures.Count}");

// â”€â”€ 4. Top spot positions by fiat value â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
const int TopN = 5;
var top = spot
    .Where(s => s.IsCash != true)
    .OrderByDescending(s => s.TotalBalanceFiat ?? 0m)
    .Take(TopN)
    .ToList();

if (top.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine($"Top {top.Count} spot holdings:");
    Console.WriteLine($"  {"Asset",-8} {"Fiat",14}  {"Allocation",10}  {"1d change",10}  {"Unrealized PnL",16}");
    foreach (var s in top)
    {
        var alloc = s.Allocation.HasValue ? s.Allocation.Value * 100m : (decimal?)null;
        Console.WriteLine(
            $"  {s.Asset,-8} {Money(s.TotalBalanceFiat),14}  {Pct(alloc),10}  {Pct(s.OneDayChange),10}  {Money(s.UnrealizedPnl),16}");
    }
}

// â”€â”€ 5. Open perp positions (compact list) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
if (perp.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("Perp positions:");
    foreach (var p in perp)
    {
        var side = p.PositionSide?.ToString() ?? "-";
        Console.WriteLine(
            $"  {p.Symbol,-12} {side,-6} size={p.NetSize}  mark={p.MarkPrice?.Value:N2}  uPnL={p.UnrealizedPnl?.Value:N2} {p.UnrealizedPnl?.Currency}");
    }
}

return 0;

// â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

static string Money(decimal? v) =>
    v is null ? "n/a" : (v < 0 ? $"-${Math.Abs(v.Value):N2}" : $"${v.Value:N2}");

static string Pct(decimal? v) =>
    v is null ? "n/a" : $"{v.Value:F2}%";

static string? ValidateCredentials(IConfiguration configuration)
{
    var keyName = configuration["Coinbase:KeyName"];
    var pem = configuration["Coinbase:PrivateKeyPem"];

    if (string.IsNullOrWhiteSpace(keyName) || IsPlaceholder(keyName))
        return "Missing Coinbase:KeyName.";
    if (string.IsNullOrWhiteSpace(pem) || IsPlaceholder(pem))
        return "Missing Coinbase:PrivateKeyPem.";

    return null;
}

static bool IsPlaceholder(string value) =>
    value.Contains("replace-me", StringComparison.OrdinalIgnoreCase)
    || value.Contains("dummy", StringComparison.OrdinalIgnoreCase)
    || value.Contains("your-", StringComparison.OrdinalIgnoreCase);

static void PrintCredentialDiagnostics(IConfiguration configuration)
{
    var userSecretsId = typeof(Program).Assembly
        .GetCustomAttributes(typeof(Microsoft.Extensions.Configuration.UserSecrets.UserSecretsIdAttribute), false)
        .OfType<Microsoft.Extensions.Configuration.UserSecrets.UserSecretsIdAttribute>()
        .FirstOrDefault()?.UserSecretsId;

    string? secretsPath = null;
    if (!string.IsNullOrEmpty(userSecretsId))
    {
        var appData = Environment.GetEnvironmentVariable("APPDATA");
        secretsPath = !string.IsNullOrEmpty(appData)
            ? Path.Join(appData, "Microsoft", "UserSecrets", userSecretsId, "secrets.json")
            : Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".microsoft", "usersecrets", userSecretsId, "secrets.json");
    }

    Console.Error.WriteLine("â”€â”€ Credential diagnostics â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
    Console.Error.WriteLine($"UserSecretsId : {userSecretsId ?? "(not set on assembly)"}");
    Console.Error.WriteLine($"Secrets file  : {secretsPath ?? "(unknown)"}");
    if (secretsPath is not null)
        Console.Error.WriteLine($"Exists        : {File.Exists(secretsPath)}");
    Console.Error.WriteLine($"AppContext    : {AppContext.BaseDirectory}");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Effective Coinbase:* values (after merging appsettings + user-secrets + env):");
    foreach (var key in new[] { "Coinbase:KeyName", "Coinbase:PrivateKeyPem", "Coinbase:BaseUrl", "Coinbase:JwtLifetimeSeconds" })
    {
        var v = configuration[key];
        var redacted = key.Contains("PrivateKeyPem", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(v)
            ? $"[{v.Length} chars]"
            : v ?? "(null)";
        Console.Error.WriteLine($"  {key,-32} = {redacted}");
    }
}
