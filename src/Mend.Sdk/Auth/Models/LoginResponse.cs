namespace Mend.Sdk.Auth.Models;

internal sealed class LoginResponse
{
    public string? RefreshToken { get; set; }
    public string? UserUuid { get; set; }
    public long JwtTtl { get; set; }
}
