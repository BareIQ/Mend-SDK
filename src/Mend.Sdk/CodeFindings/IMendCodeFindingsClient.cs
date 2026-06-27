using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.CodeFindings.Models;

namespace Mend.Sdk.CodeFindings;

public interface IMendCodeFindingsClient
{
    Task<IReadOnlyList<CodeFinding>> GetCodeFindingsAsync(
        string projectUuid, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);

    Task<CodeFinding?> GetCodeFindingAsync(
        string projectUuid, string findingUuid, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CodeFinding>> GetScanCodeFindingsAsync(
        string projectUuid, string scanUuid, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);

    Task<CodeFinding?> GetScanCodeFindingAsync(
        string projectUuid, string scanUuid, string findingUuid, CancellationToken cancellationToken = default);
}
