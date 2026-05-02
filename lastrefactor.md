# EToro .NET Client — Production Readiness Audit

**Audit date:** 2026-05-01
**Scope:** Complete codebase review for security, best practices, memory leaks, disposability, OSS hygiene, NuGet readiness.
**Repo:** `grznarai.etoro` (public, MIT, .NET 9, finance domain).
**Mode:** Findings only — no code changes performed.

Severity legend: **C**=Critical · **H**=High · **M**=Medium · **L**=Low · **I**=Informational.

---

## 1. Security — COMPLETED

### 1.1 [H] `EToroKeyedRateLimiter` — unbounded `ConcurrentDictionary` (memory leak)
File: `src/EToro/Client/EToroKeyedRateLimiter.cs:25`
The dictionary `_windows` keyed by `x-user-key` never evicts entries. In a multi-tenant host that rotates API keys, accepts new tenants, or is fed garbage user-key headers (test traffic, leaked keys, fuzz), the dictionary grows monotonically until process restart. Singleton lifetime → leak survives all scopes.
**Recommendation:** add idle-eviction (e.g., `MemoryCache` with sliding expiration matching `Window × N`, or background sweep removing windows whose queue is empty and last-touch > threshold). Cap dictionary size with eviction policy.

### 1.2 [H] `EToroClient.EnsureSuccessAsync` — full response body buffered before truncation
File: `src/EToro/Client/EToroClient.cs:75`
`response.Content.ReadAsStringAsync(ct)` materializes entire body in memory, then `FormatResponseBody` truncates to `MaxResponseBodyLength` (default 4 KB). A hostile or misbehaving upstream can send a multi-GB body and exhaust memory **before** truncation runs. HttpClient default `MaxResponseContentBufferSize` is 2 GB.
**Recommendation:** read `response.Content` via stream, copy into a bounded buffer (`MaxResponseBodyLength + small safety`), or set `HttpClient.MaxResponseContentBufferSize` on the typed client. Same pattern in `GetFromJsonAsync` reads to string then deserializes — switch to `JsonSerializer.DeserializeAsync(stream, ...)` for both perf and hardening.

### 1.3 [H] `GetSanitizedEndpoint` returns `PathAndQuery` — username PII may leak into exception/logs
File: `src/EToro/Client/EToroClient.cs:91`
Endpoints like `user-info/people/{username}/portfolio/live` and search endpoints with `usernames=alice,bob,...` end up as `Endpoint` on `EToroApiException`. Consumers will log the exception → PII (eToro usernames) ends up in observability tooling.
**Recommendation:** strip query string and replace path parameters with `{username}` placeholders, or expose only `RequestUri.AbsolutePath` and a separate `EndpointTemplate` property. At minimum document that `Endpoint` may contain PII.

### 1.4 [M] Response-body redaction regex covers JSON/key=value but not structured tokens
File: `src/EToro/Client/EToroClient.cs:139-147`
Regex catches `apiKey`, `userKey`, `token`, `password`, `x-api-key`, `x-user-key`. Misses: `accessToken`, `refreshToken` (camelCase variants are present, good), `Authorization: Bearer …` HTTP-style, JWT-shaped strings, BasicAuth `dXNlcjpwYXNz`, eToro's own session cookies, account numbers. For a finance OSS lib, secret detection should be conservative.
**Recommendation:** add patterns for `Bearer\s+[A-Za-z0-9._-]+`, JWT regex (`eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+`), `Authorization` headers literal, and configurable additional patterns. Add tests verifying redaction.

### 1.5 [M] `EToroAuthHandler.SetHeader` rejects only `\r\n`, not all control chars
File: `src/EToro/Client/EToroAuthHandler.cs:20`
Header injection mitigated for `\r\n`. But other low-ASCII (e.g., `\0`, `\x7F`) or high-bit chars are not rejected — `TryAddWithoutValidation` lets them through. Risk is low because credentials are operator-controlled config, but defense-in-depth says reject any non-printable ASCII for header values.
**Recommendation:** `value.AsSpan().IndexOfAnyInRange((char)0,(char)31) < 0 && value.AsSpan().IndexOf((char)127) < 0`.

### 1.6 [M] `Random.Shared.NextDouble()` for retry jitter
File: `src/EToro/Client/RateLimitHandler.cs:102`
Functionally fine — jitter need not be cryptographic. Flag only because finance/security audits often question every `Random` call. **No change required**, but document intent.

### 1.7 [M] `SECURITY.md` — no contact channel for private disclosure
File: `SECURITY.md`
"Report security issues privately to the maintainer" — no email, no PGP, no GitHub Security Advisories link. OSS responsible-disclosure best practice (CVD) requires a concrete channel.
**Recommendation:** add private email (`michal.grznar@seznam.cz` or dedicated alias), or enable GitHub private vulnerability reporting and link to it. Add expected response SLA (e.g., "ack within 5 working days").

### 1.8 [M] No supply-chain hardening
- `dependabot.yml` missing → no automated dependency updates / GHSA alerts surfaced as PRs.
- CI actions pinned to major (`@v4`) not SHA — supply-chain best practice for finance libraries is SHA-pinning.
- No `.github/CODEOWNERS` → any maintainer can merge anything.
- Gitleaks runs only on push/PR — consider weekly scheduled run too.

### 1.9 [L] `appsettings.example.json` and `appsettings.test.example.json` — same dummy values, OK
Verified `.gitignore` excludes real `appsettings*.json`, only `*.example.json` allowed. Good.

### 1.10 [L] `JsonOptions` reused from generated context — does not enforce strict size/depth limits
`PropertyNameCaseInsensitive = true` is set, but `MaxDepth` defaults to 64. For a finance API a hostile JSON nest can be DoS vector.
**Recommendation:** in `[JsonSourceGenerationOptions(...)]` add `MaxDepth = 32` (or set on a wrapper options instance).

### 1.11 [L] `IsValidBaseAddress` blocks user-info, query, fragment, non-`/`-terminated, non-https — good
However `AllowCustomBaseAddress=true` then accepts **any** HTTPS host. For a defensive default, document in the option that this disables a security control (host pinning). Already noted in README; ensure logged warning when `AllowCustomBaseAddress` is true.

### 1.12 [L] `Environment` option is loose string
File: `src/EToro/Configuration/EToroOptions.cs:10`
`Environment` accepts any string; only `"demo"` matches; everything else (incl. typos like `"Demoo"`, `"prod"`) silently maps to **real**. For a finance library, a typo can cause real-money trades when caller intended demo.
**Recommendation:** validate `Environment ∈ {"real","demo"}` (case-insensitive) in `AddEToroOptions`. Or replace string with enum.

### 1.13 [I] `EToroOptions.ToString()` redacts ApiKey/UserKey — good. BaseAddress not sensitive.

### 1.14 [I] No HTTPS certificate pinning
For finance public APIs against `public-api.etoro.com`, certificate pinning would defeat MITM at the cost of operational risk on cert rotation. **Out of scope** for a general-purpose client lib but worth a documented note for high-assurance consumers.

---

## 2. Memory, Disposability, Resource Leaks

### 2.1 [H] `RateLimitHandler.SendAsync` — `HttpResponseMessage` not disposed on max-retries-exhausted
File: `src/EToro/Client/RateLimitHandler.cs:49-50`
When `attempt >= MaxRetries` or `!CanRetry(request)`, the loop returns the 429 response **to the caller**. That is correct (caller owns it). However, in `EToroClient.GetFromJsonAsync` the response is wrapped in `using` so it gets disposed — also correct. **Cross-check:** any consumer using the typed client directly via `IHttpClientFactory.CreateClient(...)` and reading via extension methods will dispose. **No actual leak**, but document the contract: caller must dispose.

### 2.2 [H] `EToroClient.EnsureSuccessAsync` may not dispose response on exception path
File: `src/EToro/Client/EToroClient.cs:38-46`
`GetFromJsonAsync` declares `using var response`, so disposal is guaranteed even when `EnsureSuccessAsync` throws. **OK.** But `RateLimitHandler.CloneRequestAsync` does **not** dispose the original `request.Content` after copying it — `HttpRequestMessage` disposal is handled by the outer pipeline (HttpClient disposes on send). Still, consider explicit disposal of the **prior** `request` after clone to release content stream early.

### 2.3 [M] Mocked test `MockHttpMessageHandler` returns single shared `HttpResponseMessage` (not test problem here, but pattern used in handler tests)
File: `tests/EToro.Tests/Client/RateLimitHandlerTests.cs:84` (and similar)
`SequentialHandler` re-returns the same `HttpResponseMessage` instance once disposed in retry test. Test passes today because dispose tracking only flips a bool, but rigorous test design re-creates per call. **Test cleanliness only** — does not affect production.

### 2.4 [M] `EToroSlidingWindow` queue grows up to `permitLimit` — fine, but `EToroKeyedRateLimiter` × N user keys × `permitLimit` = unbounded memory (see 1.1).

### 2.5 [L] `_httpClient` in `EToroClient` not disposed — correct, owned by `IHttpClientFactory`.

### 2.6 [L] `RateLimitHandler.CloneRequestAsync` reads body fully into memory via `ReadAsByteArrayAsync`
File: `src/EToro/Client/RateLimitHandler.cs:131`
For large POST bodies on retry, doubles memory transiently. The library currently issues only GET; flag for future write endpoints. Consider `MemoryStream` pooling or refusing retry for streaming content.

### 2.7 [L] `EnsureSuccessAsync` reads body even when `IncludeResponseBody=false`
File: `src/EToro/Client/EToroClient.cs:73-75` — actually checks the flag first → body **not** read when disabled. OK.

---

## 3. Concurrency / Thread Safety

### 3.1 [M] `EToroSlidingWindow.WaitAsync` — busy-loop risk under contention
File: `src/EToro/Client/EToroSlidingWindow.cs:8-18`
After `Task.Delay(delay)`, all blocked callers race to re-enter `ReserveOrGetDelay` simultaneously. Fairness is not guaranteed — late arrivals may steal a slot from a long-waiter (lock ordering is OS-dependent). For 60 RPM this is benign; under sustained burst, a producer can starve.
**Recommendation:** consider switching to `System.Threading.RateLimiting` `SlidingWindowRateLimiter` (built-in, fair, async-ready) introduced in .NET 7+. It also handles cancellation and queue depth properly.

### 3.2 [M] `EToroSlidingWindow` uses `lock` synchronously — no `IAsyncDisposable`
A standard `lock` is fine for the synchronous critical section. Verified the lock is **not** held across `await`. OK.

### 3.3 [I] `EToroRateLimiter` snapshots options at construction — does not respond to `IOptionsMonitor` changes
File: `src/EToro/Client/EToroRateLimiter.cs:20-24`
If a consumer rebinds `EToroOptions:RateLimit` at runtime (e.g., feature flag), the limiter ignores the change. Document the limitation or accept `IOptionsMonitor<EToroOptions>` and rebuild the window on `OnChange`.

---

## 4. Correctness / Domain Logic (Finance)

### 4.1 [H] `EToroCalculationService.CalculateRealizedPnLAsync` — dedup logic fragile
File: `src/EToro/Services/EToroCalculationService.cs:119-148`
`pageSignature = string.Join(",", positionIds)` will mis-detect duplicates if the API returns the same trades but in a different order. The dedup heuristic also breaks on a legitimate empty page that the API might return between data pages. The current safeguards (page-size short-circuit, last-id repeat) are best-effort; for finance numbers this matters.
**Recommendations:**
- Track set of seen `PositionId`s; skip duplicates instead of breaking the loop.
- Sum `NetProfit` only for unseen positions.
- Make `MaxPages` × `PageSize` cap explicit and **fail loudly** (throw) instead of silently truncating realized PnL. Silent truncation = wrong realized PnL = wrong reported equity.

### 4.2 [H] `AccountMetrics.NetDeposits = equity - totalReturn` is a derived approximation, not a real deposit ledger
File: `src/EToro/Services/EToroCalculationService.cs:105`
This formula is correct **only** if the API's `pnl` and the realized-PnL aggregation include every cash flow. Withdrawals, bonuses, rebates, currency conversion fees, dividends, corporate actions are not accounted for. For a published finance library, this is a substantial caveat.
**Recommendation:** add prominent XML doc + docs warning that `NetDeposits`/`TotalReturn`/`TotalReturnPct` are **estimates derived from PnL endpoints**, not authoritative deposit history. Consider renaming `NetDeposits` to `EstimatedNetDeposits` to avoid suggesting authority.

### 4.3 [M] `decimal == 0m` comparison vs floating-point math
File: `src/EToro/Services/EToroCalculationService.cs:106`
`netDeposits == 0m` — exact zero check on decimal is valid (decimal is exact). OK.

### 4.4 [M] `PnlResponse.UnrealizedPnL` (API field) ignored; library recomputes from positions
File: `src/EToro/Services/EToroCalculationService.cs:59-69`
The API returns `unrealizedPnL` directly; library recomputes from `Positions.Sum(p.PnL) + MirrorPositions.Sum(p.PnL)`. If API value diverges (rounding, different grouping), library result differs from what eToro shows in UI.
**Recommendation:** at minimum document the discrepancy, or add an option to use API-reported value.

### 4.5 [M] `OrderForOpen.Amount` interpreted as both invested principal and "manual pending"
File: `src/EToro/Services/EToroCalculationService.cs:18-25`
`CalculateAvailableCash` subtracts `manualPending` from credit; `CalculateInvestedPrincipal` adds `manualPending + totalExternalCosts`. If the API definition of "amount" changes, both calculations drift. This is fragile and untested at unit level.
**Recommendation:** add unit tests with realistic synthetic `PnlResponse` payloads asserting expected outputs for representative scenarios.

### 4.6 [L] `GetTradeHistoryAsync(DateTimeOffset)` overload converts via `UtcDateTime → DateOnly`
File: `src/EToro/Client/EToroClient.Trading.cs:49-53`
For users in non-UTC timezones, the date boundary may be off by one. Document or accept `DateOnly` only.

### 4.7 [L] `decimal Default = 0` on optional record fields auto-initializes to 0 when JSON omits — potentially masks API schema changes
Not an issue per se, but if eToro adds a new required field renamed differently, `Position.Amount = 0` silently corrupts calculations. Consider integration smoke tests that assert non-zero where expected.

---

## 5. NuGet Packaging / Build / Determinism

### 5.1 [H] No `Microsoft.SourceLink.GitHub` package reference
File: `src/EToro/EToro.csproj`
`Directory.Build.props` enables `EmbedUntrackedSources`, `PublishRepositoryUrl`, `Deterministic` — correct, but **without** `<PackageReference Include="Microsoft.SourceLink.GitHub" Version="..." PrivateAssets="All" />` the SourceLink metadata is not actually generated. Symbols (snupkg) will work but will not link to GitHub line numbers in IDE debuggers.
**Recommendation:** add the SourceLink package. Validate locally by inspecting `.pdb` with `sourcelink test`.

### 5.2 [M] `IncludeSymbols=true` + `SymbolPackageFormat=snupkg` set, but no NuGet pack validation in CI
CI builds and tests but never runs `dotnet pack EToro.csproj -c Release` to assert a valid `.nupkg` is produced and validate metadata. A bad packaging change ships at release time only.
**Recommendation:** add a `pack` step in `ci.yml` (publish artifact, do not push). Run `dotnet validate` (or `Meziantou.Analyzer` / `nupkg-explorer`) for metadata sanity.

### 5.3 [M] README packed for NuGet contains relative links and SVG image
README.md L3 references `assets/branding/GrznarAi.Etoro.svg` and many `docs/...` links. NuGet renderer:
- does not render SVG images (must be PNG/JPG/GIF, raw HTTPS URLs)
- relative repository links open as 404 on NuGet.org
**Recommendation:** create a NuGet-specific README (`README.nuget.md`) or replace relative paths with absolute `https://github.com/Michal1609/eToro/blob/main/...`. Replace SVG banner in NuGet README with PNG or omit on NuGet.

### 5.4 [M] `Directory.Build.props` sets `VersionPrefix=0.1.0`/`VersionSuffix=alpha.1` for **all** projects, including tests
The test project is `IsPackable=false` so no `.nupkg` produced — fine. But the assembly version/file version of test DLLs becomes `0.1.0-alpha.1`, which is harmless but unusual.
**Recommendation:** scope the version properties to packable projects only via condition `Condition="'$(IsPackable)' != 'false'"` or move them to `EToro.csproj`.

### 5.5 [L] `EToro.csproj` declares `PackageId=grznarai.etoro` but `AssemblyName=EToro`, `RootNamespace=EToro`
Mismatch between package id and assembly is allowed but confusing for consumers (`dotnet add package grznarai.etoro` then `using EToro;`). Document in README. Alternatively rename namespace to `GrznarAi.EToro` or keep current and add a clear "Package vs namespace" note.

### 5.6 [L] No `<PackageDescription>` (uses `<Description>`) and `<PackageReleaseNotes>` missing
**Recommendation:** add `<PackageReleaseNotes>` per release (file-driven, e.g., `<PackageReleaseNotes>$([System.IO.File]::ReadAllText('$(MSBuildThisFileDirectory)CHANGELOG.md'))</PackageReleaseNotes>`) and a proper `CHANGELOG.md`.

### 5.7 [L] `LangVersion=latest` — can break determinism across SDK versions
For a published library, prefer `LangVersion=13.0` (or specific). Ensures package built today and tomorrow on a newer SDK accepts the same syntax.

### 5.8 [L] No `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
For a production finance library, recommend treat-as-errors at least in CI builds (`-warnaserror` flag in `ci.yml`).

### 5.9 [L] No `<EnablePackageValidation>true</EnablePackageValidation>` + `<PackageValidationBaselineVersion>`
Once first stable version is published, this catches accidental breaking API changes.

### 5.10 [L] Strong-naming decision unmade
For a finance library, strong-naming is no longer a hard requirement, but enterprise consumers may need it. **Decide intentionally** and document the choice. If you sign, do it from v0.1 onward; flipping later is a breaking change.

### 5.11 [I] `Deterministic=true` and `ContinuousIntegrationBuild=true` (under `CI=true`) — good.

### 5.12 [I] `DebugType=portable` — good (best for cross-platform symbols).

### 5.13 [I] `PackageIcon=icon.png` present and correctly packed — good.

---

## 6. .NET / API Design Best Practices

### 6.1 [M] No `ConfigureAwait(false)` anywhere in library code
File: all `await` sites in `src/EToro/`
For a library targeting `net9.0` (which has no `SynchronizationContext` in console/server scenarios), this is mostly cosmetic. But for consumers calling the lib from WPF, WinForms, MAUI, or any custom sync context, every `await` may post back to the captured context unnecessarily. Library best practice is `ConfigureAwait(false)`.
**Recommendation:** enforce CA2007 as warning in `src/EToro/.editorconfig` (currently suppressed at solution level), and add `ConfigureAwait(false)` to all awaits in `src/EToro/`. (Tests can stay as-is.)

### 6.2 [M] Reflection-based options binding breaks NativeAOT/trimming
`services.AddOptions<EToroOptions>().Bind(configuration.GetSection(...))` uses reflection. Library otherwise uses `JsonSerializerContext` (AOT-safe). Mixing the two prevents `IsAotCompatible=true`.
**Recommendation:**
- Add `<EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>` (.NET 8+) to the consumer-side csproj — or document this requirement.
- Annotate `AddEToro` overloads with `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` if not AOT-safe, or migrate to source-generated binder.
- Once compliant, set `<IsTrimmable>true</IsTrimmable>` and `<IsAotCompatible>true</IsAotCompatible>` on the library csproj.

### 6.3 [M] No `ILogger<T>` injected anywhere
For a production finance library, structured logging on:
- Outgoing requests (URL, method, request-id) — at Debug
- Retry decisions / wait times — at Information
- Rate-limit window saturation — at Warning
- Non-success responses (status, request-id, duration) — at Warning/Error
…is essential for diagnosing prod issues.
**Recommendation:** inject `ILogger<EToroClient>`, `ILogger<RateLimitHandler>`, `ILogger<EToroAuthHandler>`. Use `LoggerMessage` source-generator (AOT-safe). Never log secrets/headers; use the existing redactor for any body output.

### 6.4 [M] No `ActivitySource` / OpenTelemetry instrumentation
Even with logging-only goal, modern HTTP libraries expose an `ActivitySource`. `IHttpClientFactory` already produces `System.Net.Http` activities — verify those propagate from this lib's typed client and document. (No code change needed if default propagation works.)

### 6.5 [M] `EToroApiException` extends `HttpRequestException` and shadows `StatusCode`
File: `src/EToro/Client/EToroApiException.cs:22`
`new HttpStatusCode StatusCode { get; }` shadows the base property. Both refer to the same value because the base ctor is also passed `statusCode`. Acceptable, but confusing; remove the `new` and rely on base property. Keeping `Endpoint`, `ResponseBody`, `RequestId`, `RetryAfter` as additional properties is fine.

### 6.6 [M] Default `EToroClient(HttpClient)` ctor creates empty options
File: `src/EToro/Client/EToroClient.cs:15-18`
Calling this directly from app code (without DI) will produce a client with empty `ApiKey`/`UserKey` because `EToroAuthHandler` never runs (it's a separate `DelegatingHandler`). Confusing for users not reading docs.
**Recommendation:** mark this ctor `internal` (only used for DI scenarios where `HttpClientFactory` injects). Or remove and rely on the `[ActivatorUtilitiesConstructor]`-marked one.

### 6.7 [M] `IEToroClient` is god-interface; sub-interfaces exist but discovery is poor
File: `src/EToro/Client/IEToroClient.cs`
Sub-interfaces (`IEToroTradingClient`, `IEToroMarketDataClient`, …) are defined and inherited, but DI only registers `IEToroClient`. Consumers wanting `IEToroMarketDataClient` only must resolve `IEToroClient` and upcast (works through inheritance). `useKeyedRateLimiter` doc mentions blast-radius reduction.
**Recommendation:** in `AddEToroCore`, also register the sub-interfaces:
```csharp
services.AddTransient<IEToroTradingClient>(sp => sp.GetRequiredService<IEToroClient>());
// repeat for each sub-interface
```

### 6.8 [M] `QueryStringBuilder.Add(string, Enum)` uses `Enum.ToString()`
Enum names must match API tokens exactly. A future rename (refactor) silently breaks API contract.
**Recommendation:** define explicit per-enum `ToApiString()` extension or attach `[Description]`/custom attribute mapping. At least add unit tests asserting each enum serializes to expected token.

### 6.9 [M] No retry for transient HTTP failures (5xx, network) — only 429
For a finance API, intermittent 502/503/504/`HttpRequestException` is common.
**Recommendation:** consider integrating `Microsoft.Extensions.Http.Resilience` (Polly v8) with conservative config: jittered exponential backoff, 5xx retry on idempotent methods only, circuit breaker. Documented opt-in/opt-out via `EToroOptions.Resilience`.

### 6.10 [L] `EToroRequestLimits` constants are public — exposed to consumers but never referenced in interface XML docs
Document: are these meant to be consumer-tuneable? If not, mark `internal`.

### 6.11 [L] Many DTO records use `IReadOnlyList<T>` — JSON deserializer materializes as `List<T>`
This is fine, but `IReadOnlyList<T>` discourages mutation while `List<T>` allows downcasting. Acceptable convention, mention in contributing docs.

### 6.12 [L] `EToroJsonContext` does not include `JsonStringEnumConverter` globally
Only `WatchlistType` declares `[JsonConverter(typeof(JsonStringEnumConverter<WatchlistType>))]`. If any DTO field is later added as enum without converter, deserialization expects integer.
**Recommendation:** add `Converters = [typeof(JsonStringEnumConverter)]` to `[JsonSourceGenerationOptions(...)]` and remove per-enum attributes. **Verify** AOT compatibility with `JsonStringEnumConverter<T>` (closed generic) instead of open generic.

### 6.13 [L] `DateOnly` is sent as `yyyy-MM-dd` invariant — good. Verify eToro server-side timezone interpretation.

### 6.14 [L] `EToroInputValidator.ValidateRequiredString` — name implies caller already null-checked. Add explicit comment or rename to `ValidateNonNullString`.

---

## 7. CI / GitHub / OSS Hygiene

### 7.1 [M] `ci.yml` — no `dotnet pack` step (covered above 5.2).
### 7.2 [M] `ci.yml` — no NuGet cache between runs (slows feedback).
### 7.3 [M] `ci.yml` — does not upload trx test results as workflow artifact for failure inspection.
### 7.4 [M] No `dependabot.yml` for NuGet + GitHub Actions updates.
### 7.5 [M] No `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`. README has a brief "Contributing" paragraph; for a finance OSS library, full docs reduce review burden.
### 7.6 [L] No issue / PR templates (`.github/ISSUE_TEMPLATE/*`, `pull_request_template.md`).
### 7.7 [L] No `CODEOWNERS` file.
### 7.8 [L] No release workflow draft (you said you'll handle release/publish — keep parked, but consider GitHub Releases auto-generation).
### 7.9 [L] CI matrix only on `ubuntu-latest`. For NuGet on Windows-hosted projects, also testing `windows-latest` catches path/encoding/CRLF issues at low cost.
### 7.10 [I] `secret-scan.yml` runs on every push/PR — good.

---

## 8. Documentation

### 8.1 [M] XML doc comments are partial
`IEToroTradingClient` has bilingual XML docs — good. Many other interfaces (`IEToroFeedClient`, `IEToroSocialClient`, `IEToroWatchlistClient`, `IEToroUserInfoClient`) — mixed coverage; verify all public methods have at least a one-line summary so IntelliSense is useful. Configuration option properties (`EToroOptions`, `RateLimitOptions`, `ErrorHandlingOptions`) have no XML doc — IntelliSense shows nothing. (Skipped per your instruction — leaving as-is.)

### 8.2 [L] `docs/en/` and `docs/cs/` — not linked from NuGet README (relative path issue, see 5.3).

### 8.3 [L] `docs/cs/` — currently spot-checked: structure mirrors EN. No audit of content drift performed; recommend periodic diff between EN and CS to keep parity.

### 8.4 [L] No top-level CHANGELOG.md.

---

## 9. Tests

### 9.1 [M] Unit-test coverage of `EToroCalculationService` — only integration tests exist
`EToroIntegrationTests` runs the calculator with real API data, marked `[Explicit]` (good — won't run unattended). But there are **no unit tests** validating the pure-decimal math against synthetic `PnlResponse` payloads. For finance code this is a critical gap.
**Recommendation:** add `EToroCalculationServiceTests` with crafted `PnlResponse`/`ClosedTrade` fixtures asserting exact decimal results.

### 9.2 [M] No test for `EnsureSuccessAsync` redaction
The redaction regex is the security-critical surface for not leaking secrets via exception messages. Currently no unit test validates that JSON like `{"apiKey":"AKIA…"}` becomes `{"apiKey":"[REDACTED]"}`.

### 9.3 [M] No test for body-size truncation
`MaxResponseBodyLength` cap not asserted by a test feeding a huge body.

### 9.4 [M] No test for `EToroAuthHandler` header injection rejection
Add tests asserting `\r`, `\n`, control chars throw.

### 9.5 [L] `MockHttpMessageHandler` returns the same `HttpResponseMessage` instance multiple times if reused — known multi-call testing pitfall. Cosmetic.

### 9.6 [L] All integration tests `[Explicit]` + `[Category("Integration")]` — verified. Good.

### 9.7 [L] Integration tests print to `Debug.WriteLine` — visible only with verbose logger. Acceptable.

### 9.8 [L] `[Test] // nesedi, 21.59 vs 26...` in `EToroIntegrationTests.cs:235` — TODO/known discrepancy comment in production-targeted code. Investigate before stable release; this is exactly the calculation-correctness flag from §4.4.

### 9.9 [I] Test `ProactiveGetLimiter_WaitsWhenPermitWindowIsExhausted` checks ≥50 ms — flaky on heavily loaded CI. Mark as `[Category("Timing")]` or use `TimeProvider` mock.

---

## 10. Miscellaneous / Polish

### 10.1 [L] `EToroClient.cs` empty trailing line vs CRLF: `.editorconfig` enforces `crlf` + `insert_final_newline`. Spot-checked; ok.

### 10.2 [L] `Properties/AssemblyInfo.cs` — only `InternalsVisibleTo("EToro.Tests")`. Once strong-named, must include public key. Decide before signing.

### 10.3 [L] `LICENSE` is MIT — header in source files absent. Many OSS libs add a one-line SPDX header to each `.cs`. Optional.

### 10.4 [L] Folder `artifacts/packages` exists locally and is gitignored. OK.

### 10.5 [L] `assets/branding/GrznarAi.Etoro.svg` — not referenced in NuGet pack; only README. OK.

### 10.6 [I] `EToroJsonContext.Default.Options` is shared — thread-safe (System.Text.Json). OK.

---

## Summary table

| # | File | Severity | One-liner |
|---|------|----------|-----------|
| 1.1 | EToroKeyedRateLimiter.cs | H | Unbounded dictionary — memory leak under multi-tenant. |
| 1.2 | EToroClient.cs (EnsureSuccessAsync) | H | Buffers full body before truncation — OOM vector. |
| 1.3 | EToroClient.cs (GetSanitizedEndpoint) | H | PII (usernames) leaks via Exception.Endpoint. |
| 1.4 | EToroClient.cs (regex) | M | Redaction misses Bearer/JWT tokens. |
| 1.5 | EToroAuthHandler.cs | M | Only `\r\n` rejected, not all control chars. |
| 1.7 | SECURITY.md | M | No private disclosure channel. |
| 1.8 | .github | M | No dependabot, no SHA-pinned actions. |
| 1.10 | EToroJsonContext.cs | L | No MaxDepth limit. |
| 1.12 | EToroOptions.cs | L | Environment string accepts typos → real. |
| 2.1 | RateLimitHandler.cs | H | Document caller-disposes contract. |
| 2.6 | RateLimitHandler.cs | L | CloneRequestAsync materializes full body. |
| 3.1 | EToroSlidingWindow.cs | M | Consider System.Threading.RateLimiting. |
| 3.3 | EToroRateLimiter.cs | I | Doesn't follow IOptionsMonitor changes. |
| 4.1 | EToroCalculationService.cs | H | Realized-PnL paging dedup is fragile + silent truncation. |
| 4.2 | EToroCalculationService.cs | H | NetDeposits is approximation, not real ledger. |
| 4.4 | EToroCalculationService.cs | M | Recomputes UnrealizedPnL — diverges from API. |
| 4.5 | EToroCalculationService.cs | M | Calculation logic untested at unit level. |
| 5.1 | EToro.csproj | H | Missing Microsoft.SourceLink.GitHub package. |
| 5.2 | ci.yml | M | No `dotnet pack` validation. |
| 5.3 | README.md | M | Relative paths/SVG break NuGet rendering. |
| 5.4 | Directory.Build.props | M | Version applied to test project too. |
| 5.6 | EToro.csproj | L | No PackageReleaseNotes. |
| 5.7 | Directory.Build.props | L | LangVersion=latest. |
| 5.8 | Directory.Build.props | L | No TreatWarningsAsErrors in CI. |
| 5.9 | EToro.csproj | L | No PackageValidation enabled. |
| 5.10 | EToro.csproj | L | Strong-naming decision pending. |
| 6.1 | (all) | M | Missing ConfigureAwait(false). |
| 6.2 | EToroClientExtensions.cs | M | Reflection options binder blocks AOT/trim. |
| 6.3 | (all) | M | No ILogger anywhere. |
| 6.5 | EToroApiException.cs | M | StatusCode shadow. |
| 6.6 | EToroClient.cs | M | Public ctor creates empty options. |
| 6.7 | EToroClientExtensions.cs | M | Sub-interfaces not registered in DI. |
| 6.8 | QueryStringBuilder.cs | M | Enum.ToString() couples names to API. |
| 6.9 | (all) | M | No retry for transient 5xx/network. |
| 6.12 | EToroJsonContext.cs | L | Global JsonStringEnumConverter not configured. |
| 7.1-7.5 | .github/ | M | Pack/cache/artifacts/dependabot/CONTRIBUTING missing. |
| 7.9 | ci.yml | L | Linux-only matrix. |
| 8.1 | (interfaces) | M | XML doc coverage uneven. |
| 9.1 | tests/ | M | No unit tests for calculation service. |
| 9.2 | tests/ | M | No tests for body redaction. |
| 9.3 | tests/ | M | No tests for body truncation. |
| 9.4 | tests/ | M | No tests for header-injection rejection. |
| 9.8 | EToroIntegrationTests.cs:235 | L | TODO/discrepancy in code awaiting investigation. |

---

## Suggested priority order before NuGet publish

**Must fix (Critical/High) before public NuGet:**
1. §1.1 Bounded user-key cache (memory leak).
2. §1.2 Stream-based body read with bounded buffer.
3. §1.3 Endpoint sanitization (no PII).
4. §4.1 Realized-PnL paging — dedup by ID set; fail loudly on cap.
5. §4.2 Document `NetDeposits` as estimate.
6. §5.1 Add SourceLink package.
7. §5.3 NuGet README cleanup.
8. §2.1 Document disposal contract in XML docs.
9. §11.1 Full AOT compatibility (`IsAotCompatible=true`, source-gen binder, AOT smoke-test in CI).
10. §11.2 README disclaimer + liability waiver (EN + CS) + NuGet README mirror.
11. §11.3 CS translation of disclaimer block in `docs/cs/`.
12. §11.4 Disclaimer line in `<Description>` for NuGet metadata.

**Should fix (Medium) before stable v1.0:**
9. §1.4 Redaction patterns + tests (§9.2).
10. §1.7 SECURITY.md disclosure channel.
11. §1.8 Dependabot + CODEOWNERS + SHA-pinned actions.
12. §1.12 Environment validation.
13. §3.1 Migrate to `System.Threading.RateLimiting`.
14. §4.4–§4.5 Calculation correctness + unit tests (§9.1).
15. §5.2 Pack step in CI.
16. §5.4 Scope versions to packable projects.
17. §5.8 TreatWarningsAsErrors.
18. §5.9 PackageValidation post-v1.0.
19. §6.1 ConfigureAwait(false) library-wide.
20. §6.2 AOT/trim story (RequiresUnreferencedCode or source-gen binder).
21. §6.3 ILogger integration.
22. §6.5 Drop StatusCode shadow.
23. §6.6 Make secondary ctor internal.
24. §6.7 Register sub-interfaces in DI.
25. §6.8 Enum-to-API mapping tests.
26. §6.9 Resilience integration (Polly v8 / Microsoft.Extensions.Http.Resilience).
27. §7.1–§7.5 CI/OSS hygiene.
28. §9.2–§9.4 Security unit tests.

**Nice to have (Low/Info):**
- §1.10 MaxDepth on JSON.
- §5.5–§5.7 Versioning + LangVersion polish.
- §6.11–§6.12 JSON converter consolidation.
- §7.9 Windows runner in CI matrix.
- §9.8 Resolve TODO in integration test.

---

## 11. Additional requirements (added 2026-05-01)

### 11.1 [H] Full AOT support — required
Library must be NativeAOT/trim compatible end-to-end.

**Current state:**
- `EToroJsonContext` source-generated — JSON path AOT-safe.
- `services.AddOptions<EToroOptions>().Bind(...)` uses reflection — **breaks AOT**.
- No `IsAotCompatible` / `IsTrimmable` properties on csproj.

**Plan:**
1. `src/EToro/EToro.csproj` — add:
   ```xml
   <IsAotCompatible>true</IsAotCompatible>
   <IsTrimmable>true</IsTrimmable>
   <EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>
   ```
2. Replace reflection `.Bind(section)` in `EToroClientExtensions.AddEToro` with source-generated binder. .NET 9 has `Microsoft.Extensions.Configuration.Binder` source generator — annotate with `[ConfigurationKeyName]` where API field name differs from property name (none currently).
3. Audit every reflection usage: `JsonSerializer.Deserialize<T>(string, options)` needs `JsonTypeInfo<T>` overload via `EToroJsonContext.Default.<TypeName>` — switch all call sites in `EToroClient.cs` (`GetFromJsonAsync`, `GetFromJsonOrNoContentAsync`).
4. Add CI step: `dotnet publish -c Release -r linux-x64 /p:PublishAot=true` against a small consumer app under `tests/EToro.Aot.SmokeTest/` — fails build if trim/AOT warnings emitted.
5. Verify `JsonStringEnumConverter` usage AOT-compat (closed generic `JsonStringEnumConverter<T>` is OK; open generic is not).
6. Annotate any unavoidable reflection with `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` on entry points so consumers see warnings.

**Acceptance criterion:** zero `IL2026`/`IL2104`/`IL3050` warnings on `dotnet publish /p:PublishAot=true` against the smoke-test consumer.

Replaces / supersedes §6.2.

### 11.2 [H] README — disclaimer of liability + read-only/write notice
Add prominent section to `README.md` (top, after banner, before Installation) and to NuGet README:

```markdown
## ⚠️ Disclaimer

This project is **not affiliated with, endorsed by, or connected to eToro in any way**.

This software is provided for **informational and educational purposes only**.

- It does **not provide financial advice**.
- It should **not be used as the sole basis for trading decisions**.
- Data retrieved may be **inaccurate, delayed, or incomplete**.
- Currently the library exposes **read-only** endpoints. Mutating endpoints
  (POST / PUT / DELETE — order placement, watchlist edits, etc.) will be added
  later; consumers using such operations do so at their own risk.

The author and contributors **disclaim all liability** for any direct,
indirect, incidental, consequential, or punitive damages arising from the use
of this software, including but not limited to financial loss, missed trades,
incorrect calculations, downtime, data loss, or breach of eToro's terms of
service. Use entirely at your own risk.

By using this library you acknowledge that you understand the risks of
algorithmic and API-driven trading and that you alone are responsible for
your trading decisions and compliance with applicable laws and eToro's API
terms of use.
```

Mirror identical section into:
- `docs/en/getting-started.md` and `docs/en/index.md`
- `docs/cs/getting-started.md` and `docs/cs/index.md` (translated)
- `LICENSE` already covers warranty disclaimer (MIT) — keep as-is, but README disclaimer is the user-visible one.

Add reciprocal note in XML doc on `EToroClient` class summary referencing the disclaimer.

### 11.3 [M] Affiliation/disclaimer block — required snippet (verbatim)
Use the user-supplied wording as the canonical block (above). Czech translation in `docs/cs/`:

```markdown
## ⚠️ Upozornění

Tento projekt **není nijak spojen s eToro, není jím podporován ani schválen**.

Software je poskytován **výhradně pro informační a vzdělávací účely**.

- **Neposkytuje finanční poradenství.**
- **Nesmí být jediným podkladem pro obchodní rozhodnutí.**
- Získaná data mohou být **nepřesná, zpožděná nebo neúplná**.
- Knihovna v současnosti poskytuje pouze **read-only** koncové body.
  Zápisové operace (POST/PUT/DELETE) přijdou později; jejich použití je
  na vlastní riziko uživatele.

Autor a přispěvatelé **odmítají veškerou odpovědnost** za jakékoli přímé,
nepřímé, vedlejší, následné nebo sankční škody vzniklé v souvislosti
s používáním tohoto softwaru, zejména za finanční ztráty, propasené obchody,
nesprávné výpočty, výpadky, ztrátu dat či porušení podmínek eToro.
Software používáte výhradně na vlastní nebezpečí.
```

### 11.4 [L] NuGet package metadata — flag library as "not financial advice"
`<Description>` in `EToro.csproj` currently reads "Typed .NET client library for the eToro public API." Append: "Read-only at present. Not affiliated with eToro. No financial advice; informational/educational use only."

This text appears in `dotnet add package` output and on nuget.org search results, so the disclaimer reaches consumers before they install.

### 11.5 [L] Future write endpoints — prerequisite checklist
Before adding POST/PUT/DELETE to the public surface (not part of this refactor, but document now):
- Idempotency: require client-supplied idempotency-key header per request.
- Default `RetryNonIdempotentRequests=false` stays — consumers must opt in explicitly.
- Per-method rate limit (eToro write limit ≈ 20 RPM) — `EToroSlidingWindow` per method category, not just GET.
- Confirmation pattern in calculations service (no automatic placement of orders from heuristics).
- Dedicated `IEToroOrderClient` interface kept separate from read interfaces so consumers can deny-list it via DI.
- Distinct exception subtypes for write failures (e.g., `EToroOrderRejectedException`) carrying broker-side error codes.
- Audit log hook (`ILogger` at Information for every write attempt with redacted payload).

---

## Notes

- No code changes were made during this audit (per request).
- Pipelines / NuGet publish flow not assessed (per request).
- XML doc completeness left to current EN+CS docs (per request).
- Strong-naming/SourceLink/deterministic-build assessed as requested.
- Scope: source under `src/EToro/`, tests under `tests/EToro.Tests/`, root config files, `.github/` workflows, packaging metadata.
