using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Exceptions;
using Mend.Sdk.ZeroDayEvents;
using Mend.Sdk.ZeroDayEvents.Models;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.ZeroDayEvents;

public sealed class MendZeroDayEventsClientTests
{
    private const string TestOrgUuid = "test-org-uuid";
    private const string TestEventUuid = "test-event-uuid";

    private static readonly string BaseEventsPath =
        $"/api/v3.0/orgs/{TestOrgUuid}/dependencies/events/zeroday";

    private static readonly string ExpectedFindingsPath =
        $"/api/v3.0/orgs/{TestOrgUuid}/dependencies/events/zeroday/{TestEventUuid}/findings";

    // --- Deserialization ---

    [Fact]
    public void ZeroDayEvent_DeserializesFromJson_Correctly()
    {
        const string json = """
            [
              {
                "uuid": "evt-1",
                "publishedAt": "2024-03-15T10:00:00Z",
                "summary": "Critical zero-day in log4j"
              },
              {
                "uuid": "evt-2",
                "publishedAt": "2024-04-01T08:30:00Z",
                "summary": "Remote code execution in OpenSSL"
              }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<ZeroDayEvent>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("evt-1", result[0].Uuid);
        Assert.Equal("Critical zero-day in log4j", result[0].Summary);
        Assert.Equal("evt-2", result[1].Uuid);
        Assert.Equal("Remote code execution in OpenSSL", result[1].Summary);
    }

    [Fact]
    public void ZeroDayEventFinding_DeserializesFromJson_Correctly()
    {
        const string json = """
            [
              {
                "uuid": "finding-1",
                "name": "log4j-core",
                "version": "2.14.1",
                "severity": "CRITICAL"
              }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<ZeroDayEventFinding>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("finding-1", result[0].Uuid);
        Assert.Equal("log4j-core", result[0].Name);
        Assert.Equal("2.14.1", result[0].Version);
        Assert.Equal("CRITICAL", result[0].Severity);
    }

    // --- GetZeroDayEventsAsync: URL construction ---

    [Fact]
    public async Task GetZeroDayEventsAsync_WithNoDates_UsesBasePathWithoutQueryString()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(BaseEventsPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ZeroDayEvent>());

        var sut = new MendZeroDayEventsClient(clientMock.Object);
        await sut.GetZeroDayEventsAsync(TestOrgUuid);

        clientMock.Verify(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(
            BaseEventsPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetZeroDayEventsAsync_WithFromDate_AppendsFromDateQueryParam()
    {
        var fromDate = new DateTime(2024, 1, 15);
        var expectedPath = $"{BaseEventsPath}?fromDate=2024-01-15";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ZeroDayEvent>());

        var sut = new MendZeroDayEventsClient(clientMock.Object);
        await sut.GetZeroDayEventsAsync(TestOrgUuid, fromDate: fromDate);

        clientMock.Verify(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(
            expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetZeroDayEventsAsync_WithToDate_AppendsToDateQueryParam()
    {
        var toDate = new DateTime(2024, 3, 31);
        var expectedPath = $"{BaseEventsPath}?toDate=2024-03-31";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ZeroDayEvent>());

        var sut = new MendZeroDayEventsClient(clientMock.Object);
        await sut.GetZeroDayEventsAsync(TestOrgUuid, toDate: toDate);

        clientMock.Verify(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(
            expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetZeroDayEventsAsync_WithBothDates_AppendsBothQueryParams()
    {
        var fromDate = new DateTime(2024, 1, 15);
        var toDate = new DateTime(2024, 3, 31);
        var expectedPath = $"{BaseEventsPath}?fromDate=2024-01-15&toDate=2024-03-31";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ZeroDayEvent>());

        var sut = new MendZeroDayEventsClient(clientMock.Object);
        await sut.GetZeroDayEventsAsync(TestOrgUuid, fromDate: fromDate, toDate: toDate);

        clientMock.Verify(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(
            expectedPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- GetZeroDayEventsAsync: happy path / null ---

    [Fact]
    public async Task GetZeroDayEventsAsync_ReturnsMappedList()
    {
        var events = new List<ZeroDayEvent>
        {
            new ZeroDayEvent { Uuid = "evt-1", Summary = "Critical zero-day in log4j" },
            new ZeroDayEvent { Uuid = "evt-2", Summary = "RCE in OpenSSL" }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(BaseEventsPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var sut = new MendZeroDayEventsClient(clientMock.Object);
        var result = await sut.GetZeroDayEventsAsync(TestOrgUuid);

        Assert.Equal(2, result.Count);
        Assert.Equal("evt-1", result[0].Uuid);
        Assert.Equal("Critical zero-day in log4j", result[0].Summary);
        Assert.Equal("evt-2", result[1].Uuid);
    }

    [Fact]
    public async Task GetZeroDayEventsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<ZeroDayEvent>));

        var sut = new MendZeroDayEventsClient(clientMock.Object);
        var result = await sut.GetZeroDayEventsAsync(TestOrgUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetZeroDayEventsAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEvent>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(BaseEventsPath));

        var sut = new MendZeroDayEventsClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetZeroDayEventsAsync(TestOrgUuid));
        Assert.Equal(BaseEventsPath, ex.EndpointPath);
    }

    // --- GetZeroDayEventFindingsAsync ---

    [Fact]
    public async Task GetZeroDayEventFindingsAsync_UsesCorrectPath()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEventFinding>>(ExpectedFindingsPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ZeroDayEventFinding>());

        var sut = new MendZeroDayEventsClient(clientMock.Object);
        await sut.GetZeroDayEventFindingsAsync(TestOrgUuid, TestEventUuid);

        clientMock.Verify(c => c.GetAsync<IReadOnlyList<ZeroDayEventFinding>>(
            ExpectedFindingsPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetZeroDayEventFindingsAsync_ReturnsMappedList()
    {
        var findings = new List<ZeroDayEventFinding>
        {
            new ZeroDayEventFinding { Uuid = "f-1", Name = "log4j-core", Version = "2.14.1", Severity = "CRITICAL" },
            new ZeroDayEventFinding { Uuid = "f-2", Name = "log4j-api", Version = "2.14.1", Severity = "HIGH" }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEventFinding>>(ExpectedFindingsPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(findings);

        var sut = new MendZeroDayEventsClient(clientMock.Object);
        var result = await sut.GetZeroDayEventFindingsAsync(TestOrgUuid, TestEventUuid);

        Assert.Equal(2, result.Count);
        Assert.Equal("f-1", result[0].Uuid);
        Assert.Equal("log4j-core", result[0].Name);
        Assert.Equal("2.14.1", result[0].Version);
        Assert.Equal("CRITICAL", result[0].Severity);
        Assert.Equal("f-2", result[1].Uuid);
    }

    [Fact]
    public async Task GetZeroDayEventFindingsAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEventFinding>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<ZeroDayEventFinding>));

        var sut = new MendZeroDayEventsClient(clientMock.Object);
        var result = await sut.GetZeroDayEventFindingsAsync(TestOrgUuid, TestEventUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetZeroDayEventFindingsAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<IReadOnlyList<ZeroDayEventFinding>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedFindingsPath));

        var sut = new MendZeroDayEventsClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetZeroDayEventFindingsAsync(TestOrgUuid, TestEventUuid));
        Assert.Equal(ExpectedFindingsPath, ex.EndpointPath);
    }
}
