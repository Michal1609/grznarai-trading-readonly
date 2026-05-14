# CI/CD

(Stub â€” pipeline zatĂ­m neudÄ›lanĂˇ, doplnit pĹ™i zaloĹľenĂ­ GitHub repo.)

DoporuÄŤenĂˇ matice:

| Job | Co dÄ›lĂˇ |
| --- | --- |
| `build` | `dotnet build -c Release` solution. |
| `test` | `dotnet test tests/GrznarAi.Trading.ReadOnly.Coinbase.Tests`. |
| `aot-smoke` | `dotnet publish tests/GrznarAi.Trading.ReadOnly.Coinbase.Aot.SmokeTest -c Release -r {win-x64,linux-x64}`. |
| `pack` | `dotnet pack src/Coinbase -c Release` â†’ NuGet artefakt. |

Tagy uvolnÄ›nĂ­ verze: `v0.1.0`, `v0.2.0` â€¦ `git tag` triggeruje `pack` + push na NuGet.
