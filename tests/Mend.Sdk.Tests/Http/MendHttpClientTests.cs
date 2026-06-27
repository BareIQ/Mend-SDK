using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Http;
using Mend.Sdk.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.Http;

public sealed class MendHttpClientTests
{
    // --- mockability ---

    [Fact]
    public async Task IMendHttpClient_IsMockable_CallerUsesNoRealHttp()
    {
        var mock = new Mock<IMendHttpClient>();
        mock.Setup(x => x.SendAsync<SampleDto>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SampleDto("mocked"));

        var caller = new SampleCaller(mock.Object);
        var result = await caller.FetchAsync();

        Assert.Equal("mocked", result?.Value);
        mock.Verify(
            x => x.SendAsync<SampleDto>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- MendHttpClient behaviour ---

    [Fact]
    public async Task SendAsync_NoBody_SetsBaseAddressFromOptions()
    {
        const string baseUrl = "https://api.test.mend.io";
        HttpRequestMessage? captured = null;
        var handler = new FakeHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = BuildClient(handler, baseUrl);

        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/test"), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.StartsWith(baseUrl, captured!.RequestUri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendAsync_WithTypedResponse_DeserializesJson()
    {
        const string baseUrl = "https://api.test.mend.io";
        var body = JsonSerializer.Serialize(new { value = "hello" });
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });
        var client = BuildClient(handler, baseUrl);

        var result = await client.SendAsync<SampleDto>(
            new HttpRequestMessage(HttpMethod.Get, "/test"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("hello", result!.Value);
    }

    [Fact]
    public async Task SendAsync_BaseAddressNotAlreadySet_AppliesOptionsBaseUrl()
    {
        const string baseUrl = "https://custom.mend.io";
        HttpClient? usedClient = null;
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var httpClient = new HttpClient(handler);
        var factory = new FakeFactory(name =>
        {
            usedClient = httpClient;
            return httpClient;
        });
        var client = new MendHttpClient(factory, MakeOptions(baseUrl));

        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/check"), CancellationToken.None);

        Assert.Equal(new Uri(baseUrl), usedClient!.BaseAddress);
    }

    // --- helpers ---

    private static MendHttpClient BuildClient(FakeHandler handler, string baseUrl)
    {
        var httpClient = new HttpClient(handler);
        var factory = new FakeFactory(_ => httpClient);
        return new MendHttpClient(factory, MakeOptions(baseUrl));
    }

    private sealed record SampleDto(string Value);

    private sealed class SampleCaller(IMendHttpClient http)
    {
        public Task<SampleDto?> FetchAsync()
            => http.SendAsync<SampleDto>(new HttpRequestMessage(HttpMethod.Get, "/data"));
    }

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

    private static IOptions<MendOptions> MakeOptions(string baseUrl) =>
        new StaticOptions<MendOptions>(new MendOptions
        {
            BaseUrl = baseUrl,
            OrgUuid = "org",
            Email = "e@e.com",
            UserKey = "key"
        });
}
