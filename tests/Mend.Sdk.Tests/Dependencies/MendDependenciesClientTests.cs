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
                "uuid": "finding-1",
                "name": "CVE-2021-1234",
                "type": "SECURITY_VULNERABILITY",
                "findingInfo": { "findingStatus": "UNREVIEWED", "status": "ACTIVE" },
                "component": { "name": "lodash", "version": "4.17.11" },
                "vulnerability": { "name": "CVE-2021-1234", "severity": "HIGH", "score": 7.5 }
              },
              {
                "uuid": "finding-2",
                "name": "CVE-2021-5678",
                "type": "SECURITY_VULNERABILITY",
                "findingInfo": { "findingStatus": "IGNORED", "status": "IGNORED" },
                "component": { "name": "express", "version": "4.18.0" },
                "vulnerability": { "name": "CVE-2021-5678", "severity": "MEDIUM", "score": 5.3 }
              }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<SecurityFinding>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("finding-1", result[0].Uuid);
        Assert.Equal("HIGH", result[0].Severity);
        Assert.Equal("CVE-2021-1234", result[0].CveName);
        Assert.Equal("lodash", result[0].LibraryName);
        Assert.Equal("ACTIVE", result[0].Status);
        Assert.Equal("finding-2", result[1].Uuid);
        Assert.Equal("MEDIUM", result[1].Severity);
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
