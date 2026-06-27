using System.Text.Json.Serialization;

namespace Mend.Sdk.Projects.Models;

public sealed class ProjectSummariesTotals
{
    [JsonPropertyName("totalProjects")]
    public int TotalProjects { get; set; }

    [JsonPropertyName("highSeverityCount")]
    public int HighSeverityCount { get; set; }

    [JsonPropertyName("mediumSeverityCount")]
    public int MediumSeverityCount { get; set; }

    [JsonPropertyName("lowSeverityCount")]
    public int LowSeverityCount { get; set; }
}
