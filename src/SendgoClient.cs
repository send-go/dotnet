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

    /// <summary>
    /// 카카오 친구톡 전송.
    /// </summary>
    /// <remarks>
    /// 친구톡은 카카오 정책에 따라 2025-12-31 종료되었습니다. 2026-01-01 부터 친구톡
    /// 발송 요청은 카카오 측에서 브랜드메시지(자유형)로 자동 대체 발송되므로, 이 메서드를
    /// 호출해도 실제로 나가는 것은 브랜드메시지입니다. 신규 연동은
    /// <see cref="SendBrandMessageAsync"/> 를 사용하세요. 다만 자유 본문 타입(FT/FI/FW)을
    /// 개별 수신자에게 보내는 경로는 아직 이 메서드뿐입니다 — 브랜드메시지 API 는 그
    /// 조합에 NOT_A_BRAND_MESSAGE 를 반환합니다. 메시지 타입은 1:1 대응됩니다 —
    /// FT→BT, FI→BI, FW→BW, FL→BL, FC→BC, FM→BM, FP→BP, FA→BA.
    /// </remarks>
    [Obsolete("친구톡은 2025-12-31 종료되었습니다. SendBrandMessageAsync 를 사용하세요.")]
    public Task<Dictionary<string, object?>> SendFriendtalkAsync(object request, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToElement(request, _jsonOptions);
        var merged = MergeWithSenderKeys(body, includeKakao: true);
        return PostAsync($"{_options.BaseUrl}/api/{_options.ApiVersion}/friends/send", merged, ct);
    }

    /// <summary>
    /// 카카오 브랜드메시지 전송. 친구톡의 후속 채널로, 채널 친구가 아닌 수신자에게도
    /// 보낼 수 있습니다(Targeting "N"). v2 전용.
    /// </summary>
    public Task<Dictionary<string, object?>> SendBrandMessageAsync(
        BrandMessageRequest request, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToElement(request, _jsonOptions);
        var merged = MergeWithSenderKeys(body, includeKakao: true);

        // 동보는 수신자 목록이 없다. 빈 배열을 보내면 잘못된 요청으로 거절되므로
        // 키 자체를 제거한다.
        if (request.Targeting == "F") merged.Remove("contacts");

        return PostAsync($"{_options.BaseUrl}/api/{_options.ApiVersion}/brand-messages/send", merged, ct);
    }

    /// <summary>
    /// 브랜드메시지 동보 전송 — 수신 동의한 전체 채널 친구(Targeting "F").
    /// 결과는 즉시 알 수 없으므로 <see cref="GetBrandMessagesAsync"/> 로 확인합니다.
    /// </summary>
    public Task<Dictionary<string, object?>> BroadcastBrandMessageAsync(
        BrandMessageRequest request, CancellationToken ct = default) =>
        SendBrandMessageAsync(request with { Targeting = "F", Contacts = null }, ct);

    /// <summary>브랜드메시지 캠페인 목록 조회. null 인 조건은 서버 기본값이 적용됩니다.</summary>
    public Task<Dictionary<string, object?>> GetBrandMessagesAsync(
        string? from = null, string? to = null, int? count = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (from is not null) query.Add($"from={Uri.EscapeDataString(from)}");
        if (to is not null) query.Add($"to={Uri.EscapeDataString(to)}");
        if (count is not null) query.Add($"count={count}");

        var url = $"{_options.BaseUrl}/api/{_options.ApiVersion}/brand-messages";
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return GetAsync(url, ct);
    }

    /// <summary>브랜드메시지 캠페인 상세 조회. campaignId 는 발송 응답의 campaignId(UUID).</summary>
    public Task<Dictionary<string, object?>> GetBrandMessageAsync(
        string campaignId, CancellationToken ct = default) =>
        GetAsync($"{_options.BaseUrl}/api/{_options.ApiVersion}/brand-messages/{campaignId}", ct);

    /// <summary>
    /// 짧은 URL 을 만듭니다. v2 전용입니다.
    ///
    /// 같은 원본 URL 을 다시 줄이면 기존 링크가 그대로 반환됩니다.
    /// 캠페인별로 반응을 분리해 집계하려면 <c>ForceNew = true</c> 를 사용하세요.
    /// </summary>
    public Task<Dictionary<string, object?>> CreateShortUrlAsync(
        ShortUrlRequest request, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToElement(request, _jsonOptions);
        var payload = body.Deserialize<Dictionary<string, object?>>(_jsonOptions)
                      ?? new Dictionary<string, object?>();

        return PostAsync(ShortUrlUrl(), payload, ct);
    }

    /// <summary>짧은 URL 목록 조회. null 인 조건은 서버 기본값이 적용됩니다.</summary>
    public Task<Dictionary<string, object?>> GetShortUrlsAsync(
        string? from = null, string? to = null, int? count = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (from is not null) query.Add($"from={Uri.EscapeDataString(from)}");
        if (to is not null) query.Add($"to={Uri.EscapeDataString(to)}");
        if (count is not null) query.Add($"count={count}");

        var url = ShortUrlUrl();
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return GetAsync(url, ct);
    }

    /// <summary>짧은 URL 상세 조회.</summary>
    public Task<Dictionary<string, object?>> GetShortUrlAsync(
        string code, CancellationToken ct = default) =>
        GetAsync(ShortUrlUrl(code), ct);

    /// <summary>
    /// 짧은 URL 반응 통계. 일별 추이와 디바이스/유입경로/국가별 분해를 반환합니다.
    /// </summary>
    public Task<Dictionary<string, object?>> GetShortUrlStatsAsync(
        string code, string? from = null, string? to = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (from is not null) query.Add($"from={Uri.EscapeDataString(from)}");
        if (to is not null) query.Add($"to={Uri.EscapeDataString(to)}");

        var url = ShortUrlUrl(code) + "/stats";
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return GetAsync(url, ct);
    }

    /// <summary>
    /// 짧은 URL 리다이렉트를 중지합니다.
    ///
    /// 링크는 삭제되지 않고 누적 통계도 남습니다. 이후 그 링크로 들어오면
    /// 410 Gone 이 반환됩니다.
    /// </summary>
    public Task<Dictionary<string, object?>> DeactivateShortUrlAsync(
        string code, CancellationToken ct = default) =>
        DeleteAsync(ShortUrlUrl(code), ct);

    private string ShortUrlUrl(string? code = null)
    {
        var baseUrl = $"{_options.BaseUrl}/api/{_options.ApiVersion}/short-urls";

        return code is null ? baseUrl : $"{baseUrl}/{Uri.EscapeDataString(code)}";
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

    private Task<Dictionary<string, object?>> PostAsync(
        string url, Dictionary<string, object?> body, CancellationToken ct, bool isRetry = false) =>
        SendRequestAsync(HttpMethod.Post, url, body, ct, isRetry);

    /// <summary>GET without a body — used by the campaign lookup endpoints.</summary>
    private Task<Dictionary<string, object?>> GetAsync(string url, CancellationToken ct) =>
        SendRequestAsync(HttpMethod.Get, url, null, ct, isRetry: false);

    /// <summary>DELETE without a body — used to stop a short URL redirecting.</summary>
    private Task<Dictionary<string, object?>> DeleteAsync(string url, CancellationToken ct) =>
        SendRequestAsync(HttpMethod.Delete, url, null, ct, isRetry: false);

    private async Task<Dictionary<string, object?>> SendRequestAsync(
        HttpMethod method, string url, Dictionary<string, object?>? body,
        CancellationToken ct, bool isRetry = false)
    {
        var token = await _tokenManager.GetTokenAsync(ct);

        var req = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            req.Content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }
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
                return await SendRequestAsync(method, url, body, ct, isRetry: true);
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
