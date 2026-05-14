# Dokumentace

ÄŚeskĂˇ dokumentace pro eToro .NET client.

## âš ď¸Ź UpozornÄ›nĂ­

Tento projekt **nenĂ­ nijak spojen s eToro, nenĂ­ jĂ­m podporovĂˇn ani schvĂˇlen**.

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
nesprĂˇvnĂ© vĂ˝poÄŤty, vĂ˝padky, ztrĂˇtu dat ÄŤi poruĹˇenĂ­ podmĂ­nek eToro.
Software pouĹľĂ­vejte vĂ˝hradnÄ› na vlastnĂ­ nebezpeÄŤĂ­.

- [ZaÄŤĂ­nĂˇme](getting-started.md)
- [Konfigurace](configuration.md)
- [API reference](api-reference.md)
- [Error handling](error-handling.md)
- [Rate limiting a retry](rate-limiting.md)
- [VĂ˝poÄŤty ĂşÄŤtu](account-calculations.md)
- [TestovĂˇnĂ­ a pĹ™ispĂ­vĂˇnĂ­](testing-and-contributing.md)
- [CI/CD pipeliny](ci-cd.md)

HlavnĂ­ balĂ­ÄŤek: `GrznarAi.Trading.ReadOnly.Etoro`

HlavnĂ­ sluĹľby:

- `IEToroClient`
- `IEToroCalculationService`

HlavnĂ­ registraÄŤnĂ­ metoda:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

## BudoucĂ­ zĂˇpisovĂ© endpointy

PĹ™ed pĹ™idĂˇnĂ­m POST, PUT nebo DELETE endpointĹŻ do veĹ™ejnĂ©ho API:

- KaĹľdĂ˝ zĂˇpisovĂ˝ poĹľadavek bude vyĹľadovat idempotency key.
- `RetryNonIdempotentRequests` zĹŻstane ve vĂ˝chozĂ­m stavu `false`; retry zĂˇpisĹŻ musĂ­ bĂ˝t vĂ˝slovnĂ˝.
- ZĂˇpisovĂ© operace budou mĂ­t per-method rate limit oddÄ›lenĂ˝ od ÄŤtecĂ­ch limitĹŻ.
- VĂ˝poÄŤetnĂ­ helpery nebudou automaticky zadĂˇvat obchody podle heuristik.
- SamostatnĂ© rozhranĂ­ pro objednĂˇvky zĹŻstane oddÄ›lenĂ© od read rozhranĂ­.
- Chyby zĂˇpisu budou mĂ­t vlastnĂ­ typy vĂ˝jimek s broker-side error kĂłdy.
- KaĹľdĂ˝ pokus o zĂˇpis bude mĂ­t audit-log hook s redigovanĂ˝m payloadem.
