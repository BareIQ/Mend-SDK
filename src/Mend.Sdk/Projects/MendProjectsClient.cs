using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Options;
using Mend.Sdk.Projects.Models;
using Microsoft.Extensions.Options;

namespace Mend.Sdk.Projects;

public sealed class MendProjectsClient : IMendProjectsClient
{
    private readonly IMendClient _client;
    private readonly IOptions<MendOptions> _options;

    public MendProjectsClient(IMendClient client, IOptions<MendOptions> options)
    {
        _client = client;
        _options = options;
    }

    private string BasePath => $"/api/v3.0/orgs/{_options.Value.OrgUuid}/projects";

    public async Task<IReadOnlyList<Project>> GetProjectsAsync(
        string? applicationUuid = null,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrEmpty(applicationUuid)
            ? BasePath
            : $"{BasePath}?applicationUuid={Uri.EscapeDataString(applicationUuid)}";

        var response = await _client
            .GetPagedAsync<IReadOnlyList<Project>>(path, pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<Project>();
    }

    public async Task<IReadOnlyList<ProjectSummary>> GetProjectSummariesAsync(
        ProjectSummariesRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .PostAsync<IReadOnlyList<ProjectSummary>>($"{BasePath}/summaries", request, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<ProjectSummary>();
    }

    public Task<ProjectSummariesTotals?> GetProjectSummariesTotalsAsync(
        CancellationToken cancellationToken = default)
        => _client.GetAsync<ProjectSummariesTotals>($"{BasePath}/summaries/totals", cancellationToken);
}
