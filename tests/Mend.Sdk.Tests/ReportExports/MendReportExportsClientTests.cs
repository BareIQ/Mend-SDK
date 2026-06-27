using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Exceptions;
using Mend.Sdk.ReportExports;
using Mend.Sdk.ReportExports.Models;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.ReportExports;

public sealed class MendReportExportsClientTests
{
    private const string TestProjectUuid = "test-project-uuid";
    private const string TestApplicationUuid = "test-application-uuid";
    private const string TestOrgUuid = "test-org-uuid";
    private const string TestReportUuid = "test-report-uuid";

    // --- Deserialization ---

    [Fact]
    public void ReportJob_DeserializesFromJson_Correctly()
    {
        const string json = """{ "uuid": "report-abc-123" }""";

        var result = JsonSerializer.Deserialize<ReportJob>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal("report-abc-123", result!.ReportUuid);
    }

    // --- ExportProjectSbomReportAsync ---

    [Fact]
    public async Task ExportProjectSbomReportAsync_PostsToCorrectPathAndReturnsReportUuid()
    {
        var expectedPath = $"/api/v3.0/projects/{TestProjectUuid}/dependencies/reports/SBOM";
        var reportJob = new ReportJob { ReportUuid = TestReportUuid };
        var clientMock = BuildMock(expectedPath, reportJob);

        var sut = new MendReportExportsClient(clientMock.Object);
        var result = await sut.ExportProjectSbomReportAsync(TestProjectUuid);

        Assert.NotNull(result);
        Assert.Equal(TestReportUuid, result!.ReportUuid);
        VerifyPost(clientMock, expectedPath);
    }

    // --- ExportProjectDueDiligenceReportAsync ---

    [Fact]
    public async Task ExportProjectDueDiligenceReportAsync_PostsToCorrectPathAndReturnsReportUuid()
    {
        var expectedPath = $"/api/v3.0/projects/{TestProjectUuid}/dependencies/reports/dueDiligence";
        var reportJob = new ReportJob { ReportUuid = TestReportUuid };
        var clientMock = BuildMock(expectedPath, reportJob);

        var sut = new MendReportExportsClient(clientMock.Object);
        var result = await sut.ExportProjectDueDiligenceReportAsync(TestProjectUuid);

        Assert.NotNull(result);
        Assert.Equal(TestReportUuid, result!.ReportUuid);
        VerifyPost(clientMock, expectedPath);
    }

    // --- ExportProjectFindingsReportAsync ---

    [Fact]
    public async Task ExportProjectFindingsReportAsync_PostsToCorrectPathAndReturnsReportUuid()
    {
        var expectedPath = $"/api/v3.0/projects/{TestProjectUuid}/code/reports/findings";
        var reportJob = new ReportJob { ReportUuid = TestReportUuid };
        var clientMock = BuildMock(expectedPath, reportJob);

        var sut = new MendReportExportsClient(clientMock.Object);
        var result = await sut.ExportProjectFindingsReportAsync(TestProjectUuid);

        Assert.NotNull(result);
        Assert.Equal(TestReportUuid, result!.ReportUuid);
        VerifyPost(clientMock, expectedPath);
    }

    // --- ExportProjectComplianceReportAsync ---

    [Fact]
    public async Task ExportProjectComplianceReportAsync_PostsToCorrectPathAndReturnsReportUuid()
    {
        var expectedPath = $"/api/v3.0/projects/{TestProjectUuid}/code/reports/compliance";
        var reportJob = new ReportJob { ReportUuid = TestReportUuid };
        var clientMock = BuildMock(expectedPath, reportJob);

        var sut = new MendReportExportsClient(clientMock.Object);
        var result = await sut.ExportProjectComplianceReportAsync(TestProjectUuid);

        Assert.NotNull(result);
        Assert.Equal(TestReportUuid, result!.ReportUuid);
        VerifyPost(clientMock, expectedPath);
    }

    // --- ExportApplicationSbomReportAsync ---

    [Fact]
    public async Task ExportApplicationSbomReportAsync_PostsToCorrectPathAndReturnsReportUuid()
    {
        var expectedPath = $"/api/v3.0/applications/{TestApplicationUuid}/dependencies/reports/SBOM";
        var reportJob = new ReportJob { ReportUuid = TestReportUuid };
        var clientMock = BuildMock(expectedPath, reportJob);

        var sut = new MendReportExportsClient(clientMock.Object);
        var result = await sut.ExportApplicationSbomReportAsync(TestApplicationUuid);

        Assert.NotNull(result);
        Assert.Equal(TestReportUuid, result!.ReportUuid);
        VerifyPost(clientMock, expectedPath);
    }

    // --- ExportApplicationDueDiligenceReportAsync ---

    [Fact]
    public async Task ExportApplicationDueDiligenceReportAsync_PostsToCorrectPathAndReturnsReportUuid()
    {
        var expectedPath = $"/api/v3.0/applications/{TestApplicationUuid}/dependencies/reports/dueDiligence";
        var reportJob = new ReportJob { ReportUuid = TestReportUuid };
        var clientMock = BuildMock(expectedPath, reportJob);

        var sut = new MendReportExportsClient(clientMock.Object);
        var result = await sut.ExportApplicationDueDiligenceReportAsync(TestApplicationUuid);

        Assert.NotNull(result);
        Assert.Equal(TestReportUuid, result!.ReportUuid);
        VerifyPost(clientMock, expectedPath);
    }

    // --- ExportOrgInventoryReportAsync ---

    [Fact]
    public async Task ExportOrgInventoryReportAsync_PostsToCorrectPathAndReturnsReportUuid()
    {
        var expectedPath = $"/api/v3.0/orgs/{TestOrgUuid}/dependencies/reports/inventory";
        var reportJob = new ReportJob { ReportUuid = TestReportUuid };
        var clientMock = BuildMock(expectedPath, reportJob);

        var sut = new MendReportExportsClient(clientMock.Object);
        var result = await sut.ExportOrgInventoryReportAsync(TestOrgUuid);

        Assert.NotNull(result);
        Assert.Equal(TestReportUuid, result!.ReportUuid);
        VerifyPost(clientMock, expectedPath);
    }

    // --- ExportOrgComplianceReportAsync ---

    [Fact]
    public async Task ExportOrgComplianceReportAsync_PostsToCorrectPathAndReturnsReportUuid()
    {
        var expectedPath = $"/api/v3.0/orgs/{TestOrgUuid}/code/reports/compliance";
        var reportJob = new ReportJob { ReportUuid = TestReportUuid };
        var clientMock = BuildMock(expectedPath, reportJob);

        var sut = new MendReportExportsClient(clientMock.Object);
        var result = await sut.ExportOrgComplianceReportAsync(TestOrgUuid);

        Assert.NotNull(result);
        Assert.Equal(TestReportUuid, result!.ReportUuid);
        VerifyPost(clientMock, expectedPath);
    }

    // --- Null response ---

    [Fact]
    public async Task ExportProjectSbomReportAsync_WhenResponseIsNull_ReturnsNull()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostAsync<ReportJob>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ReportJob));

        var sut = new MendReportExportsClient(clientMock.Object);
        var result = await sut.ExportProjectSbomReportAsync(TestProjectUuid);

        Assert.Null(result);
    }

    // --- 401 throws MendAuthException ---

    [Fact]
    public async Task ExportProjectSbomReportAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"/api/v3.0/projects/{TestProjectUuid}/dependencies/reports/SBOM";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostAsync<ReportJob>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendReportExportsClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.ExportProjectSbomReportAsync(TestProjectUuid));
        Assert.Equal(expectedPath, ex.EndpointPath);
    }

    [Fact]
    public async Task ExportOrgInventoryReportAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"/api/v3.0/orgs/{TestOrgUuid}/dependencies/reports/inventory";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostAsync<ReportJob>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendReportExportsClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.ExportOrgInventoryReportAsync(TestOrgUuid));
        Assert.Equal(expectedPath, ex.EndpointPath);
    }

    // --- Helpers ---

    private static Mock<IMendClient> BuildMock(string expectedPath, ReportJob returnValue)
    {
        var mock = new Mock<IMendClient>();
        mock.Setup(c => c.PostAsync<ReportJob>(expectedPath, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(returnValue);
        return mock;
    }

    private static void VerifyPost(Mock<IMendClient> mock, string expectedPath) =>
        mock.Verify(c => c.PostAsync<ReportJob>(expectedPath, It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
}
