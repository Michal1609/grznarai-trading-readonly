# TestovĂˇnĂ­ a pĹ™ispĂ­vĂˇnĂ­

## Build

```powershell
dotnet restore
dotnet build
```

## Unit testy

```powershell
dotnet test
```

Unit testy jsou navrĹľenĂ© tak, aby bÄ›Ĺľely offline.

## IntegraÄŤnĂ­ testy

IntegraÄŤnĂ­ testy volajĂ­ reĂˇlnĂ© eToro API a vyĹľadujĂ­ credentials.

PouĹľijte user secrets nebo environment promÄ›nnĂ©:

```powershell
dotnet user-secrets set "EToroOptions:ApiKey" "<api-key>" --project tests/GrznarAi.Trading.ReadOnly.Etoro.Tests
dotnet user-secrets set "EToroOptions:UserKey" "<user-key>" --project tests/GrznarAi.Trading.ReadOnly.Etoro.Tests
```

Nebo vytvoĹ™te lokĂˇlnĂ­ ignorovanĂ˝ `tests/GrznarAi.Trading.ReadOnly.Etoro.Tests/appsettings.test.json` podle `appsettings.test.example.json`.

SkuteÄŤnĂ© credentials nikdy necommitujte.

## Pravidla pro pĹ™Ă­spÄ›vky

- Pull requesty drĹľte ĂşzkĂ© a zamÄ›Ĺ™enĂ©.
- Pro zmÄ›ny chovĂˇnĂ­ pĹ™idejte nebo upravte testy.
- ZmÄ›ny public API popiĹˇte explicitnÄ› v dokumentaci.
- Necommitujte secrets, build output ani lokĂˇlnĂ­ IDE stav.
- Preferujte existujĂ­cĂ­ vzory v `src/Etoro/` (zdrojovĂ˝ adresĂˇĹ™).

## Struktura projektu

```text
src/Etoro/                                        ZdrojovĂ˝ kĂłd knihovny
tests/GrznarAi.Trading.ReadOnly.Etoro.Tests/            Unit a integraÄŤnĂ­ testy
docs/en/                       AnglickĂˇ dokumentace
docs/cs/                       ÄŚeskĂˇ dokumentace
```
