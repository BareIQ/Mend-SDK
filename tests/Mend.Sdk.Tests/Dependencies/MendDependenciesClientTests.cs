using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Dependencies;
using Mend.Sdk.Dependencies.Models;
using Mend.Sdk.Exceptions;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.Dependencies;

public sealed class MendDependenciesClientTests
{
    private const string TestProjectUuid = "test-project-uuid";
    private const string TestRootLibraryUuid = "test-root-library-uuid";
    private static readonly string ExpectedBasePath =
        $"/api/v3.0/projects/{TestProjectUuid}/dependencies/findings/security";

    // --- Deserialization ---

    [Fact]
    public void SecurityFinding_DeserializesFromJson_Correctly()
    {
        const string json = """
            [
              {
                "uuid": "4a4dbdef-ed2d-4825-aaaf-7c245da99175",
                "name": "CVE-2026-33671",
                "type": "SECURITY_VULNERABILITY",
                "findingInfo": {
                  "findingStatus": "UNREVIEWED",
                  "status": "ACTIVE",
                  "comment": {},
                  "detectedAt": "2026-03-25T22:22:08Z",
                  "modifiedAt": "2026-03-25T22:22:08Z"
                },
                "project": {
                  "uuid": "a5f37c00-c888-46b7-923f-e41d6772f449",
                  "name": "Clarizen.Modernization.Api",
                  "path": "Clarizen.Modernization.Api",
                  "applicationUuid": "964aa0c6-53dd-4906-83d9-36075c888bdd"
                },
                "application": {
                  "uuid": "964aa0c6-53dd-4906-83d9-36075c888bdd",
                  "name": "Clarizen.Modernization.Api"
                },
                "component": {
                  "uuid": "6bf04787-e184-47f6-83b8-bb5540c1b5a3",
                  "name": "picomatch-2.3.1.tgz",
                  "description": "Blazing fast glob matcher",
                  "componentType": "Library",
                  "libraryType": "NODE_PACKAGED_MODULE",
                  "language": "NODE_PACKAGED_MODULE",
                  "references": {
                    "url": "https://registry.npmjs.org/picomatch/-/picomatch-2.3.1.tgz",
                    "homePage": "https://github.com/micromatch/picomatch",
                    "genericPackageIndex": "https://www.npmjs.org/package/picomatch"
                  },
                  "groupId": "picomatch",
                  "artifactId": "picomatch-2.3.1.tgz",
                  "version": "2.3.1",
                  "directDependency": false,
                  "rootLibrary": false,
                  "path": "/home/jenkins/workspace/package.json",
                  "dependencyFile": "/home/jenkins/workspace/package.json",
                  "dependencyType": "Transitive"
                },
                "vulnerability": {
                  "name": "CVE-2026-33671",
                  "type": "CVSS_3",
                  "description": "ReDoS vulnerability in picomatch.",
                  "score": 7.5,
                  "severity": "HIGH",
                  "publishDate": "2026-03-26T21:20:48Z",
                  "modifiedDate": "2026-03-26T22:31:27Z",
                  "vulnerabilityScoring": [
                    { "score": 7.5, "severity": "HIGH", "type": "CVSS_3" }
                  ]
                },
                "topFix": {
                  "id": 181892,
                  "vulnerability": "CVE-2026-33671",
                  "type": "UPGRADE_VERSION",
                  "origin": "WHITESOURCE_EXPERT",
                  "url": "https://github.com/micromatch/picomatch/commit/abc123",
                  "fixResolution": "https://github.com/micromatch/picomatch.git - 2.3.2",
                  "date": "2026-03-25T22:07:10Z",
                  "message": "Upgrade to version"
                },
                "effective": "NO_SHIELD",
                "reachability": "REACHABILITY_UNAVAILABLE",
                "threatAssessment": {
                  "exploitCodeMaturity": "NOT_DEFINED",
                  "epssPercentage": 0.412
                },
                "exploitable": false,
                "malicious": false,
                "scoreMetadataVector": "CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:N/I:N/A:H",
                "violations": 0,
                "workflowUuids": [ "" ],
                "dependencyContexts": [
                  {
                    "dependencyType": "TRANSITIVE",
                    "isDirect": false,
                    "isTransitive": true,
                    "directRoots": [
                      {
                        "rootLibraryUuid": "7799588d-b3fd-49c3-8393-1fd7fd487d20",
                        "rootLibraryName": "jest-29.5.14.tgz",
                        "rootLibraryVersion": "29.5.14"
                      }
                    ]
                  }
                ]
              },
              {
                "uuid": "c67ee91d-acd7-42ff-9c52-88712ef373ed",
                "name": "CVE-2025-8262",
                "type": "SECURITY_VULNERABILITY",
                "findingInfo": {
                  "findingStatus": "REMEDIATED",
                  "status": "LIBRARY_REMOVED",
                  "detectedAt": "2025-07-28T08:30:46Z",
                  "modifiedAt": "2025-10-04T08:57:58Z"
                },
                "component": { "name": "yarn-1.22.22.tgz", "version": "1.22.22" },
                "vulnerability": { "name": "CVE-2025-8262", "severity": "MEDIUM", "score": 4.3 },
                "topFix": {},
                "effective": "NO_SHIELD",
                "reachability": "REACHABILITY_UNAVAILABLE",
                "exploitable": false,
                "malicious": false,
                "scoreMetadataVector": "CVSS:3.1/AV:N/AC:L/PR:L/UI:N/S:U/C:N/I:N/A:L",
                "violations": 0,
                "workflowUuids": [],
                "dependencyContexts": []
              }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<SecurityFinding>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);

        var f1 = result[0];
        Assert.Equal("4a4dbdef-ed2d-4825-aaaf-7c245da99175", f1.Uuid);
        Assert.Equal("CVE-2026-33671", f1.CveName);
        Assert.Equal("HIGH", f1.Severity);
        Assert.Equal("picomatch-2.3.1.tgz", f1.LibraryName);
        Assert.Equal("ACTIVE", f1.Status);
        Assert.Equal("SECURITY_VULNERABILITY", f1.Type);

        // findingInfo
        Assert.Equal("UNREVIEWED", f1.FindingInfo!.FindingStatus);
        Assert.Equal("2026-03-25T22:22:08Z", f1.FindingInfo.DetectedAt);
        Assert.Equal("2026-03-25T22:22:08Z", f1.FindingInfo.ModifiedAt);

        // project
        Assert.Equal("a5f37c00-c888-46b7-923f-e41d6772f449", f1.Project!.Uuid);
        Assert.Equal("Clarizen.Modernization.Api", f1.Project.Name);
        Assert.Equal("964aa0c6-53dd-4906-83d9-36075c888bdd", f1.Project.ApplicationUuid);

        // application
        Assert.Equal("964aa0c6-53dd-4906-83d9-36075c888bdd", f1.Application!.Uuid);
        Assert.Equal("Clarizen.Modernization.Api", f1.Application.Name);

        // component
        Assert.Equal("6bf04787-e184-47f6-83b8-bb5540c1b5a3", f1.Component!.Uuid);
        Assert.Equal("Library", f1.Component.ComponentType);
        Assert.Equal("NODE_PACKAGED_MODULE", f1.Component.LibraryType);
        Assert.Equal("picomatch", f1.Component.GroupId);
        Assert.Equal("picomatch-2.3.1.tgz", f1.Component.ArtifactId);
        Assert.Equal("2.3.1", f1.Component.Version);
        Assert.False(f1.Component.DirectDependency);
        Assert.False(f1.Component.RootLibrary);
        Assert.Equal("Transitive", f1.Component.DependencyType);
        Assert.Equal("https://registry.npmjs.org/picomatch/-/picomatch-2.3.1.tgz", f1.Component.References!.Url);
        Assert.Equal("https://www.npmjs.org/package/picomatch", f1.Component.References.GenericPackageIndex);

        // vulnerability
        Assert.Equal("CVSS_3", f1.Vulnerability!.Type);
        Assert.Equal("2026-03-26T21:20:48Z", f1.Vulnerability.PublishDate);
        Assert.Equal("2026-03-26T22:31:27Z", f1.Vulnerability.ModifiedDate);
        Assert.Single(f1.Vulnerability.VulnerabilityScoring);
        Assert.Equal(7.5, f1.Vulnerability.VulnerabilityScoring[0].Score);
        Assert.Equal("HIGH", f1.Vulnerability.VulnerabilityScoring[0].Severity);
        Assert.Equal("CVSS_3", f1.Vulnerability.VulnerabilityScoring[0].Type);

        // topFix
        Assert.Equal(181892L, f1.TopFix!.Id);
        Assert.Equal("CVE-2026-33671", f1.TopFix.Vulnerability);
        Assert.Equal("UPGRADE_VERSION", f1.TopFix.Type);
        Assert.Equal("WHITESOURCE_EXPERT", f1.TopFix.Origin);
        Assert.Equal("Upgrade to version", f1.TopFix.Message);
        Assert.Equal("2026-03-25T22:07:10Z", f1.TopFix.Date);

        // top-level scalar fields
        Assert.Equal("NO_SHIELD", f1.Effective);
        Assert.Equal("REACHABILITY_UNAVAILABLE", f1.Reachability);
        Assert.Equal("CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:N/I:N/A:H", f1.ScoreMetadataVector);
        Assert.False(f1.Exploitable);
        Assert.False(f1.Malicious);
        Assert.Equal(0, f1.Violations);

        // threatAssessment
        Assert.Equal("NOT_DEFINED", f1.ThreatAssessment!.ExploitCodeMaturity);
        Assert.Equal(0.412, f1.ThreatAssessment.EpssPercentage);

        // dependencyContexts
        Assert.Single(f1.DependencyContexts);
        var ctx = f1.DependencyContexts[0];
        Assert.Equal("TRANSITIVE", ctx.DependencyType);
        Assert.False(ctx.IsDirect);
        Assert.True(ctx.IsTransitive);
        Assert.Single(ctx.DirectRoots);
        Assert.Equal("7799588d-b3fd-49c3-8393-1fd7fd487d20", ctx.DirectRoots[0].RootLibraryUuid);
        Assert.Equal("jest-29.5.14.tgz", ctx.DirectRoots[0].RootLibraryName);
        Assert.Equal("29.5.14", ctx.DirectRoots[0].RootLibraryVersion);

        // second finding — empty topFix, no threatAssessment
        var f2 = result[1];
        Assert.Equal("c67ee91d-acd7-42ff-9c52-88712ef373ed", f2.Uuid);
        Assert.Equal("MEDIUM", f2.Severity);
        Assert.Equal("LIBRARY_REMOVED", f2.Status);
        Assert.NotNull(f2.TopFix);
        Assert.Equal(0L, f2.TopFix!.Id);
        Assert.Null(f2.ThreatAssessment);
        Assert.Empty(f2.DependencyContexts);
        Assert.Equal("2025-07-28T08:30:46Z", f2.FindingInfo!.DetectedAt);
    }

    [Fact]
    public void LibraryGroup_DeserializesFromJson_Correctly()
    {
        const string json = """
            [
              {
                "uuid": "lib-1",
                "name": "lodash",
                "findings": [
                  {
                    "uuid": "finding-1",
                    "name": "CVE-2021-1234",
                    "findingInfo": { "findingStatus": "UNREVIEWED", "status": "ACTIVE" },
                    "component": { "name": "lodash", "version": "4.17.11" },
                    "vulnerability": { "name": "CVE-2021-1234", "severity": "HIGH", "score": 7.5 }
                  }
                ]
              }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<LibraryGroup>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("lib-1", result[0].Uuid);
        Assert.Equal("lodash", result[0].Name);
        Assert.Single(result[0].Findings);
        Assert.Equal("finding-1", result[0].Findings[0].Uuid);
        Assert.Equal("HIGH", result[0].Findings[0].Severity);
        Assert.Equal("CVE-2021-1234", result[0].Findings[0].CveName);
    }

    [Fact]
    public void RootLibraryGroup_DeserializesFromJson_Correctly()
    {
        const string json = """
            [
              {
                "uuid": "root-lib-1",
                "name": "root-lodash",
                "findings": [
                  {
                    "uuid": "finding-1",
                    "name": "CVE-2020-9999",
                    "findingInfo": { "findingStatus": "UNREVIEWED", "status": "ACTIVE" },
                    "component": { "name": "lodash", "version": "4.17.11" },
                    "vulnerability": { "name": "CVE-2020-9999", "severity": "CRITICAL", "score": 9.8 }
                  },
                  {
                    "uuid": "finding-2",
                    "name": "CVE-2021-1234",
                    "findingInfo": { "findingStatus": "RESOLVED", "status": "FIXED" },
                    "component": { "name": "lodash-sub", "version": "1.0.0" },
                    "vulnerability": { "name": "CVE-2021-1234", "severity": "HIGH", "score": 7.5 }
                  }
                ]
              }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<RootLibraryGroup>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("root-lib-1", result[0].Uuid);
        Assert.Equal("root-lodash", result[0].Name);
        Assert.Equal(2, result[0].Findings.Count);
        Assert.Equal("finding-1", result[0].Findings[0].Uuid);
        Assert.Equal("CRITICAL", result[0].Findings[0].Severity);
        Assert.Equal("finding-2", result[0].Findings[1].Uuid);
        Assert.Equal("HIGH", result[0].Findings[1].Severity);
    }

    // --- GetDependencySecurityFindingsAsync ---

    [Fact]
    public async Task GetDependencySecurityFindingsAsync_ReturnsMappedFindingsList()
    {
        var findings = new List<SecurityFinding>
        {
            new SecurityFinding
            {
                Uuid = "f-1", Name = "CVE-2021-1",
                Vulnerability = new SecurityFindingVulnerability { Severity = "HIGH" },
                Component = new SecurityFindingComponent { Name = "lib-a" },
                FindingInfo = new SecurityFindingInfo { Status = "ACTIVE" }
            },
            new SecurityFinding
            {
                Uuid = "f-2", Name = "CVE-2021-2",
                Vulnerability = new SecurityFindingVulnerability { Severity = "MEDIUM" },
                Component = new SecurityFindingComponent { Name = "lib-b" },
                FindingInfo = new SecurityFindingInfo { Status = "IGNORED" }
            }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<SecurityFinding>>(
                ExpectedBasePath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(findings);

        var sut = new MendDependenciesClient(clientMock.Object);
        var result = await sut.GetDependencySecurityFindingsAsync(TestProjectUuid);

        Assert.Equal(2, result.Count);
        Assert.Equal("f-1", result[0].Uuid);
        Assert.Equal("HIGH", result[0].Severity);
        Assert.Equal("CVE-2021-1", result[0].CveName);
        Assert.Equal("lib-a", result[0].LibraryName);
        Assert.Equal("ACTIVE", result[0].Status);
        Assert.Equal("f-2", result[1].Uuid);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<SecurityFinding>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<SecurityFinding>));

        var sut = new MendDependenciesClient(clientMock.Object);
        var result = await sut.GetDependencySecurityFindingsAsync(TestProjectUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsAsync_WithPageSize_PassesPageSizeToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<SecurityFinding>>(
                ExpectedBasePath, 25, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SecurityFinding>());

        var sut = new MendDependenciesClient(clientMock.Object);
        await sut.GetDependencySecurityFindingsAsync(TestProjectUuid, pageSize: 25);

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<SecurityFinding>>(
            ExpectedBasePath, 25, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsAsync_WithCursor_PassesCursorToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<SecurityFinding>>(
                ExpectedBasePath, null, "my-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SecurityFinding>());

        var sut = new MendDependenciesClient(clientMock.Object);
        await sut.GetDependencySecurityFindingsAsync(TestProjectUuid, cursor: "my-cursor");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<SecurityFinding>>(
            ExpectedBasePath, null, "my-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<SecurityFinding>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedBasePath));

        var sut = new MendDependenciesClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetDependencySecurityFindingsAsync(TestProjectUuid));
        Assert.Equal(ExpectedBasePath, ex.EndpointPath);
    }

    // --- GetDependencySecurityFindingsByLibraryAsync ---

    [Fact]
    public async Task GetDependencySecurityFindingsByLibraryAsync_ReturnsGroupedByLibrary()
    {
        var expectedPath = $"{ExpectedBasePath}/groupBy/library";
        var groups = new List<LibraryGroup>
        {
            new LibraryGroup
            {
                Uuid = "lib-1",
                Name = "lodash",
                Findings = new List<SecurityFinding>
                {
                    new SecurityFinding
                    {
                        Uuid = "f-1", Name = "CVE-2021-1",
                        Vulnerability = new SecurityFindingVulnerability { Severity = "HIGH" },
                        Component = new SecurityFindingComponent { Name = "lodash" },
                        FindingInfo = new SecurityFindingInfo { Status = "ACTIVE" }
                    }
                }
            }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<LibraryGroup>>(
                expectedPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);

        var sut = new MendDependenciesClient(clientMock.Object);
        var result = await sut.GetDependencySecurityFindingsByLibraryAsync(TestProjectUuid);

        Assert.Single(result);
        Assert.Equal("lib-1", result[0].Uuid);
        Assert.Equal("lodash", result[0].Name);
        Assert.Single(result[0].Findings);
        Assert.Equal("f-1", result[0].Findings[0].Uuid);
        Assert.Equal("HIGH", result[0].Findings[0].Severity);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsByLibraryAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<LibraryGroup>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<LibraryGroup>));

        var sut = new MendDependenciesClient(clientMock.Object);
        var result = await sut.GetDependencySecurityFindingsByLibraryAsync(TestProjectUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsByLibraryAsync_WithPagination_PassesParamsToClient()
    {
        var expectedPath = $"{ExpectedBasePath}/groupBy/library";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<LibraryGroup>>(
                expectedPath, 50, "next-page", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<LibraryGroup>());

        var sut = new MendDependenciesClient(clientMock.Object);
        await sut.GetDependencySecurityFindingsByLibraryAsync(TestProjectUuid, pageSize: 50, cursor: "next-page");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<LibraryGroup>>(
            expectedPath, 50, "next-page", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsByLibraryAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedBasePath}/groupBy/library";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<LibraryGroup>>(
                expectedPath, It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendDependenciesClient(clientMock.Object);

        await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetDependencySecurityFindingsByLibraryAsync(TestProjectUuid));
    }

    // --- GetDependencySecurityFindingsByRootLibraryAsync ---

    [Fact]
    public async Task GetDependencySecurityFindingsByRootLibraryAsync_ReturnsGroupedByRootLibrary()
    {
        var expectedPath = $"{ExpectedBasePath}/groupBy/rootLibrary";
        var groups = new List<RootLibraryGroup>
        {
            new RootLibraryGroup
            {
                Uuid = "root-1",
                Name = "root-lodash",
                Findings = new List<SecurityFinding>
                {
                    new SecurityFinding
                    {
                        Uuid = "f-1", Name = "CVE-2020-1",
                        Vulnerability = new SecurityFindingVulnerability { Severity = "CRITICAL" },
                        Component = new SecurityFindingComponent { Name = "lodash" },
                        FindingInfo = new SecurityFindingInfo { Status = "ACTIVE" }
                    },
                    new SecurityFinding
                    {
                        Uuid = "f-2", Name = "CVE-2021-2",
                        Vulnerability = new SecurityFindingVulnerability { Severity = "HIGH" },
                        Component = new SecurityFindingComponent { Name = "lodash-sub" },
                        FindingInfo = new SecurityFindingInfo { Status = "FIXED" }
                    }
                }
            }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<RootLibraryGroup>>(
                expectedPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);

        var sut = new MendDependenciesClient(clientMock.Object);
        var result = await sut.GetDependencySecurityFindingsByRootLibraryAsync(TestProjectUuid);

        Assert.Single(result);
        Assert.Equal("root-1", result[0].Uuid);
        Assert.Equal("root-lodash", result[0].Name);
        Assert.Equal(2, result[0].Findings.Count);
        Assert.Equal("f-1", result[0].Findings[0].Uuid);
        Assert.Equal("CRITICAL", result[0].Findings[0].Severity);
        Assert.Equal("f-2", result[0].Findings[1].Uuid);
        Assert.Equal("FIXED", result[0].Findings[1].Status);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsByRootLibraryAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<RootLibraryGroup>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<RootLibraryGroup>));

        var sut = new MendDependenciesClient(clientMock.Object);
        var result = await sut.GetDependencySecurityFindingsByRootLibraryAsync(TestProjectUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsByRootLibraryAsync_WithPagination_PassesParamsToClient()
    {
        var expectedPath = $"{ExpectedBasePath}/groupBy/rootLibrary";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<RootLibraryGroup>>(
                expectedPath, 10, "cursor-abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RootLibraryGroup>());

        var sut = new MendDependenciesClient(clientMock.Object);
        await sut.GetDependencySecurityFindingsByRootLibraryAsync(TestProjectUuid, pageSize: 10, cursor: "cursor-abc");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<RootLibraryGroup>>(
            expectedPath, 10, "cursor-abc", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDependencySecurityFindingsByRootLibraryAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedBasePath}/groupBy/rootLibrary";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<RootLibraryGroup>>(
                expectedPath, It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendDependenciesClient(clientMock.Object);

        await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetDependencySecurityFindingsByRootLibraryAsync(TestProjectUuid));
    }

    // --- UpdateDependencySecurityFindingAsync ---

    [Fact]
    public async Task UpdateDependencySecurityFindingAsync_IssuesPutToCorrectPath()
    {
        var expectedPath = $"{ExpectedBasePath}/rootLibrary/{TestRootLibraryUuid}";
        var request = new UpdateSecurityFindingRequest { Status = "IGNORED", Comments = "Not applicable" };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PutAsync<object>(expectedPath, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(object));

        var sut = new MendDependenciesClient(clientMock.Object);
        await sut.UpdateDependencySecurityFindingAsync(TestProjectUuid, TestRootLibraryUuid, request);

        clientMock.Verify(c => c.PutAsync<object>(
            expectedPath, It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDependencySecurityFindingAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedBasePath}/rootLibrary/{TestRootLibraryUuid}";
        var request = new UpdateSecurityFindingRequest { Status = "FIXED" };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PutAsync<object>(expectedPath, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendDependenciesClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.UpdateDependencySecurityFindingAsync(TestProjectUuid, TestRootLibraryUuid, request));
        Assert.Equal(expectedPath, ex.EndpointPath);
    }
}
