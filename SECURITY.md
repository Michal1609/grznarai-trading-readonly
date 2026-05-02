# Security Policy

## Credentials

Do not commit real eToro `ApiKey` or `UserKey` values.

Use one of these mechanisms instead:

- .NET user secrets for local development.
- Environment variables such as `EToroOptions__ApiKey` and `EToroOptions__UserKey`.
- CI or production secret stores for deployed applications.

`appsettings.example.json` and `tests/GrznarAi.Trading.ReadOnly.Tests/appsettings.test.example.json` contain dummy values only.

## Reporting

**Please do not open a public GitHub issue for security vulnerabilities.**

Report security issues through [GitHub Private Vulnerability Reporting](https://github.com/Michal1609/grznarai-trading-readonly/security/advisories/new)
or by e-mail to **michal.grznar@seznam.cz**.

Please include:

- A description of the vulnerability and its potential impact.
- Steps to reproduce or a proof-of-concept.
- Affected version(s) / commit range.

You can expect an acknowledgement within **5 business days** and a status update within **14 calendar days**.

## Supported Versions

Until the first stable public release, only the latest code on `main` is supported.
