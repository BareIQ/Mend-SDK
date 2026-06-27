using System.Text.Json.Serialization;

namespace Mend.Sdk.Applications.Models;

public sealed class ApplicationStatistics
{
    [JsonPropertyName("ALERTS")]
    public AlertsStatistics? Alerts { get; set; }

    [JsonPropertyName("GENERAL")]
    public GeneralStatistics? General { get; set; }

    [JsonPropertyName("LICENSE_RISK")]
    public LicenseRiskStatistics? LicenseRisk { get; set; }

    [JsonPropertyName("VULNERABILITY_SEVERITY_LIBRARIES")]
    public VulnerabilitySeverityLibrariesStatistics? VulnerabilitySeverityLibraries { get; set; }

    [JsonPropertyName("LAST_SCAN")]
    public LastScanStatistics? LastScan { get; set; }

    [JsonPropertyName("OUTDATED_LIBRARIES")]
    public OutdatedLibrariesStatistics? OutdatedLibraries { get; set; }

    [JsonPropertyName("POLICY_VIOLATION_LIBRARIES")]
    public PolicyViolationLibrariesStatistics? PolicyViolationLibraries { get; set; }

    [JsonPropertyName("SCA_VIOLATING_FINDINGS")]
    public ScaViolatingFindingsStatistics? ScaViolatingFindings { get; set; }

    [JsonPropertyName("SCA_EXPLOITABLE_VULNERABILITIES")]
    public ScaExploitableVulnerabilitiesStatistics? ScaExploitableVulnerabilities { get; set; }

    [JsonPropertyName("UNIFIED_VULNERABILITIES")]
    public UnifiedVulnerabilitiesStatistics? UnifiedVulnerabilities { get; set; }

    [JsonPropertyName("SAST_VIOLATING_FINDINGS")]
    public SastViolatingFindingsStatistics? SastViolatingFindings { get; set; }

    [JsonPropertyName("SAST_SCAN")]
    public SastScanStatistics? SastScan { get; set; }

    [JsonPropertyName("SAST_VULNERABILITIES_BY_SEVERITY")]
    public SastVulnerabilitiesBySeverityStatistics? SastVulnerabilitiesBySeverity { get; set; }

    [JsonPropertyName("AI_SECURITY")]
    public AiSecurityStatistics? AiSecurity { get; set; }

    [JsonPropertyName("AI_VIOLATING_FINDINGS")]
    public AiViolatingFindingsStatistics? AiViolatingFindings { get; set; }

    [JsonPropertyName("DAST_SECURITY")]
    public DastSecurityStatistics? DastSecurity { get; set; }

    [JsonPropertyName("IAC_SECURITY")]
    public IacSecurityStatistics? IacSecurity { get; set; }

    [JsonPropertyName("IMG_SECURITY")]
    public ImgSecurityStatistics? ImgSecurity { get; set; }

    [JsonPropertyName("IMG_VIOLATING_FINDINGS")]
    public ImgViolatingFindingsStatistics? ImgViolatingFindings { get; set; }

    [JsonPropertyName("VULNERABILITY_EFFECTIVENESS")]
    public VulnerabilityEffectivenessStatistics? VulnerabilityEffectiveness { get; set; }

    [JsonPropertyName("LLM_SECURITY")]
    public LlmSecurityStatistics? LlmSecurity { get; set; }
}
