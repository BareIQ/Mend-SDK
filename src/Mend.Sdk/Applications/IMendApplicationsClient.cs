using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Applications.Models;

namespace Mend.Sdk.Applications;

public interface IMendApplicationsClient
{
    Task<IReadOnlyList<Application>> GetApplicationsAsync(int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationSummary>> GetApplicationSummariesAsync(ApplicationSummariesRequest request, int? limit = null, string? cursor = null, CancellationToken cancellationToken = default);
    Task<ApplicationSummariesTotals?> GetApplicationSummariesTotalsAsync(CancellationToken cancellationToken = default);
}
