# Mend SDK API

## SDK Features
 - Build SDK for Mend Platform 3.0 documented at https://api-docs.mend.io/platform/3.0
 - The current draft should include the following feature integration



## Development Principles

All code in this SDK **must** follow SOLID principles:

| Principle | Requirement |
|-----------|-------------|
| **S** — Single Responsibility | Each class has one reason to change. Token management, HTTP transport, and API method logic live in separate classes. |
| **O** — Open/Closed | Extend behaviour through interfaces and new implementations — do not modify existing classes to add new API coverage. |
| **L** — Liskov Substitution | Any `IMendTokenManager` or `IMendHttpClient` implementation must be substitutable without breaking callers. |
| **I** — Interface Segregation | Define narrow, focused interfaces. Do not add unrelated methods to `IMendTokenManager` or `IMendHttpClient`. |
| **D** — Dependency Inversion | All dependencies flow in via constructor injection. No `new` of concrete dependencies inside SDK classes. |

## Tech Stack

| Concern | Choice |
|---------|--------|
| Target framework | .NET Standard (class library) |
| Unit testing | xUnit |
| Mocking | Moq (used only where needed) |
| Solution format | Modern `.slnx` format |

### Project Structure

```
mend-sdk.slnx
src/
  Mend.Sdk/           # SDK class library (.NET Standard)
tests/
  Mend.Sdk.Tests/     # xUnit test project
```

## Service Registration

The SDK provides an `IServiceCollection` extension method so consuming projects can register all Mend services in a single call.

### Usage

```csharp
// Program.cs / Startup.cs of the consuming project
builder.Services.AddMendSdk(builder.Configuration);
```

### What `AddMendSdk` Registers

| Service | Lifetime | Notes |
|---------|----------|-------|
| `IMendTokenManager` → `MendTokenManager` | Singleton | One token lifecycle per app instance |
| `IMendHttpClient` → `MendHttpClient` | Singleton | Wraps `HttpClient`; registered via `IHttpClientFactory` |
| `IMendClient` → `MendClient` | Singleton | Main entry point for all API calls |

### Implementation Location

```
src/
  Mend.Sdk/
    Extensions/
      MendServiceCollectionExtensions.cs   ← AddMendSdk lives here
```

### Rules

- `AddMendSdk` must not reference any concrete class directly — only register through their interfaces
- Configuration binding (`Mend:*` keys) is handled inside `MendTokenManager`, not in the extension method
- The extension method must remain the **only** setup step required by the consumer — no manual wiring needed

## Authentication Design

### Token Management

Token lifecycle is handled by a dedicated `IMendTokenManager` interface, separating auth concerns from API method logic.

```csharp
public interface IMendTokenManager
{
    Task<string> GetAccessTokenAsync();
}
```

The main SDK client accepts `IMendTokenManager` via constructor injection and calls `GetAccessTokenAsync()` before each API request — it never holds or manages tokens directly.

### Default Implementation: `MendTokenManager`

The default implementation reads configuration from `appsettings.json` and manages the full token lifecycle in memory:

| Responsibility | Behaviour |
|----------------|-----------|
| Configuration | Reads `Mend:Email`, `Mend:UserKey`, and `Mend:OrgUuid` from `IConfiguration` (appsettings) |
| Lazy fetch | Calls `POST /api/v3.0/login` on the first `GetAccessTokenAsync()` call — no login at construction time |
| In-memory storage | Holds the current `accessToken` and `refreshToken` in private fields |
| Proactive refresh | Checks token expiry before each call; refreshes via `POST /api/v3.0/login/accessToken` if the token is at or near expiry (e.g. within 60 seconds of expiry) |

### Configuration Shape (`appsettings.json`)

```json
{
  "Mend": {
    "BaseUrl": "https://api.mend.io",
    "OrgUuid": "<your-org-uuid>",
    "Email": "<your-email>",
    "UserKey": "<your-user-key>"
  }
}
```

### Design Notes

- Callers that need custom auth behaviour (e.g. secret vault, multi-tenant) can supply their own `IMendTokenManager` implementation
- `MendTokenManager` is thread-safe — concurrent calls during a refresh are serialised so only one login/refresh request is made
- The SDK never exposes tokens to the caller; `IMendTokenManager` is the only path to a token

## SDK Endpoint Integration

### Access Management

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v3.0/login` | Authenticates with email and user key; returns a JWT access token (30 min validity) and a refresh token |
| POST | `/api/v3.0/login/accessToken` | Generates a new access token from a valid refresh token |
| POST | `/api/v3.0/logout` | Revokes the refresh token, ending the session |

### Fetching Organization product (application) and project information

The org UUID is returned from the login response. All endpoints below require a Bearer JWT token.

#### Applications (Products)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v3.0/orgs/{orgUuid}/applications` | Returns a list of all applications accessible to the current user |
| POST | `/api/v3.0/orgs/{orgUuid}/applications` | Creates a new application |
| POST | `/api/v3.0/orgs/{orgUuid}/applications/summaries` | Retrieves statistics for specified applications |
| GET | `/api/v3.0/orgs/{orgUuid}/applications/summaries/totals` | Obtains aggregate statistics across all applications |

#### Projects

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v3.0/orgs/{orgUuid}/projects` | Returns a list of all projects accessible to the current user |
| POST | `/api/v3.0/orgs/{orgUuid}/projects` | Creates a new project |
| POST | `/api/v3.0/orgs/{orgUuid}/projects/summaries` | Retrieves statistics for specified projects |
| GET | `/api/v3.0/orgs/{orgUuid}/projects/summaries/totals` | Obtains aggregate statistics across all projects |
| POST | `/api/v3.0/orgs/{orgUuid}/applications/{applicationUuid}/projects` | Creates a project within a specific application |

### Fetching Scan Information

Scans are tied to a project. All scan endpoints require `orgUuid` and `projectUuid`.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v3.0/orgs/{orgUuid}/projects/{projectUuid}/scans` | Returns a list of scan records for a project |
| GET | `/api/v3.0/orgs/{orgUuid}/projects/{projectUuid}/scans/{scanUuid}` | Fetches detailed information about a specific scan |
| GET | `/api/v3.0/orgs/{orgUuid}/projects/{projectUuid}/scans/{scanUuid}/summary` | Returns aggregated scan statistics |
| GET | `/api/v3.0/orgs/{orgUuid}/projects/{projectUuid}/scans/{scanUuid}/tags` | Retrieves metadata tags assigned to a scan |
| GET | `/api/v3.0/projects/{projectUuid}/scans/{scanUuid}/dependencies/SBOM/logs` | Returns processing logs from SBOM import operations |

## Fetching Report

### Security Findings (Vulnerability Detection)

#### SCA — Dependency Vulnerabilities

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v3.0/projects/{projectUuid}/dependencies/findings/security` | Returns all security findings for a given project |
| GET | `/api/v3.0/projects/{projectUuid}/dependencies/findings/security/groupBy/library` | Lists open source libraries with known vulnerabilities |
| GET | `/api/v3.0/projects/{projectUuid}/dependencies/findings/security/groupBy/rootLibrary` | Lists root libraries with security findings |
| PUT | `/api/v3.0/projects/{projectUuid}/dependencies/findings/security/rootLibrary/{rootLibraryUuid}` | Updates a security finding's status and comments |

#### SAST — Code Findings

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v3.0/projects/{projectUuid}/code/findings` | Returns a paginated list of SAST findings for a project |
| PATCH | `/api/v3.0/projects/{projectUuid}/code/findings` | Batch updates findings (state, suppression, severity) |
| PATCH | `/api/v3.0/projects/{projectUuid}/code/findings/{findingSnapshotId}` | Updates an individual finding's state or severity |
| GET | `/api/v3.0/projects/{projectUuid}/code/findings/{findingUuid}` | Fetches details for a specific code finding |
| GET | `/api/v3.0/projects/{projectUuid}/scans/{scanUuid}/code/findings` | Returns findings for a specific project scan |
| GET | `/api/v3.0/projects/{projectUuid}/scans/{scanUuid}/code/findings/{findingUuid}` | Retrieves a specific finding from a project scan |

#### AI Vulnerabilities

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v3.0/projects/{projectUuid}/ai/vulnerabilities` | Lists AI vulnerabilities detected in a specific project |
| GET | `/api/v3.0/projects/{projectUuid}/ai/vulnerabilities/{vulnerabilityId}` | Returns details of a specific AI vulnerability |
| GET | `/api/v3.0/applications/{applicationUuid}/ai/vulnerabilities` | Lists AI vulnerabilities across all projects in an application |
| GET | `/api/v3.0/applications/{applicationUuid}/ai/vulnerabilities/{vulnerabilityId}` | Returns application-level AI vulnerability details |

#### Zero-Day Events

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v3.0/orgs/{orgUuid}/dependencies/events/zeroday` | Returns all zero-day events with optional date filters |
| GET | `/api/v3.0/orgs/{orgUuid}/dependencies/events/zeroday/{eventUuid}/findings` | Lists affected packages and CVEs for a specific zero-day event |

### Async Report Exports

Reports are generated asynchronously. Post to trigger export, then poll status, then download.

#### Triggering Exports

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v3.0/projects/{projectUuid}/dependencies/reports/SBOM` | Export project SBOM report |
| POST | `/api/v3.0/projects/{projectUuid}/dependencies/reports/dueDiligence` | Export project due diligence findings |
| POST | `/api/v3.0/projects/{projectUuid}/code/reports/findings` | Export SAST findings report |
| POST | `/api/v3.0/projects/{projectUuid}/code/reports/compliance` | Export SAST compliance report |
| POST | `/api/v3.0/applications/{applicationUuid}/dependencies/reports/SBOM` | Export application SBOM report |
| POST | `/api/v3.0/applications/{applicationUuid}/dependencies/reports/dueDiligence` | Export application due diligence report |
| POST | `/api/v3.0/orgs/{orgUuid}/dependencies/reports/inventory` | Export organization inventory report |
| POST | `/api/v3.0/orgs/{orgUuid}/code/reports/compliance` | Export organization code compliance report |

#### Report Status and Download

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v3.0/orgs/{orgUuid}/reports` | List all report statuses for the organization |
| GET | `/api/v3.0/orgs/{orgUuid}/reports/{reportUuid}` | Get the status of a specific report |
| GET | `/api/v3.0/orgs/{orgUuid}/reports/download/{reportUuid}` | Download a completed report |
| DELETE | `/api/v3.0/orgs/{orgUuid}/reports/{reportUuid}` | Delete a report |
