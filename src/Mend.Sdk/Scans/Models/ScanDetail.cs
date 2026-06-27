using System.Text.Json.Serialization;

namespace Mend.Sdk.Scans.Models;

public sealed class ScanDetail
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("projectUuid")]
    public string ProjectUuid { get; set; } = string.Empty;
}
