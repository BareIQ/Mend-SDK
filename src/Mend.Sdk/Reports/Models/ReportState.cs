using System.Text.Json.Serialization;

namespace Mend.Sdk.Reports.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportState
{
    Pending,
    Complete,
    Failed
}
