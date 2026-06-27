using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mend.Sdk.Auth.Models;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Http;
using Mend.Sdk.Options;
using Microsoft.Extensions.Options;

namespace Mend.Sdk.Auth;

public sealed class MendTokenManager : IMendTokenManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private static readonly TimeSpan RefreshBuffer = TimeSpan.FromSeconds(60);

    private readonly IMendHttpClient _httpClient;
    private readonly MendOptions _options;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private string? _accessToken;
    private string? _refreshToken;
    private DateTimeOffset _tokenExpiry;

    public MendTokenManager(IMendHttpClient httpClient, IOptions<MendOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (IsTokenValid())
            return _accessToken!;

        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (IsTokenValid())
                return _accessToken!;

            if (_refreshToken != null)
                await RefreshAsync().ConfigureAwait(false);
            else
                await LoginAsync().ConfigureAwait(false);

            return _accessToken!;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task LogoutAsync()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_refreshToken == null)
                return;

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v3.0/logout");
            request.Headers.Add("wss-refresh-token", _refreshToken);

            try
            {
                await _httpClient.SendAsync(request).ConfigureAwait(false);
            }
            catch
            {
                // Best-effort: clear local state regardless of server response.
            }
        }
        finally
        {
            _accessToken = null;
            _refreshToken = null;
            _tokenExpiry = default;
            _semaphore.Release();
        }
    }

    private bool IsTokenValid()
        => _accessToken != null && DateTimeOffset.UtcNow < _tokenExpiry - RefreshBuffer;

    // POST /api/v3.0/login → refresh token only; chains into RefreshAsync to get the access token.
    private async Task LoginAsync()
    {
        var body = new LoginRequest
        {
            Email = _options.Email,
            UserKey = _options.UserKey,
            OrgToken = _options.OrgUuid
        };

        ApiEnvelope<LoginResponse>? envelope;
        try
        {
            using var request = CreateJsonRequest(HttpMethod.Post, "/api/v3.0/login", body);
            envelope = await _httpClient.SendAsync<ApiEnvelope<LoginResponse>>(request).ConfigureAwait(false);
        }
        catch (MendAuthException)
        {
            ClearTokens();
            throw;
        }
        catch (Exception ex)
        {
            ClearTokens();
            throw new MendAuthException("/api/v3.0/login", ex);
        }

        var loginData = envelope?.Response;
        if (loginData?.RefreshToken == null)
        {
            ClearTokens();
            throw new MendAuthException("/api/v3.0/login");
        }

        _refreshToken = loginData.RefreshToken;

        // Exchange the refresh token immediately for an access token.
        await RefreshAsync().ConfigureAwait(false);
    }

    private async Task RefreshAsync()
    {
        var currentRefreshToken = _refreshToken;

        ApiEnvelope<RefreshTokenResponse>? envelope;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v3.0/login/accessToken");
            request.Headers.Add("wss-refresh-token", currentRefreshToken);
            envelope = await _httpClient.SendAsync<ApiEnvelope<RefreshTokenResponse>>(request).ConfigureAwait(false);
        }
        catch (MendAuthException)
        {
            ClearTokens();
            throw;
        }
        catch (Exception ex)
        {
            ClearTokens();
            throw new MendAuthException("/api/v3.0/login/accessToken", ex);
        }

        var refreshData = envelope?.Response;
        if (refreshData?.JwtToken == null)
        {
            ClearTokens();
            throw new MendAuthException("/api/v3.0/login/accessToken");
        }

        _accessToken = refreshData.JwtToken;
        _tokenExpiry = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(refreshData.TokenTtl);
    }

    private void ClearTokens()
    {
        _accessToken = null;
        _refreshToken = null;
        _tokenExpiry = default;
    }

    private static HttpRequestMessage CreateJsonRequest<T>(HttpMethod method, string path, T body)
        => new HttpRequestMessage(method, path)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
}
