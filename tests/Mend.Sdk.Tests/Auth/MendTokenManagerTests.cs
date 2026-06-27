using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Auth;
using Mend.Sdk.Auth.Models;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Http;
using Mend.Sdk.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Mend.Sdk.Tests.Auth;

public sealed class MendTokenManagerTests
{
    private static IOptions<MendOptions> CreateOptions() =>
        Microsoft.Extensions.Options.Options.Create(new MendOptions
        {
            BaseUrl = "https://api.mend.io",
            Email = "test@example.com",
            UserKey = "test-key",
            OrgUuid = "test-org-uuid"
        });

    // Login returns a refresh token only (access token comes from the subsequent /accessToken call).
    private static ApiEnvelope<LoginResponse> LoginEnvelope(string refreshToken = "refresh-token") =>
        new ApiEnvelope<LoginResponse>
        {
            Response = new LoginResponse { RefreshToken = refreshToken, JwtTtl = 86_400_000 }
        };

    // /login/accessToken returns the actual bearer token; tokenTtl is in milliseconds.
    private static ApiEnvelope<RefreshTokenResponse> RefreshEnvelope(
        string accessToken = "access-token",
        long jwtTtl = 1_800_000) =>
        new ApiEnvelope<RefreshTokenResponse>
        {
            Response = new RefreshTokenResponse { JwtToken = accessToken, TokenTtl = jwtTtl }
        };

    // Sets up a mock that handles both the login call and the refresh call.
    private static Mock<IMendHttpClient> SetupMock(
        ApiEnvelope<LoginResponse>? loginEnv = null,
        ApiEnvelope<RefreshTokenResponse>? refreshEnv = null)
    {
        var mock = new Mock<IMendHttpClient>();
        mock.Setup(h => h.SendAsync<ApiEnvelope<LoginResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginEnv ?? LoginEnvelope());
        mock.Setup(h => h.SendAsync<ApiEnvelope<RefreshTokenResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshEnv ?? RefreshEnvelope());
        return mock;
    }

    // --- First-call / cache behaviour ---

    [Fact]
    public async Task GetAccessTokenAsync_FirstCall_TriggersLoginThenRefresh()
    {
        var mock = SetupMock();
        var sut = new MendTokenManager(mock.Object, CreateOptions());

        var token = await sut.GetAccessTokenAsync();

        Assert.Equal("access-token", token);
        mock.Verify(
            h => h.SendAsync<ApiEnvelope<LoginResponse>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
        mock.Verify(
            h => h.SendAsync<ApiEnvelope<RefreshTokenResponse>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAccessTokenAsync_SecondCallWithinValidity_DoesNotReLoginOrRefresh()
    {
        var mock = SetupMock(refreshEnv: RefreshEnvelope(jwtTtl: 1_800_000));
        var sut = new MendTokenManager(mock.Object, CreateOptions());

        await sut.GetAccessTokenAsync();
        await sut.GetAccessTokenAsync();

        mock.Verify(
            h => h.SendAsync<ApiEnvelope<LoginResponse>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
        mock.Verify(
            h => h.SendAsync<ApiEnvelope<RefreshTokenResponse>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAccessTokenAsync_SecondCallWithinValidity_ReturnsSameToken()
    {
        var mock = SetupMock(refreshEnv: RefreshEnvelope(accessToken: "my-token", jwtTtl: 1_800_000));
        var sut = new MendTokenManager(mock.Object, CreateOptions());

        var first = await sut.GetAccessTokenAsync();
        var second = await sut.GetAccessTokenAsync();

        Assert.Equal("my-token", first);
        Assert.Equal("my-token", second);
    }

    // --- Proactive refresh ---

    [Fact]
    public async Task GetAccessTokenAsync_NearExpiry_TriggersAdditionalRefresh()
    {
        var mock = new Mock<IMendHttpClient>();
        mock.Setup(h => h.SendAsync<ApiEnvelope<LoginResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LoginEnvelope());
        // First refresh (from LoginAsync): token expires in 0ms → immediately invalid.
        // Second refresh (triggered by GetAccessTokenAsync): long-lived.
        mock.SetupSequence(h => h.SendAsync<ApiEnvelope<RefreshTokenResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RefreshEnvelope(accessToken: "initial-token", jwtTtl: 0))
            .ReturnsAsync(RefreshEnvelope(accessToken: "refreshed-token", jwtTtl: 1_800_000));

        var sut = new MendTokenManager(mock.Object, CreateOptions());

        await sut.GetAccessTokenAsync();             // login + first refresh (expired immediately)
        var token = await sut.GetAccessTokenAsync(); // second refresh

        mock.Verify(
            h => h.SendAsync<ApiEnvelope<LoginResponse>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
        mock.Verify(
            h => h.SendAsync<ApiEnvelope<RefreshTokenResponse>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        Assert.Equal("refreshed-token", token);
    }

    [Fact]
    public async Task GetAccessTokenAsync_NearExpiry_DoesNotTriggerFullLogin()
    {
        var mock = new Mock<IMendHttpClient>();
        mock.Setup(h => h.SendAsync<ApiEnvelope<LoginResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LoginEnvelope());
        mock.SetupSequence(h => h.SendAsync<ApiEnvelope<RefreshTokenResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RefreshEnvelope(jwtTtl: 0))
            .ReturnsAsync(RefreshEnvelope(jwtTtl: 1_800_000));

        var sut = new MendTokenManager(mock.Object, CreateOptions());
        await sut.GetAccessTokenAsync();
        await sut.GetAccessTokenAsync();

        mock.Verify(
            h => h.SendAsync<ApiEnvelope<LoginResponse>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- Error paths ---

    [Fact]
    public async Task GetAccessTokenAsync_LoginThrowsMendAuthException_Rethrows()
    {
        var mock = new Mock<IMendHttpClient>();
        mock.Setup(h => h.SendAsync<ApiEnvelope<LoginResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MendAuthException("/api/v3.0/login"));

        var sut = new MendTokenManager(mock.Object, CreateOptions());

        await Assert.ThrowsAsync<MendAuthException>(() => sut.GetAccessTokenAsync());
    }

    [Fact]
    public async Task GetAccessTokenAsync_LoginThrowsGenericException_ThrowsMendAuthException()
    {
        var mock = new Mock<IMendHttpClient>();
        mock.Setup(h => h.SendAsync<ApiEnvelope<LoginResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("network error"));

        var sut = new MendTokenManager(mock.Object, CreateOptions());

        await Assert.ThrowsAsync<MendAuthException>(() => sut.GetAccessTokenAsync());
    }

    [Fact]
    public async Task GetAccessTokenAsync_LoginReturnsNullRefreshToken_ThrowsMendAuthException()
    {
        var mock = new Mock<IMendHttpClient>();
        mock.Setup(h => h.SendAsync<ApiEnvelope<LoginResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiEnvelope<LoginResponse> { Response = new LoginResponse { RefreshToken = null } });

        var sut = new MendTokenManager(mock.Object, CreateOptions());

        await Assert.ThrowsAsync<MendAuthException>(() => sut.GetAccessTokenAsync());
    }

    [Fact]
    public async Task GetAccessTokenAsync_RefreshThrowsMendAuthException_Rethrows()
    {
        var mock = new Mock<IMendHttpClient>();
        mock.Setup(h => h.SendAsync<ApiEnvelope<LoginResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LoginEnvelope());
        // First refresh (from LoginAsync) succeeds with an immediately-expired token;
        // second refresh (from GetAccessTokenAsync near-expiry path) throws.
        mock.SetupSequence(h => h.SendAsync<ApiEnvelope<RefreshTokenResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RefreshEnvelope(jwtTtl: 0))
            .ThrowsAsync(new MendAuthException("/api/v3.0/login/accessToken"));

        var sut = new MendTokenManager(mock.Object, CreateOptions());
        await sut.GetAccessTokenAsync(); // login + first refresh

        await Assert.ThrowsAsync<MendAuthException>(() => sut.GetAccessTokenAsync());
    }

    // --- Thread safety ---

    [Fact]
    public async Task GetAccessTokenAsync_ConcurrentCalls_TriggerOnlyOneLoginAndRefresh()
    {
        var loginCallCount = 0;
        var loginGate = new TaskCompletionSource<ApiEnvelope<LoginResponse>?>();

        var mock = new Mock<IMendHttpClient>();
        mock.Setup(h => h.SendAsync<ApiEnvelope<LoginResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns<HttpRequestMessage, CancellationToken>((_, _) =>
            {
                Interlocked.Increment(ref loginCallCount);
                return loginGate.Task;
            });
        mock.Setup(h => h.SendAsync<ApiEnvelope<RefreshTokenResponse>>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RefreshEnvelope(accessToken: "shared-token", jwtTtl: 1_800_000));

        var sut = new MendTokenManager(mock.Object, CreateOptions());

        // Launch 8 concurrent callers before login completes.
        var tasks = new List<Task<string>>();
        for (var i = 0; i < 8; i++)
            tasks.Add(Task.Run(() => sut.GetAccessTokenAsync()));

        await Task.Delay(50); // let all tasks queue at the semaphore

        loginGate.SetResult(LoginEnvelope());

        var results = await Task.WhenAll(tasks);

        Assert.Equal(1, loginCallCount);
        Assert.All(results, r => Assert.Equal("shared-token", r));
    }

    // --- Logout ---

    [Fact]
    public async Task LogoutAsync_CallsLogoutEndpoint()
    {
        var mock = SetupMock();
        mock.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new MendTokenManager(mock.Object, CreateOptions());
        await sut.GetAccessTokenAsync();
        await sut.LogoutAsync();

        mock.Verify(
            h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenNotLoggedIn_DoesNotCallLogoutEndpoint()
    {
        var mock = new Mock<IMendHttpClient>();
        var sut = new MendTokenManager(mock.Object, CreateOptions());

        await sut.LogoutAsync();

        mock.Verify(
            h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_AfterLogout_NextCallTriggersLoginAgain()
    {
        var mock = SetupMock();
        mock.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new MendTokenManager(mock.Object, CreateOptions());
        await sut.GetAccessTokenAsync();
        await sut.LogoutAsync();
        await sut.GetAccessTokenAsync();

        mock.Verify(
            h => h.SendAsync<ApiEnvelope<LoginResponse>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
