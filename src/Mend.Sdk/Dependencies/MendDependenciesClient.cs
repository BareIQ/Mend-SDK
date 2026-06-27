using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Dependencies.Models;

namespace Mend.Sdk.Dependencies;

public sealed class MendDependenciesClient : IMendDependenciesClient
{
    private readonly IMendClient _client;

    public MendDependenciesClient(IMendClient client)
    {
        _client = client;
    }

    private static string SecurityFindingsPath(string projectUuid) =>
        $"/api/v3.0/projects/{Uri.EscapeDataString(projectUuid)}/dependencies/findings/security";

    public async Task<IReadOnlyList<SecurityFinding>> GetDependencySecurityFindingsAsync(
        string projectUuid,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetPagedAsync<IReadOnlyList<SecurityFinding>>(SecurityFindingsPath(projectUuid), pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<SecurityFinding>();
    }

    public async Task<IReadOnlyList<LibraryGroup>> GetDependencySecurityFindingsByLibraryAsync(
        string projectUuid,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"{SecurityFindingsPath(projectUuid)}/groupBy/library";
        var response = await _client
            .GetPagedAsync<IReadOnlyList<LibraryGroup>>(path, pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<LibraryGroup>();
    }

    public async Task<IReadOnlyList<RootLibraryGroup>> GetDependencySecurityFindingsByRootLibraryAsync(
        string projectUuid,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"{SecurityFindingsPath(projectUuid)}/groupBy/rootLibrary";
        var response = await _client
            .GetPagedAsync<IReadOnlyList<RootLibraryGroup>>(path, pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<RootLibraryGroup>();
    }

    public async Task UpdateDependencySecurityFindingAsync(
        string projectUuid,
        string rootLibraryUuid,
        UpdateSecurityFindingRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = $"{SecurityFindingsPath(projectUuid)}/rootLibrary/{Uri.EscapeDataString(rootLibraryUuid)}";
        await _client.PutAsync<object>(path, request, cancellationToken).ConfigureAwait(false);
    }
}
