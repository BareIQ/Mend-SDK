using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Options;
using Mend.Sdk.Reports;
using Mend.Sdk.Reports.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.Reports;

public sealed class MendReportsClientTests
{
    private const string TestOrgUuid = "test-org-uuid";
    private const string TestReportUuid = "test-report-uuid";
    private static readonly string ExpectedBasePath = $"/api/v3.0/orgs/{TestOrgUuid}/reports";

    private static IOptions<MendOptions> CreateOptions() =>
        Microsoft.Extensions.Options.Options.Create(new MendOptions { OrgUuid = TestOrgUuid });

    // --- Deserialization ---

    [Fact]
    public void ReportStatus_DeserializesStateComplete_ToEnum()
    {
        const string json = """{ "uuid": "abc-123", "status": "Complete", "createdAt": "2026-01-01" }""";

        var result = JsonSerializer.Deserialize<ReportStatus>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(ReportState.Complete, result!.State);
        Assert.Equal("abc-123", result.Uuid);
    }

    [Fact]
    public void ReportStatus_DeserializesStatePending_ToEnum()
    {
        const string json = """{ "uuid": "pending-uuid", "status": "Pending" }""";

        var result = JsonSerializer.Deserialize<ReportStatus>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(ReportState.Pending, result!.State);
    }

    [Fact]
    public void ReportStatus_DeserializesStateFailed_ToEnum()
    {
        const string json = """{ "uuid": "failed-uuid", "status": "Failed" }""";

        var result = JsonSerializer.Deserialize<ReportStatus>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(ReportState.Failed, result!.State);
    }

    // --- GetReportsAsync ---

    [Fact]
    public async Task GetReportsAsync_ReturnsMappedReportList()
    {
        var statuses = new List<ReportStatus>
        {
            new ReportStatus { Uuid = TestReportUuid, State = ReportState.Complete }
        };
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<ReportStatus>>(
                ExpectedBasePath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statuses);

        var sut = new MendReportsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetReportsAsync();

        Assert.Single(result);
        Assert.Equal(TestReportUuid, result[0].Uuid);
        Assert.Equal(ReportState.Complete, result[0].State);
    }

    [Fact]
    public async Task GetReportsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<ReportStatus>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<ReportStatus>));

        var sut = new MendReportsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetReportsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReportsAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<ReportStatus>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedBasePath));

        var sut = new MendReportsClient(clientMock.Object, CreateOptions());
        var ex = await Assert.ThrowsAsync<MendAuthException>(() => sut.GetReportsAsync());

        Assert.Equal(ExpectedBasePath, ex.EndpointPath);
    }

    // --- GetReportStatusAsync ---

    [Fact]
    public async Task GetReportStatusAsync_GetsFromCorrectPath_AndReturnsStatus()
    {
        var expectedPath = $"{ExpectedBasePath}/{TestReportUuid}";
        var status = new ReportStatus { Uuid = TestReportUuid, State = ReportState.Complete };
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ReportStatus>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        var sut = new MendReportsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetReportStatusAsync(TestReportUuid);

        Assert.NotNull(result);
        Assert.Equal(ReportState.Complete, result!.State);
        clientMock.Verify(c => c.GetAsync<ReportStatus>(expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReportStatusAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedBasePath}/{TestReportUuid}";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ReportStatus>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendReportsClient(clientMock.Object, CreateOptions());
        var ex = await Assert.ThrowsAsync<MendAuthException>(() => sut.GetReportStatusAsync(TestReportUuid));

        Assert.Equal(expectedPath, ex.EndpointPath);
    }

    // --- DownloadReportAsync ---

    [Fact]
    public async Task DownloadReportAsync_ReturnsReadableStream()
    {
        var expectedPath = $"{ExpectedBasePath}/download/{TestReportUuid}";
        var content = new MemoryStream(Encoding.UTF8.GetBytes("report-content"));
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetStreamAsync(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        var sut = new MendReportsClient(clientMock.Object, CreateOptions());
        var result = await sut.DownloadReportAsync(TestReportUuid);

        Assert.NotNull(result);
        Assert.True(result.CanRead);
        clientMock.Verify(c => c.GetStreamAsync(expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DownloadReportAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedBasePath}/download/{TestReportUuid}";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendReportsClient(clientMock.Object, CreateOptions());
        var ex = await Assert.ThrowsAsync<MendAuthException>(() => sut.DownloadReportAsync(TestReportUuid));

        Assert.Equal(expectedPath, ex.EndpointPath);
    }

    // --- DeleteReportAsync ---

    [Fact]
    public async Task DeleteReportAsync_DeletesFromCorrectPath()
    {
        var expectedPath = $"{ExpectedBasePath}/{TestReportUuid}";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.DeleteAsync(expectedPath, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new MendReportsClient(clientMock.Object, CreateOptions());
        await sut.DeleteReportAsync(TestReportUuid);

        clientMock.Verify(c => c.DeleteAsync(expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReportAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedBasePath}/{TestReportUuid}";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendReportsClient(clientMock.Object, CreateOptions());
        var ex = await Assert.ThrowsAsync<MendAuthException>(() => sut.DeleteReportAsync(TestReportUuid));

        Assert.Equal(expectedPath, ex.EndpointPath);
    }
}
