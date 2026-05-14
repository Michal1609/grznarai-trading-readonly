# Core

`GrznarAi.Trading.ReadOnly` je sdileny infrastrukturni balicek pro platformni klienty.

Pro bezne pouziti instalujte platformni balicky:

```powershell
dotnet add package GrznarAi.Trading.ReadOnly.Etoro
dotnet add package GrznarAi.Trading.ReadOnly.Coinbase
```

Core instalujte primo jen tehdy, kdyz potrebujete sdilene typy bez platformniho klienta:

```powershell
dotnet add package GrznarAi.Trading.ReadOnly
```

## Co Core obsahuje

- HTTP handlery: rate limiting, retry pro transient HTTP chyby, diagnosticky capture.
- Konfigurace: `RateLimitOptions`, `ResilienceOptions`, `ErrorHandlingOptions`, `DiagnosticOptions`.
- Diagnostika: `IApiDiagnostics`, `ApiDiagnostics`, `ApiResponseSnapshot`.
- Query helper: `QueryStringBuilder`.
- JSON helpery: decimal string konvertory.
- Vyjimky: `TradingApiException`.

Core neobsahuje platformni klienty, autentizacni handlery ani platformni modely.

## Navody

- [Diagnostika](diagnostics.md)
- [Konfigurace](configuration.md)
- [Rate limiting](rate-limiting.md)
- [Osetreni chyb](error-handling.md)

