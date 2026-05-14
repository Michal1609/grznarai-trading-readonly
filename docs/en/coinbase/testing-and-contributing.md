# Testing & contributing

## Layout

```
src/
  Coinbase/                                            â€” production code (assembly GrznarAi.Trading.ReadOnly.Coinbase)
examples/
  Coinbase/Demo01.PortfolioBreakdown/                  â€” sample call against the live API
tests/
  GrznarAi.Trading.ReadOnly.Coinbase.Tests/            â€” xUnit + RichardSzalay.MockHttp
  GrznarAi.Trading.ReadOnly.Coinbase.Aot.SmokeTest/    â€” Native AOT smoke (PublishAot=true)
```

## Run tests

```bash
dotnet test tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests.csproj
```

## Run Demo01

1. Set credentials via user-secrets:
   ```bash
   cd examples/Coinbase/Demo01.PortfolioBreakdown
   dotnet user-secrets set "Coinbase:KeyName" "organizations/{org}/apiKeys/{id}"
   dotnet user-secrets set "Coinbase:PrivateKeyPem" "-----BEGIN EC PRIVATE KEY-----\n...\n-----END EC PRIVATE KEY-----\n"
   ```
2. `dotnet run --project examples/Coinbase/Demo01.PortfolioBreakdown`

## AOT smoke

```bash
dotnet publish tests/GrznarAi.Trading.ReadOnly.Coinbase.Aot.SmokeTest -c Release -r win-x64
```

Verifies that `CoinbaseJsonContext` (source-gen) covers all serialised types.

## Conventions

- New endpoints:
  1. Models in `Models/<Domain>/`.
  2. Register the top-level response in `CoinbaseJsonContext`.
  3. Domain interface `ICoinbase<Domain>Client`.
  4. Implementation in `Client/CoinbaseClient.<Domain>.cs`.
  5. Add the interface to `ICoinbaseClient`.
  6. Test: deserialisation + request path (MockHttp) + 4xx/5xx error.
- Strings â†’ `Uri.EscapeDataString` via `QueryStringBuilder`.
- Decimal values from API strings â†’ `DecimalStringConverter` / `NullableDecimalStringConverter`.
- No bare `JsonStringEnumConverter` (reflection-based, non-AOT). Always `JsonStringEnumConverter<T>` + `[JsonStringEnumMemberName]`.
