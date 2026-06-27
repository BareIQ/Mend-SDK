---
title: Mend.Tool v0.1.0 — mendcli Developer CLI
status: Implemented
version: v2
epic: EPIC-002
commit: f2f50a4e94d572097d7e6376747572f5a1fd4f5d
lastUpdated: 2026-06-28
---

# PRD: Mend.Tool v0.1.0 — mendcli Developer CLI

> **Status: Implemented** — commit `f2f50a4`

## 1. Overview

`mendcli` is a globally-installable .NET tool that wraps the Mend SDK to provide a fast, terminal-native view of Mend security vulnerabilities. Developers run it from any repository directory without writing any code — credentials live in a machine-level config file; project identity lives in a per-repo `.mend-info` file.

This document also covers the SDK model enrichments and new client methods shipped in the same commit.

## 2. Goals

- Zero-friction access to project vulnerabilities from the terminal
- One-time interactive setup via `mendcli --setup` — no manual JSON editing required
- Per-repo project identity via a `.mend-info` file (project UUID or name)
- Partial, case-insensitive project name search with an interactive selection prompt on multiple matches
- Active vulnerabilities shown sorted by criticality then detection date, with fix recommendations
- Polished Spectre.Console output (coloured severity, square-bordered tables, row separators, spinner feedback)
- 41 unit tests with no real network calls; tests run sequentially to avoid Spectre global-state races

## 3. Scope

### 3.1 Mend.Tool (new project)

| Area | Included |
|---|---|
| `mendcli --setup` | Interactive credential wizard, live validation, writes `~/.mendcli/config.json` |
| `mendcli --list-projects` | Paginated project list rendered as a Spectre table |
| `mendcli --project-id <UUID>` | Vulnerability view by project UUID (auto-resolves project name) |
| `mendcli --project-name <NAME>` | Case-insensitive partial match; Spectre selection prompt for ambiguous results |
| `mendcli` (no args) | Reads `.mend-info` from CWD, routes to id or name path |
| Vulnerability display | Filter ACTIVE, sort by severity + detected-at, table + summary line |
| Error handling | Spectre-formatted errors, exit code 1 on all failure paths |

### 3.2 SDK enhancements (shipped in same commit)

| Change | Details |
|---|---|
| `IMendClient.PostPagedAsync<T>` | POST with `limit` and `cursor` query params (distinct from `GetPagedAsync` which uses `pageSize`) |
| `ApplicationSummary` model rewrite | Replaced flat severity counts with `creationDate`, `tags`, `labels`, `statistics` (nested `ApplicationStatistics`) |
| `ApplicationStatistics` + 21 sub-classes | Strongly-typed coverage of all statistics categories returned by the API |
| `SecurityFinding` model enrichment | 12 new top-level fields (`project`, `application`, `topFix`, `effective`, `reachability`, `threatAssessment`, `exploitable`, `malicious`, `scoreMetadataVector`, `violations`, `workflowUuids`, `dependencyContexts`); `SecurityFindingComponent` +11 fields; `SecurityFindingVulnerability` +4 fields; 8 new nested types |
| `IMendApplicationsClient.GetApplicationSummariesAsync` | New `limit` and `cursor` pagination params |

## 4. Non-Goals (v0.1.0)

- Updating or suppressing vulnerability findings
- Triggering new Mend scans
- SAST / AI / zero-day findings display (SDK clients exist; CLI exposes SCA only)
- NuGet packaging or publishing to nuget.org
- Cursor-based multi-page pagination (single call with `pageSize: 10000` covers real-world org sizes)
- Windows/macOS/Linux installer (install via `dotnet tool install`)

## 5. Technical Constraints

| Constraint | Value |
|---|---|
| Project type | `dotnet tool` (`PackAsTool=true`) |
| Package ID | `BareIQ.MendCli` |
| Command name | `mendcli` |
| Version | `0.1.0` |
| Target frameworks | `net8.0; net9.0; net10.0` |
| Key dependencies | `Spectre.Console 0.49.1`, `System.CommandLine 2.0.0-beta4.22272.1` |
| SDK dependency | `../../src/Mend.Sdk` (project reference) |
| Credentials store | `~/.mendcli/config.json` (user profile, not per-repo) |
| Project identity store | `.mend-info` in the current working directory |

## 6. Architecture

```
tools/Mend.Tool/
├── Program.cs                    # Entry point: option definitions, DI bootstrap, command routing
├── Config/
│   └── MendCliConfig.cs          # Loads ~/.mendcli/config.json; builds in-memory IConfiguration for --setup validation
├── MendInfo/
│   └── MendInfoFile.cs           # MendInfo model + MendInfoReader (reads .mend-info from CWD)
├── Handlers/
│   ├── SetupHandler.cs           # Interactive wizard: prompt → validate → write config
│   ├── ListProjectsHandler.cs    # Fetches all projects, delegates to ProjectTable
│   └── VulnerabilityHandler.cs   # Routes from .mend-info / --project-id / --project-name; delegates to VulnerabilityTable
├── Services/
│   ├── ProjectResolver.cs        # Pure: case-insensitive partial name match over a project list
│   └── VulnerabilityFilter.cs    # Pure: ACTIVE filter, severity+date sort, TopFix formatting
└── Display/
    ├── ProjectTable.cs           # Spectre square-bordered table with row separators
    └── VulnerabilityTable.cs     # Spectre table + severity summary line
```

### Dependency flow

```
Program.cs
  → SetupHandler          (standalone — no DI needed)
  → ListProjectsHandler   (IMendProjectsClient)
  → VulnerabilityHandler  (IMendProjectsClient + IMendDependenciesClient)
       → ProjectResolver  (pure, no DI)
       → VulnerabilityFilter (pure, no DI)
       → VulnerabilityTable  (Spectre, no DI)
```

## 7. Configuration

### 7.1 Machine-level credential file

**Path:** `~/.mendcli/config.json`

```json
{
  "Mend": {
    "BaseUrl": "https://api-saas.whitesourcesoftware.com",
    "OrgUuid": "<organisation-uuid>",
    "Email": "user@example.com",
    "UserKey": "<api-key>"
  }
}
```

Read via `Microsoft.Extensions.Configuration.Json` and passed to `AddMendSdk(IConfiguration)`. All non-`--setup` commands require this file; if missing, a Spectre error message directs the user to run `mendcli --setup`.

### 7.2 Per-repo project identity file

**Filename:** `.mend-info` (in the current working directory)

```json
{
  "projectId": "<mend-project-uuid>",
  "projectName": "MyProject"
}
```

At least one of `projectId` or `projectName` must be present. `projectId` takes priority when both are present. `projectId` is the Mend project UUID (matches `Project.Uuid` in the SDK).

## 8. Use Cases

### UC-1: First-time setup (`mendcli --setup`)

Runs without a pre-existing config file.

1. Prompt `Base URL` (default `https://api-saas.whitesourcesoftware.com`)
2. Prompt `Organisation UUID`
3. Prompt `Email`
4. Prompt `User key` (masked input)
5. Spinner: validate by calling `IMendProjectsClient.GetProjectsAsync(pageSize: 1)`
6. On failure → error message, exit 1, no file written
7. On success → create `~/.mendcli/`, write `config.json`, print `✓ Configuration saved to ...`, exit 0

### UC-2: List all projects (`mendcli --list-projects`)

- Fetches up to 10 000 projects in a single call
- Renders a square-bordered Spectre table (UUID | Name) sorted alphabetically by name
- Shows row separators between entries
- Prints project count below table

### UC-3: Vulnerabilities by UUID (`mendcli --project-id <UUID>`)

- Fetches all projects (10 000 limit) to resolve the human-readable project name for display
- Fetches up to 10 000 security findings for the given UUID
- Renders vulnerability table titled `Name (UUID)`

### UC-4: Vulnerabilities by name (`mendcli --project-name <NAME>`)

- Fetches all projects, filters by case-insensitive substring match
- **0 matches** → error with hint to use `--list-projects`
- **1 match** → proceeds directly
- **Multiple matches** → interactive `SelectionPrompt` (falls back to listing UUIDs when terminal is non-interactive)
- Proceeds to UC-3 with the selected project's UUID and name pre-resolved (no second lookup)

### UC-5: Auto-detect from `.mend-info` (`mendcli`)

- Reads `.mend-info` from CWD
- Missing file or neither field present → error, exit 1
- `projectId` present → UC-3
- `projectName` present → UC-4

### Vulnerability display (shared by UC-3, UC-4, UC-5)

**Filter:** `FindingInfo.Status == "ACTIVE"` (case-insensitive)

**Sort:** severity rank (CRITICAL=0, HIGH=1, MEDIUM=2, LOW=3, other=4) → then `FindingInfo.DetectedAt` ascending

**Table columns:**

| Column | Source |
|---|---|
| Severity | `Vulnerability?.Severity` (coloured: bold red / red / yellow / blue) |
| Library | `Component?.Name` |
| CVE | `finding.CveName` (= `finding.Name`) |
| Status | `FindingInfo?.Status` |
| Top Fix | `TopFix.Message - TopFix.FixResolution` when `TopFix.Id > 0`, else `–` |

**Summary line below table:**
```
Total active: N   Critical: N   High: N   Medium: N   Low: N
```

## 9. Error Handling

| Condition | Message | Exit |
|---|---|---|
| `~/.mendcli/config.json` missing | `Error: Config file not found at {path}. Run mendcli --setup ...` | 1 |
| `.mend-info` missing (no-args mode) | `Error: No .mend-info file found in the current directory.` | 1 |
| `.mend-info` has neither field | `Error: .mend-info must contain projectId or projectName.` | 1 |
| No project matches name | `Error: No project matching "{name}" found.` | 1 |
| Non-interactive terminal + multiple matches | Lists UUIDs + names, directs user to `--project-id` | 1 |
| `MendAuthException` | `Error: Authentication failed for {endpoint}. Check credentials or run mendcli --setup.` | 1 |
| `MendApiException` | `Error: API error {statusCode}: {message}` | 1 |
| `--setup` validation failure | `Error: Authentication failed. Check your credentials and try again.` | 1 |

## 10. Test Coverage (`tests/Mend.Tool.Tests`)

41 tests, `net10.0`, sequential execution (`CollectionBehavior(DisableTestParallelization = true)`).

| Suite | Count | What is tested |
|---|---|---|
| `MendInfoReaderTests` | 7 | File parsing (id-only, name-only, both, missing file), `IsValid` for all combinations |
| `ProjectResolverTests` | 7 | Exact match, partial match, case-insensitive, no match, multiple matches, empty list, empty name |
| `VulnerabilityFilterTests` | 17 | ACTIVE filter (include/exclude/case), severity sort order, date tie-breaking, unknown severity last, `SeverityRank` for all values, `FormatTopFix` (null, Id=0, Id>0, empty resolution) |
| `ListProjectsHandlerTests` | 3 | Correct `pageSize: 10000` call, empty list, return code 0 |
| `VulnerabilityHandlerTests` | 7 | Correct finding fetch params, name-based resolve, no-match exit code, single-match flow, auth exception propagation |

## 11. SDK Changes Detail

### `IMendClient.PostPagedAsync<T>`

```csharp
Task<T?> PostPagedAsync<T>(string path, object? body = null, int? limit = null, string? cursor = null, CancellationToken cancellationToken = default);
```

Appends `limit=` and `cursor=` query parameters (not `pageSize=`). Used by `GetApplicationSummariesAsync`. `BuildLimitedPath` helper mirrors the existing `BuildPagedPath` used by `GetPagedAsync`.

### `ApplicationSummary` rewrite

Old flat properties `highSeverityCount`, `mediumSeverityCount`, `lowSeverityCount` removed — they did not exist in the real API response. New fields: `creationDate`, `tags`, `labels`, `statistics` (`ApplicationStatistics`). The `statistics` object maps 21 API category keys (e.g. `ALERTS`, `GENERAL`, `LICENSE_RISK`, `SAST_SCAN`, `AI_SECURITY`) to strongly-typed sub-classes.

### `SecurityFinding` enrichment

Captures the full API response. New top-level fields:

| Field | Type |
|---|---|
| `project` | `SecurityFindingProject?` (uuid, name, path, applicationUuid) |
| `application` | `SecurityFindingApplication?` (uuid, name) |
| `topFix` | `SecurityFindingTopFix?` (id, vulnerability, type, origin, url, fixResolution, date, message) |
| `effective` | `string` (e.g. `NO_SHIELD`) |
| `reachability` | `string` (e.g. `REACHABILITY_UNAVAILABLE`) |
| `threatAssessment` | `SecurityFindingThreatAssessment?` (exploitCodeMaturity, epssPercentage) |
| `exploitable` | `bool` |
| `malicious` | `bool` |
| `scoreMetadataVector` | `string` |
| `violations` | `int` |
| `workflowUuids` | `IReadOnlyList<string>` |
| `dependencyContexts` | `IReadOnlyList<SecurityFindingDependencyContext>` |

`SecurityFindingInfo` gains `detectedAt` and `modifiedAt`. `SecurityFindingComponent` gains 11 fields including `uuid`, `description`, `componentType`, `libraryType`, `references`, `groupId`, `artifactId`, `directDependency`, `rootLibrary`, `path`, `dependencyFile`, `dependencyType`. `SecurityFindingVulnerability` gains `type`, `publishDate`, `modifiedDate`, `vulnerabilityScoring`.

## 12. Installation

```shell
# Pack
dotnet pack tools/Mend.Tool -c Release -o tools/Mend.Tool/nupkg

# Install globally
dotnet tool install --global --add-source tools/Mend.Tool/nupkg BareIQ.MendCli

# First-time setup
mendcli --setup

# Update after code changes
dotnet pack tools/Mend.Tool -c Release -o tools/Mend.Tool/nupkg
dotnet tool update --global --add-source tools/Mend.Tool/nupkg BareIQ.MendCli
```

## 13. Success Metrics

- `dotnet build tools/Mend.Tool` — 0 errors, 0 warnings on all three target frameworks
- `dotnet test tests/Mend.Tool.Tests` — 41/41 pass
- `dotnet test tests/Mend.Sdk.Tests` — 181/181 pass (no regressions)
- `mendcli --help` prints all four options
- `mendcli --setup` with valid credentials writes `~/.mendcli/config.json` and exits 0
- `mendcli --list-projects` renders a Spectre table with row separators
- `mendcli --project-id <UUID>` renders the vulnerability table headed `Name (UUID)`
- `mendcli` without `.mend-info` exits 1 with a clear error message
