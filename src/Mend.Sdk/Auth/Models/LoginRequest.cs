namespace Mend.Sdk.Auth.Models;

internal sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserKey { get; set; } = string.Empty;
    public string OrgToken { get; set; } = string.Empty;
}
