using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mend.Sdk.Dependencies.Models;

public sealed class RootLibraryGroup
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("findings")]
    public IReadOnlyList<SecurityFinding> Findings { get; set; } = System.Array.Empty<SecurityFinding>();
}
