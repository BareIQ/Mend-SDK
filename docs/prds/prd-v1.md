---
title: Mend SDK v1 — C# Client for Mend Platform API 3.0
status: Draft
version: v1
epic: EPIC-001
source: docs/wikis/mend-sdk-v1.md
lastUpdated: 2026-06-27
---

# PRD: Mend SDK v1 — C# Client for Mend Platform API 3.0

> **Status: Draft**

## 1. Overview

The Mend SDK is a .NET Standard class library providing a strongly-typed, idiomatic C# client for the Mend Platform 3.0 REST API. It lets internal and external consumers integrate Mend's security capabilities — vulnerability detection (SCA, SAST, AI, zero-day), scan history, org/application/project listing, and async report exports — without hand-rolling HTTP or JSON handling.

The primary goal is to **find vulnerabilities across all or selected projects**.

## 2. Goals

- Full coverage of the Mend Platform 3.0 API surface defined in the source wiki
- Strongly-typed POCO models for every response — no raw JSON handling by callers
- Token lifecycle owned by `IMendTokenManager` (lazy fetch, in-memory, proactive refresh, thread-safe)
- Single-call DI registration via `AddMendSdk(IConfiguration)`
- SOLID throughout — narrow interfaces, constructor injection, no concrete `new` inside SDK classes
- xUnit + Moq tests with no real network calls

## 3. Scope

In scope: authentication, applications, projects, scans, SCA/SAST/AI/zero-day findings, async report exports, DI registration.

## 4. Non-Goals (v1)

- Triggering new Mend scans (scan submission)
- Administration endpoints (groups, users, labels, integrations)
- CLI/UI layer; NuGet publishing
- Automatic re-auth/retry above the `IMendTokenManager` layer (caller owns higher-level retry)

## 5. Technical Constraints

- Target framework: .NET Standard (class library)
- Solution: modern `.slnx`; source under `src/`, tests under `tests/`
- Serialization: `System.Text.Json`; transport behind `IMendHttpClient` (mockable)
- 401 on any endpoint → `MendAuthException`; other non-success → `MendApiException`
- Configuration via `Mend:*` keys (`BaseUrl`, `OrgUuid`, `Email`, `UserKey`)

See `docs/wikis/mend-sdk-v1.md` for the full endpoint catalogue and design detail.

## 6. Stories

All work is tracked as DevBoard stories under **EPIC-001**. Full descriptions and acceptance criteria live in DevBoard; this PRD references them only.

| ID | TITLE | DEPENDS_ON |
|----|-------|------------|
| 00001-STORY | Solution scaffold: .slnx, src/Mend.Sdk, tests/Mend.Sdk.Tests | — |
| 00002-STORY | Options binding: MendOptions from Mend:* configuration | 00001-STORY |
| 00003-STORY | HTTP transport abstraction: IMendHttpClient + MendHttpClient | 00001-STORY |
| 00004-STORY | Exception model: MendException, MendAuthException, MendApiException | 00001-STORY |
| 00005-STORY | Token lifecycle: IMendTokenManager + MendTokenManager (login, refresh, in-memory, thread-safe) | 00002-STORY, 00003-STORY, 00004-STORY |
| 00006-STORY | Core client: IMendClient + MendClient with token injection and auth header | 00003-STORY, 00004-STORY, 00005-STORY |
| 00007-STORY | Applications endpoints: list, summaries, totals | 00006-STORY |
| 00008-STORY | Projects endpoints: list (filterable by application), summaries, totals | 00006-STORY |
| 00009-STORY | Scans endpoints: list, detail, summary, tags, SBOM logs | 00006-STORY |
| 00010-STORY | SCA dependency findings: list, groupBy library, groupBy rootLibrary, update status | 00006-STORY |
| 00011-STORY | SAST code findings: list, single, scan-scoped | 00006-STORY |
| 00012-STORY | AI vulnerabilities: project- and application-level | 00006-STORY |
| 00013-STORY | Zero-day events: list (date-filterable) and affected findings | 00006-STORY |
| 00014-STORY | Async report exports: trigger (project, application, org) | 00006-STORY |
| 00015-STORY | Service registration: AddMendSdk(IConfiguration) extension method | 00005-STORY, 00006-STORY |
| 00016-STORY | Report status, list, download, delete | 00006-STORY, 00014-STORY |

### Suggested delivery order

1. **Foundation:** 00001 → (00002, 00003, 00004) → 00005 → 00006
2. **DI:** 00015 (once 00005, 00006 land)
3. **Vulnerability path (primary goal):** 00008 → 00010 → 00011
4. **Remaining coverage:** 00007, 00009, 00012, 00013, 00014 → 00016

## 7. Success Metrics

- `dotnet build` succeeds with zero warnings on a clean checkout
- `dotnet test` passes with >80% line coverage on `Mend.Sdk`
- A consumer can authenticate, list projects, and retrieve SCA findings in under 20 lines of calling code
- All 16 stories meet their acceptance criteria

## 8. Open Questions

- Should `DownloadReportAsync` return a `Stream` or write to a caller-provided path?
- Do Mend cloud regions (EU/US/APAC) need distinct base-URL presets?
- Should rate limiting surface as a dedicated `MendRateLimitException`?
