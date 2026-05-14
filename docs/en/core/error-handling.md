# Error Handling

`TradingApiException` is the Core base exception for platform API failures. Platform packages derive their own exception types from it.

The exception includes:

- `Platform`
- `Endpoint`
- `StatusCode`
- `ResponseBody`
- `RequestId`
- `RetryAfter`
- `Snapshot`

`Snapshot` contains the same diagnostic structure used by `IApiDiagnostics`. Platform clients should populate it on HTTP errors even when opt-in diagnostics are disabled.

`ErrorHandlingOptions` controls how much response body is included in exceptions. Keep body capture enabled in development; be careful when logging response bodies in production.

