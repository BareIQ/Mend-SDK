using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.Options;
using Mend.Sdk.Reports.Models;
using Microsoft.Extensions.Options;

namespace Mend.Sdk.Reports;

public sealed class MendReportsClient : IMendReportsClient
{
    private readonly IMendClient _client;
    private readonly IOptions<MendOptions> _options;

    public MendReportsClient(IMendClient client, IOptions<MendOptions> options)
    {
        _client = client;
        _options = options;
    }

    private string BasePath => $"/api/v3.0/orgs/{_options.Value.OrgUuid}/reports";

    public async Task<IReadOnlyList<ReportStatus>> GetReportsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetPagedAsync<IReadOnlyList<ReportStatus>>(BasePath, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response ?? Array.Empty<ReportStatus>();
    }

    public Task<ReportStatus?> GetReportStatusAsync(string reportUuid, CancellationToken cancellationToken = default)
        => _client.GetAsync<ReportStatus>($"{BasePath}/{Uri.EscapeDataString(reportUuid)}", cancellationToken);

    public Task<Stream> DownloadReportAsync(string reportUuid, CancellationToken cancellationToken = default)
        => _client.GetStreamAsync($"{BasePath}/download/{Uri.EscapeDataString(reportUuid)}", cancellationToken);

    public Task DeleteReportAsync(string reportUuid, CancellationToken cancellationToken = default)
        => _client.DeleteAsync($"{BasePath}/{Uri.EscapeDataString(reportUuid)}", cancellationToken);
}
