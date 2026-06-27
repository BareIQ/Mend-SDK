using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.ReportExports.Models;

namespace Mend.Sdk.ReportExports;

public interface IMendReportExportsClient
{
    Task<ReportJob?> ExportProjectSbomReportAsync(string projectUuid, CancellationToken cancellationToken = default);
    Task<ReportJob?> ExportProjectDueDiligenceReportAsync(string projectUuid, CancellationToken cancellationToken = default);
    Task<ReportJob?> ExportProjectFindingsReportAsync(string projectUuid, CancellationToken cancellationToken = default);
    Task<ReportJob?> ExportProjectComplianceReportAsync(string projectUuid, CancellationToken cancellationToken = default);
    Task<ReportJob?> ExportApplicationSbomReportAsync(string applicationUuid, CancellationToken cancellationToken = default);
    Task<ReportJob?> ExportApplicationDueDiligenceReportAsync(string applicationUuid, CancellationToken cancellationToken = default);
    Task<ReportJob?> ExportOrgInventoryReportAsync(string orgUuid, CancellationToken cancellationToken = default);
    Task<ReportJob?> ExportOrgComplianceReportAsync(string orgUuid, CancellationToken cancellationToken = default);
}
