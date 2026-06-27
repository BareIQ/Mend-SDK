using System.Text.Json.Serialization;

namespace Mend.Sdk.ReportExports.Models;

public sealed class ReportJob
{
    [JsonPropertyName("uuid")]
    public string ReportUuid { get; set; } = string.Empty;
}
