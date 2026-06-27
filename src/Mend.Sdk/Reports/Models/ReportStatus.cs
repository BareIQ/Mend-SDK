using System.Text.Json.Serialization;

namespace Mend.Sdk.Reports.Models;

public sealed class ReportStatus
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public ReportState State { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;
}
