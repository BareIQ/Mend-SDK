using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mend.Sdk.Applications.Models;

public sealed class ApplicationSummariesRequest
{
    [JsonPropertyName("applicationUuids")]
    public IReadOnlyList<string> ApplicationUuids { get; set; } = System.Array.Empty<string>();
}
