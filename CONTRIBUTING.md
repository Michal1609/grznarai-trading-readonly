# Contributing to GrznarAi.Trading.ReadOnly

Thank you for your interest in contributing.

## Before you start

- Check existing [issues](../../issues) and [pull requests](../../pulls) to avoid duplicates.
- For significant changes, open an issue first to discuss the approach.
- This is a **finance library** — correctness and safety take priority over convenience.

## Development setup

Requirements: .NET 9 SDK.

```bash
git clone https://github.com/Michal1609/grznarai-trading-readonly.git
cd grznarai-trading-readonly
dotnet restore GrznarAi.Trading.ReadOnly.slnx
dotnet build GrznarAi.Trading.ReadOnly.slnx --configuration Release -warnaserror
dotnet test GrznarAi.Trading.ReadOnly.slnx --configuration Release
```

Integration tests (`[Category("Integration")]`) require real eToro credentials and are marked `[Explicit]` — they never run in CI.

## Pull request checklist

- [ ] All unit tests pass (`dotnet test`)
- [ ] Build has zero warnings (`-warnaserror`)
- [ ] New public API has XML doc comments
- [ ] Finance calculations have unit tests with exact decimal assertions
- [ ] No secrets committed (run `dotnet tool run gitleaks detect` locally)

## Code style

- Follow existing conventions (`.editorconfig` enforces most rules).
- `ConfigureAwait(false)` on every `await` in library code (`src/Etoro/` and `src/Coinbase/`).
- No comments that explain *what* the code does — only *why* when non-obvious.
- Prefer `decimal` for all monetary values; never `double`/`float`.

## Reporting security issues

See [SECURITY.md](SECURITY.md).

## Code of Conduct

See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
