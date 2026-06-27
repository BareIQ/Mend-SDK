using System.Text.Json.Serialization;

namespace Mend.Sdk.Dependencies.Models;

public sealed class UpdateSecurityFindingRequest
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("comments")]
    public string? Comments { get; set; }
}
