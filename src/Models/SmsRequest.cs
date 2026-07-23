using System.Text.Json.Serialization;

namespace Sendgo.Models;

/// <summary>SMS / LMS / MMS 전송 요청.</summary>
public record SmsRequest
{
    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("contacts")]
    public required IReadOnlyList<Contact> Contacts { get; init; }

    [JsonPropertyName("messageType")]
    public string MessageType { get; init; } = "SMS";

    [JsonPropertyName("campaignType")]
    public string CampaignType { get; init; } = "MESSAGE";

    [JsonPropertyName("scheduleType")]
    public string ScheduleType { get; init; } = "DIRECTLY";

    [JsonPropertyName("at")]
    public string? At { get; init; }

    [JsonPropertyName("subject")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Subject { get; init; }

    [JsonPropertyName("files")]
    public IReadOnlyList<object> Files { get; init; } = Array.Empty<object>();
}
