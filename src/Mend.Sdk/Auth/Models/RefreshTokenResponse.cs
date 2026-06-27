using System.Text.Json.Serialization;

namespace Mend.Sdk.Auth.Models;

internal sealed class RefreshTokenResponse
{
    [JsonPropertyName("jwtToken")]
    public string? JwtToken { get; set; }

    [JsonPropertyName("tokenTTL")]
    public long TokenTtl { get; set; }
}
