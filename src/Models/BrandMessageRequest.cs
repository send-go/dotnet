using System.Text.Json.Serialization;

namespace Sendgo.Models;

/// <summary>
/// 카카오 브랜드메시지 전송 요청.
/// </summary>
/// <remarks>
/// 브랜드메시지는 친구톡의 후속 채널로, <see cref="MessageType"/> 에는 친구톡 코드
/// (FT/FI/FW/FL/FC/FM/FP/FA)를 그대로 넘기며 브랜드메시지 코드
/// (BT/BI/BW/BL/BC/BM/BP/BA) 변환은 서버가 처리합니다.
///
/// <see cref="Targeting"/> 은 M(채널 친구) / N(비친구) / I(전체) / F(동보)이며,
/// F 는 수신자 목록을 카카오 측에서 확장하므로 <see cref="Contacts"/> 를 넘기지 않습니다.
/// </remarks>
public record BrandMessageRequest
{
    /// <summary>브랜드 템플릿 UUID.</summary>
    [JsonPropertyName("friendTemplateUuid")]
    public required string FriendTemplateUuid { get; init; }

    /// <summary>발송 대상. M | N | I | F</summary>
    [JsonPropertyName("targeting")]
    public string Targeting { get; init; } = "M";

    /// <summary>친구톡 메시지 코드. FT | FI | FW | FL | FC | FM | FP | FA</summary>
    [JsonPropertyName("messageType")]
    public string MessageType { get; init; } = "FT";

    /// <summary>수신자 목록. Targeting 이 M/N/I 일 때 필요합니다.</summary>
    [JsonPropertyName("contacts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<Contact>? Contacts { get; init; }

    [JsonPropertyName("scheduleType")]
    public string ScheduleType { get; init; } = "DIRECTLY";

    [JsonPropertyName("at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? At { get; init; }

    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; init; }

    [JsonPropertyName("buttons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<object>? Buttons { get; init; }

    [JsonPropertyName("imageUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("imageLink")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageLink { get; init; }

    /// <summary>광고성 메시지 여부.</summary>
    [JsonPropertyName("adFlag")]
    public string AdFlag { get; init; } = "Y";

    [JsonPropertyName("adult")]
    public string Adult { get; init; } = "N";

    [JsonPropertyName("pushAlarm")]
    public string PushAlarm { get; init; } = "Y";

    [JsonPropertyName("header")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Header { get; init; }

    [JsonPropertyName("coupon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Coupon { get; init; }

    /// <summary>와이드 아이템 리스트(BL) 정보.</summary>
    [JsonPropertyName("item")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Item { get; init; }

    /// <summary>커머스(BM) 정보.</summary>
    [JsonPropertyName("commerce")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Commerce { get; init; }

    /// <summary>캐러셀 리스트(BC / BA).</summary>
    [JsonPropertyName("list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<object>? List { get; init; }

    [JsonPropertyName("head")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Head { get; init; }

    [JsonPropertyName("tail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Tail { get; init; }

    /// <summary>프리미엄 동영상(BP) 정보.</summary>
    [JsonPropertyName("video")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Video { get; init; }

    [JsonPropertyName("additionalContent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AdditionalContent { get; init; }

    /// <summary>동보(Targeting "F") 발송 대상 그룹 키.</summary>
    [JsonPropertyName("friendGroupKey")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FriendGroupKey { get; init; }

    [JsonPropertyName("replaceSms")]
    public string ReplaceSms { get; init; } = "N";

    [JsonPropertyName("smsSubject")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SmsSubject { get; init; }

    [JsonPropertyName("smsContent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SmsContent { get; init; }

    /// <summary>수신거부(080) 서비스 ID.</summary>
    [JsonPropertyName("rejectServiceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RejectServiceId { get; init; }

    [JsonPropertyName("webhooks")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Webhooks { get; init; }
}
