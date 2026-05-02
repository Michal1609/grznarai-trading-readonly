# CI/CD Pipelines

This repository uses GitHub Actions for pull request validation, security checks, package verification, and NuGet publishing.

The default contribution flow is:

```text
feature branch -> pull request -> required checks -> merge to main
```

Direct pushes to `main` should be blocked by GitHub branch protection. The workflow files live in `.github/workflows/`, but this page is kept in `docs/` so contributors can find the pipeline contract together with the rest of the project documentation.

## CI

Workflow file: `.github/workflows/ci.yml`

Runs on:

- pull requests targeting `main`
- pushes to `main`
- manual `workflow_dispatch`

Purpose:

- restore NuGet packages
- build `GrznarAi.Trading.ReadOnly.slnx` in `Release`
- treat warnings as errors
- run the test suite
- upload `.trx` test results
- validate NativeAOT compatibility on Linux
- create NuGet package artifacts for inspection
- run a Podman smoke test inside the official .NET SDK container

The CI workflow runs on both `ubuntu-latest` and `windows-latest` for the main build and test job. Linux additionally performs the NativeAOT publish and package creation because those checks are enough to validate package output once per run.

The Podman job is separate from the OS matrix. It verifies that the project can be tested inside a clean containerized .NET SDK environment. This is useful for a public repository because contributors may use different local machines, while the container gives a repeatable Linux baseline.

CI only packs artifacts. It does not publish packages to NuGet.org.

## Secret Scan

Workflow file: `.github/workflows/secret-scan.yml`

Runs on:

- pull requests targeting `main`
- pushes to `main`
- a weekly scheduled scan
- manual `workflow_dispatch`

Purpose:

- scan the repository history with Gitleaks
- catch accidentally committed API keys, tokens, local config values, and other secrets
- keep the public repository safe for external contributors

The checkout uses full history (`fetch-depth: 0`) so Gitleaks can inspect more than just the latest commit.

## CodeQL

Workflow file: `.github/workflows/codeql.yml`

Runs on:

- pull requests targeting `main`
- pushes to `main`
- a weekly scheduled scan
- manual `workflow_dispatch`

Purpose:

- run GitHub CodeQL analysis for C#
- report security and code quality findings to GitHub code scanning
- catch common vulnerability patterns before changes are merged

The workflow uses `build-mode: none`, which is suitable for this C# library because the normal build is already covered by CI and CodeQL can analyze the source without repeating the full build.

## Dependency Review

Workflow file: `.github/workflows/dependency-review.yml`

Runs on:

- pull requests targeting `main`

Purpose:

- review NuGet and GitHub Actions dependency changes introduced by a pull request
- fail the pull request when a new high-severity vulnerable dependency is introduced
- make dependency risk visible before merge

This workflow is intentionally PR-only because dependency review compares the pull request against the target branch.

## Publish NuGet

Workflow file: `.github/workflows/publish-nuget.yml`

Runs on:

- manual `workflow_dispatch`

Purpose:

- publish an approved release from `main`
- validate the requested package version
- check that `CHANGELOG.md` contains release notes for the version
- check that the matching git tag does not already point to a different commit
- check that the package version does not already exist on NuGet.org
- restore packages
- build in `Release`
- run tests
- create `.nupkg` and `.snupkg` artifacts
- upload package artifacts to the workflow run
- create an annotated `v{version}` git tag
- publish the package and symbols to NuGet.org
- create a GitHub Release with the package files attached

This workflow requires the repository secret `NUGET_API_KEY`. It also asks for `confirm_publish=publish` to make accidental runs harder.

The workflow must be started from `main`. It checks out `main` explicitly and fails if the selected workflow branch is not `main`.

## Versioning

NuGet versions are SemVer-style package versions:

```text
Major.Minor.Patch[-prerelease]
```

The first public prerelease for this project is:

```text
1.0.0-alpha.1
```

Recommended prerelease progression:

```text
1.0.0-alpha.1
1.0.0-alpha.2
1.0.0-beta.1
1.0.0-rc.1
1.0.0
```

Use dotted prerelease numbers such as `alpha.1`, `beta.1`, and `rc.1`. Do not reuse a version after it has been published to NuGet.org. Package versions are treated as immutable for consumers, and publishing an existing package id/version pair returns a conflict.

NuGet marks packages as prerelease automatically from the version suffix. Any version with a suffix after the hyphen, such as `1.0.0-alpha.1`, `1.0.0-beta.1`, or `1.0.0-rc.1`, is a prerelease package. Stable versions have no suffix, for example `1.0.0`. The publish workflow does not need a separate prerelease flag for NuGet.org.

Release versions are not inferred from commit count or pull request history. The release owner chooses the exact version when manually starting the `Publish NuGet` workflow. The workflow passes that value to MSBuild as `Version` and `PackageVersion`, so the repository does not need a release commit that only changes the version.

Before running a release:

- merge the intended changes into `main`
- add a `CHANGELOG.md` section matching the exact version, for example `## [1.0.0-alpha.1] - 2026-05-02`
- start the `Publish NuGet` workflow from `main`
- set `version` to the exact NuGet version
- set `confirm_publish` to `publish`

`src/EToro/GrznarAi.Trading.ReadOnly.csproj` contains a local fallback version for normal local packing. The publish workflow is the source of truth for released package versions.

## Branch Protection

The repository settings should require pull requests before merging into `main`.

Recommended required checks:

- `Build and test (ubuntu-latest)`
- `Build and test (windows-latest)`
- `Podman smoke`
- `Gitleaks`
- `Analyze C#`
- `Review dependency changes`

Recommended branch protection settings:

- require a pull request before merging
- require status checks to pass before merging
- require branches to be up to date before merging
- block force pushes
- block branch deletion
- dismiss stale approvals when new commits are pushed
- require review from code owners when CODEOWNERS is expanded beyond the current catch-all owner

## NuGet Publishing Safety

The publish workflow uses these safeguards:

- manual trigger only
- `main` branch only
- explicit version input
- explicit `confirm_publish=publish` input
- NuGet package existence check before publishing
- changelog section check before publishing
- release tag check before publishing
- GitHub environment named `nuget.org`, which can be configured with required reviewers in repository settings
