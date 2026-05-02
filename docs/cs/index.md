# Dokumentace

Česká dokumentace pro eToro .NET client.

## ⚠️ Upozornění

Tento projekt **není nijak spojen s eToro, není jím podporován ani schválen**.

Software je poskytován **výhradně pro informační a vzdělávací účely**.

- **Neposkytuje finanční poradenství.**
- **Nesmí být jediným podkladem pro obchodní rozhodnutí.**
- Získaná data mohou být **nepřesná, zpožděná nebo neúplná**.
- Knihovna v současnosti poskytuje pouze **read-only** koncové body.
  Zápisové operace (POST/PUT/DELETE) přijdou později; jejich použití je
  na vlastní riziko uživatele.

Autor a přispěvatelé **odmítají veškerou odpovědnost** za jakékoli přímé,
nepřímé, vedlejší, následné nebo sankční škody vzniklé v souvislosti
s používáním tohoto softwaru, zejména za finanční ztráty, propasené obchody,
nesprávné výpočty, výpadky, ztrátu dat či porušení podmínek eToro.
Software používejte výhradně na vlastní nebezpečí.

- [Začínáme](getting-started.md)
- [Konfigurace](configuration.md)
- [API reference](api-reference.md)
- [Error handling](error-handling.md)
- [Rate limiting a retry](rate-limiting.md)
- [Výpočty účtu](account-calculations.md)
- [Testování a přispívání](testing-and-contributing.md)
- [CI/CD pipeliny](ci-cd.md)

Hlavní balíček: `GrznarAi.Trading.ReadOnly`

Hlavní služby:

- `IEToroClient`
- `IEToroCalculationService`

Hlavní registrační metoda:

```csharp
builder.Services.AddEToro(builder.Configuration);
```

## Budoucí zápisové endpointy

Před přidáním POST, PUT nebo DELETE endpointů do veřejného API:

- Každý zápisový požadavek bude vyžadovat idempotency key.
- `RetryNonIdempotentRequests` zůstane ve výchozím stavu `false`; retry zápisů musí být výslovný.
- Zápisové operace budou mít per-method rate limit oddělený od čtecích limitů.
- Výpočetní helpery nebudou automaticky zadávat obchody podle heuristik.
- Samostatné rozhraní pro objednávky zůstane oddělené od read rozhraní.
- Chyby zápisu budou mít vlastní typy výjimek s broker-side error kódy.
- Každý pokus o zápis bude mít audit-log hook s redigovaným payloadem.
