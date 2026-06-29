# MendCli — `mendcli`

[![NuGet](https://img.shields.io/nuget/v/MendCli)](https://www.nuget.org/packages/MendCli)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Terminal-native Mend security vulnerability viewer.  
List projects, browse active CVEs, and get fix recommendations directly from your shell — no browser required.

---

## Installation

```
dotnet tool install --global MendCli
```

**Requires:** .NET 8, 9, or 10 runtime.

---

## Setup

Run the interactive setup wizard once to configure your credentials:

```
mendcli --setup
```

You will be prompted for:

| Prompt | Description |
|--------|-------------|
| Base URL | Mend API endpoint (default: `https://api-saas.whitesourcesoftware.com`) |
| Organisation UUID | Found in your Mend account settings |
| Email | Email address of your Mend API user |
| User key | API key from Mend account settings |

Credentials are validated against the live API before being saved.  
The config file is written to `~/.mendcli/config.json`.

---

## Usage

```
mendcli [options]
```

### Options

| Option | Description |
|--------|-------------|
| `--setup` | Interactive credential configuration wizard |
| `--list-projects` | List all projects in the organisation |
| `--project-id <uuid>` | Show vulnerabilities for a project by UUID |
| `--project-name <name>` | Show vulnerabilities for a project by name (partial, case-insensitive match) |

### Examples

```bash
# First-time setup
mendcli --setup

# List all projects in your org
mendcli --list-projects

# Show vulnerabilities for a project by UUID
mendcli --project-id 9f4b1c2d-3e5f-...

# Show vulnerabilities by name (partial match supported)
mendcli --project-name "my-api"

# Auto-detect project from .mend-info file in the current directory
mendcli
```

---

## Auto-detect mode (`.mend-info`)

When you run `mendcli` with no options, it looks for a `.mend-info` file in the current directory:

```json
{ "projectId": "9f4b1c2d-3e5f-..." }
```

or by name:

```json
{ "projectName": "my-api" }
```

Commit this file to your repository root so any team member can run `mendcli` from the repo without extra arguments.

---

## Output

Vulnerabilities are displayed as a colour-coded table sorted by severity:

```
 #    Severity   CVE                Library                  Status
──────────────────────────────────────────────────────────────────────
 1    CRITICAL   CVE-2021-44228     log4j-core-2.14.1        ACTIVE
 2    HIGH       CVE-2022-25845     fastjson-1.2.76           ACTIVE
 3    MEDIUM     CVE-2023-20860     spring-webmvc-5.3.18      ACTIVE
──────────────────────────────────────────────────────────────────────
Total: 3 finding(s)  [Critical: 1  High: 1  Medium: 1  Low: 0]
```

Only `ACTIVE` findings are shown by default.

If multiple projects match a `--project-name` search, an interactive selection menu is displayed.

---

## Configuration file

Credentials are stored in `~/.mendcli/config.json`:

```json
{
  "Mend": {
    "BaseUrl": "https://api-saas.whitesourcesoftware.com",
    "OrgUuid": "<your-org-uuid>",
    "Email": "<your-email>",
    "UserKey": "<your-user-key>"
  }
}
```

To update credentials, re-run `mendcli --setup` or edit the file directly.

---

## Updating

```
dotnet tool update --global MendCli
```

---

## Uninstalling

```
dotnet tool uninstall --global MendCli
```

---

## License

[MIT](https://opensource.org/licenses/MIT) © Parimal Raj
