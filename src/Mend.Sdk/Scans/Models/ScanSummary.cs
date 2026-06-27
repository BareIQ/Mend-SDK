using System.Text.Json.Serialization;

namespace Mend.Sdk.Scans.Models;

public sealed class ScanSummary
{
    [JsonPropertyName("highSeverityCount")]
    public int HighSeverityCount { get; set; }

    [JsonPropertyName("mediumSeverityCount")]
    public int MediumSeverityCount { get; set; }

    [JsonPropertyName("lowSeverityCount")]
    public int LowSeverityCount { get; set; }
}
