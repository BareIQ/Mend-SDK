using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.ZeroDayEvents.Models;

namespace Mend.Sdk.ZeroDayEvents;

public interface IMendZeroDayEventsClient
{
    Task<IReadOnlyList<ZeroDayEvent>> GetZeroDayEventsAsync(
        string orgUuid, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ZeroDayEventFinding>> GetZeroDayEventFindingsAsync(
        string orgUuid, string eventUuid, CancellationToken cancellationToken = default);
}
