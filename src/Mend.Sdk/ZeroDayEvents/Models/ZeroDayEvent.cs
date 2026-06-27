using System;
using System.Text.Json.Serialization;

namespace Mend.Sdk.ZeroDayEvents.Models;

public sealed class ZeroDayEvent
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("publishedAt")]
    public DateTimeOffset PublishedAt { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}
