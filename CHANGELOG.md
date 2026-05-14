# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [1.0.0-alpha.4] - 2026-05-14

### Breaking Changes
- `GrznarAi.Trading.ReadOnly` is now the shared Core package and no longer contains the eToro client.
- eToro consumers must install `GrznarAi.Trading.ReadOnly.Etoro` and use the `GrznarAi.Trading.ReadOnly.Etoro.*` namespaces.

### Added
- Added `GrznarAi.Trading.ReadOnly.Etoro` package for the eToro client.
- Added `GrznarAi.Trading.ReadOnly.Coinbase` package for Coinbase Advanced Trade read endpoints.
- Added shared Core package with HTTP handlers, rate limiting, resilience options, query helpers, JSON helpers, and base exception type.
- Added opt-in diagnostics: `DiagnosticOptions`, `IApiDiagnostics`, `ApiDiagnostics`, `ApiResponseSnapshot`, and `DiagnosticCapturingHandler`.
- Added Core, eToro, and Coinbase AOT smoke tests.

### Changed
- Split repository structure into `src/Core`, `src/Etoro`, and `src/Coinbase`.
- CI now builds, tests, AOT-publishes smoke tests, packs all three packages, and validates package contents.

### Documentation
- Added Core documentation in English and Czech.
- Updated README and NuGet README for the Core plus per-platform package structure.
- Added Coinbase documentation in English and Czech.

## [1.0.0-alpha.3] - 2026-05-09

### Added
- Added `Demo04.PortfolioAllocation`, a console example that prints invested allocation by symbol, asset class, and industry, including positions from copied people/portfolio mirrors.
- Extended `IEToroCalculationService` with portfolio allocation analytics:
  - `GetPortfolioInstrumentAllocationAsync`
  - `CalculatePortfolioAssetClassAllocation`
  - `CalculatePortfolioIndustryAllocation`
- Added `PortfolioInstrumentAllocation` and `PortfolioGroupAllocation` calculation models.

### Documentation
- Documented portfolio allocation calculations in English and Czech account-calculation docs.

## [1.0.0-alpha.2] - 2026-05-07

### Fixed
- Raised the `SearchInstrumentsAsync` `pageSize` limit to `20000`, allowing all roughly 12000 instruments to be downloaded in a single request.

## [1.0.0-alpha.1] - 2026-05-02

### Added
- Initial public release.
- Typed .NET client for the eToro public API (`IEToroClient`).
- `HttpClientFactory` integration with DI extension `AddEToro`.
- Rate-limit handling with sliding window and per-user-key keyed limiter.
- Authentication handler for `x-api-key` / `x-user-key` headers.
- Automatic retry on HTTP 429 with `Retry-After` support.
- API exception type (`EToroApiException`) with response body redaction.
- Account calculation service (`IEToroCalculationService`): available cash, invested principal, unrealized PnL, realized PnL, equity, total return.
- Read-only API areas: Trading, Market Data, User Info, Social, Feed, Watchlists.
- Source-generated JSON serialization (AOT-path ready).
- Symbols package (snupkg) with SourceLink.
