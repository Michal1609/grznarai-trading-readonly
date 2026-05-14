# Osetreni chyb

`TradingApiException` je Core base exception pro platformni API chyby. Platformni balicky z ni odvozuji vlastni typy vyjimek.

Vyjimka obsahuje:

- `Platform`
- `Endpoint`
- `StatusCode`
- `ResponseBody`
- `RequestId`
- `RetryAfter`
- `Snapshot`

`Snapshot` pouziva stejnou diagnostickou strukturu jako `IApiDiagnostics`. Platformni klienti ho maji naplnit pri HTTP chybach i tehdy, kdyz je opt-in diagnostika vypnuta.

`ErrorHandlingOptions` ridi, kolik response body se vlozi do vyjimky. Pri vyvoji nechte body capture zapnuty; v produkci budte opatrni pri logovani response body.

