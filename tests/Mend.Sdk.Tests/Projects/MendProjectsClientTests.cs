using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Options;
using Mend.Sdk.Projects;
using Mend.Sdk.Projects.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.Projects;

public sealed class MendProjectsClientTests
{
    private const string TestOrgUuid = "test-org-uuid";
    private const string ExpectedBasePath = $"/api/v3.0/orgs/{TestOrgUuid}/projects";

    private static IOptions<MendOptions> CreateOptions() =>
        Microsoft.Extensions.Options.Options.Create(new MendOptions { OrgUuid = TestOrgUuid });

    // --- Deserialization ---

    [Fact]
    public void Project_DeserializesFromJson_Correctly()
    {
        const string json = """
            [
              { "uuid": "proj-1", "name": "Project One", "applicationUuid": "app-1" },
              { "uuid": "proj-2", "name": "Project Two", "applicationUuid": "app-2" }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<Project>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("proj-1", result[0].Uuid);
        Assert.Equal("Project One", result[0].Name);
        Assert.Equal("app-1", result[0].ApplicationUuid);
        Assert.Equal("proj-2", result[1].Uuid);
        Assert.Equal("Project Two", result[1].Name);
        Assert.Equal("app-2", result[1].ApplicationUuid);
    }

    // --- GetProjectsAsync: list ---

    [Fact]
    public async Task GetProjectsAsync_ReturnsMappedProjectList()
    {
        var projects = new List<Project>
        {
            new Project { Uuid = "proj-1", Name = "My Project", ApplicationUuid = "app-1" },
            new Project { Uuid = "proj-2", Name = "Another Project", ApplicationUuid = "app-2" }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Project>>(
                ExpectedBasePath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetProjectsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("proj-1", result[0].Uuid);
        Assert.Equal("My Project", result[0].Name);
        Assert.Equal("app-1", result[0].ApplicationUuid);
        Assert.Equal("proj-2", result[1].Uuid);
    }

    [Fact]
    public async Task GetProjectsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Project>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<Project>));

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetProjectsAsync();

        Assert.Empty(result);
    }

    // --- ApplicationUuid filter ---

    [Fact]
    public async Task GetProjectsAsync_WithApplicationUuidFilter_AppendsQueryParameter()
    {
        const string appUuid = "filter-app-uuid";
        var expectedPath = $"{ExpectedBasePath}?applicationUuid={appUuid}";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Project>>(
                expectedPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Project>());

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        await sut.GetProjectsAsync(applicationUuid: appUuid);

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Project>>(
            expectedPath, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectsAsync_WithoutApplicationUuidFilter_UsesBasePath()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Project>>(
                ExpectedBasePath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Project>());

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        await sut.GetProjectsAsync();

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Project>>(
            ExpectedBasePath, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Pagination ---

    [Fact]
    public async Task GetProjectsAsync_WithPageSize_PassesPageSizeToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Project>>(
                ExpectedBasePath, 25, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Project>());

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        await sut.GetProjectsAsync(pageSize: 25);

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Project>>(
            ExpectedBasePath, 25, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectsAsync_WithCursor_PassesCursorToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Project>>(
                ExpectedBasePath, null, "my-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Project>());

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        await sut.GetProjectsAsync(cursor: "my-cursor");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Project>>(
            ExpectedBasePath, null, "my-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectsAsync_WithPageSizeAndCursor_PassesBothToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Project>>(
                ExpectedBasePath, 50, "next-page", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Project>());

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        await sut.GetProjectsAsync(pageSize: 50, cursor: "next-page");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<Project>>(
            ExpectedBasePath, 50, "next-page", It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- 401 propagation ---

    [Fact]
    public async Task GetProjectsAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<Project>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedBasePath));

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());

        var ex = await Assert.ThrowsAsync<MendAuthException>(() => sut.GetProjectsAsync());
        Assert.Equal(ExpectedBasePath, ex.EndpointPath);
    }

    // --- GetProjectSummariesTotalsAsync ---

    [Fact]
    public async Task GetProjectSummariesTotalsAsync_ReturnsTypedTotals()
    {
        var totals = new ProjectSummariesTotals
        {
            TotalProjects = 10,
            HighSeverityCount = 4,
            MediumSeverityCount = 6,
            LowSeverityCount = 15
        };
        var expectedPath = $"{ExpectedBasePath}/summaries/totals";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ProjectSummariesTotals>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetProjectSummariesTotalsAsync();

        Assert.NotNull(result);
        Assert.Equal(10, result!.TotalProjects);
        Assert.Equal(4, result.HighSeverityCount);
        Assert.Equal(6, result.MediumSeverityCount);
        Assert.Equal(15, result.LowSeverityCount);
    }

    [Fact]
    public async Task GetProjectSummariesTotalsAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedBasePath}/summaries/totals";
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<ProjectSummariesTotals>(expectedPath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());

        await Assert.ThrowsAsync<MendAuthException>(() => sut.GetProjectSummariesTotalsAsync());
    }

    // --- GetProjectSummariesAsync ---

    [Fact]
    public async Task GetProjectSummariesAsync_ReturnsSummaryList()
    {
        var summaries = new List<ProjectSummary>
        {
            new ProjectSummary { Uuid = "proj-1", Name = "My Project", HighSeverityCount = 3 }
        };
        var request = new ProjectSummariesRequest { ProjectUuids = new[] { "proj-1" } };
        var expectedPath = $"{ExpectedBasePath}/summaries";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostAsync<IReadOnlyList<ProjectSummary>>(
                expectedPath, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetProjectSummariesAsync(request);

        Assert.Single(result);
        Assert.Equal("proj-1", result[0].Uuid);
        Assert.Equal("My Project", result[0].Name);
        Assert.Equal(3, result[0].HighSeverityCount);
    }

    [Fact]
    public async Task GetProjectSummariesAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var request = new ProjectSummariesRequest { ProjectUuids = Array.Empty<string>() };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.PostAsync<IReadOnlyList<ProjectSummary>>(
                It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<ProjectSummary>));

        var sut = new MendProjectsClient(clientMock.Object, CreateOptions());
        var result = await sut.GetProjectSummariesAsync(request);

        Assert.Empty(result);
    }
}
