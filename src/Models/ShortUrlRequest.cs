using System.Text.Json.Serialization;

namespace Sendgo.Models;

/// <summary>
/// 짧은 URL 생성 요청.
/// </summary>
/// <example>
/// var created = await sendgo.CreateShortUrlAsync(new ShortUrlRequest
/// {
///     TargetUrl = "https://example.com/promotions/summer-sale",
///     Title = "여름 세일 랜딩",
/// });
/// </example>
public record ShortUrlRequest
{
    /// <summary>줄일 원본 URL. http/https 만 허용됩니다.</summary>
    [JsonPropertyName("targetUrl")]
    public required string TargetUrl { get; init; }

    /// <summary>관리 화면에서 구분하기 위한 이름.</summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }

    /// <summary>이 시각 이후에는 리다이렉트하지 않고 410 Gone 을 반환합니다.</summary>
    [JsonPropertyName("expiresAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExpiresAt { get; init; }

    /// <summary>
    /// true 면 같은 URL 이라도 새 코드를 만듭니다.
    /// 캠페인별로 반응을 분리해 집계할 때 사용합니다.
    /// </summary>
    [JsonPropertyName("forceNew")]
    public bool ForceNew { get; init; }
}
