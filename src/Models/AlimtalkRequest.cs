using System.Text.Json.Serialization;

namespace Sendgo.Models;

/// <summary>카카오 알림톡 전송 요청.</summary>
public record AlimtalkRequest
{
    [JsonPropertyName("templateCode")]
    public required string TemplateCode { get; init; }

    [JsonPropertyName("contacts")]
    public required IReadOnlyList<Contact> Contacts { get; init; }

    [JsonPropertyName("scheduleType")]
    public string ScheduleType { get; init; } = "DIRECTLY";

    [JsonPropertyName("at")]
    public string? At { get; init; }

    [JsonPropertyName("replaceSms")]
    public string ReplaceSms { get; init; } = "N";

    [JsonPropertyName("smsSubject")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SmsSubject { get; init; }

    [JsonPropertyName("smsContent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SmsContent { get; init; }
}
