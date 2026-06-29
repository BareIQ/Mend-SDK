# MendSdk

[![NuGet](https://img.shields.io/nuget/v/MendSdk)](https://www.nuget.org/packages/MendSdk)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Strongly-typed C# client for the [Mend Platform 3.0 REST API](https://api-docs.mend.io/platform/3.0).  
Covers authentication, projects, applications, scans, SCA/SAST/AI/zero-day findings, and async report exports.

---

## Installation

```
dotnet add package MendSdk
```

**Target framework:** .NET Standard 2.0 — compatible with .NET 6, 7, 8, 9, 10 and .NET Framework 4.6.1+.

---

## Quick start

### 1. Add configuration

Add a `Mend` section to your `appsettings.json`:

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

> **Tip:** Use [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables to keep credentials out of source control.

### 2. Register the SDK

```csharp
// Program.cs
builder.Services.AddMendSdk(builder.Configuration);
```

This one call registers all SDK clients as singletons and wires up the token manager and HTTP client.

### 3. Use a client

```csharp
public class MyService(IMendProjectsClient projects, IMendDependenciesClient deps)
{
    public async Task PrintVulnerabilitiesAsync()
    {
        var allProjects = await projects.GetProjectsAsync();

        foreach (var project in allProjects)
        {
            var findings = await deps.GetDependencySecurityFindingsAsync(project.Uuid);
            var critical = findings.Count(f => f.Severity == "CRITICAL" && f.Status == "ACTIVE");
            Console.WriteLine($"{project.Name}: {critical} critical finding(s)");
        }
    }
}
```

---

## Registered clients

`AddMendSdk` registers the following interfaces. Inject any of them where needed:

| Interface | Description |
|-----------|-------------|
| `IMendProjectsClient` | List, create, and summarise projects |
| `IMendApplicationsClient` | List, create, and summarise applications (products) |
| `IMendScansClient` | Retrieve scan records, summaries, tags, and SBOM logs |
| `IMendDependenciesClient` | SCA security findings (by project, library, or root library) |
| `IMendCodeFindingsClient` | SAST code findings — list, update, and retrieve by scan |
| `IMendAiVulnerabilitiesClient` | AI vulnerability findings at project or application level |
| `IMendZeroDayEventsClient` | Zero-day events and their affected packages/CVEs |
| `IMendReportsClient` | List, status-check, download, and delete async reports |
| `IMendReportExportsClient` | Trigger SBOM, due diligence, SAST, compliance, and inventory exports |

---

## Configuration reference

| Key | Required | Description |
|-----|----------|-------------|
| `Mend:BaseUrl` | Yes | Mend API base URL (e.g. `https://api-saas.whitesourcesoftware.com`) |
| `Mend:OrgUuid` | Yes | Organisation UUID from your Mend account |
| `Mend:Email` | Yes | Email address of the API user |
| `Mend:UserKey` | Yes | API user key from Mend account settings |

---

## Authentication

The SDK handles the full token lifecycle automatically:

- **Lazy login** — authenticates on the first API call, not at startup.
- **Proactive refresh** — renews the access token before it expires (within 60 s of expiry), with no downtime.
- **Thread-safe** — concurrent requests during a refresh are serialised so only one login/refresh call is made.

### Custom authentication

To use a secret vault, multi-tenant credentials, or any other auth source, implement `IMendTokenManager` and register your implementation before calling `AddMendSdk`:

```csharp
services.AddSingleton<IMendTokenManager, MyVaultTokenManager>();
services.AddMendSdk(configuration); // will not overwrite your custom registration
```

---

## API coverage

### Access management
`POST /login` · `POST /login/accessToken` · `POST /logout`

### Projects
`GET|POST /orgs/{orgUuid}/projects` · `POST /projects/summaries` · `GET /projects/summaries/totals`

### Applications
`GET|POST /orgs/{orgUuid}/applications` · `POST /applications/summaries` · `GET /applications/summaries/totals`

### Scans
`GET /projects/{projectUuid}/scans` · `GET /scans/{scanUuid}` · `GET /scans/{scanUuid}/summary` · `GET /scans/{scanUuid}/tags`

### SCA — dependency findings
`GET /projects/{projectUuid}/dependencies/findings/security`  
`GET …/groupBy/library` · `GET …/groupBy/rootLibrary` · `PUT …/rootLibrary/{uuid}`

### SAST — code findings
`GET|PATCH /projects/{projectUuid}/code/findings`  
`PATCH /code/findings/{snapshotId}` · `GET /code/findings/{uuid}`  
`GET /scans/{scanUuid}/code/findings` · `GET /scans/{scanUuid}/code/findings/{uuid}`

### AI vulnerabilities
`GET /projects/{projectUuid}/ai/vulnerabilities[/{id}]`  
`GET /applications/{applicationUuid}/ai/vulnerabilities[/{id}]`

### Zero-day events
`GET /orgs/{orgUuid}/dependencies/events/zeroday`  
`GET /zeroday/{eventUuid}/findings`

### Report exports (async)
Trigger: SBOM · Due Diligence · SAST Findings · SAST Compliance · Inventory  
Manage: list · status · download · delete

---

## Example project

See [`examples/Mend.Sdk.ExampleCli`](../../examples/Mend.Sdk.ExampleCli) for a complete interactive console app that lists projects and displays vulnerability findings.

---

## License

[MIT](https://opensource.org/licenses/MIT) © Parimal Raj
