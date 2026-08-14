# Sendgo.SDK

> **.NET / ASP.NET Core에서 카카오 알림톡, 브랜드메시지, SMS를 가장 쉽게 발송하는 공식 .NET SDK**

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

> ⚠️ **Deprecated — 친구톡은 카카오 정책에 따라 2025-12-31 종료되었습니다.**
> 2026-01-01 부터 친구톡 발송 요청은 카카오 측에서 **브랜드메시지(자유형)** 로 자동 대체 발송됩니다.
> 호출은 계속 성공하며, 자유 본문 타입(`FT`/`FI`/`FW`)을 개별 수신자에게 보내는 경로는
> 현재 이것뿐이므로 기존 코드를 당장 바꿀 필요는 없습니다.
>
> 다음의 경우에는 **브랜드메시지**를 사용하세요.
> - 템플릿 기반 리치 타입 (`FL`/`FC`/`FM`/`FP`/`FA`)
> - 채널 친구가 **아닌** 수신자 (`targeting` = `N` / `I`)
> - 수신 동의한 전체 채널 친구 동보 (`targeting` = `F`)
>
> 메시지 타입은 1:1 대응되며 변환은 서버가 처리합니다 — `FT`→`BT`, `FI`→`BI`, `FW`→`BW`,
> `FL`→`BL`, `FC`→`BC`, `FM`→`BM`, `FP`→`BP`, `FA`→`BA`.

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

## 브랜드메시지 사용법

브랜드메시지는 친구톡의 후속 채널입니다. 메시지 타입이 친구톡과 1:1 대응되며
(`FT`→`BT`, `FI`→`BI`, `FW`→`BW`, `FL`→`BL`, `FC`→`BC`, `FM`→`BM`, `FP`→`BP`, `FA`→`BA`),
요청에는 **친구톡 코드를 그대로** 넘기고 변환은 서버가 처리합니다.

친구톡과 달리 다음이 가능합니다.

- 채널 친구가 **아닌** 수신자에게 발송 (`targeting: N`)
- 수신 동의한 **전체 채널 친구 동보** 발송 (`targeting: F`, 수신자 목록 불필요)
- 리스트·캐러셀·커머스·동영상 등 **템플릿 기반 리치 메시지**

> v2 전용입니다. 자유 본문 타입(`FT`/`FI`/`FW`)을 개별 수신자에게 보낼 때는 여전히 친구톡 API 를 쓰세요 — 이 엔드포인트는 그 조합에 `NOT_A_BRAND_MESSAGE` 를 반환합니다. 친구톡 요청은 카카오 측에서 브랜드메시지(자유형)로 대체 발송됩니다.

```csharp
using Sendgo.Models;

// 단건 발송 — 채널 친구 대상
await client.SendBrandMessageAsync(new BrandMessageRequest
{
    Targeting = "M",
    MessageType = "FL",
    FriendTemplateUuid = "9cd5460b-6458-4edc-9b11-c26d3013c340",
    Contacts = new[] { new Contact { PhoneNumber = "01012345678", Var1 = "29,000원" } },
});

// 동보 발송 — 수신 동의한 전체 채널 친구 (Contacts 불필요)
await client.BroadcastBrandMessageAsync(new BrandMessageRequest
{
    MessageType = "FW",
    FriendTemplateUuid = "9cd5460b-6458-4edc-9b11-c26d3013c340",
});

// 캠페인 조회
var list = await client.GetBrandMessagesAsync(count: 10);
var one  = await client.GetBrandMessageAsync("1f0a6d0e-6b3b-4f0f-9b2f-2f6f6a1b7c11");
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
| `BaseUrl` | `string` | 선택 | `"https://sendgo.io"` | API 기본 URL |

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

## 짧은 URL

짧은 URL 은 메시지 본문의 링크를 줄이고, 그 링크가 실제로 눌렸는지 집계합니다.
문자는 바이트 수가 요금과 직결되므로 링크를 줄이면 그만큼 본문을 더 쓸 수 있습니다.

같은 원본 URL 을 다시 줄이면 **기존 링크가 그대로 반환**됩니다. 캠페인별로 반응을
따로 집계하려면 `forceNew` 로 새 코드를 만드세요.

`deactivate` 는 링크를 삭제하지 않고 리다이렉트만 중지합니다. 이미 발송한 메시지의
링크를 무효화할 때 쓰며, 누적 통계는 남고 이후 접속은 `410 Gone` 이 됩니다.

```csharp
// 짧은 URL 생성 (v2 전용)
var created = await sendgo.CreateShortUrlAsync(new ShortUrlRequest
{
    TargetUrl = "https://example.com/promotions/summer-sale",
    Title = "여름 세일 랜딩",
}, ct);

// 반응 통계 — 일별 추이 + 디바이스/유입경로/국가별 분해
var stats = await sendgo.GetShortUrlStatsAsync(code, from: "2026-08-01", ct: ct);

await sendgo.GetShortUrlsAsync(count: 10, ct: ct);
await sendgo.GetShortUrlAsync(code, ct);
await sendgo.DeactivateShortUrlAsync(code, ct);   // 리다이렉트만 중지, 통계는 남는다
```

`stats` 는 일별 추이(`daily`)와 디바이스(`byDevice`)·유입경로(`byReferer`)·국가(`byCountry`)별
분해를 반환합니다. 일별 추이는 사전 집계 표에서 읽으므로 클릭이 많아도 응답 시간이 일정합니다.

## 변경 사항

### 1.2.1 (2026-08-14)

- 레지스트리 목록에 노출되는 패키지 설명에서 친구톡을 브랜드메시지로 교체했습니다.
  npm/PyPI/Packagist/Maven/NuGet/RubyGems 검색 결과에 그대로 찍히는 문자열이라
  종료된 채널을 계속 홍보하고 있었습니다.
- 검색 키워드에 `brand-message` 를 추가했습니다 (`friendtalk` 은 유입 검색어라 유지).

### 1.2.0 (2026-08-14)

- **친구톡 Deprecated 표기** — 친구톡은 카카오 정책에 따라 2025-12-31 종료되었고,
  2026-01-01 부터 발송 요청이 브랜드메시지(자유형)로 자동 대체 발송됩니다.
  관련 API 에 각 언어의 표준 deprecation 표기를 달았습니다.
- 자유 본문 타입(`FT`/`FI`/`FW`)의 개별 발송 경로는 아직 친구톡 API 뿐이라는 점을
  문서에 명시했습니다 — 브랜드메시지 API 는 그 조합에 `NOT_A_BRAND_MESSAGE` 를 반환합니다.
- 브랜드메시지 전환 안내와 메시지 타입 1:1 대응표를 README 에 추가했습니다.

### 1.1.0 (2026-08-11)

- 짧은 URL 추가 — `CreateShortUrlAsync` / `GetShortUrlsAsync` / `GetShortUrlAsync` / `GetShortUrlStatsAsync` / `DeactivateShortUrlAsync`
- `ShortUrlRequest` record 추가
- `DeleteAsync` 헬퍼 추가

## 라이선스

MIT License © 2026 [Sendgo](https://sendgo.io)

---

*키워드: 카카오 알림톡 .NET, 카카오 친구톡 ASP.NET, SMS 발송 C#, 알림톡 NuGet, .NET 카카오 API 연동, Sendgo .NET SDK, ASP.NET Core 알림 발송*
