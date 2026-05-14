# CI/CD

(Stub â€” pipeline not wired yet, add when the GitHub repo is set up.)

Recommended matrix:

| Job | What it does |
| --- | --- |
| `build` | `dotnet build -c Release` for the solution. |
| `test` | `dotnet test tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests`. |
| `aot-smoke` | `dotnet publish tests/GrznarAi.Trading.ReadOnly.Coinbase.Aot.SmokeTest -c Release -r {win-x64,linux-x64}`. |
| `pack` | `dotnet pack src/Coinbase -c Release` â†’ NuGet artifact. |

Release tagging: `v0.1.0`, `v0.2.0`, â€¦ `git tag` triggers `pack` + push to NuGet.
