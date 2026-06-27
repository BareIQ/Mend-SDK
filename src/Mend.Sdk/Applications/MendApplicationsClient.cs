using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Applications.Models;
using Mend.Sdk.Client;
using Mend.Sdk.Options;
using Microsoft.Extensions.Options;

namespace Mend.Sdk.Applications;

public sealed class MendApplicationsClient : IMendApplicationsClient
{
    private readonly IMendClient _client;
    private readonly IOptions<MendOptions> _options;

    public MendApplicationsClient(IMendClient client, IOptions<MendOptions> options)
    {
        _client = client;
        _options = options;
    }

    private string BasePath => $"/api/v3.0/orgs/{_options.Value.OrgUuid}/applications";

    public async Task<IReadOnlyList<Application>> GetApplicationsAsync(
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetPagedAsync<IReadOnlyList<Application>>(BasePath, pageSize, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<Application>();
    }

    public async Task<IReadOnlyList<ApplicationSummary>> GetApplicationSummariesAsync(
        ApplicationSummariesRequest request,
        int? limit = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _client
            .PostPagedAsync<IReadOnlyList<ApplicationSummary>>($"{BasePath}/summaries", request, limit, cursor, cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<ApplicationSummary>();
    }

    public Task<ApplicationSummariesTotals?> GetApplicationSummariesTotalsAsync(
        CancellationToken cancellationToken = default)
        => _client.GetAsync<ApplicationSummariesTotals>($"{BasePath}/summaries/totals", cancellationToken);
}
