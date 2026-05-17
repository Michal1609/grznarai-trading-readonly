# Dokumentace

ÄŚeskĂˇ dokumentace pro Coinbase Advanced Trade .NET klient.

## âš ď¸Ź UpozornÄ›nĂ­

Tento projekt **nenĂ­ nijak spojen s Coinbase, nenĂ­ jĂ­m podporovĂˇn ani schvĂˇlen**.

Software je poskytovĂˇn **vĂ˝hradnÄ› pro informaÄŤnĂ­ a vzdÄ›lĂˇvacĂ­ ĂşÄŤely**.

- **Neposkytuje finanÄŤnĂ­ poradenstvĂ­.**
- **NesmĂ­ bĂ˝t jedinĂ˝m podkladem pro obchodnĂ­ rozhodnutĂ­.**
- ZĂ­skanĂˇ data mohou bĂ˝t **nepĹ™esnĂˇ, zpoĹľdÄ›nĂˇ nebo neĂşplnĂˇ**.
- Knihovna poskytuje pouze **read-only** koncové body. **Implementovány jsou všechny dokumentované GET endpointy Coinbase Advanced Trade API.** Zápisové operace (zadání orderu, zrušení, convert commit atd.) nejsou vystaveny a ani se to neplánuje — balíček je read-only záměrně.

Autor a pĹ™ispÄ›vatelĂ© **odmĂ­tajĂ­ veĹˇkerou odpovÄ›dnost** za jakĂ©koli pĹ™Ă­mĂ©,
nepĹ™Ă­mĂ©, vedlejĹˇĂ­, nĂˇslednĂ© nebo sankÄŤnĂ­ Ĺˇkody vzniklĂ© v souvislosti
s pouĹľĂ­vĂˇnĂ­m tohoto softwaru, zejmĂ©na za finanÄŤnĂ­ ztrĂˇty, propasenĂ© obchody,
nesprĂˇvnĂ© vĂ˝poÄŤty, vĂ˝padky, ztrĂˇtu dat ÄŤi poruĹˇenĂ­ podmĂ­nek Coinbase.
Software pouĹľĂ­vejte vĂ˝hradnÄ› na vlastnĂ­ nebezpeÄŤĂ­.

- [ZaÄŤĂ­nĂˇme](getting-started.md)
- [Konfigurace](configuration.md)
- [API reference](api-reference.md)
- [Error handling](error-handling.md)
- [Rate limiting a retry](rate-limiting.md)
- [TestovĂˇnĂ­ a pĹ™ispĂ­vĂˇnĂ­](testing-and-contributing.md)
- [CI/CD pipeliny](ci-cd.md)

HlavnĂ­ balĂ­ÄŤek: `GrznarAi.Trading.ReadOnly.Coinbase`

HlavnĂ­ sluĹľby:

- `ICoinbaseClient` (facade kombinující všechna doménová rozhraní)
- `ICoinbaseAccountsClient`
- `ICoinbasePortfoliosClient`
- `ICoinbaseOrdersClient`
- `ICoinbaseProductsClient`
- `ICoinbasePublicClient`
- `ICoinbaseFeesClient`
- `ICoinbaseFuturesClient`
- `ICoinbasePerpetualClient`
- `ICoinbasePaymentMethodsClient`
- `ICoinbaseConvertClient`
- `ICoinbaseDataApiClient`

Kompletní katalog metod je v [API reference](api-reference.md). Samostatná examples aplikace pro jednotlivé endpointy neexistuje — nejúplnější ukázky použití najdete v unit a integračních testech v `tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests/`.

HlavnĂ­ registraÄŤnĂ­ metoda:

```csharp
builder.Services.AddCoinbaseClient(builder.Configuration);
```
