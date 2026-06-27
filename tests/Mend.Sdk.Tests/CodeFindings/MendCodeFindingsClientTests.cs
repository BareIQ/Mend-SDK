using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.CodeFindings;
using Mend.Sdk.CodeFindings.Models;
using Mend.Sdk.Exceptions;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.CodeFindings;

public sealed class MendCodeFindingsClientTests
{
    private const string TestProjectUuid = "test-project-uuid";
    private const string TestScanUuid = "test-scan-uuid";
    private const string TestFindingUuid = "test-finding-uuid";

    private static readonly string ExpectedCodeFindingsPath =
        $"/api/v3.0/projects/{TestProjectUuid}/code/findings";

    private static readonly string ExpectedScanCodeFindingsPath =
        $"/api/v3.0/projects/{TestProjectUuid}/scans/{TestScanUuid}/code/findings";

    // --- Deserialization ---

    [Fact]
    public void CodeFinding_ListDeserializesFromJson_Correctly()
    {
        const string json = """
            [
              {
                "uuid": "finding-1",
                "severity": "HIGH",
                "title": "SQL Injection",
                "filePath": "src/DataAccess.cs",
                "state": "OPEN"
              },
              {
                "uuid": "finding-2",
                "severity": "MEDIUM",
                "title": "XSS Vulnerability",
                "filePath": "src/Controllers/HomeController.cs",
                "state": "SUPPRESSED"
              }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<CodeFinding>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("finding-1", result[0].Uuid);
        Assert.Equal("HIGH", result[0].Severity);
        Assert.Equal("SQL Injection", result[0].Title);
        Assert.Equal("src/DataAccess.cs", result[0].FilePath);
        Assert.Equal("OPEN", result[0].State);
        Assert.Equal("finding-2", result[1].Uuid);
        Assert.Equal("MEDIUM", result[1].Severity);
        Assert.Equal("XSS Vulnerability", result[1].Title);
        Assert.Equal("SUPPRESSED", result[1].State);
    }

    [Fact]
    public void CodeFinding_SingleDeserializesFromJson_Correctly()
    {
        const string json = """
            {
              "uuid": "finding-1",
              "severity": "CRITICAL",
              "title": "Command Injection",
              "filePath": "src/Services/ShellService.cs",
              "state": "OPEN"
            }
            """;

        var result = JsonSerializer.Deserialize<CodeFinding>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal("finding-1", result!.Uuid);
        Assert.Equal("CRITICAL", result.Severity);
        Assert.Equal("Command Injection", result.Title);
        Assert.Equal("src/Services/ShellService.cs", result.FilePath);
        Assert.Equal("OPEN", result.State);
    }

    // --- GetCodeFindingsAsync ---

    [Fact]
    public async Task GetCodeFindingsAsync_ReturnsMappedFindingsList()
    {
        var findings = new List<CodeFinding>
        {
            new CodeFinding { Uuid = "f-1", Severity = "HIGH", Title = "SQL Injection", FilePath = "src/Db.cs", State = "OPEN" },
            new CodeFinding { Uuid = "f-2", Severity = "MEDIUM", Title = "XSS", FilePath = "src/Web.cs", State = "SUPPRESSED" }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
                ExpectedCodeFindingsPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(findings);

        var sut = new MendCodeFindingsClient(clientMock.Object);
        var result = await sut.GetCodeFindingsAsync(TestProjectUuid);

        Assert.Equal(2, result.Count);
        Assert.Equal("f-1", result[0].Uuid);
        Assert.Equal("HIGH", result[0].Severity);
        Assert.Equal("SQL Injection", result[0].Title);
        Assert.Equal("src/Db.cs", result[0].FilePath);
        Assert.Equal("OPEN", result[0].State);
        Assert.Equal("f-2", result[1].Uuid);
    }

    [Fact]
    public async Task GetCodeFindingsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<CodeFinding>));

        var sut = new MendCodeFindingsClient(clientMock.Object);
        var result = await sut.GetCodeFindingsAsync(TestProjectUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCodeFindingsAsync_WithPageSize_PassesPageSizeToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
                ExpectedCodeFindingsPath, 25, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CodeFinding>());

        var sut = new MendCodeFindingsClient(clientMock.Object);
        await sut.GetCodeFindingsAsync(TestProjectUuid, pageSize: 25);

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
            ExpectedCodeFindingsPath, 25, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCodeFindingsAsync_WithCursor_PassesCursorToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
                ExpectedCodeFindingsPath, null, "my-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CodeFinding>());

        var sut = new MendCodeFindingsClient(clientMock.Object);
        await sut.GetCodeFindingsAsync(TestProjectUuid, cursor: "my-cursor");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
            ExpectedCodeFindingsPath, null, "my-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCodeFindingsAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedCodeFindingsPath));

        var sut = new MendCodeFindingsClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetCodeFindingsAsync(TestProjectUuid));
        Assert.Equal(ExpectedCodeFindingsPath, ex.EndpointPath);
    }

    // --- GetCodeFindingAsync ---

    [Fact]
    public async Task GetCodeFindingAsync_ReturnsSingleFinding()
    {
        var expectedPath = $"{ExpectedCodeFindingsPath}/{TestFindingUuid}";
        var finding = new CodeFinding
        {
            Uuid = TestFindingUuid,
            Severity = "CRITICAL",
            Title = "Command Injection",
            FilePath = "src/Services/ShellService.cs",
            State = "OPEN"
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<CodeFinding>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(finding);

        var sut = new MendCodeFindingsClient(clientMock.Object);
        var result = await sut.GetCodeFindingAsync(TestProjectUuid, TestFindingUuid);

        Assert.NotNull(result);
        Assert.Equal(TestFindingUuid, result!.Uuid);
        Assert.Equal("CRITICAL", result.Severity);
        Assert.Equal("Command Injection", result.Title);
        Assert.Equal("src/Services/ShellService.cs", result.FilePath);
        Assert.Equal("OPEN", result.State);
    }

    [Fact]
    public async Task GetCodeFindingAsync_WhenNotFound_ReturnsNull()
    {
        var expectedPath = $"{ExpectedCodeFindingsPath}/{TestFindingUuid}";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<CodeFinding>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CodeFinding));

        var sut = new MendCodeFindingsClient(clientMock.Object);
        var result = await sut.GetCodeFindingAsync(TestProjectUuid, TestFindingUuid);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCodeFindingAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedCodeFindingsPath}/{TestFindingUuid}";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<CodeFinding>(expectedPath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendCodeFindingsClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetCodeFindingAsync(TestProjectUuid, TestFindingUuid));
        Assert.Equal(expectedPath, ex.EndpointPath);
    }

    // --- GetScanCodeFindingsAsync ---

    [Fact]
    public async Task GetScanCodeFindingsAsync_ReturnsFindingsScopedToScan()
    {
        var findings = new List<CodeFinding>
        {
            new CodeFinding { Uuid = "sf-1", Severity = "HIGH", Title = "Path Traversal", FilePath = "src/FileUtils.cs", State = "OPEN" }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
                ExpectedScanCodeFindingsPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(findings);

        var sut = new MendCodeFindingsClient(clientMock.Object);
        var result = await sut.GetScanCodeFindingsAsync(TestProjectUuid, TestScanUuid);

        Assert.Single(result);
        Assert.Equal("sf-1", result[0].Uuid);
        Assert.Equal("HIGH", result[0].Severity);
        Assert.Equal("Path Traversal", result[0].Title);
        Assert.Equal("src/FileUtils.cs", result[0].FilePath);
        Assert.Equal("OPEN", result[0].State);
    }

    [Fact]
    public async Task GetScanCodeFindingsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<CodeFinding>));

        var sut = new MendCodeFindingsClient(clientMock.Object);
        var result = await sut.GetScanCodeFindingsAsync(TestProjectUuid, TestScanUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetScanCodeFindingsAsync_WithPagination_PassesParamsToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
                ExpectedScanCodeFindingsPath, 50, "next-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CodeFinding>());

        var sut = new MendCodeFindingsClient(clientMock.Object);
        await sut.GetScanCodeFindingsAsync(TestProjectUuid, TestScanUuid, pageSize: 50, cursor: "next-cursor");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
            ExpectedScanCodeFindingsPath, 50, "next-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetScanCodeFindingsAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<CodeFinding>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedScanCodeFindingsPath));

        var sut = new MendCodeFindingsClient(clientMock.Object);

        await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetScanCodeFindingsAsync(TestProjectUuid, TestScanUuid));
    }

    // --- GetScanCodeFindingAsync ---

    [Fact]
    public async Task GetScanCodeFindingAsync_ReturnsSingleFindingFromScan()
    {
        var expectedPath = $"{ExpectedScanCodeFindingsPath}/{TestFindingUuid}";
        var finding = new CodeFinding
        {
            Uuid = TestFindingUuid,
            Severity = "HIGH",
            Title = "Path Traversal",
            FilePath = "src/FileUtils.cs",
            State = "OPEN"
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<CodeFinding>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(finding);

        var sut = new MendCodeFindingsClient(clientMock.Object);
        var result = await sut.GetScanCodeFindingAsync(TestProjectUuid, TestScanUuid, TestFindingUuid);

        Assert.NotNull(result);
        Assert.Equal(TestFindingUuid, result!.Uuid);
        Assert.Equal("HIGH", result.Severity);
        Assert.Equal("Path Traversal", result.Title);
    }

    [Fact]
    public async Task GetScanCodeFindingAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedScanCodeFindingsPath}/{TestFindingUuid}";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<CodeFinding>(expectedPath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendCodeFindingsClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetScanCodeFindingAsync(TestProjectUuid, TestScanUuid, TestFindingUuid));
        Assert.Equal(expectedPath, ex.EndpointPath);
    }
}
