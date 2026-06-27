using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Auth;
using Mend.Sdk.Http;

namespace Mend.Sdk.Client;

public sealed class MendClient : IMendClient
{
    private static readonly JsonSerializerOptions BodySerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly HttpMethod PatchMethod = new HttpMethod("PATCH");

    private readonly IMendTokenManager _tokenManager;
    private readonly IMendHttpClient _httpClient;

    public MendClient(IMendTokenManager tokenManager, IMendHttpClient httpClient)
    {
        _tokenManager = tokenManager;
        _httpClient = httpClient;
    }

    public Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default)
        => SendWithResponseAsync<T>(HttpMethod.Get, path, null, cancellationToken);

    public Task<Stream> GetStreamAsync(string path, CancellationToken cancellationToken = default)
        => SendStreamAsync(path, cancellationToken);

    public Task<T?> PostAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default)
        => SendWithResponseAsync<T>(HttpMethod.Post, path, body, cancellationToken);

    public Task PostAsync(string path, object? body = null, CancellationToken cancellationToken = default)
        => SendVoidAsync(HttpMethod.Post, path, body, cancellationToken);

    public Task<T?> PutAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default)
        => SendWithResponseAsync<T>(HttpMethod.Put, path, body, cancellationToken);

    public Task<T?> PatchAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default)
        => SendWithResponseAsync<T>(PatchMethod, path, body, cancellationToken);

    public Task PatchAsync(string path, object? body = null, CancellationToken cancellationToken = default)
        => SendVoidAsync(PatchMethod, path, body, cancellationToken);

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        => SendVoidAsync(HttpMethod.Delete, path, null, cancellationToken);

    public Task<T?> GetPagedAsync<T>(string path, int? pageSize = null, string? cursor = null, CancellationToken cancellationToken = default)
        => GetAsync<T>(BuildPagedPath(path, pageSize, cursor), cancellationToken);

    private async Task<T?> SendWithResponseAsync<T>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        var token = await _tokenManager.GetAccessTokenAsync().ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(method, path, token, body);
        var envelope = await _httpClient.SendAsync<ApiEnvelope<T>>(request, cancellationToken).ConfigureAwait(false);
        return envelope != null ? envelope.Response : default;
    }

    private async Task SendVoidAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        var token = await _tokenManager.GetAccessTokenAsync().ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(method, path, token, body);
        await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Stream> SendStreamAsync(string path, CancellationToken cancellationToken)
    {
        var token = await _tokenManager.GetAccessTokenAsync().ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(HttpMethod.Get, path, token, null);
        return await _httpClient.GetStreamAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string path, string token, object? body)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, BodySerializerOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private static string BuildPagedPath(string path, int? pageSize, string? cursor)
    {
        var hasPageSize = pageSize.HasValue;
        var hasCursor = !string.IsNullOrEmpty(cursor);

        if (!hasPageSize && !hasCursor)
            return path;

        var separator = path.IndexOf('?') >= 0 ? "&" : "?";
        var sb = new StringBuilder(path);

        if (hasPageSize)
        {
            sb.Append(separator).Append("pageSize=").Append(pageSize!.Value);
            separator = "&";
        }

        if (hasCursor)
            sb.Append(separator).Append("cursor=").Append(Uri.EscapeDataString(cursor!));

        return sb.ToString();
    }
}
