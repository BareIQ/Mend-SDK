using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Options;
using Mend.Sdk.Scans.Models;
using Microsoft.Extensions.Options;

namespace Mend.Sdk.Scans;

public sealed class MendScansClient : IMendScansClient
{
    private readonly IMendClient _client;
    private readonly IOptions<MendOptions> _options;

    public MendScansClient(IMendClient client, IOptions<MendOptions> options)
    {
        _client = client;
        _options = options;
    }

    private string OrgProjectsBasePath => $"/api/v3.0/orgs/{_options.Value.OrgUuid}/projects";

    private string ScansPath(string projectUuid) =>
        $"{OrgProjectsBasePath}/{Uri.EscapeDataString(projectUuid)}/scans";

    private string ScanPath(string projectUuid, string scanUuid) =>
        $"{ScansPath(projectUuid)}/{Uri.EscapeDataString(scanUuid)}";

    public async Task<IReadOnlyList<Scan>> GetScansAsync(
        string projectUuid,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetPagedAsync<IReadOnlyList<Scan>>(ScansPath(projectUuid), pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<Scan>();
    }

    public Task<ScanDetail?> GetScanAsync(
        string projectUuid,
        string scanUuid,
        CancellationToken cancellationToken = default)
        => _client.GetAsync<ScanDetail>(ScanPath(projectUuid, scanUuid), cancellationToken);

    public Task<ScanSummary?> GetScanSummaryAsync(
        string projectUuid,
        string scanUuid,
        CancellationToken cancellationToken = default)
        => _client.GetAsync<ScanSummary>($"{ScanPath(projectUuid, scanUuid)}/summary", cancellationToken);

    public async Task<IReadOnlyList<ScanTag>> GetScanTagsAsync(
        string projectUuid,
        string scanUuid,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetPagedAsync<IReadOnlyList<ScanTag>>($"{ScanPath(projectUuid, scanUuid)}/tags", null, null, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<ScanTag>();
    }

    public async Task<IReadOnlyList<SbomLog>> GetScanSbomLogsAsync(
        string projectUuid,
        string scanUuid,
        CancellationToken cancellationToken = default)
    {
        var path = $"/api/v3.0/projects/{Uri.EscapeDataString(projectUuid)}/scans/{Uri.EscapeDataString(scanUuid)}/dependencies/SBOM/logs";
        var response = await _client
            .GetPagedAsync<IReadOnlyList<SbomLog>>(path, null, null, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<SbomLog>();
    }
}
