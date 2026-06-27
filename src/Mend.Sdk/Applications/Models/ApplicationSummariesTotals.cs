using System.Text.Json.Serialization;

namespace Mend.Sdk.Applications.Models;

public sealed class ApplicationSummariesTotals
{
    [JsonPropertyName("totalApplications")]
    public int TotalApplications { get; set; }

    [JsonPropertyName("highSeverityCount")]
    public int HighSeverityCount { get; set; }

    [JsonPropertyName("mediumSeverityCount")]
    public int MediumSeverityCount { get; set; }

    [JsonPropertyName("lowSeverityCount")]
    public int LowSeverityCount { get; set; }
}
