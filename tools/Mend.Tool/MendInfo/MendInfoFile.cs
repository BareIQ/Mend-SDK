using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mend.Tool.MendInfo;

internal sealed class MendInfo
{
    [JsonPropertyName("projectId")]
    public string? ProjectId { get; set; }

    [JsonPropertyName("projectName")]
    public string? ProjectName { get; set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(ProjectId) || !string.IsNullOrWhiteSpace(ProjectName);
}

internal static class MendInfoReader
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static MendInfo? TryRead(string directory)
    {
        var path = Path.Combine(directory, ".mend-info");
        if (!File.Exists(path))
            return null;

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MendInfo>(json, Options);
    }
}
