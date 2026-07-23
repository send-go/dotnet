namespace Sendgo;

/// <summary>Sendgo SDK 설정.</summary>
public class SendgoOptions
{
    /// <summary>Sendgo API 기본 URL (기본값: https://sendgo.io)</summary>
    public string BaseUrl { get; set; } = "https://sendgo.io";

    /// <summary>Access Key (필수)</summary>
    public required string AccessKey { get; set; }

    /// <summary>Secret Key (필수)</summary>
    public required string SecretKey { get; set; }

    /// <summary>카카오 발신프로필 키</summary>
    public string? KakaoSenderKey { get; set; }

    /// <summary>SMS 발신자 키</summary>
    public string? SmsSenderKey { get; set; }

    /// <summary>API 버전 (v1 | v2, 기본값: v1)</summary>
    public string ApiVersion { get; set; } = "v1";
}
