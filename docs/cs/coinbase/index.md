# Dokumentace

ÄŚeskĂˇ dokumentace pro Coinbase Advanced Trade .NET klient.

## âš ď¸Ź UpozornÄ›nĂ­

Tento projekt **nenĂ­ nijak spojen s Coinbase, nenĂ­ jĂ­m podporovĂˇn ani schvĂˇlen**.

Software je poskytovĂˇn **vĂ˝hradnÄ› pro informaÄŤnĂ­ a vzdÄ›lĂˇvacĂ­ ĂşÄŤely**.

- **Neposkytuje finanÄŤnĂ­ poradenstvĂ­.**
- **NesmĂ­ bĂ˝t jedinĂ˝m podkladem pro obchodnĂ­ rozhodnutĂ­.**
- ZĂ­skanĂˇ data mohou bĂ˝t **nepĹ™esnĂˇ, zpoĹľdÄ›nĂˇ nebo neĂşplnĂˇ**.
- Knihovna v souÄŤasnosti poskytuje pouze **read-only** koncovĂ© body.
  ZĂˇpisovĂ© operace (POST/PUT/DELETE) pĹ™ijdou pozdÄ›ji; jejich pouĹľitĂ­ je
  na vlastnĂ­ riziko uĹľivatele.

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

- `ICoinbaseClient` (facade kombinujĂ­cĂ­ domĂ©novĂˇ rozhranĂ­)
- `ICoinbaseAccountsClient`
- `ICoinbasePortfoliosClient`

HlavnĂ­ registraÄŤnĂ­ metoda:

```csharp
builder.Services.AddCoinbaseClient(builder.Configuration);
```
