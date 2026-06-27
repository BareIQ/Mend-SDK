using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mend.Sdk.Http;

public interface IMendHttpClient
{
    Task<T?> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default);
    Task SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    Task<Stream> GetStreamAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}
