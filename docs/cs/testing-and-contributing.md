# Testování a přispívání

## Build

```powershell
dotnet restore
dotnet build
```

## Unit testy

```powershell
dotnet test
```

Unit testy jsou navržené tak, aby běžely offline.

## Integrační testy

Integrační testy volají reálné eToro API a vyžadují credentials.

Použijte user secrets nebo environment proměnné:

```powershell
dotnet user-secrets set "EToroOptions:ApiKey" "<api-key>" --project tests/GrznarAi.Trading.ReadOnly.Tests
dotnet user-secrets set "EToroOptions:UserKey" "<user-key>" --project tests/GrznarAi.Trading.ReadOnly.Tests
```

Nebo vytvořte lokální ignorovaný `tests/GrznarAi.Trading.ReadOnly.Tests/appsettings.test.json` podle `appsettings.test.example.json`.

Skutečné credentials nikdy necommitujte.

## Pravidla pro příspěvky

- Pull requesty držte úzké a zaměřené.
- Pro změny chování přidejte nebo upravte testy.
- Změny public API popište explicitně v dokumentaci.
- Necommitujte secrets, build output ani lokální IDE stav.
- Preferujte existující vzory v `src/EToro/` (zdrojový adresář).

## Struktura projektu

```text
src/EToro/                                        Zdrojový kód knihovny
tests/GrznarAi.Trading.ReadOnly.Tests/            Unit a integrační testy
docs/en/                       Anglická dokumentace
docs/cs/                       Česká dokumentace
```
