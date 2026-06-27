using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Projects.Models;

namespace Mend.Sdk.Projects;

public interface IMendProjectsClient
{
    Task<IReadOnlyList<Project>> GetProjectsAsync(string? applicationUuid = null, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectSummary>> GetProjectSummariesAsync(ProjectSummariesRequest request, CancellationToken cancellationToken = default);
    Task<ProjectSummariesTotals?> GetProjectSummariesTotalsAsync(CancellationToken cancellationToken = default);
}
