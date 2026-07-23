# Sendgo.SDK

> **.NET / ASP.NET Core에서 카카오 알림톡, 친구톡, SMS를 가장 쉽게 발송하는 공식 .NET SDK**

[![NuGet](https://img.shields.io/nuget/v/Sendgo.SDK)](https://www.nuget.org/packages/Sendgo.SDK)
[![.NET](https://img.shields.io/badge/.NET-8+-purple)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

`Sendgo.SDK`는 [Sendgo](https://sendgo.io) 알림 API를 위한 공식 .NET SDK입니다.
`HttpClient` 기반의 완전한 비동기(`async/await`) 지원, ASP.NET Core DI 통합을 제공합니다.

---

## 설치

```bash
dotnet add package Sendgo.SDK
```

또는 NuGet Package Manager:

```
Install-Package Sendgo.SDK
```

---

## 빠른 시작

### 1단계 — appsettings.json 설정

```json
{
  "Sendgo": {
    "AccessKey":      "your_access_key",
    "SecretKey":      "your_secret_key",
    "KakaoSenderKey": "your_kakao_key",
    "SmsSenderKey":   "your_sms_key",
    "ApiVersion":     "v2"
  }
}
```

### 2단계 — 클라이언트 초기화

```csharp
using Sendgo;
using Sendgo.Models;

var client = new SendgoClient(new SendgoOptions
{
    AccessKey      = Environment.GetEnvironmentVariable("SENDGO_ACCESS_KEY")!,
    SecretKey      = Environment.GetEnvironmentVariable("SENDGO_SECRET_KEY")!,
    KakaoSenderKey = Environment.GetEnvironmentVariable("SENDGO_KAKAO_KEY"),
    SmsSenderKey   = Environment.GetEnvironmentVariable("SENDGO_SMS_KEY"),
    ApiVersion     = "v2",
});
```

### 3단계 — 알림톡 전송

```csharp
await client.SendAlimtalkAsync(new AlimtalkRequest
{
    TemplateCode = "ORDER_CONFIRM_001",
    Contacts =
    [
        new Contact { PhoneNumber = "01012345678", Name = "홍길동", Var1 = "ORD-001", Var2 = "29,000원" }
    ],
});
```

---

## 알림톡 상세 사용법

```csharp
using Sendgo;
using Sendgo.Models;

// 다건 발송
await client.SendAlimtalkAsync(new AlimtalkRequest
{
    TemplateCode = "ORDER_CONFIRM_001",
    Contacts =
    [
        new Contact { PhoneNumber = "01011111111", Name = "홍길동", Var1 = "ORD-001", Var2 = "29,000원" },
        new Contact { PhoneNumber = "01022222222", Name = "김철수", Var1 = "ORD-002", Var2 = "15,000원" },
        new Contact { PhoneNumber = "01033333333", Name = "이영희", Var1 = "ORD-003", Var2 = "52,000원" },
    ],
});

// 예약 발송
await client.SendAlimtalkAsync(new AlimtalkRequest
{
    TemplateCode = "PROMO_SUMMER_2026",
    ScheduleType = "SCHEDULED",
    At           = "2026-07-28 09:00:00",
    Contacts     = [new Contact { PhoneNumber = "01012345678", Var1 = "여름 한정 50% 할인" }],
});

// SMS 자동 대체 발송
await client.SendAlimtalkAsync(new AlimtalkRequest
{
    TemplateCode = "DELIVERY_START_001",
    ReplaceSms   = "Y",
    SmsSubject   = "[배송 시작 안내]",
    SmsContent   = "주문하신 상품이 출고되었습니다.\n송장번호: #{var2}",
    Contacts     = [new Contact { PhoneNumber = "01012345678", Var1 = "ORD-001", Var2 = "1234567890" }],
});
```

---

## 친구톡 사용법

```csharp
// 텍스트형
await client.SendFriendtalkAsync(new FriendtalkRequest
{
    Content  = "안녕하세요! 7월 한정 특가 이벤트를 확인해보세요.",
    Contacts = [new Contact { PhoneNumber = "01012345678" }],
});

// 이미지형
await client.SendFriendtalkAsync(new FriendtalkRequest
{
    MessageType = "FI",
    Content     = "이번 주 특가 상품을 확인하세요!",
    ImageUrl    = "https://cdn.example.com/banner.jpg",
    ImageLink   = "https://example.com/event",
    Contacts    = [new Contact { PhoneNumber = "01012345678" }],
});
```

---

## SMS / LMS / MMS 사용법

```csharp
// SMS
await client.SendSmsAsync(new SmsRequest
{
    Content  = "[Sendgo] 인증번호: 123456 (5분 이내 입력)",
    Contacts = [new Contact { PhoneNumber = "01012345678" }],
});

// LMS
await client.SendLmsAsync(new SmsRequest
{
    Subject  = "[중요] 서비스 점검 안내",
    Content  = "안녕하세요. 서비스 점검이 예정되어 있습니다.\n■ 일시: 2026-07-25 02:00 ~ 06:00",
    Contacts = [new Contact { PhoneNumber = "01012345678" }],
});

// MMS
await client.SendMmsAsync(new SmsRequest
{
    Subject  = "[이벤트] 7월 특가",
    Content  = "이번 달 특가 상품을 확인하세요!",
    Contacts = [
        new Contact { PhoneNumber = "01011111111" },
        new Contact { PhoneNumber = "01022222222" },
    ],
});
```

---

## ASP.NET Core DI 통합

```csharp
// Program.cs
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new SendgoClient(config.GetSection("Sendgo").Get<SendgoOptions>()!);
});
```

```csharp
// Services/NotificationService.cs
public class NotificationService
{
    private readonly SendgoClient _sendgo;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(SendgoClient sendgo, ILogger<NotificationService> logger)
    {
        _sendgo = sendgo;
        _logger = logger;
    }

    public async Task SendOrderConfirmAsync(string phone, string orderNo, string amount)
    {
        await _sendgo.SendAlimtalkAsync(new AlimtalkRequest
        {
            TemplateCode = "ORDER_CONFIRM_001",
            Contacts     = [new Contact { PhoneNumber = phone, Var1 = orderNo, Var2 = amount }],
        });
    }

    public async Task SendVerificationCodeAsync(string phone, string code)
    {
        await _sendgo.SendAlimtalkAsync(new AlimtalkRequest
        {
            TemplateCode = "VERIFY_CODE_001",
            ReplaceSms   = "Y",
            SmsContent   = $"[인증] 인증번호: {code} (5분 이내 입력)",
            Contacts     = [new Contact { PhoneNumber = phone, Var1 = code }],
        });
    }
}
```

```csharp
// Controllers/NotifyController.cs
[ApiController]
[Route("api/[controller]")]
public class NotifyController(SendgoClient sendgo) : ControllerBase
{
    [HttpPost("order")]
    public async Task<IActionResult> Order([FromBody] OrderNotifyRequest req)
    {
        await sendgo.SendAlimtalkAsync(new AlimtalkRequest
        {
            TemplateCode = "ORDER_CONFIRM_001",
            Contacts     = [new Contact { PhoneNumber = req.Phone, Var1 = req.OrderNo, Var2 = req.Amount }],
        });
        return Ok(new { success = true });
    }

    [HttpPost("sms/verify")]
    public async Task<IActionResult> SendVerification([FromBody] VerifyRequest req)
    {
        await sendgo.SendSmsAsync(new SmsRequest
        {
            Content  = $"[인증] 인증번호: {req.Code} (5분 이내 입력)",
            Contacts = [new Contact { PhoneNumber = req.Phone }],
        });
        return Ok(new { success = true });
    }
}
```

---

## Hangfire 비동기 발송

```csharp
// Jobs/NotificationJobs.cs
public class NotificationJobs
{
    private readonly SendgoClient _sendgo;

    public NotificationJobs(SendgoClient sendgo) => _sendgo = sendgo;

    [AutomaticRetry(Attempts = 3)]
    public async Task SendOrderConfirmJob(string phone, string orderNo)
    {
        await _sendgo.SendAlimtalkAsync(new AlimtalkRequest
        {
            TemplateCode = "ORDER_CONFIRM_001",
            Contacts     = [new Contact { PhoneNumber = phone, Var1 = orderNo }],
        });
    }
}

// 사용
BackgroundJob.Enqueue<NotificationJobs>(j => j.SendOrderConfirmJob(phone, orderNo));
```

---

## 예외 처리

```csharp
using Sendgo.Exceptions;

try
{
    await client.SendAlimtalkAsync(new AlimtalkRequest { ... });
}
catch (SendgoException ex)
{
    logger.LogError("알림톡 발송 실패: status={Status}, code={Code}", ex.StatusCode, ex.ErrorCode);

    switch (ex.ErrorCode)
    {
        case "INVALID_ACCESS_KEY":
        case "INVALID_SECRET_KEY":
            AlertOps("Sendgo 인증키를 확인하세요.");
            break;
        case "INVALID_TEMPLATE_CODE":
            logger.LogWarning("존재하지 않는 템플릿: {Template}", ex.Message);
            break;
        case "PAYMENT_REQUIRED":
            AlertOps("Sendgo 크레딧이 부족합니다.");
            break;
        case "IP_NOT_ALLOWED":
            AlertOps("허용되지 않은 IP에서 요청이 발생했습니다.");
            break;
    }
}
```

---

## 설정 옵션

| 프로퍼티 | 타입 | 필수 | 기본값 | 설명 |
|---------|------|------|--------|------|
| `AccessKey` | `string` | **필수** | — | Sendgo 액세스 키 |
| `SecretKey` | `string` | **필수** | — | Sendgo 시크릿 키 |
| `KakaoSenderKey` | `string?` | 선택 | `null` | 카카오 발신프로필 키 |
| `SmsSenderKey` | `string?` | 선택 | `null` | SMS 발신자 키 |
| `ApiVersion` | `string` | 선택 | `"v2"` | API 버전 (`v1` \| `v2`) |
| `BaseUrl` | `string` | 선택 | `"https://api.sendgo.io"` | API 기본 URL |

---

## 관련 패키지

| 언어/프레임워크 | 패키지 | GitHub |
|----------------|--------|--------|
| Spring Boot | `io.sendgo:sendgo-spring` | [spring](https://github.com/send-go/spring) |
| Node.js | `@sendgo/node` | [node](https://github.com/send-go/node) |
| Python | `sendgo-python` | [python](https://github.com/send-go/python) |
| PHP | `sendgo/php` | [php](https://github.com/send-go/php) |
| 전체 목록 | — | [send-go GitHub 조직](https://github.com/send-go) |

---

## 라이선스

MIT License © 2026 [Sendgo](https://sendgo.io)

---

*키워드: 카카오 알림톡 .NET, 카카오 친구톡 ASP.NET, SMS 발송 C#, 알림톡 NuGet, .NET 카카오 API 연동, Sendgo .NET SDK, ASP.NET Core 알림 발송*
