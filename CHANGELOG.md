# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

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
