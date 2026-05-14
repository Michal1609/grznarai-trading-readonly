# CI/CD pipeliny

Repozitář používá GitHub Actions pro validaci pull requestů, bezpečnostní kontroly, ověření balíčku a publikování na NuGet.

Výchozí tok přispívání je:

```text
feature branch -> pull request -> povinné kontroly -> merge do main
```

Přímé pushe do `main` mají být blokované přes GitHub branch protection. Workflow soubory jsou v `.github/workflows/`, ale tato dokumentace je v `docs/`, aby byl pipeline kontrakt dostupný spolu se zbytkem dokumentace pro přispěvatele.

## CI

Workflow soubor: `.github/workflows/ci.yml`

Spouští se při:

- pull requestech mířících do `main`
- pushi do `main`
- ručním spuštění přes `workflow_dispatch`

Účel:

- obnovit NuGet balíčky
- sestavit `GrznarAi.Trading.ReadOnly.slnx` v konfiguraci `Release`
- považovat warningy za chyby
- spustit test suite
- nahrát `.trx` výsledky testů
- ověřit NativeAOT kompatibilitu na Linuxu
- vytvořit NuGet package artefakty pro kontrolu
- spustit Podman smoke test v oficiálním .NET SDK kontejneru

Hlavní build a test job běží na `ubuntu-latest` i `windows-latest`. Linux navíc provádí NativeAOT publish a vytvoření balíčku, protože výstup balíčku stačí ověřit jednou za běh.

Podman job je oddělený od OS matrixu. Ověřuje, že projekt jde otestovat v čistém kontejnerizovaném .NET SDK prostředí. U public repozitáře je to užitečné, protože přispěvatelé mohou používat různé lokální stroje, zatímco kontejner dává opakovatelný Linux baseline.

CI pouze vytváří package artefakty. Nepublikuje balíčky na NuGet.org.

## Secret Scan

Workflow soubor: `.github/workflows/secret-scan.yml`

Spouští se při:

- pull requestech mířících do `main`
- pushi do `main`
- týdenním plánovaném scanu
- ručním spuštění přes `workflow_dispatch`

Účel:

- proskenovat historii repozitáře přes Gitleaks
- zachytit omylem commitnuté API klíče, tokeny, lokální konfigurace a další secrets
- udržet public repozitář bezpečný pro externí přispěvatele

Checkout používá celou historii (`fetch-depth: 0`), aby Gitleaks nekontroloval jen poslední commit.

## CodeQL

Workflow soubor: `.github/workflows/codeql.yml`

Spouští se při:

- pull requestech mířících do `main`
- pushi do `main`
- týdenním plánovaném scanu
- ručním spuštění přes `workflow_dispatch`

Účel:

- spustit GitHub CodeQL analýzu pro C#
- zapsat bezpečnostní a code quality nálezy do GitHub code scanning
- zachytit běžné zranitelnostní vzory před mergem

Workflow používá `build-mode: none`. Pro tuto C# knihovnu je to vhodné, protože normální build už pokrývá CI a CodeQL umí analyzovat zdrojové kódy bez opakování celého buildu.

## Dependency Review

Workflow soubor: `.github/workflows/dependency-review.yml`

Spouští se při:

- pull requestech mířících do `main`

Účel:

- zkontrolovat změny NuGet a GitHub Actions závislostí přidané pull requestem
- shodit pull request, pokud přidává novou high-severity zranitelnou závislost
- zviditelnit riziko závislostí před mergem

Tento workflow je záměrně jen pro PR, protože dependency review porovnává pull request proti cílové větvi.

## Publish NuGet

Workflow soubor: `.github/workflows/publish-nuget.yml`

Spouští se při:

- ručním spuštění přes `workflow_dispatch`

Účel:

- publikovat schválený release z `main`
- zvalidovat zadanou package verzi
- ověřit, že `CHANGELOG.md` obsahuje release notes pro danou verzi
- ověřit, že odpovídající git tag už neukazuje na jiný commit
- ověřit, že daná verze balíčku ještě neexistuje na NuGet.org
- obnovit balíčky
- sestavit projekt v `Release`
- spustit testy
- vytvořit `.nupkg` a `.snupkg` artefakty
- nahrát package artefakty k workflow runu
- vytvořit anotovaný git tag `v{version}`
- publikovat package a symboly na NuGet.org
- vytvořit GitHub Release s přiloženými package soubory

Workflow vyžaduje repository secret `NUGET_API_KEY`. Zároveň vyžaduje hodnotu `confirm_publish=publish`, aby bylo těžší spustit publikování omylem.

Workflow se musí spouštět z `main`. Checkoutuje `main` explicitně a selže, pokud při ručním spuštění není vybraná větev `main`.

## Verzování

NuGet verze jsou SemVer-style package verze:

```text
Major.Minor.Patch[-prerelease]
```

První public prerelease pro tento projekt je:

```text
1.0.0-alpha.1
```

Doporučená prerelease posloupnost:

```text
1.0.0-alpha.1
1.0.0-alpha.2
1.0.0-beta.1
1.0.0-rc.1
1.0.0
```

Používejte tečkované prerelease číslování jako `alpha.1`, `beta.1` a `rc.1`. Po publikování na NuGet.org už stejnou verzi znovu nepoužívejte. Package verze jsou pro konzumenty prakticky neměnné a pokus publikovat stejný package id/version pár skončí konfliktem.

NuGet označuje balíčky jako prerelease automaticky podle suffixu ve verzi. Jakákoli verze se suffixem za pomlčkou, například `1.0.0-alpha.1`, `1.0.0-beta.1` nebo `1.0.0-rc.1`, je prerelease balíček. Stable verze suffix nemá, například `1.0.0`. Publish workflow pro NuGet.org nepotřebuje žádný samostatný prerelease flag.

Release verze se neodvozuje z počtu commitů ani pull requestů. Release owner zvolí přesnou verzi při ručním spuštění workflow `Publish NuGet`. Workflow tuto hodnotu předává do MSBuildu jako `Version` a `PackageVersion`, takže v repozitáři není potřeba release commit, který mění jen číslo verze.

Před spuštěním releasu:

- mergněte zamýšlené změny do `main`
- přidejte do `CHANGELOG.md` sekci pro přesnou verzi, například `## [1.0.0-alpha.1] - 2026-05-02`
- spusťte workflow `Publish NuGet` z větve `main`
- nastavte `version` na přesnou NuGet verzi
- nastavte `confirm_publish` na `publish`

`src/Etoro/GrznarAi.Trading.ReadOnly.Etoro.csproj` obsahuje lokální fallback verzi pro běžné lokální balení. Source of truth pro publikované verze je publish workflow.

## Branch Protection

Nastavení repozitáře by mělo vyžadovat pull request před mergem do `main`.

Doporučené required checks:

- `Build and test (ubuntu-latest)`
- `Build and test (windows-latest)`
- `Podman smoke`
- `Gitleaks`
- `Analyze C#`
- `Review dependency changes`

Doporučené branch protection nastavení:

- vyžadovat pull request před mergem
- vyžadovat úspěšné status checks před mergem
- vyžadovat aktuální branch před mergem
- blokovat force push
- blokovat smazání větve
- zneplatnit starší approvals po pushi nových commitů
- vyžadovat review od code owners, až bude `CODEOWNERS` rozšířen za aktuální catch-all owner záznam

## Bezpečnost NuGet publish

Publish workflow používá tyto pojistky:

- pouze ruční trigger
- pouze větev `main`
- explicitní vstup `version`
- explicitní vstup `confirm_publish=publish`
- kontrolu existence package verze na NuGet.org před publikováním
- kontrolu changelogu před publikováním
- kontrolu release tagu před publikováním
- GitHub environment `nuget.org`, ve kterém lze v repository settings nastavit required reviewers
