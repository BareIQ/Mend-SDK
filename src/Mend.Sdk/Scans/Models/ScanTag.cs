using System.Text.Json.Serialization;

namespace Mend.Sdk.Scans.Models;

public sealed class ScanTag
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}
