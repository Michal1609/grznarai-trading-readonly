# Testing and Contributing

## Build

```powershell
dotnet restore
dotnet build
```

## Unit Tests

```powershell
dotnet test
```

Unit tests are designed to run offline.

## Integration Tests

Integration tests call the real eToro API and require credentials.

Use user secrets or environment variables:

```powershell
dotnet user-secrets set "EToroOptions:ApiKey" "<api-key>" --project tests/GrznarAi.Trading.ReadOnly.Etoro.Tests
dotnet user-secrets set "EToroOptions:UserKey" "<user-key>" --project tests/GrznarAi.Trading.ReadOnly.Etoro.Tests
```

Or create a local ignored `tests/GrznarAi.Trading.ReadOnly.Etoro.Tests/appsettings.test.json` from `appsettings.test.example.json`.

Do not commit real credentials.

## Contribution Guidelines

- Keep pull requests focused.
- Add or update tests for behavioral changes.
- Keep public API changes explicit and documented.
- Do not commit secrets, generated build output, or local IDE state.
- Prefer existing patterns in `src/Etoro/` (the source directory).

## Project Structure

```text
src/Etoro/                                        Library source
tests/GrznarAi.Trading.ReadOnly.Etoro.Tests/            Unit and integration tests
docs/en/                       English documentation
docs/cs/                       Czech documentation
```
