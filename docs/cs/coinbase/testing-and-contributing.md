# TestovĂˇnĂ­ a pĹ™ispĂ­vĂˇnĂ­

## Struktura

```
src/
  Coinbase/                                            â€” produkÄŤnĂ­ kĂłd (assembly GrznarAi.Trading.ReadOnly.Coinbase)
examples/
  Coinbase/Demo01.PortfolioBreakdown/                  â€” ukĂˇzka volĂˇnĂ­ proti ĹľivĂ©mu API
tests/
  GrznarAi.Trading.ReadOnly.Coinbase.Tests/            â€” xUnit + RichardSzalay.MockHttp
  GrznarAi.Trading.ReadOnly.Coinbase.Aot.SmokeTest/    â€” Native AOT smoke (PublishAot=true)
```

## SpuĹˇtÄ›nĂ­ testĹŻ

```bash
dotnet test tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests.csproj
```

## SpuĹˇtÄ›nĂ­ Demo01

1. Nastavte credentials pĹ™es user-secrets:
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

OvÄ›Ĺ™Ă­, Ĺľe `CoinbaseJsonContext` (source-gen) pokrĂ˝vĂˇ vĹˇechny serializovanĂ© typy.

## Konvence

- NovĂ© endpointy:
  1. Modely v `Models/<DomĂ©na>/`.
  2. Zaregistrovat top-level response v `CoinbaseJsonContext`.
  3. DomĂ©novĂ© rozhranĂ­ `ICoinbase<DomĂ©na>Client`.
  4. Implementace v `Client/CoinbaseClient.<DomĂ©na>.cs`.
  5. ZaĹ™adit domĂ©novĂ© rozhranĂ­ do `ICoinbaseClient`.
  6. Test: deserializace + cesta requestu (MockHttp) + chyba 4xx/5xx.
- Strings â†’ `Uri.EscapeDataString` pĹ™es `QueryStringBuilder`.
- Decimal hodnoty z API stringĹŻ â†’ `DecimalStringConverter`/`NullableDecimalStringConverter`.
- Ĺ˝ĂˇdnĂ˝ `JsonStringEnumConverter` bez generickĂ©ho parametru (reflection-based, ne-AOT). VĹľdy `JsonStringEnumConverter<T>` + `[JsonStringEnumMemberName]`.
