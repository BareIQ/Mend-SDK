using System.Text.Json.Serialization;

namespace Mend.Sdk.Applications.Models;

public sealed class AlertsStatistics
{
    [JsonPropertyName("criticalSeverityVulnerabilities")]
    public int CriticalSeverityVulnerabilities { get; set; }

    [JsonPropertyName("highSeverityVulnerabilities")]
    public int HighSeverityVulnerabilities { get; set; }

    [JsonPropertyName("mediumSeverityVulnerabilities")]
    public int MediumSeverityVulnerabilities { get; set; }

    [JsonPropertyName("lowSeverityVulnerabilities")]
    public int LowSeverityVulnerabilities { get; set; }

    [JsonPropertyName("vulnerableLibraries")]
    public int VulnerableLibraries { get; set; }
}

public sealed class GeneralStatistics
{
    [JsonPropertyName("totalLibraries")]
    public int TotalLibraries { get; set; }

    [JsonPropertyName("totalProjects")]
    public int TotalProjects { get; set; }
}

public sealed class LicenseRiskStatistics
{
    [JsonPropertyName("noLicenseLibraries")]
    public int NoLicenseLibraries { get; set; }

    [JsonPropertyName("unknownRiskLicenses")]
    public int UnknownRiskLicenses { get; set; }

    [JsonPropertyName("highRiskLicenses")]
    public int HighRiskLicenses { get; set; }

    [JsonPropertyName("mediumRiskLicenses")]
    public int MediumRiskLicenses { get; set; }

    [JsonPropertyName("lowRiskLicenses")]
    public int LowRiskLicenses { get; set; }
}

public sealed class VulnerabilitySeverityLibrariesStatistics
{
    [JsonPropertyName("criticalSeverityLibraries")]
    public int CriticalSeverityLibraries { get; set; }

    [JsonPropertyName("highSeverityLibraries")]
    public int HighSeverityLibraries { get; set; }

    [JsonPropertyName("mediumSeverityLibraries")]
    public int MediumSeverityLibraries { get; set; }

    [JsonPropertyName("lowSeverityLibraries")]
    public int LowSeverityLibraries { get; set; }
}

public sealed class LastScanStatistics
{
    [JsonPropertyName("lastScanTime")]
    public long LastScanTime { get; set; }

    [JsonPropertyName("lastScaScanTime")]
    public long LastScaScanTime { get; set; }

    [JsonPropertyName("lastImgScanTime")]
    public long LastImgScanTime { get; set; }

    [JsonPropertyName("lastSastScanTime")]
    public long LastSastScanTime { get; set; }
}

public sealed class OutdatedLibrariesStatistics
{
    [JsonPropertyName("outdatedLibraries")]
    public int OutdatedLibraries { get; set; }
}

public sealed class PolicyViolationLibrariesStatistics
{
    [JsonPropertyName("policyViolatingLibraries")]
    public int PolicyViolatingLibraries { get; set; }
}

public sealed class ScaViolatingFindingsStatistics
{
    [JsonPropertyName("scaCriticalViolatingFindings")]
    public int CriticalViolatingFindings { get; set; }

    [JsonPropertyName("scaHighViolatingFindings")]
    public int HighViolatingFindings { get; set; }

    [JsonPropertyName("scaMediumViolatingFindings")]
    public int MediumViolatingFindings { get; set; }

    [JsonPropertyName("scaLowViolatingFindings")]
    public int LowViolatingFindings { get; set; }

    [JsonPropertyName("scaViolatingFindings")]
    public int TotalViolatingFindings { get; set; }
}

public sealed class ScaExploitableVulnerabilitiesStatistics
{
    [JsonPropertyName("scaExploitableVulnerabilitiesCritical")]
    public int Critical { get; set; }

    [JsonPropertyName("scaExploitableVulnerabilitiesHigh")]
    public int High { get; set; }

    [JsonPropertyName("scaExploitableVulnerabilitiesMedium")]
    public int Medium { get; set; }

    [JsonPropertyName("scaExploitableVulnerabilitiesLow")]
    public int Low { get; set; }

    [JsonPropertyName("scaExploitableVulnerabilitiesTotal")]
    public int Total { get; set; }
}

public sealed class UnifiedVulnerabilitiesStatistics
{
    [JsonPropertyName("unifiedCriticalVulnerabilities")]
    public int Critical { get; set; }

    [JsonPropertyName("unifiedHighVulnerabilities")]
    public int High { get; set; }

    [JsonPropertyName("unifiedMediumVulnerabilities")]
    public int Medium { get; set; }

    [JsonPropertyName("unifiedLowVulnerabilities")]
    public int Low { get; set; }

    [JsonPropertyName("unifiedVulnerabilities")]
    public int Total { get; set; }
}

public sealed class SastViolatingFindingsStatistics
{
    [JsonPropertyName("sastCriticalViolatingFindings")]
    public int CriticalViolatingFindings { get; set; }

    [JsonPropertyName("sastHighViolatingFindings")]
    public int HighViolatingFindings { get; set; }

    [JsonPropertyName("sastMediumViolatingFindings")]
    public int MediumViolatingFindings { get; set; }

    [JsonPropertyName("sastLowViolatingFindings")]
    public int LowViolatingFindings { get; set; }

    [JsonPropertyName("sastViolatingFindings")]
    public int TotalViolatingFindings { get; set; }
}

public sealed class SastScanStatistics
{
    [JsonPropertyName("sastTotalFiles")]
    public int TotalFiles { get; set; }

    [JsonPropertyName("sastTestedFiles")]
    public int TestedFiles { get; set; }

    [JsonPropertyName("sastTotalLines")]
    public long TotalLines { get; set; }

    [JsonPropertyName("sastTestedLines")]
    public long TestedLines { get; set; }

    [JsonPropertyName("sastTotalRemediations")]
    public int TotalRemediations { get; set; }

    [JsonPropertyName("sastTotalMended")]
    public int TotalMended { get; set; }
}

public sealed class SastVulnerabilitiesBySeverityStatistics
{
    [JsonPropertyName("sastCriticalVulnerabilities")]
    public int Critical { get; set; }

    [JsonPropertyName("sastHighVulnerabilities")]
    public int High { get; set; }

    [JsonPropertyName("sastMediumVulnerabilities")]
    public int Medium { get; set; }

    [JsonPropertyName("sastLowVulnerabilities")]
    public int Low { get; set; }

    [JsonPropertyName("sastVulnerabilities")]
    public int Total { get; set; }
}

public sealed class AiSecurityStatistics
{
    [JsonPropertyName("aiCriticalVulnerabilities")]
    public int CriticalVulnerabilities { get; set; }

    [JsonPropertyName("aiHighVulnerabilities")]
    public int HighVulnerabilities { get; set; }

    [JsonPropertyName("aiMediumVulnerabilities")]
    public int MediumVulnerabilities { get; set; }

    [JsonPropertyName("aiLowVulnerabilities")]
    public int LowVulnerabilities { get; set; }

    [JsonPropertyName("aiTotalVulnerabilities")]
    public int TotalVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentsTotal")]
    public int AgentsTotal { get; set; }

    [JsonPropertyName("aiAgentsVulnerable")]
    public int AgentsVulnerable { get; set; }

    [JsonPropertyName("aiAgentsTotalVulnerabilities")]
    public int AgentsTotalVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentsCriticalVulnerabilities")]
    public int AgentsCriticalVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentsHighVulnerabilities")]
    public int AgentsHighVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentsMediumVulnerabilities")]
    public int AgentsMediumVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentsLowVulnerabilities")]
    public int AgentsLowVulnerabilities { get; set; }

    [JsonPropertyName("aiSystemPromptsTotal")]
    public int SystemPromptsTotal { get; set; }

    [JsonPropertyName("aiSystemPromptsVulnerable")]
    public int SystemPromptsVulnerable { get; set; }

    [JsonPropertyName("aiSystemPromptsTotalVulnerabilities")]
    public int SystemPromptsTotalVulnerabilities { get; set; }

    [JsonPropertyName("aiSystemPromptsCriticalVulnerabilities")]
    public int SystemPromptsCriticalVulnerabilities { get; set; }

    [JsonPropertyName("aiSystemPromptsHighVulnerabilities")]
    public int SystemPromptsHighVulnerabilities { get; set; }

    [JsonPropertyName("aiSystemPromptsMediumVulnerabilities")]
    public int SystemPromptsMediumVulnerabilities { get; set; }

    [JsonPropertyName("aiSystemPromptsLowVulnerabilities")]
    public int SystemPromptsLowVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentConfigurationsTotal")]
    public int AgentConfigurationsTotal { get; set; }

    [JsonPropertyName("aiAgentConfigurationsVulnerable")]
    public int AgentConfigurationsVulnerable { get; set; }

    [JsonPropertyName("aiAgentConfigurationsTotalVulnerabilities")]
    public int AgentConfigurationsTotalVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentConfigurationsCriticalVulnerabilities")]
    public int AgentConfigurationsCriticalVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentConfigurationsHighVulnerabilities")]
    public int AgentConfigurationsHighVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentConfigurationsMediumVulnerabilities")]
    public int AgentConfigurationsMediumVulnerabilities { get; set; }

    [JsonPropertyName("aiAgentConfigurationsLowVulnerabilities")]
    public int AgentConfigurationsLowVulnerabilities { get; set; }
}

public sealed class AiViolatingFindingsStatistics
{
    [JsonPropertyName("aiCriticalViolatingFindings")]
    public int CriticalViolatingFindings { get; set; }

    [JsonPropertyName("aiHighViolatingFindings")]
    public int HighViolatingFindings { get; set; }

    [JsonPropertyName("aiMediumViolatingFindings")]
    public int MediumViolatingFindings { get; set; }

    [JsonPropertyName("aiLowViolatingFindings")]
    public int LowViolatingFindings { get; set; }

    [JsonPropertyName("aiViolatingFindings")]
    public int TotalViolatingFindings { get; set; }
}

public sealed class DastSecurityStatistics
{
    [JsonPropertyName("dastCriticalVulnerabilities")]
    public int CriticalVulnerabilities { get; set; }

    [JsonPropertyName("dastHighVulnerabilities")]
    public int HighVulnerabilities { get; set; }

    [JsonPropertyName("dastMediumVulnerabilities")]
    public int MediumVulnerabilities { get; set; }

    [JsonPropertyName("dastLowVulnerabilities")]
    public int LowVulnerabilities { get; set; }

    [JsonPropertyName("dastTotalVulnerabilities")]
    public int TotalVulnerabilities { get; set; }
}

public sealed class IacSecurityStatistics
{
    [JsonPropertyName("iacCriticalMisconfigurations")]
    public int CriticalMisconfigurations { get; set; }

    [JsonPropertyName("iacHighMisconfigurations")]
    public int HighMisconfigurations { get; set; }

    [JsonPropertyName("iacMediumMisconfigurations")]
    public int MediumMisconfigurations { get; set; }

    [JsonPropertyName("iacLowMisconfigurations")]
    public int LowMisconfigurations { get; set; }

    [JsonPropertyName("iacTotalMisconfigurations")]
    public int TotalMisconfigurations { get; set; }
}

public sealed class ImgSecurityStatistics
{
    [JsonPropertyName("imgCriticalVulnerabilities")]
    public int CriticalVulnerabilities { get; set; }

    [JsonPropertyName("imgHighVulnerabilities")]
    public int HighVulnerabilities { get; set; }

    [JsonPropertyName("imgMediumVulnerabilities")]
    public int MediumVulnerabilities { get; set; }

    [JsonPropertyName("imgLowVulnerabilities")]
    public int LowVulnerabilities { get; set; }

    [JsonPropertyName("imgUnknownVulnerabilities")]
    public int UnknownVulnerabilities { get; set; }

    [JsonPropertyName("imgTotalVulnerabilities")]
    public int TotalVulnerabilities { get; set; }

    [JsonPropertyName("imgSecretCriticalVulnerabilities")]
    public int SecretCriticalVulnerabilities { get; set; }

    [JsonPropertyName("imgSecretHighVulnerabilities")]
    public int SecretHighVulnerabilities { get; set; }

    [JsonPropertyName("imgSecretMediumVulnerabilities")]
    public int SecretMediumVulnerabilities { get; set; }

    [JsonPropertyName("imgSecretLowVulnerabilities")]
    public int SecretLowVulnerabilities { get; set; }

    [JsonPropertyName("imgMaxRiskScore")]
    public double MaxRiskScore { get; set; }
}

public sealed class ImgViolatingFindingsStatistics
{
    [JsonPropertyName("imgCriticalViolatingFindings")]
    public int CriticalViolatingFindings { get; set; }

    [JsonPropertyName("imgHighViolatingFindings")]
    public int HighViolatingFindings { get; set; }

    [JsonPropertyName("imgMediumViolatingFindings")]
    public int MediumViolatingFindings { get; set; }

    [JsonPropertyName("imgLowViolatingFindings")]
    public int LowViolatingFindings { get; set; }

    [JsonPropertyName("imgViolatingFindings")]
    public int TotalViolatingFindings { get; set; }
}

public sealed class VulnerabilityEffectivenessStatistics
{
    [JsonPropertyName("greenShieldTotalSeverityVulnerabilities")]
    public int GreenShieldTotal { get; set; }

    [JsonPropertyName("greenShieldCriticalSeverityVulnerabilities")]
    public int GreenShieldCritical { get; set; }

    [JsonPropertyName("greenShieldHighSeverityVulnerabilities")]
    public int GreenShieldHigh { get; set; }

    [JsonPropertyName("greenShieldMediumSeverityVulnerabilities")]
    public int GreenShieldMedium { get; set; }

    [JsonPropertyName("greenShieldLowSeverityVulnerabilities")]
    public int GreenShieldLow { get; set; }

    [JsonPropertyName("yellowShieldTotalSeverityVulnerabilities")]
    public int YellowShieldTotal { get; set; }

    [JsonPropertyName("yellowShieldCriticalSeverityVulnerabilities")]
    public int YellowShieldCritical { get; set; }

    [JsonPropertyName("yellowShieldHighSeverityVulnerabilities")]
    public int YellowShieldHigh { get; set; }

    [JsonPropertyName("yellowShieldMediumSeverityVulnerabilities")]
    public int YellowShieldMedium { get; set; }

    [JsonPropertyName("yellowShieldLowSeverityVulnerabilities")]
    public int YellowShieldLow { get; set; }

    [JsonPropertyName("redShieldTotalSeverityVulnerabilities")]
    public int RedShieldTotal { get; set; }

    [JsonPropertyName("redShieldCriticalSeverityVulnerabilities")]
    public int RedShieldCritical { get; set; }

    [JsonPropertyName("redShieldHighSeverityVulnerabilities")]
    public int RedShieldHigh { get; set; }

    [JsonPropertyName("redShieldMediumSeverityVulnerabilities")]
    public int RedShieldMedium { get; set; }

    [JsonPropertyName("redShieldLowSeverityVulnerabilities")]
    public int RedShieldLow { get; set; }

    [JsonPropertyName("noShieldTotalSeverityVulnerabilities")]
    public int NoShieldTotal { get; set; }

    [JsonPropertyName("noShieldCriticalSeverityVulnerabilities")]
    public int NoShieldCritical { get; set; }

    [JsonPropertyName("noShieldHighSeverityVulnerabilities")]
    public int NoShieldHigh { get; set; }

    [JsonPropertyName("noShieldMediumSeverityVulnerabilities")]
    public int NoShieldMedium { get; set; }

    [JsonPropertyName("noShieldLowSeverityVulnerabilities")]
    public int NoShieldLow { get; set; }
}

public sealed class LlmSecurityStatistics
{
    [JsonPropertyName("llmTotalLines")]
    public long TotalLines { get; set; }
}
