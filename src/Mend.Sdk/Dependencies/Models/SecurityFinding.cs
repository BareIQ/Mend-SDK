using System;
using System.Collections.Generic;
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

    [JsonPropertyName("project")]
    public SecurityFindingProject? Project { get; set; }

    [JsonPropertyName("application")]
    public SecurityFindingApplication? Application { get; set; }

    [JsonPropertyName("component")]
    public SecurityFindingComponent? Component { get; set; }

    [JsonPropertyName("vulnerability")]
    public SecurityFindingVulnerability? Vulnerability { get; set; }

    [JsonPropertyName("topFix")]
    public SecurityFindingTopFix? TopFix { get; set; }

    [JsonPropertyName("effective")]
    public string Effective { get; set; } = string.Empty;

    [JsonPropertyName("reachability")]
    public string Reachability { get; set; } = string.Empty;

    [JsonPropertyName("threatAssessment")]
    public SecurityFindingThreatAssessment? ThreatAssessment { get; set; }

    [JsonPropertyName("exploitable")]
    public bool Exploitable { get; set; }

    [JsonPropertyName("malicious")]
    public bool Malicious { get; set; }

    [JsonPropertyName("scoreMetadataVector")]
    public string ScoreMetadataVector { get; set; } = string.Empty;

    [JsonPropertyName("violations")]
    public int Violations { get; set; }

    [JsonPropertyName("workflowUuids")]
    public IReadOnlyList<string> WorkflowUuids { get; set; } = Array.Empty<string>();

    [JsonPropertyName("dependencyContexts")]
    public IReadOnlyList<SecurityFindingDependencyContext> DependencyContexts { get; set; } = Array.Empty<SecurityFindingDependencyContext>();

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

    [JsonPropertyName("detectedAt")]
    public string DetectedAt { get; set; } = string.Empty;

    [JsonPropertyName("modifiedAt")]
    public string ModifiedAt { get; set; } = string.Empty;
}

public sealed class SecurityFindingProject
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("applicationUuid")]
    public string ApplicationUuid { get; set; } = string.Empty;
}

public sealed class SecurityFindingApplication
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class SecurityFindingComponent
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("componentType")]
    public string ComponentType { get; set; } = string.Empty;

    [JsonPropertyName("libraryType")]
    public string LibraryType { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("references")]
    public SecurityFindingComponentReferences? References { get; set; }

    [JsonPropertyName("groupId")]
    public string GroupId { get; set; } = string.Empty;

    [JsonPropertyName("artifactId")]
    public string ArtifactId { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("directDependency")]
    public bool DirectDependency { get; set; }

    [JsonPropertyName("rootLibrary")]
    public bool RootLibrary { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("dependencyFile")]
    public string DependencyFile { get; set; } = string.Empty;

    [JsonPropertyName("dependencyType")]
    public string DependencyType { get; set; } = string.Empty;
}

public sealed class SecurityFindingComponentReferences
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("homePage")]
    public string HomePage { get; set; } = string.Empty;

    [JsonPropertyName("genericPackageIndex")]
    public string GenericPackageIndex { get; set; } = string.Empty;
}

public sealed class SecurityFindingVulnerability
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    [JsonPropertyName("publishDate")]
    public string PublishDate { get; set; } = string.Empty;

    [JsonPropertyName("modifiedDate")]
    public string ModifiedDate { get; set; } = string.Empty;

    [JsonPropertyName("vulnerabilityScoring")]
    public IReadOnlyList<SecurityFindingVulnerabilityScore> VulnerabilityScoring { get; set; } = Array.Empty<SecurityFindingVulnerabilityScore>();
}

public sealed class SecurityFindingVulnerabilityScore
{
    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public sealed class SecurityFindingTopFix
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("vulnerability")]
    public string Vulnerability { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("origin")]
    public string Origin { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("fixResolution")]
    public string FixResolution { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public sealed class SecurityFindingThreatAssessment
{
    [JsonPropertyName("exploitCodeMaturity")]
    public string ExploitCodeMaturity { get; set; } = string.Empty;

    [JsonPropertyName("epssPercentage")]
    public double EpssPercentage { get; set; }
}

public sealed class SecurityFindingDependencyContext
{
    [JsonPropertyName("dependencyType")]
    public string DependencyType { get; set; } = string.Empty;

    [JsonPropertyName("isDirect")]
    public bool IsDirect { get; set; }

    [JsonPropertyName("isTransitive")]
    public bool IsTransitive { get; set; }

    [JsonPropertyName("directRoots")]
    public IReadOnlyList<SecurityFindingDependencyRoot> DirectRoots { get; set; } = Array.Empty<SecurityFindingDependencyRoot>();
}

public sealed class SecurityFindingDependencyRoot
{
    [JsonPropertyName("rootLibraryUuid")]
    public string RootLibraryUuid { get; set; } = string.Empty;

    [JsonPropertyName("rootLibraryName")]
    public string RootLibraryName { get; set; } = string.Empty;

    [JsonPropertyName("rootLibraryVersion")]
    public string RootLibraryVersion { get; set; } = string.Empty;
}
