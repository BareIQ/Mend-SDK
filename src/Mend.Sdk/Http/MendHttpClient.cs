using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Options;
using Microsoft.Extensions.Options;

namespace Mend.Sdk.Http;

public sealed class MendHttpClient : IMendHttpClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _baseAddress;

    public MendHttpClient(IHttpClientFactory httpClientFactory, IOptions<MendOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _baseAddress = new Uri(options.Value.BaseUrl);
    }

    public async Task<T?> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(request, response).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(request, response).ConfigureAwait(false);
    }

    public async Task<Stream> GetStreamAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(request, response).ConfigureAwait(false);
        var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        return new MemoryStream(bytes);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("MendSdk");
        client.BaseAddress ??= _baseAddress;
        return client;
    }

    private static async Task EnsureSuccessAsync(HttpRequestMessage request, HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var path = request.RequestUri?.PathAndQuery ?? string.Empty;

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new MendAuthException(path);

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        throw new MendApiException(response.StatusCode, body);
    }
}
