using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Applications;
using Mend.Sdk.Applications.Models;
using Mend.Sdk.Client;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.Applications;

public sealed class MendApplicationsClientTests
{
    private const string TestOrgUuid = "test-org-uuid";
    private const string ExpectedBasePath = $"/api/v3.0/orgs/{TestOrgUuid}/applications";

    private static IOptions<MendOptions> CreateOptions() =>
        Microsoft.Extensions.Options.Options.Create(new MendOptions { OrgUuid = TestOrgUuid });

    // --- GetApplicationsAsync: list deserialization ---

    [Fact]
    public void Application_DeserializesFromJson_Correctly()
    {
        const string json = """
            [
              { "uuid": "uuid-1", "name": "App One" },
              { "uuid": "uuid-2", "name": "App Two" }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<Application>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("uuid-1", result[0].Uuid);
        Assert.Equal("App One", result[0].Name);
        Assert.Equal("uuid-2", result[1].Uuid);
        Assert.Equal("App Two", result[1].Name);
    }

    [Fact]
    public async Task GetApplicationsAsync_ReturnsMappedApplicationList()
    {
        var applications = new List<Application>
        {
            new Application { Uuid = "app-1", Name = "My App" },
            new Application { Uuid = "app-2", Name = "Another App" }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Application>>(
                ExpectedBasePath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetApplicationsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("app-1", result[0].Uuid);
        Assert.Equal("My App", result[0].Name);
        Assert.Equal("app-2", result[1].Uuid);
        Assert.Equal("Another App", result[1].Name);
    }

    [Fact]
    public async Task GetApplicationsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Application>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<Application>));

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetApplicationsAsync();

        Assert.Empty(result);
    }

    // --- Pagination ---

    [Fact]
    public async Task GetApplicationsAsync_WithPageSize_PassesPageSizeToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Application>>(
                ExpectedBasePath, 25, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Application>());

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        await sut.GetApplicationsAsync(pageSize: 25);

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Application>>(
            ExpectedBasePath, 25, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetApplicationsAsync_WithCursor_PassesCursorToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Application>>(
                ExpectedBasePath, null, "my-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Application>());

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        await sut.GetApplicationsAsync(cursor: "my-cursor");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Application>>(
            ExpectedBasePath, null, "my-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetApplicationsAsync_WithPageSizeAndCursor_PassesBothToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Application>>(
                ExpectedBasePath, 50, "next-page", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Application>());

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        await sut.GetApplicationsAsync(pageSize: 50, cursor: "next-page");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Application>>(
            ExpectedBasePath, 50, "next-page", It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- 401 propagation ---

    [Fact]
    public async Task GetApplicationsAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Application>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedBasePath));

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());

        var ex = await Assert.ThrowsAsync<MendAuthException>(() => sut.GetApplicationsAsync());
        Assert.Equal(ExpectedBasePath, ex.EndpointPath);
    }

    // --- GetApplicationSummariesTotalsAsync ---

    [Fact]
    public async Task GetApplicationSummariesTotalsAsync_ReturnsTypedTotals()
    {
        var totals = new ApplicationSummariesTotals
        {
            TotalApplications = 5,
            HighSeverityCount = 3,
            MediumSeverityCount = 7,
            LowSeverityCount = 12
        };
        var expectedPath = $"{ExpectedBasePath}/summaries/totals";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ApplicationSummariesTotals>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetApplicationSummariesTotalsAsync();

        Assert.NotNull(result);
        Assert.Equal(5, result!.TotalApplications);
        Assert.Equal(3, result.HighSeverityCount);
        Assert.Equal(7, result.MediumSeverityCount);
        Assert.Equal(12, result.LowSeverityCount);
    }

    [Fact]
    public async Task GetApplicationSummariesTotalsAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedBasePath}/summaries/totals";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ApplicationSummariesTotals>(expectedPath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());

        await Assert.ThrowsAsync<MendAuthException>(() => sut.GetApplicationSummariesTotalsAsync());
    }

    // --- ApplicationSummary deserialization ---

    [Fact]
    public void ApplicationSummary_DeserializesFromJson_Correctly()
    {
        const string json = """
            [
              {
                "uuid": "930ed3f0-e0bb-4e3a-806b-e8448bd66597",
                "name": "android",
                "creationDate": "2025-07-05T05:47:20Z",
                "tags": [],
                "labels": [],
                "statistics": {
                  "ALERTS": {
                    "criticalSeverityVulnerabilities": 3,
                    "highSeverityVulnerabilities": 7,
                    "mediumSeverityVulnerabilities": 12,
                    "lowSeverityVulnerabilities": 5,
                    "vulnerableLibraries": 4
                  },
                  "GENERAL": {
                    "totalLibraries": 150,
                    "totalProjects": 2
                  },
                  "LICENSE_RISK": {
                    "noLicenseLibraries": 4,
                    "unknownRiskLicenses": 25,
                    "highRiskLicenses": 14,
                    "mediumRiskLicenses": 1,
                    "lowRiskLicenses": 176
                  },
                  "LAST_SCAN": {
                    "lastScanTime": 1768657615145,
                    "lastScaScanTime": 1768657615145,
                    "lastImgScanTime": 0,
                    "lastSastScanTime": 0
                  }
                }
              }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<ApplicationSummary>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Single(result!);
        var s = result[0];
        Assert.Equal("930ed3f0-e0bb-4e3a-806b-e8448bd66597", s.Uuid);
        Assert.Equal("android", s.Name);
        Assert.Equal("2025-07-05T05:47:20Z", s.CreationDate);
        Assert.Empty(s.Tags);
        Assert.Empty(s.Labels);
        Assert.NotNull(s.Statistics);
        Assert.Equal(3,   s.Statistics!.Alerts!.CriticalSeverityVulnerabilities);
        Assert.Equal(7,   s.Statistics.Alerts.HighSeverityVulnerabilities);
        Assert.Equal(4,   s.Statistics.Alerts.VulnerableLibraries);
        Assert.Equal(150, s.Statistics.General!.TotalLibraries);
        Assert.Equal(2,   s.Statistics.General.TotalProjects);
        Assert.Equal(14,  s.Statistics.LicenseRisk!.HighRiskLicenses);
        Assert.Equal(1768657615145L, s.Statistics.LastScan!.LastScanTime);
    }

    // --- GetApplicationSummariesAsync ---

    [Fact]
    public async Task GetApplicationSummariesAsync_ReturnsSummaryList()
    {
        var summaries = new List<ApplicationSummary>
        {
            new ApplicationSummary
            {
                Uuid = "app-1", Name = "My App",
                Statistics = new ApplicationStatistics
                {
                    Alerts = new AlertsStatistics { HighSeverityVulnerabilities = 2 }
                }
            }
        };
        var request = new ApplicationSummariesRequest { ApplicationUuids = new[] { "app-1" } };
        var expectedPath = $"{ExpectedBasePath}/summaries";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostPagedAsync<IReadOnlyList<ApplicationSummary>>(
                expectedPath, request, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetApplicationSummariesAsync(request);

        Assert.Single(result);
        Assert.Equal("app-1", result[0].Uuid);
        Assert.Equal("My App", result[0].Name);
        Assert.Equal(2, result[0].Statistics!.Alerts!.HighSeverityVulnerabilities);
    }

    [Fact]
    public async Task GetApplicationSummariesAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var request = new ApplicationSummariesRequest { ApplicationUuids = Array.Empty<string>() };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostPagedAsync<IReadOnlyList<ApplicationSummary>>(
                It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<ApplicationSummary>));

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetApplicationSummariesAsync(request);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetApplicationSummariesAsync_WithLimit_PassesLimitToClient()
    {
        var request = new ApplicationSummariesRequest { ApplicationUuids = new[] { "app-1" } };
        var expectedPath = $"{ExpectedBasePath}/summaries";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostPagedAsync<IReadOnlyList<ApplicationSummary>>(
                expectedPath, request, 100, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ApplicationSummary>());

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        await sut.GetApplicationSummariesAsync(request, limit: 100);

        clientMock.Verify(c => c.PostPagedAsync<IReadOnlyList<ApplicationSummary>>(
            expectedPath, request, 100, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetApplicationSummariesAsync_WithCursor_PassesCursorToClient()
    {
        var request = new ApplicationSummariesRequest { ApplicationUuids = new[] { "app-1" } };
        var expectedPath = $"{ExpectedBasePath}/summaries";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostPagedAsync<IReadOnlyList<ApplicationSummary>>(
                expectedPath, request, null, "my-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ApplicationSummary>());

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        await sut.GetApplicationSummariesAsync(request, cursor: "my-cursor");

        clientMock.Verify(c => c.PostPagedAsync<IReadOnlyList<ApplicationSummary>>(
            expectedPath, request, null, "my-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetApplicationSummariesAsync_WithLimitAndCursor_PassesBothToClient()
    {
        var request = new ApplicationSummariesRequest { ApplicationUuids = new[] { "app-1" } };
        var expectedPath = $"{ExpectedBasePath}/summaries";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostPagedAsync<IReadOnlyList<ApplicationSummary>>(
                expectedPath, request, 50, "next-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ApplicationSummary>());

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        await sut.GetApplicationSummariesAsync(request, limit: 50, cursor: "next-cursor");

        clientMock.Verify(c => c.PostPagedAsync<IReadOnlyList<ApplicationSummary>>(
            expectedPath, request, 50, "next-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetApplicationSummariesAsync_When401_ThrowsMendAuthException()
    {
        var request = new ApplicationSummariesRequest { ApplicationUuids = new[] { "app-1" } };
        var expectedPath = $"{ExpectedBasePath}/summaries";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostPagedAsync<IReadOnlyList<ApplicationSummary>>(
                It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendApplicationsClient(clientMock.Object, CreateOptions());
        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetApplicationSummariesAsync(request));

        Assert.Equal(expectedPath, ex.EndpointPath);
    }
}
