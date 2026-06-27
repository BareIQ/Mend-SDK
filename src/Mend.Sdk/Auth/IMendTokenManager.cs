using System.Threading.Tasks;

namespace Mend.Sdk.Auth;

public interface IMendTokenManager
{
    Task<string> GetAccessTokenAsync();
    Task LogoutAsync();
}
