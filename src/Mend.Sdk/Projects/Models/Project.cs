using System.Text.Json.Serialization;

namespace Mend.Sdk.Projects.Models;

public sealed class Project
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("applicationUuid")]
    public string ApplicationUuid { get; set; } = string.Empty;
}
