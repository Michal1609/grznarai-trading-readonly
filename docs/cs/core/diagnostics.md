# Diagnostika

Diagnostika je opt-in. Kdyz je zapnuta, platformni balicek vlozi `DiagnosticCapturingHandler` do HTTP pipeline a uklada snapshoty do `IApiDiagnostics`.

## Zapnuti

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

## Pouziti

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

## Zachycena data

`ApiResponseSnapshot` obsahuje timestamp, delku requestu, metodu a URL, volitelne request headers, response status, reason phrase, response headers, response body, priznak truncation a content type.

Citlive headers uvedene v `DiagnosticOptions.RedactedHeaders` se nahradi hodnotou `[REDACTED]`. Core neredaktuje response body; nelogujte ho automaticky.

## Thread safety

`ApiResponseSnapshot` je immutable. `ApiDiagnostics` je thread-safe a vraci kopii historie. `History` vraci nejnovejsi snapshoty jako prvni.
