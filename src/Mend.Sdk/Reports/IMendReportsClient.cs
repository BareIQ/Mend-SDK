using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Reports.Models;

namespace Mend.Sdk.Reports;

public interface IMendReportsClient
{
    Task<IReadOnlyList<ReportStatus>> GetReportsAsync(CancellationToken cancellationToken = default);
    Task<ReportStatus?> GetReportStatusAsync(string reportUuid, CancellationToken cancellationToken = default);
    Task<Stream> DownloadReportAsync(string reportUuid, CancellationToken cancellationToken = default);
    Task DeleteReportAsync(string reportUuid, CancellationToken cancellationToken = default);
}
