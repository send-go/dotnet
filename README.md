# Sendgo.SDK

> **Sendgo** .NET SDK — 카카오 알림톡/친구톡, SMS/LMS/MMS
> .NET 8+, ASP.NET Core에서 사용 가능합니다.

[![NuGet](https://img.shields.io/nuget/v/Sendgo.SDK)](https://www.nuget.org/packages/Sendgo.SDK)
[![.NET](https://img.shields.io/badge/.NET-8+-purple)](https://dotnet.microsoft.com)

---

## 빠른 시작 (3단계)

### 1단계 — 설치

```bash
dotnet add package Sendgo.SDK
```

### 2단계 — appsettings.json 설정

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

### 3단계 — 알림톡 전송

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

await client.SendAlimtalkAsync(new AlimtalkRequest
{
    TemplateCode = "ORDER_CONFIRM_001",
    Contacts     = [new Contact { PhoneNumber = "01012345678", Name = "홍길동", Var1 = "ORD-001" }],
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
// Controllers/NotifyController.cs
[ApiController]
[Route("api/[controller]")]
public class NotifyController(SendgoClient sendgo) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] NotifyRequest req)
    {
        await sendgo.SendAlimtalkAsync(new AlimtalkRequest
        {
            TemplateCode = "ORDER_CONFIRM_001",
            Contacts     = [new Contact { PhoneNumber = req.Phone, Var1 = req.OrderNumber }],
        });
        return Ok(new { success = true });
    }
}
```

---

## 기능별 사용법

### 알림톡

```csharp
// SMS 대체 발송
await client.SendAlimtalkAsync(new AlimtalkRequest
{
    TemplateCode = "DELIVERY_001",
    ReplaceSms   = "Y",
    SmsSubject   = "[배송 안내]",
    SmsContent   = "상품이 출고되었습니다.",
    Contacts     = [new Contact { PhoneNumber = "01012345678", Var1 = "ORD-001" }],
});
```

### SMS / LMS / MMS

```csharp
// SMS
await client.SendSmsAsync(new SmsRequest
{
    Content  = "인증번호: 123456",
    Contacts = [new Contact { PhoneNumber = "01012345678" }],
});

// LMS
await client.SendLmsAsync(new SmsRequest
{
    Subject  = "[공지사항]",
    Content  = "서비스 점검이 예정되어 있습니다.",
    Contacts = [new Contact { PhoneNumber = "01012345678" }],
});
```

---

## 예외 처리

```csharp
using Sendgo.Exceptions;

try
{
    await client.SendAlimtalkAsync(...);
}
catch (SendgoException ex)
{
    logger.LogError("알림톡 발송 실패: status={Status}, code={Code}", ex.StatusCode, ex.ErrorCode);
    switch (ex.ErrorCode)
    {
        case "INVALID_TEMPLATE_CODE": /* 템플릿 코드 확인 */ break;
        case "PAYMENT_REQUIRED":      /* 크레딧 부족 처리 */ break;
    }
}
```

---

## 라이선스

MIT License © [Sendgo](https://sendgo.io)
