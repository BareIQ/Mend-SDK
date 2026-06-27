using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Http;
using Mend.Sdk.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace Mend.Sdk.Tests.Exceptions;

public sealed class MendExceptionTests
{
    // --- exception hierarchy ---

    [Fact]
    public void MendAuthException_IsA_MendException()
    {
        var ex = new MendAuthException("/api/v3.0/login");
        Assert.IsAssignableFrom<MendException>(ex);
    }

    [Fact]
    public void MendApiException_IsA_MendException()
    {
        var ex = new MendApiException(HttpStatusCode.InternalServerError, "error");
        Assert.IsAssignableFrom<MendException>(ex);
    }

    // --- MendAuthException properties ---

    [Fact]
    public void MendAuthException_StoresEndpointPath()
    {
        const string path = "/api/v3.0/login";
        var ex = new MendAuthException(path);
        Assert.Equal(path, ex.EndpointPath);
    }

    [Fact]
    public void MendAuthException_MessageContainsEndpointPath()
    {
        const string path = "/api/v3.0/login";
        var ex = new MendAuthException(path);
        Assert.Contains(path, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MendAuthException_WithInnerException_PreservesInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new MendAuthException("/api/v3.0/login", inner);
        Assert.Same(inner, ex.InnerException);
    }

    // --- MendApiException properties ---

    [Fact]
    public void MendApiException_StoresStatusCodeAndBody()
    {
        const string body = "{\"error\":\"Internal Server Error\"}";
        var ex = new MendApiException(HttpStatusCode.InternalServerError, body);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Equal(body, ex.ResponseBody);
    }

    [Fact]
    public void MendApiException_MessageContainsStatusCode()
    {
        var ex = new MendApiException(HttpStatusCode.BadRequest, "bad request");
        Assert.Contains("400", ex.Message, StringComparison.Ordinal);
    }

    // --- MendHttpClient integration: 401 → MendAuthException ---

    [Fact]
    public async Task SendAsync_401Response_ThrowsMendAuthException()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = BuildClient(handler);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/v3.0/orgs/abc/projects")));

        Assert.Contains("/api/v3.0/orgs/abc/projects", ex.EndpointPath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendAsync_Typed_401Response_ThrowsMendAuthException()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = BuildClient(handler);

        await Assert.ThrowsAsync<MendAuthException>(
            () => client.SendAsync<object>(new HttpRequestMessage(HttpMethod.Get, "/api/v3.0/login")));
    }

    // --- MendHttpClient integration: 500 → MendApiException with status code ---

    [Fact]
    public async Task SendAsync_500Response_ThrowsMendApiExceptionWithStatusCode()
    {
        const string responseBody = "{\"message\":\"Internal Server Error\"}";
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
        });
        var client = BuildClient(handler);

        var ex = await Assert.ThrowsAsync<MendApiException>(
            () => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/v3.0/orgs/abc/projects")));

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Equal(responseBody, ex.ResponseBody);
    }

    [Fact]
    public async Task SendAsync_404Response_ThrowsMendApiExceptionWithStatusCode()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("not found", Encoding.UTF8, "text/plain")
        });
        var client = BuildClient(handler);

        var ex = await Assert.ThrowsAsync<MendApiException>(
            () => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/v3.0/missing")));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    // --- helpers ---

    private static MendHttpClient BuildClient(FakeHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var factory = new FakeFactory(_ => httpClient);
        return new MendHttpClient(factory, MakeOptions("https://api.test.mend.io"));
    }

    private static IOptions<MendOptions> MakeOptions(string baseUrl) =>
        new StaticOptions<MendOptions>(new MendOptions
        {
            BaseUrl = baseUrl,
            OrgUuid = "org",
            Email = "e@e.com",
            UserKey = "key"
        });

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respond(request));
    }

    private sealed class FakeFactory(Func<string, HttpClient> factory) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => factory(name);
    }

    private sealed class StaticOptions<T>(T value) : IOptions<T> where T : class
    {
        public T Value => value;
    }
}
