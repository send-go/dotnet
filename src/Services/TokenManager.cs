using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Sendgo.Exceptions;

namespace Sendgo.Services;

internal sealed class TokenManager : IDisposable
{
    private static readonly HashSet<string> NoRefreshCodes = new(StringComparer.Ordinal)
    {
        "INVALID_AUTH_HEADER", "INVALID_BASIC_AUTH", "INVALID_BASIC_AUTH_PAYLOAD",
        "INVALID_ACCESS_KEY", "INVALID_SECRET_KEY", "ACCESS_KEY_NOT_APPROVED",
        "TEAM_REQUIRED_FOR_KAKAO", "IP_NOT_ALLOWED", "INVALID_SENDER_KEY", "INVALID_KAKAO_SENDER_KEY",
    };

    private readonly HttpClient _http = new();
    private readonly SendgoOptions _options;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private string? _token;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public TokenManager(SendgoOptions options) => _options = options;

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_token is not null && DateTimeOffset.UtcNow < _expiresAt)
            return _token;

        await _semaphore.WaitAsync(ct);
        try
        {
            if (_token is not null && DateTimeOffset.UtcNow < _expiresAt) return _token;
            return await FetchTokenAsync(ct);
        }
        finally { _semaphore.Release(); }
    }

    public void Invalidate() { _token = null; _expiresAt = DateTimeOffset.MinValue; }

    public bool ShouldRefresh(int status, string? errorCode)
    {
        if (status is not (401 or 403)) return false;
        if (_options.ApiVersion == "v2" && errorCode is not null && NoRefreshCodes.Contains(errorCode))
            return false;
        return true;
    }

    private async Task<string> FetchTokenAsync(CancellationToken ct)
    {
        var url = $"{_options.BaseUrl}/api/{_options.ApiVersion}/token";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.AccessKey}:{_options.SecretKey}"));

        var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        req.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var resp = await _http.SendAsync(req, ct);
        var bodyText = await resp.Content.ReadAsStringAsync(ct);
        var body = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bodyText) ?? new();

        if (!resp.IsSuccessStatusCode)
        {
            var errBody = body.ToDictionary(k => k.Key, v => (object?)v.Value.ToString());
            throw SendgoException.FromResponse((int)resp.StatusCode, errBody, "token", _options.ApiVersion);
        }

        var token = body.GetValueOrDefault("data").GetProperty("token").GetString()
                    ?? throw new SendgoException("token 필드가 응답에 없습니다.");

        _token = token;
        _expiresAt = DateTimeOffset.UtcNow.AddMinutes(50);
        return _token;
    }

    public void Dispose() { _http.Dispose(); _semaphore.Dispose(); }
}
