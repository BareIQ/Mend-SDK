using System.Text.Json.Serialization;

namespace Mend.Sdk.Dependencies.Models;

public sealed class SecurityFinding
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("findingInfo")]
    public SecurityFindingInfo? FindingInfo { get; set; }

    [JsonPropertyName("component")]
    public SecurityFindingComponent? Component { get; set; }

    [JsonPropertyName("vulnerability")]
    public SecurityFindingVulnerability? Vulnerability { get; set; }

    [JsonIgnore]
    public string CveName => Name;

    [JsonIgnore]
    public string Severity => Vulnerability?.Severity ?? string.Empty;

    [JsonIgnore]
    public string LibraryName => Component?.Name ?? string.Empty;

    [JsonIgnore]
    public string Status => FindingInfo?.Status ?? string.Empty;
}

public sealed class SecurityFindingInfo
{
    [JsonPropertyName("findingStatus")]
    public string FindingStatus { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class SecurityFindingComponent
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;
}

public sealed class SecurityFindingVulnerability
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
