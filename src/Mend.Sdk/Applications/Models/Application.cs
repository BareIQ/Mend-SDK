using System.Text.Json.Serialization;

namespace Mend.Sdk.Applications.Models;

public sealed class Application
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
