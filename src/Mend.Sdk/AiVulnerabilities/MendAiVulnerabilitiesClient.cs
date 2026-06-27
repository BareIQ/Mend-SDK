using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.AiVulnerabilities.Models;
using Mend.Sdk.Client;

namespace Mend.Sdk.AiVulnerabilities;

public sealed class MendAiVulnerabilitiesClient : IMendAiVulnerabilitiesClient
{
    private readonly IMendClient _client;

    public MendAiVulnerabilitiesClient(IMendClient client)
    {
        _client = client;
    }

    private static string ProjectAiVulnerabilitiesPath(string projectUuid) =>
        $"/api/v3.0/projects/{Uri.EscapeDataString(projectUuid)}/ai/vulnerabilities";

    private static string ApplicationAiVulnerabilitiesPath(string applicationUuid) =>
        $"/api/v3.0/applications/{Uri.EscapeDataString(applicationUuid)}/ai/vulnerabilities";

    public async Task<IReadOnlyList<AiVulnerability>> GetProjectAiVulnerabilitiesAsync(
        string projectUuid,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetPagedAsync<IReadOnlyList<AiVulnerability>>(ProjectAiVulnerabilitiesPath(projectUuid), pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<AiVulnerability>();
    }

    public Task<AiVulnerability?> GetProjectAiVulnerabilityAsync(
        string projectUuid,
        string vulnerabilityId,
        CancellationToken cancellationToken = default)
    {
        var path = $"{ProjectAiVulnerabilitiesPath(projectUuid)}/{Uri.EscapeDataString(vulnerabilityId)}";
        return _client.GetAsync<AiVulnerability>(path, cancellationToken);
    }

    public async Task<IReadOnlyList<AiVulnerability>> GetApplicationAiVulnerabilitiesAsync(
        string applicationUuid,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetPagedAsync<IReadOnlyList<AiVulnerability>>(ApplicationAiVulnerabilitiesPath(applicationUuid), pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<AiVulnerability>();
    }

    public Task<AiVulnerability?> GetApplicationAiVulnerabilityAsync(
        string applicationUuid,
        string vulnerabilityId,
        CancellationToken cancellationToken = default)
    {
        var path = $"{ApplicationAiVulnerabilitiesPath(applicationUuid)}/{Uri.EscapeDataString(vulnerabilityId)}";
        return _client.GetAsync<AiVulnerability>(path, cancellationToken);
    }
}
