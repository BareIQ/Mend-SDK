using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.AiVulnerabilities.Models;

namespace Mend.Sdk.AiVulnerabilities;

public interface IMendAiVulnerabilitiesClient
{
    Task<IReadOnlyList<AiVulnerability>> GetProjectAiVulnerabilitiesAsync(
        string projectUuid, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);

    Task<AiVulnerability?> GetProjectAiVulnerabilityAsync(
        string projectUuid, string vulnerabilityId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiVulnerability>> GetApplicationAiVulnerabilitiesAsync(
        string applicationUuid, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);

    Task<AiVulnerability?> GetApplicationAiVulnerabilityAsync(
        string applicationUuid, string vulnerabilityId, CancellationToken cancellationToken = default);
}
