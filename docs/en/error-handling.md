# Error Handling

The client throws `EToroApiException` for non-success HTTP responses.

## Exception Data

`EToroApiException` exposes:

| Property | Description |
| --- | --- |
| `StatusCode` | HTTP status code returned by the eToro API. |
| `Endpoint` | Sanitized request endpoint path and query. |
| `ResponseBody` | Redacted and truncated response body, or `null` when no body is available. |
| `RequestId` | Request ID from response headers or the generated request header when available. |
| `RetryAfter` | Parsed `Retry-After` value when the API sends it. |

Example:

```csharp
try
{
    var portfolio = await client.GetPortfolioAsync(EToroEnvironment.Real, ct);
}
catch (EToroApiException ex)
{
    logger.LogWarning(
        ex,
        "eToro API failed with {StatusCode} at {Endpoint}. RequestId: {RequestId}. RetryAfter: {RetryAfter}",
        ex.StatusCode,
        ex.Endpoint,
        ex.RequestId,
        ex.RetryAfter);
}
```

## Response Body Redaction

`ResponseBody` is intentionally not the raw response body.

Before the exception is thrown, the library:

- Trims the response body.
- Redacts common secret fields such as `apiKey`, `userKey`, `token`, `access_token`, `refresh_token`, `authorization`, `secret`, `password`, `x-api-key`, and `x-user-key`.
- Truncates the final value to 4096 characters.

This keeps error diagnostics useful while reducing the chance of credentials or very large payloads being written to logs.

## Configuration

Response-body handling is configurable through `EToroOptions.ErrorHandling`.

```json
{
  "EToroOptions": {
    "ErrorHandling": {
      "IncludeResponseBody": true,
      "RedactResponseBody": true,
      "MaxResponseBodyLength": 4096
    }
  }
}
```

| Option | Default | Description |
| --- | --- | --- |
| `IncludeResponseBody` | `true` | Stores the response body in `EToroApiException.ResponseBody`. Set to `false` to avoid capturing response bodies. |
| `RedactResponseBody` | `true` | Redacts common secret fields before storing the body. |
| `MaxResponseBodyLength` | `4096` | Maximum stored body length. Set to `null` for no truncation or `0` to keep an empty string. |

For local debugging you can temporarily keep the full raw body:

```json
{
  "EToroOptions": {
    "ErrorHandling": {
      "IncludeResponseBody": true,
      "RedactResponseBody": false,
      "MaxResponseBodyLength": null
    }
  }
}
```

Use raw bodies only in controlled local debugging. Do not log raw eToro error bodies in production unless you have reviewed the data for credentials and personal information.

## Empty or Invalid Success Responses

If the API returns a success status code but the body is empty or cannot be deserialized into the expected response type, the client throws `InvalidOperationException`.

Input validation errors, such as invalid IDs, page sizes, or search fields, throw standard .NET exceptions such as `ArgumentException` or `ArgumentOutOfRangeException` before the request is sent.
