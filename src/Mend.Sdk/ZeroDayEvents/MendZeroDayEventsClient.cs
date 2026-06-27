using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Client;
using Mend.Sdk.ZeroDayEvents.Models;

namespace Mend.Sdk.ZeroDayEvents;

public sealed class MendZeroDayEventsClient : IMendZeroDayEventsClient
{
    private readonly IMendClient _client;

    public MendZeroDayEventsClient(IMendClient client)
    {
        _client = client;
    }

    private static string ZeroDayEventsBasePath(string orgUuid) =>
        $"/api/v3.0/orgs/{Uri.EscapeDataString(orgUuid)}/dependencies/events/zeroday";

    public async Task<IReadOnlyList<ZeroDayEvent>> GetZeroDayEventsAsync(
        string orgUuid,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var path = BuildZeroDayEventsPath(orgUuid, fromDate, toDate);
        var response = await _client.GetAsync<IReadOnlyList<ZeroDayEvent>>(path, cancellationToken).ConfigureAwait(false);
        return response ?? Array.Empty<ZeroDayEvent>();
    }

    public async Task<IReadOnlyList<ZeroDayEventFinding>> GetZeroDayEventFindingsAsync(
        string orgUuid,
        string eventUuid,
        CancellationToken cancellationToken = default)
    {
        var path = $"{ZeroDayEventsBasePath(orgUuid)}/{Uri.EscapeDataString(eventUuid)}/findings";
        var response = await _client.GetAsync<IReadOnlyList<ZeroDayEventFinding>>(path, cancellationToken).ConfigureAwait(false);
        return response ?? Array.Empty<ZeroDayEventFinding>();
    }

    private static string BuildZeroDayEventsPath(string orgUuid, DateTime? fromDate, DateTime? toDate)
    {
        var basePath = ZeroDayEventsBasePath(orgUuid);
        var hasFrom = fromDate.HasValue;
        var hasTo = toDate.HasValue;

        if (!hasFrom && !hasTo)
            return basePath;

        var sb = new StringBuilder(basePath).Append('?');

        if (hasFrom)
        {
            sb.Append("fromDate=").Append(fromDate!.Value.ToString("yyyy-MM-dd"));
            if (hasTo)
                sb.Append('&');
        }

        if (hasTo)
            sb.Append("toDate=").Append(toDate!.Value.ToString("yyyy-MM-dd"));

        return sb.ToString();
    }
}
