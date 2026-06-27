using System.Text.Json.Serialization;

namespace Mend.Sdk.Applications.Models;

public sealed class ApplicationSummary
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("highSeverityCount")]
    public int HighSeverityCount { get; set; }

    [JsonPropertyName("mediumSeverityCount")]
    public int MediumSeverityCount { get; set; }

    [JsonPropertyName("lowSeverityCount")]
    public int LowSeverityCount { get; set; }
}
