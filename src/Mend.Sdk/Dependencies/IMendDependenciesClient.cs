using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Dependencies.Models;

namespace Mend.Sdk.Dependencies;

public interface IMendDependenciesClient
{
    Task<IReadOnlyList<SecurityFinding>> GetDependencySecurityFindingsAsync(
        string projectUuid, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LibraryGroup>> GetDependencySecurityFindingsByLibraryAsync(
        string projectUuid, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RootLibraryGroup>> GetDependencySecurityFindingsByRootLibraryAsync(
        string projectUuid, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);

    Task UpdateDependencySecurityFindingAsync(
        string projectUuid, string rootLibraryUuid, UpdateSecurityFindingRequest request, CancellationToken cancellationToken = default);
}
