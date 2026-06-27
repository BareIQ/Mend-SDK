using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Scans.Models;

namespace Mend.Sdk.Scans;

public interface IMendScansClient
{
    Task<IReadOnlyList<Scan>> GetScansAsync(string projectUuid, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);
    Task<ScanDetail?> GetScanAsync(string projectUuid, string scanUuid, CancellationToken cancellationToken = default);
    Task<ScanSummary?> GetScanSummaryAsync(string projectUuid, string scanUuid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScanTag>> GetScanTagsAsync(string projectUuid, string scanUuid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SbomLog>> GetScanSbomLogsAsync(string projectUuid, string scanUuid, CancellationToken cancellationToken = default);
}
