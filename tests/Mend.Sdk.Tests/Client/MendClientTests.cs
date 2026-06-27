using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Auth;
using Mend.Sdk.Client;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Http;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.Client;

public sealed class MendClientTests
{
    private sealed class TestPayload
    {
        public string? Value { get; set; }
    }

    private static Mock<IMendTokenManager> TokenManagerMock(string token = "test-token")
    {
        var mock = new Mock<IMendTokenManager>();
        mock.Setup(t => t.GetAccessTokenAsync()).ReturnsAsync(token);
        return mock;
    }

    // --- Bearer header ---

    [Fact]
    public async Task GetAsync_SetsBearerAuthorizationHeader()
    {
        HttpRequestMessage? captured = null;
        var tokenManager = TokenManagerMock("my-bearer-token");
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.GetAsync<TestPayload>("/api/test");

        Assert.NotNull(captured);
        Assert.Equal("Bearer", captured!.Headers.Authorization?.Scheme);
        Assert.Equal("my-bearer-token", captured.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task GetAsync_FetchesTokenFromTokenManagerBeforeEachRequest()
    {
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.GetAsync<TestPayload>("/api/test");
        await sut.GetAsync<TestPayload>("/api/test");

        tokenManager.Verify(t => t.GetAccessTokenAsync(), Times.Exactly(2));
    }

    // --- Auth failure on non-auth endpoints ---

    [Fact]
    public async Task GetAsync_WhenHttpClientThrowsMendAuthException_PropagatesWithEndpointPath()
    {
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException("/api/v3.0/orgs/abc/applications"));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);

        var ex = await Assert.ThrowsAsync<MendAuthException>(
            () => sut.GetAsync<TestPayload>("/api/v3.0/orgs/abc/applications"));

        Assert.Equal("/api/v3.0/orgs/abc/applications", ex.EndpointPath);
    }

    [Fact]
    public async Task PostAsync_WhenHttpClientThrowsMendAuthException_PropagatesException()
    {
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException("/api/v3.0/orgs/abc/projects"));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);

        await Assert.ThrowsAsync<MendAuthException>(
            () => sut.PostAsync("/api/v3.0/orgs/abc/projects"));
    }

    // --- Non-auth API failures ---

    [Fact]
    public async Task GetAsync_WhenHttpClientThrowsMendApiException_PropagatesWithStatusCode()
    {
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendApiException(HttpStatusCode.InternalServerError, "server error"));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);

        var ex = await Assert.ThrowsAsync<MendApiException>(
            () => sut.GetAsync<TestPayload>("/api/test"));

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    // --- Pagination ---

    [Fact]
    public async Task GetPagedAsync_WithPageSize_AppendsPageSizeQueryParameter()
    {
        HttpRequestMessage? captured = null;
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.GetPagedAsync<TestPayload>("/api/items", pageSize: 50);

        Assert.NotNull(captured);
        Assert.Contains("pageSize=50", captured!.RequestUri?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task GetPagedAsync_WithCursor_AppendsCursorQueryParameter()
    {
        HttpRequestMessage? captured = null;
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.GetPagedAsync<TestPayload>("/api/items", cursor: "next-page-token");

        Assert.NotNull(captured);
        Assert.Contains("cursor=next-page-token", captured!.RequestUri?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task GetPagedAsync_WithPageSizeAndCursor_AppendsBothQueryParameters()
    {
        HttpRequestMessage? captured = null;
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.GetPagedAsync<TestPayload>("/api/items", pageSize: 25, cursor: "abc");

        Assert.NotNull(captured);
        var uri = captured!.RequestUri?.ToString() ?? string.Empty;
        Assert.Contains("pageSize=25", uri);
        Assert.Contains("cursor=abc", uri);
    }

    [Fact]
    public async Task GetPagedAsync_WithNoParams_PathIsUnchanged()
    {
        HttpRequestMessage? captured = null;
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.GetPagedAsync<TestPayload>("/api/items");

        Assert.NotNull(captured);
        Assert.DoesNotContain("?", captured!.RequestUri?.ToString() ?? string.Empty);
    }

    // --- PostPagedAsync ---

    [Fact]
    public async Task PostPagedAsync_WithLimit_AppendsLimitQueryParameter()
    {
        HttpRequestMessage? captured = null;
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.PostPagedAsync<TestPayload>("/api/items", limit: 100);

        Assert.NotNull(captured);
        Assert.Contains("limit=100", captured!.RequestUri?.ToString() ?? string.Empty);
        Assert.DoesNotContain("pageSize", captured.RequestUri?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task PostPagedAsync_WithCursor_AppendsCursorQueryParameter()
    {
        HttpRequestMessage? captured = null;
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.PostPagedAsync<TestPayload>("/api/items", cursor: "next-page-token");

        Assert.NotNull(captured);
        Assert.Contains("cursor=next-page-token", captured!.RequestUri?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task PostPagedAsync_WithLimitAndCursor_AppendsBothQueryParameters()
    {
        HttpRequestMessage? captured = null;
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.PostPagedAsync<TestPayload>("/api/items", limit: 50, cursor: "abc");

        Assert.NotNull(captured);
        var uri = captured!.RequestUri?.ToString() ?? string.Empty;
        Assert.Contains("limit=50", uri);
        Assert.Contains("cursor=abc", uri);
    }

    [Fact]
    public async Task PostPagedAsync_WithNoParams_PathIsUnchanged()
    {
        HttpRequestMessage? captured = null;
        var tokenManager = TokenManagerMock();
        var httpClient = new Mock<IMendHttpClient>();
        httpClient
            .Setup(h => h.SendAsync<ApiEnvelope<TestPayload>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(default(ApiEnvelope<TestPayload>));

        var sut = new MendClient(tokenManager.Object, httpClient.Object);
        await sut.PostPagedAsync<TestPayload>("/api/items");

        Assert.NotNull(captured);
        Assert.DoesNotContain("?", captured!.RequestUri?.ToString() ?? string.Empty);
    }
}
