using System;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.ReportExports.Models;

namespace Mend.Sdk.ReportExports;

public sealed class MendReportExportsClient : IMendReportExportsClient
{
    private readonly IMendClient _client;

    public MendReportExportsClient(IMendClient client)
    {
        _client = client;
    }

    public Task<ReportJob?> ExportProjectSbomReportAsync(string projectUuid, CancellationToken cancellationToken = default) =>
        _client.PostAsync<ReportJob>(
            $"/api/v3.0/projects/{Uri.EscapeDataString(projectUuid)}/dependencies/reports/SBOM",
            cancellationToken: cancellationToken);

    public Task<ReportJob?> ExportProjectDueDiligenceReportAsync(string projectUuid, CancellationToken cancellationToken = default) =>
        _client.PostAsync<ReportJob>(
            $"/api/v3.0/projects/{Uri.EscapeDataString(projectUuid)}/dependencies/reports/dueDiligence",
            cancellationToken: cancellationToken);

    public Task<ReportJob?> ExportProjectFindingsReportAsync(string projectUuid, CancellationToken cancellationToken = default) =>
        _client.PostAsync<ReportJob>(
            $"/api/v3.0/projects/{Uri.EscapeDataString(projectUuid)}/code/reports/findings",
            cancellationToken: cancellationToken);

    public Task<ReportJob?> ExportProjectComplianceReportAsync(string projectUuid, CancellationToken cancellationToken = default) =>
        _client.PostAsync<ReportJob>(
            $"/api/v3.0/projects/{Uri.EscapeDataString(projectUuid)}/code/reports/compliance",
            cancellationToken: cancellationToken);

    public Task<ReportJob?> ExportApplicationSbomReportAsync(string applicationUuid, CancellationToken cancellationToken = default) =>
        _client.PostAsync<ReportJob>(
            $"/api/v3.0/applications/{Uri.EscapeDataString(applicationUuid)}/dependencies/reports/SBOM",
            cancellationToken: cancellationToken);

    public Task<ReportJob?> ExportApplicationDueDiligenceReportAsync(string applicationUuid, CancellationToken cancellationToken = default) =>
        _client.PostAsync<ReportJob>(
            $"/api/v3.0/applications/{Uri.EscapeDataString(applicationUuid)}/dependencies/reports/dueDiligence",
            cancellationToken: cancellationToken);

    public Task<ReportJob?> ExportOrgInventoryReportAsync(string orgUuid, CancellationToken cancellationToken = default) =>
        _client.PostAsync<ReportJob>(
            $"/api/v3.0/orgs/{Uri.EscapeDataString(orgUuid)}/dependencies/reports/inventory",
            cancellationToken: cancellationToken);

    public Task<ReportJob?> ExportOrgComplianceReportAsync(string orgUuid, CancellationToken cancellationToken = default) =>
        _client.PostAsync<ReportJob>(
            $"/api/v3.0/orgs/{Uri.EscapeDataString(orgUuid)}/code/reports/compliance",
            cancellationToken: cancellationToken);
}
