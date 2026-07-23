using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Sendgo.Exceptions;
using Sendgo.Models;
using Sendgo.Services;

namespace Sendgo;

/// <summary>
/// Sendgo API 메인 클라이언트.
/// </summary>
/// <example>
/// <code>
/// var client = new SendgoClient(new SendgoOptions {
///     AccessKey      = Environment.GetEnvironmentVariable("SENDGO_ACCESS_KEY")!,
///     SecretKey      = Environment.GetEnvironmentVariable("SENDGO_SECRET_KEY")!,
///     KakaoSenderKey = Environment.GetEnvironmentVariable("SENDGO_KAKAO_KEY"),
///     SmsSenderKey   = Environment.GetEnvironmentVariable("SENDGO_SMS_KEY"),
///     ApiVersion     = "v2",
/// });
///
/// await client.SendAlimtalkAsync(new AlimtalkRequest {
///     TemplateCode = "ORDER_CONFIRM_001",
///     Contacts     = [new Contact { PhoneNumber = "01012345678", Var1 = "ORD-001" }],
/// });
/// </code>
/// </example>
public sealed class SendgoClient : IDisposable
{
    private readonly SendgoOptions _options;
    private readonly TokenManager _tokenManager;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = null, // camelCase는 각 모델에서 직접 설정
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public SendgoClient(SendgoOptions options)
    {
        _options = options;
        _tokenManager = new TokenManager(options);
    }

    /// <summary>카카오 알림톡 전송.</summary>
    public Task<Dictionary<string, object?>> SendAlimtalkAsync(AlimtalkRequest request, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToElement(request, _jsonOptions);
        var merged = MergeWithSenderKeys(body, includeKakao: true);
        return PostAsync($"{_options.BaseUrl}/api/{_options.ApiVersion}/notices/send", merged, ct);
    }

    /// <summary>카카오 친구톡 전송.</summary>
    public Task<Dictionary<string, object?>> SendFriendtalkAsync(object request, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToElement(request, _jsonOptions);
        var merged = MergeWithSenderKeys(body, includeKakao: true);
        return PostAsync($"{_options.BaseUrl}/api/{_options.ApiVersion}/friends/send", merged, ct);
    }

    /// <summary>SMS 전송.</summary>
    public Task<Dictionary<string, object?>> SendSmsAsync(SmsRequest request, CancellationToken ct = default) =>
        SendMessageAsync(request with { MessageType = "SMS" }, ct);

    /// <summary>LMS 전송.</summary>
    public Task<Dictionary<string, object?>> SendLmsAsync(SmsRequest request, CancellationToken ct = default) =>
        SendMessageAsync(request with { MessageType = "LMS" }, ct);

    /// <summary>MMS 전송.</summary>
    public Task<Dictionary<string, object?>> SendMmsAsync(SmsRequest request, CancellationToken ct = default) =>
        SendMessageAsync(request with { MessageType = "MMS" }, ct);

    private Task<Dictionary<string, object?>> SendMessageAsync(SmsRequest request, CancellationToken ct)
    {
        var body = JsonSerializer.SerializeToElement(request, _jsonOptions);
        var merged = MergeWithSenderKeys(body, includeKakao: false);
        return PostAsync($"{_options.BaseUrl}/api/{_options.ApiVersion}/messages/send", merged, ct);
    }

    private async Task<Dictionary<string, object?>> PostAsync(
        string url, Dictionary<string, object?> body, CancellationToken ct, bool isRetry = false)
    {
        var token = await _tokenManager.GetTokenAsync(ct);
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", MakeBearerToken(token));

        var resp = await _http.SendAsync(req, ct);
        var bodyText = await resp.Content.ReadAsStringAsync(ct);
        var responseBody = JsonSerializer.Deserialize<Dictionary<string, object?>>(bodyText) ?? new();

        if (!resp.IsSuccessStatusCode)
        {
            var errorCode = responseBody.GetValueOrDefault("code") as string;
            var endpoint = url.Split('/').Last();
            if (!isRetry && _tokenManager.ShouldRefresh((int)resp.StatusCode, errorCode))
            {
                _tokenManager.Invalidate();
                return await PostAsync(url, body, ct, isRetry: true);
            }
            throw SendgoException.FromResponse((int)resp.StatusCode,
                responseBody.ToDictionary(k => k.Key, v => v.Value), endpoint, _options.ApiVersion);
        }

        return responseBody;
    }

    private string MakeBearerToken(string token) =>
        _options.ApiVersion == "v2" ? token : Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

    private Dictionary<string, object?> MergeWithSenderKeys(JsonElement element, bool includeKakao)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(element.GetRawText()) ?? new();
        if (includeKakao) dict["kakaoSenderKey"] = _options.KakaoSenderKey;
        dict["senderKey"] = _options.SmsSenderKey;
        return dict;
    }

    public void Dispose() { _http.Dispose(); _tokenManager.Dispose(); }
}
