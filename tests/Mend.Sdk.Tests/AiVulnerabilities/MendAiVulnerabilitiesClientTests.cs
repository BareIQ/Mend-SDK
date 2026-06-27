using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.AiVulnerabilities;
using Mend.Sdk.AiVulnerabilities.Models;
using Mend.Sdk.Client;
using Mend.Sdk.Exceptions;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.AiVulnerabilities;

public sealed class MendAiVulnerabilitiesClientTests
{
    private const string TestProjectUuid = "test-project-uuid";
    private const string TestApplicationUuid = "test-application-uuid";
    private const string TestVulnerabilityId = "test-vuln-id";

    private static readonly string ExpectedProjectPath =
        $"/api/v3.0/projects/{TestProjectUuid}/ai/vulnerabilities";

    private static readonly string ExpectedApplicationPath =
        $"/api/v3.0/applications/{TestApplicationUuid}/ai/vulnerabilities";

    // --- Deserialization ---

    [Fact]
    public void AiVulnerability_ListDeserializesFromJson_Correctly()
    {
        const string json = """
            [
              {
                "id": "vuln-1",
                "severity": "HIGH",
                "description": "Prompt injection vulnerability"
              },
              {
                "id": "vuln-2",
                "severity": "MEDIUM",
                "description": "Insecure model output handling"
              }
            ]
            """;

        var result = JsonSerializer.Deserialize<List<AiVulnerability>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("vuln-1", result[0].Id);
        Assert.Equal("HIGH", result[0].Severity);
        Assert.Equal("Prompt injection vulnerability", result[0].Description);
        Assert.Equal("vuln-2", result[1].Id);
        Assert.Equal("MEDIUM", result[1].Severity);
        Assert.Equal("Insecure model output handling", result[1].Description);
    }

    [Fact]
    public void AiVulnerability_SingleDeserializesFromJson_Correctly()
    {
        const string json = """
            {
              "id": "vuln-1",
              "severity": "CRITICAL",
              "description": "Data exfiltration via LLM"
            }
            """;

        var result = JsonSerializer.Deserialize<AiVulnerability>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal("vuln-1", result!.Id);
        Assert.Equal("CRITICAL", result.Severity);
        Assert.Equal("Data exfiltration via LLM", result.Description);
    }

    // --- GetProjectAiVulnerabilitiesAsync ---

    [Fact]
    public async Task GetProjectAiVulnerabilitiesAsync_ReturnsMappedList()
    {
        var vulns = new List<AiVulnerability>
        {
            new AiVulnerability { Id = "v-1", Severity = "HIGH", Description = "Prompt injection" },
            new AiVulnerability { Id = "v-2", Severity = "LOW", Description = "Data leakage" }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
                ExpectedProjectPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vulns);

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        var result = await sut.GetProjectAiVulnerabilitiesAsync(TestProjectUuid);

        Assert.Equal(2, result.Count);
        Assert.Equal("v-1", result[0].Id);
        Assert.Equal("HIGH", result[0].Severity);
        Assert.Equal("Prompt injection", result[0].Description);
        Assert.Equal("v-2", result[1].Id);
    }

    [Fact]
    public async Task GetProjectAiVulnerabilitiesAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<AiVulnerability>));

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        var result = await sut.GetProjectAiVulnerabilitiesAsync(TestProjectUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProjectAiVulnerabilitiesAsync_WithPageSize_PassesPageSizeToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
                ExpectedProjectPath, 25, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AiVulnerability>());

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        await sut.GetProjectAiVulnerabilitiesAsync(TestProjectUuid, pageSize: 25);

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
            ExpectedProjectPath, 25, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectAiVulnerabilitiesAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedProjectPath));

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetProjectAiVulnerabilitiesAsync(TestProjectUuid));
        Assert.Equal(ExpectedProjectPath, ex.EndpointPath);
    }

    // --- GetProjectAiVulnerabilityAsync ---

    [Fact]
    public async Task GetProjectAiVulnerabilityAsync_ReturnsSingleVulnerability()
    {
        var expectedPath = $"{ExpectedProjectPath}/{TestVulnerabilityId}";
        var vuln = new AiVulnerability
        {
            Id = TestVulnerabilityId,
            Severity = "CRITICAL",
            Description = "Data exfiltration via LLM"
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<AiVulnerability>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vuln);

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        var result = await sut.GetProjectAiVulnerabilityAsync(TestProjectUuid, TestVulnerabilityId);

        Assert.NotNull(result);
        Assert.Equal(TestVulnerabilityId, result!.Id);
        Assert.Equal("CRITICAL", result.Severity);
        Assert.Equal("Data exfiltration via LLM", result.Description);
    }

    [Fact]
    public async Task GetProjectAiVulnerabilityAsync_WhenNotFound_ReturnsNull()
    {
        var expectedPath = $"{ExpectedProjectPath}/{TestVulnerabilityId}";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<AiVulnerability>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(AiVulnerability));

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        var result = await sut.GetProjectAiVulnerabilityAsync(TestProjectUuid, TestVulnerabilityId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProjectAiVulnerabilityAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedProjectPath}/{TestVulnerabilityId}";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<AiVulnerability>(expectedPath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetProjectAiVulnerabilityAsync(TestProjectUuid, TestVulnerabilityId));
        Assert.Equal(expectedPath, ex.EndpointPath);
    }

    // --- GetApplicationAiVulnerabilitiesAsync ---

    [Fact]
    public async Task GetApplicationAiVulnerabilitiesAsync_ReturnsMappedList()
    {
        var vulns = new List<AiVulnerability>
        {
            new AiVulnerability { Id = "av-1", Severity = "HIGH", Description = "Model inversion attack" }
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
                ExpectedApplicationPath, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vulns);

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        var result = await sut.GetApplicationAiVulnerabilitiesAsync(TestApplicationUuid);

        Assert.Single(result);
        Assert.Equal("av-1", result[0].Id);
        Assert.Equal("HIGH", result[0].Severity);
        Assert.Equal("Model inversion attack", result[0].Description);
    }

    [Fact]
    public async Task GetApplicationAiVulnerabilitiesAsync_WhenResponseIsNull_ReturnsEmptyList()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IReadOnlyList<AiVulnerability>));

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        var result = await sut.GetApplicationAiVulnerabilitiesAsync(TestApplicationUuid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetApplicationAiVulnerabilitiesAsync_WithCursor_PassesCursorToClient()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
                ExpectedApplicationPath, null, "next-cursor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AiVulnerability>());

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        await sut.GetApplicationAiVulnerabilitiesAsync(TestApplicationUuid, cursor: "next-cursor");

        clientMock.Verify(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
            ExpectedApplicationPath, null, "next-cursor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetApplicationAiVulnerabilitiesAsync_When401_ThrowsMendAuthException()
    {
        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetPagedAsync<IReadOnlyList<AiVulnerability>>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(ExpectedApplicationPath));

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetApplicationAiVulnerabilitiesAsync(TestApplicationUuid));
        Assert.Equal(ExpectedApplicationPath, ex.EndpointPath);
    }

    // --- GetApplicationAiVulnerabilityAsync ---

    [Fact]
    public async Task GetApplicationAiVulnerabilityAsync_ReturnsSingleVulnerability()
    {
        var expectedPath = $"{ExpectedApplicationPath}/{TestVulnerabilityId}";
        var vuln = new AiVulnerability
        {
            Id = TestVulnerabilityId,
            Severity = "HIGH",
            Description = "Model inversion attack"
        };

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<AiVulnerability>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vuln);

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        var result = await sut.GetApplicationAiVulnerabilityAsync(TestApplicationUuid, TestVulnerabilityId);

        Assert.NotNull(result);
        Assert.Equal(TestVulnerabilityId, result!.Id);
        Assert.Equal("HIGH", result.Severity);
        Assert.Equal("Model inversion attack", result.Description);
    }

    [Fact]
    public async Task GetApplicationAiVulnerabilityAsync_WhenNotFound_ReturnsNull()
    {
        var expectedPath = $"{ExpectedApplicationPath}/{TestVulnerabilityId}";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<AiVulnerability>(expectedPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(AiVulnerability));

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);
        var result = await sut.GetApplicationAiVulnerabilityAsync(TestApplicationUuid, TestVulnerabilityId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetApplicationAiVulnerabilityAsync_When401_ThrowsMendAuthException()
    {
        var expectedPath = $"{ExpectedApplicationPath}/{TestVulnerabilityId}";

        var clientMock = new Mock<IMendClient>();
        clientMock
            .Setup(c => c.GetAsync<AiVulnerability>(expectedPath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException(expectedPath));

        var sut = new MendAiVulnerabilitiesClient(clientMock.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetApplicationAiVulnerabilityAsync(TestApplicationUuid, TestVulnerabilityId));
        Assert.Equal(expectedPath, ex.EndpointPath);
    }
}
