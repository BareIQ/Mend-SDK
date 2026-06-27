using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Options;
using Mend.Sdk.Scans;
using Mend.Sdk.Scans.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.Scans;

public sealed class MendScansClientTests
{
    private const string TestOrgUuid = "test-org-uuid";
    private const string TestProjectUuid = "test-project-uuid";
    private const string TestScanUuid = "test-scan-uuid";
    private static readonly string ExpectedScansPath =
        $"/api/v3.0/orgs/{TestOrgUuid}/projects/{TestProjectUuid}/scans";
    private static readonly string ExpectedScanPath =
        $"{ExpectedScansPath}/{TestScanUuid}";

    private static IOptions<MendOptions> CreateOptions() =>
        Microsoft.Extensions.Options.Options.Create(new MendOptions { OrgUuid = TestOrgUuid });

    // --- Deserialization ---

    [Fact]
    public void Scan_DeserializesFromJson_Correctly()
    {
        const string json = """
            [
              { "uuid": "scan-1", "status": "DONE", "createdAt": "2024-01-01T00:00:00Z" },
              { "uuid": "scan-2", "status": "IN_PROGRESS", "createdAt": "2024-01-02T00:00:00Z" }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<Scan>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("scan-1", result[0].Uuid);
        Assert.Equal("DONE", result[0].Status);
        Assert.Equal("2024-01-01T00:00:00Z", result[0].CreatedAt);
        Assert.Equal("scan-2", result[1].Uuid);
        Assert.Equal("IN_PROGRESS", result[1].Status);
    }

    [Fact]
    public void ScanSummary_DeserializesFromJson_Correctly()
    {
        const string json = """
            {
              "highSeverityCount": 5,
              "mediumSeverityCount": 10,
              "lowSeverityCount": 20
            }
            """;

        var result = JsonSerializer.Deserialize<ScanSummary>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(5, result!.HighSeverityCount);
        Assert.Equal(10, result.MediumSeverityCount);
        Assert.Equal(20, result.LowSeverityCount);
    }

    // --- GetScansAsync ---

    [Fact]
    public async Task GetScansAsync_ReturnsMappedScanList()
    {
        var scans = new List<Scan>
        {
            new Scan { Uuid = "scan-1", Status = "DONE", CreatedAt = "2024-01-01T00:00:00Z" },
            new Scan { Uuid = "scan-2", Status = "IN_PROGRESS", CreatedAt = "2024-01-02T00:00:00Z" }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Scan>>(
                ExpectedScansPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scans);

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        var result = await sut.GetScansAsync(TestProjectUuid);

        Assert.Equal(2, result.Count);
        Assert.Equal("scan-1", result[0].Uuid);
        Assert.Equal("DONE", result[0].Status);
        Assert.Equal("2024-01-01T00:00:00Z", result[0].CreatedAt);
        Assert.Equal("scan-2", result[1].Uuid);
    }

    [Fact]
    public async Task GetScansAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Scan>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<Scan>));

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        var result = await sut.GetScansAsync(TestProjectUuid);

        Assert.Empty(result);
    }

    // --- Pagination ---

    [Fact]
    public async Task GetScansAsync_WithPageSize_PassesPageSizeToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Scan>>(
                ExpectedScansPath, 25, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Scan>());

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        await sut.GetScansAsync(TestProjectUuid, pageSize: 25);

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Scan>>(
            ExpectedScansPath, 25, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetScansAsync_WithCursor_PassesCursorToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Scan>>(
                ExpectedScansPath, null, "my-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Scan>());

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        await sut.GetScansAsync(TestProjectUuid, cursor: "my-cursor");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Scan>>(
            ExpectedScansPath, null, "my-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetScansAsync_WithPageSizeAndCursor_PassesBothToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Scan>>(
                ExpectedScansPath, 50, "next-page", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Scan>());

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        await sut.GetScansAsync(TestProjectUuid, pageSize: 50, cursor: "next-page");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Scan>>(
            ExpectedScansPath, 50, "next-page", It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- 401 propagation on list ---

    [Fact]
    public async Task GetScansAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Scan>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedScansPath));

        var sut = new MendScansClient(clientMock.Object, CreateOptions());

        var ex = await Assert.ThrowsAsync<MendAuthException>(() => sut.GetScansAsync(TestProjectUuid));
        Assert.Equal(ExpectedScansPath, ex.EndpointPath);
    }

    // --- GetScanAsync ---

    [Fact]
    public async Task GetScanAsync_ReturnsScanDetail()
    {
        var detail = new ScanDetail
        {
            Uuid = TestScanUuid,
            Status = "DONE",
            CreatedAt = "2024-01-01T00:00:00Z",
            ProjectUuid = TestProjectUuid
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ScanDetail>(ExpectedScanPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        var result = await sut.GetScanAsync(TestProjectUuid, TestScanUuid);

        Assert.NotNull(result);
        Assert.Equal(TestScanUuid, result!.Uuid);
        Assert.Equal("DONE", result.Status);
        Assert.Equal(TestProjectUuid, result.ProjectUuid);
    }

    [Fact]
    public async Task GetScanAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ScanDetail>(ExpectedScanPath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedScanPath));

        var sut = new MendScansClient(clientMock.Object, CreateOptions());

        await Assert.ThrowsAsync<MendAuthException>(() => sut.GetScanAsync(TestProjectUuid, TestScanUuid));
    }

    // --- GetScanSummaryAsync ---

    [Fact]
    public async Task GetScanSummaryAsync_ReturnsSummaryWithSeverityCounts()
    {
        var summary = new ScanSummary
        {
            HighSeverityCount = 5,
            MediumSeverityCount = 10,
            LowSeverityCount = 20
        };
        var expectedPath = $"{ExpectedScanPath}/summary";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ScanSummary>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        var result = await sut.GetScanSummaryAsync(TestProjectUuid, TestScanUuid);

        Assert.NotNull(result);
        Assert.Equal(5, result!.HighSeverityCount);
        Assert.Equal(10, result.MediumSeverityCount);
        Assert.Equal(20, result.LowSeverityCount);
    }

    [Fact]
    public async Task GetScanSummaryAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedScanPath}/summary";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ScanSummary>(expectedPath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendScansClient(clientMock.Object, CreateOptions());

        await Assert.ThrowsAsync<MendAuthException>(() => sut.GetScanSummaryAsync(TestProjectUuid, TestScanUuid));
    }

    // --- GetScanTagsAsync ---

    [Fact]
    public async Task GetScanTagsAsync_ReturnsTags()
    {
        var tags = new List<ScanTag>
        {
            new ScanTag { Key = "env", Value = "production" },
            new ScanTag { Key = "team", Value = "security" }
        };
        var expectedPath = $"{ExpectedScanPath}/tags";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<ScanTag>>(
                expectedPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        var result = await sut.GetScanTagsAsync(TestProjectUuid, TestScanUuid);

        Assert.Equal(2, result.Count);
        Assert.Equal("env", result[0].Key);
        Assert.Equal("production", result[0].Value);
        Assert.Equal("team", result[1].Key);
        Assert.Equal("security", result[1].Value);
    }

    [Fact]
    public async Task GetScanTagsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<ScanTag>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<ScanTag>));

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        var result = await sut.GetScanTagsAsync(TestProjectUuid, TestScanUuid);

        Assert.Empty(result);
    }

    // --- GetScanSbomLogsAsync ---

    [Fact]
    public async Task GetScanSbomLogsAsync_ReturnsSbomLogs()
    {
        var logs = new List<SbomLog>
        {
            new SbomLog { Message = "Import started", Level = "INFO", Timestamp = "2024-01-01T00:00:00Z" },
            new SbomLog { Message = "Component parsed", Level = "DEBUG", Timestamp = "2024-01-01T00:00:01Z" }
        };
        var expectedPath = $"/api/v3.0/projects/{TestProjectUuid}/scans/{TestScanUuid}/dependencies/SBOM/logs";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<SbomLog>>(
                expectedPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        var result = await sut.GetScanSbomLogsAsync(TestProjectUuid, TestScanUuid);

        Assert.Equal(2, result.Count);
        Assert.Equal("Import started", result[0].Message);
        Assert.Equal("INFO", result[0].Level);
        Assert.Equal("2024-01-01T00:00:00Z", result[0].Timestamp);
    }

    [Fact]
    public async Task GetScanSbomLogsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<SbomLog>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<SbomLog>));

        var sut = new MendScansClient(clientMock.Object, CreateOptions());
        var result = await sut.GetScanSbomLogsAsync(TestProjectUuid, TestScanUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetScanSbomLogsAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"/api/v3.0/projects/{TestProjectUuid}/scans/{TestScanUuid}/dependencies/SBOM/logs";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<SbomLog>>(
                expectedPath, null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendScansClient(clientMock.Object, CreateOptions());

        await Assert.ThrowsAsync<MendAuthException>(() => sut.GetScanSbomLogsAsync(TestProjectUuid, TestScanUuid));
    }
}
