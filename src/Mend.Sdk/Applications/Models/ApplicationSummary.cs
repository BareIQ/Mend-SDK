using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mend.Sdk.Applications.Models;

public sealed class ApplicationSummary
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("creationDate")]
    public string CreationDate { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    [JsonPropertyName("labels")]
    public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();

    [JsonPropertyName("statistics")]
    public ApplicationStatistics? Statistics { get; set; }
}
