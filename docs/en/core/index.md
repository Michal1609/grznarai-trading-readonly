# Core

`GrznarAi.Trading.ReadOnly` is the shared infrastructure package for platform clients.

Install platform packages for normal use:

```powershell
dotnet add package GrznarAi.Trading.ReadOnly.Etoro
dotnet add package GrznarAi.Trading.ReadOnly.Coinbase
```

Install Core directly only when you need shared types:

```powershell
dotnet add package GrznarAi.Trading.ReadOnly
```

## What Core Contains

- HTTP handlers: rate limiting, transient HTTP retry, diagnostic capture.
- Configuration types: `RateLimitOptions`, `ResilienceOptions`, `ErrorHandlingOptions`, `DiagnosticOptions`.
- Diagnostics: `IApiDiagnostics`, `ApiDiagnostics`, `ApiResponseSnapshot`.
- Query helper: `QueryStringBuilder`.
- JSON helpers: decimal string converters.
- Exceptions: `TradingApiException`.

Core does not contain platform clients, authentication handlers, or platform models.

## Guides

- [Diagnostics](diagnostics.md)
- [Configuration](configuration.md)
- [Rate limiting](rate-limiting.md)
- [Error handling](error-handling.md)

