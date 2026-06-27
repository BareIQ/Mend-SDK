using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mend.Sdk.Projects.Models;

public sealed class ProjectSummariesRequest
{
    [JsonPropertyName("projectUuids")]
    public IReadOnlyList<string> ProjectUuids { get; set; } = System.Array.Empty<string>();
}
