using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.CodeFindings.Models;

namespace Mend.Sdk.CodeFindings;

public sealed class MendCodeFindingsClient : IMendCodeFindingsClient
{
    private readonly IMendClient _client;

    public MendCodeFindingsClient(IMendClient client)
    {
        _client = client;
    }

    private static string CodeFindingsPath(string projectUuid) =>
        $"/api/v3.0/projects/{Uri.EscapeDataString(projectUuid)}/code/findings";

    private static string ScanCodeFindingsPath(string projectUuid, string scanUuid) =>
        $"/api/v3.0/projects/{Uri.EscapeDataString(projectUuid)}/scans/{Uri.EscapeDataString(scanUuid)}/code/findings";

    public async Task<IReadOnlyList<CodeFinding>> GetCodeFindingsAsync(
        string projectUuid,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetPagedAsync<IReadOnlyList<CodeFinding>>(CodeFindingsPath(projectUuid), pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<CodeFinding>();
    }

    public Task<CodeFinding?> GetCodeFindingAsync(
        string projectUuid,
        string findingUuid,
        CancellationToken cancellationToken = default)
    {
        var path = $"{CodeFindingsPath(projectUuid)}/{Uri.EscapeDataString(findingUuid)}";
        return _client.GetAsync<CodeFinding>(path, cancellationToken);
    }

    public async Task<IReadOnlyList<CodeFinding>> GetScanCodeFindingsAsync(
        string projectUuid,
        string scanUuid,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetPagedAsync<IReadOnlyList<CodeFinding>>(ScanCodeFindingsPath(projectUuid, scanUuid), pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<CodeFinding>();
    }

    public Task<CodeFinding?> GetScanCodeFindingAsync(
        string projectUuid,
        string scanUuid,
        string findingUuid,
        CancellationToken cancellationToken = default)
    {
        var path = $"{ScanCodeFindingsPath(projectUuid, scanUuid)}/{Uri.EscapeDataString(findingUuid)}";
        return _client.GetAsync<CodeFinding>(path, cancellationToken);
    }
}
