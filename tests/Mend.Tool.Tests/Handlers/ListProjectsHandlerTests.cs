using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Projects;
using Mend.Sdk.Projects.Models;
using Mend.Tool.Handlers;
using Moq;
using Xunit;

namespace Mend.Tool.Tests.Handlers;

public sealed class ListProjectsHandlerTests
{
    [Fact]
    public async Task RunAsync_CallsGetProjectsWithLargePageSize()
    {
        var mock = new Mock<IMendProjectsClient>();
        mock.Setup(c => c.GetProjectsAsync(null, 10000, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Project>());

        var handler = new ListProjectsHandler(mock.Object);
        await handler.RunAsync();

        mock.Verify(c => c.GetProjectsAsync(null, 10000, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenNoProjects_ReturnsZero()
    {
        var mock = new Mock<IMendProjectsClient>();
        mock.Setup(c => c.GetProjectsAsync(null, 10000, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Project>());

        var handler = new ListProjectsHandler(mock.Object);
        var result = await handler.RunAsync();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_ReturnsZeroOnSuccess()
    {
        var projects = new List<Project>
        {
            new() { Uuid = "1", Name = "Alpha" },
            new() { Uuid = "2", Name = "Beta" }
        };
        var mock = new Mock<IMendProjectsClient>();
        mock.Setup(c => c.GetProjectsAsync(null, 10000, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        var handler = new ListProjectsHandler(mock.Object);
        var result = await handler.RunAsync();

        Assert.Equal(0, result);
    }
}
