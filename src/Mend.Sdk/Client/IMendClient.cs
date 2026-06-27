using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mend.Sdk.Client;

public interface IMendClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default);
    Task<Stream> GetStreamAsync(string path, CancellationToken cancellationToken = default);
    Task<T?> PostAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default);
    Task PostAsync(string path, object? body = null, CancellationToken cancellationToken = default);
    Task<T?> PutAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default);
    Task<T?> PatchAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default);
    Task PatchAsync(string path, object? body = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
    Task<T?> GetPagedAsync<T>(string path, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default);
    Task<T?> PostPagedAsync<T>(string path, object? body = null, int? limit = null, string? cursor = null, CancellationToken cancellationToken = default);
}
