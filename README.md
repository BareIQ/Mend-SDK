# Mend SDK

A .NET ecosystem for interacting with the [Mend Platform 3.0 REST API](https://api-docs.mend.io/platform/3.0). This repository ships two independently consumable packages:

| Package | Type | NuGet |
|---------|------|-------|
| **MendSdk** | Library | [![NuGet](https://img.shields.io/nuget/v/MendSdk)](https://www.nuget.org/packages/MendSdk) |
| **MendCli** | .NET Global Tool | [![NuGet](https://img.shields.io/nuget/v/MendCli)](https://www.nuget.org/packages/MendCli) |

---

## What's included

### `MendSdk` — C# Library

A strongly-typed client library that integrates into any .NET application via dependency injection. Covers:

- 🔐 Authentication (login, token refresh, logout)
- 📁 Projects & Applications
- 🔍 Scans
- 🐛 SCA dependency vulnerability findings
- 💻 SAST code findings
- 🤖 AI vulnerability detection
- ⚡ Zero-day events
- 📊 Async report exports (SBOM, due diligence, compliance, inventory)

→ [SDK documentation](src/Mend.Sdk/README.md)

---

### `MendCli` — `mendcli` Terminal Tool

A terminal-native Mend viewer. List projects, browse active CVEs, and get fix recommendations directly from your shell — no browser required.

```
mendcli --setup                          # Interactive credential setup
mendcli                                  # Auto-detect project from .mend-info
mendcli --list-projects                  # List all projects in your org
mendcli --project-name "my-service"      # Show vulnerabilities by project name
mendcli --project-id <uuid>              # Show vulnerabilities by project UUID
```

→ [CLI documentation](tools/Mend.Tool/README.md)

---

## Prerequisites

- .NET 8, 9, or 10 (for the CLI tool)
- .NET Standard 2.0 compatible runtime (for the SDK library)
- A Mend Platform account with an API user key

## Repository structure

```
src/
  Mend.Sdk/               # SDK library (BareIQ.MendSdk)
tools/
  Mend.Tool/              # CLI tool (BareIQ.MendCli / mendcli)
examples/
  Mend.Sdk.ExampleCli/    # Example console app using the SDK
tests/                    # xUnit test projects
docs/                     # API design docs and wikis
```

## License

[MIT](https://opensource.org/licenses/MIT) © Parimal Raj
