using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Sendgo.Exceptions;

namespace Sendgo;

/// <summary>서버 전용 계정 API. 에이전트 토큰은 자동 갱신하지 않습니다.</summary>
public sealed class AccountClient : IDisposable
{
    private readonly string _agentToken;
    private readonly string _baseUrl;
    private readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(15) };

    public AccountClient(string agentToken, string baseUrl = "https://sendgo.io")
    {
        if (string.IsNullOrWhiteSpace(agentToken)) throw new ArgumentException("Sendgo: agentToken은 필수입니다.", nameof(agentToken));
        _agentToken = agentToken;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>계정 상태와 다음 단계 조회.</summary>
    public Task<Dictionary<string, object?>> MeAsync(CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Get, $"", null, ct);

    /// <summary>조직 목록 조회.</summary>
    public Task<Dictionary<string, object?>> OrganizationsAsync(CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Get, $"organizations", null, ct);

    /// <summary>조직 선택. null은 개인 계정.</summary>
    public Task<Dictionary<string, object?>> SelectOrganizationAsync(string? organizationId, CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Post, $"organizations/select", new Dictionary<string, object?> { ["organizationId"] = organizationId }, ct);

    /// <summary>현재 조직의 API 키 목록.</summary>
    public Task<Dictionary<string, object?>> ApiKeysAsync(CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Get, $"api-keys", null, ct);

    /// <summary>API 키 발급. secretKey는 이 응답에서만 반환.</summary>
    public Task<Dictionary<string, object?>> CreateApiKeyAsync(Dictionary<string, object?> parameters, CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Post, $"api-keys", parameters, ct);

    /// <summary>API 키 상세 조회.</summary>
    public Task<Dictionary<string, object?>> ApiKeyAsync(string apiKeyId, CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Get, $"api-keys/{Uri.EscapeDataString(apiKeyId)}", null, ct);

    /// <summary>API 키 이름 변경.</summary>
    public Task<Dictionary<string, object?>> UpdateApiKeyAsync(string apiKeyId, string name, CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Patch, $"api-keys/{Uri.EscapeDataString(apiKeyId)}", new Dictionary<string, object?> { ["name"] = name }, ct);

    /// <summary>API 키 폐기.</summary>
    public Task<Dictionary<string, object?>> DeleteApiKeyAsync(string apiKeyId, CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Delete, $"api-keys/{Uri.EscapeDataString(apiKeyId)}", null, ct);

    /// <summary>승인된 API 키의 발송용 토큰 발급.</summary>
    public Task<Dictionary<string, object?>> IssueTokenAsync(string apiKeyId, CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Post, $"api-keys/{Uri.EscapeDataString(apiKeyId)}/token", new Dictionary<string, object?> {  }, ct);

    /// <summary>허용 IP 목록과 호출자 IP 조회.</summary>
    public Task<Dictionary<string, object?>> AllowedIpsAsync(string apiKeyId, CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Get, $"api-keys/{Uri.EscapeDataString(apiKeyId)}/allowed-ips", null, ct);

    /// <summary>허용 IP 추가. ip와 선택적 description 사용.</summary>
    public Task<Dictionary<string, object?>> AddAllowedIpAsync(string apiKeyId, Dictionary<string, object?> parameters, CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Post, $"api-keys/{Uri.EscapeDataString(apiKeyId)}/allowed-ips", parameters, ct);

    /// <summary>허용 IP 삭제.</summary>
    public Task<Dictionary<string, object?>> DeleteAllowedIpAsync(string apiKeyId, string ipId, CancellationToken ct = default) =>
        RequestAsync(HttpMethod.Delete, $"api-keys/{Uri.EscapeDataString(apiKeyId)}/allowed-ips/{Uri.EscapeDataString(ipId)}", null, ct);

    private async Task<Dictionary<string, object?>> RequestAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, _baseUrl + "/api/v2/account" + (path.Length == 0 ? "" : "/" + path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _agentToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (body is not null) request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var response = await _http.SendAsync(request, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);
        Dictionary<string, object?> data;
        try { data = JsonSerializer.Deserialize<Dictionary<string, object?>>(raw) ?? new(); }
        catch (JsonException) { data = new(); }
        if (!response.IsSuccessStatusCode)
            throw SendgoException.FromResponse((int)response.StatusCode, data, path.Length == 0 ? "account" : path, "v2");
        return data;
    }

    public void Dispose() => _http.Dispose();
}
