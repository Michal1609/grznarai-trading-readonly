# Diagnostics

Diagnostics are opt-in. When enabled, a platform package inserts `DiagnosticCapturingHandler` into the HTTP pipeline and stores snapshots in `IApiDiagnostics`.

## Enable

eToro:

```json
{
  "EToroOptions": {
    "Diagnostics": {
      "Enabled": true,
      "CaptureResponseBody": true,
      "HistorySize": 8
    }
  }
}
```

Coinbase:

```json
{
  "Coinbase": {
    "Diagnostics": {
      "Enabled": true,
      "CaptureResponseHeaders": true
    }
  }
}
```

## Use

```csharp
using GrznarAi.Trading.ReadOnly.Diagnostics;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;

public sealed class PortfolioPage(IEToroClient client, IApiDiagnostics diagnostics)
{
    public async Task LoadAsync(CancellationToken ct)
    {
        await client.GetPortfolioAsync(EToroEnvironment.Real, ct);

        var last = diagnostics.Last;
        if (last?.ResponseHeaders?.TryGetValue("Sunset", out var sunset) == true)
        {
            Console.WriteLine($"API endpoint sunsetting: {sunset[0]}");
        }
    }
}
```

## Captured Data

`ApiResponseSnapshot` includes timestamp, elapsed time, request method and URI, optional request headers, response status, reason phrase, response headers, response body, body truncation flag, and content type.

Sensitive headers listed in `DiagnosticOptions.RedactedHeaders` are replaced with `[REDACTED]`. Response bodies are not redacted by Core; do not log them blindly.

## Thread Safety

`ApiResponseSnapshot` is immutable. `ApiDiagnostics` is thread-safe and returns a copied history list. `History` returns newest snapshots first.
