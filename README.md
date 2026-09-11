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

## 관리 API — 채널·템플릿·발신번호 등록 (v2 전용)

발송은 처음부터 API였지만 **등록과 심사는 콘솔에서만** 되던 것들이 있었습니다.
1.3.0 부터 그 작업도 코드로 처리합니다.

| 메서드 묶음 | 하는 일 | 계정 |
| --- | --- | --- |
| `*KakaoSender*`, `*KakaoChannelCode*`, `*BrandMessageTargeting*` | 카카오 채널 인증·등록·동기화, 브랜드메시지 M/N 신청 | 기업 |
| `*NoticeTemplate*` | 알림톡 템플릿 CRUD, 검수 요청·취소, 승인 취소, 휴면 해제 | 기업 |
| `*BrandTemplate*` | 브랜드메시지(구 친구톡) 템플릿 CRUD, 동기화, 가져오기 | 기업 |
| `*Sender*` (등록 계열) | 발신번호 등록 신청, 중복 확인, 유형 안내 | 개인·기업 |
| `*MessageTemplate*` | 문자 상용구 템플릿 CRUD | 개인·기업 |
| `*KakaoImage*` | 카카오 이미지 업로드 — 템플릿용 URL 발급 | 기업 |
| `*RejectedNumbers*` | 수신거부(080) 번호 조회 | 개인·기업 |
| `*Webhook*` | 이벤트 웹훅 구독 — 심사 결과 수신 | 개인·기업 |

> **sendgo.io 콘솔에 들어올 일이 없습니다.** 고객의 채널·발신번호·템플릿을
> 여러분 화면만으로 끝까지 처리할 수 있습니다. 휴대폰 발신번호는 콘솔의 PASS
> 본인인증 대신 **신분증 사본(`identityDocument`)을 받아 sendgo 운영자가 대신
> 심사**합니다.
>
> 사람이 개입하는 지점은 **카카오 채널 인증번호 하나**뿐이고, 그마저도
> 여러분 화면에서 입력받으면 됩니다 — 카카오가 관리자 휴대폰으로 직접 보내는
> 확인이라 없앨 수 없습니다.
>
> 심사가 붙는 것들은 **비동기**입니다. 등록 호출이 성공했다는 건 "접수됐다"는
> 뜻이지 "쓸 수 있다"는 뜻이 아닙니다 — 웹훅을 구독해 결과를 받으세요.

### 카카오 채널 등록

```csharp
// 1단계 — 카카오가 관리자 휴대폰으로 인증번호를 SMS 발송한다 (응답에 번호는 없다)
await sendgo.RequestKakaoChannelCodeAsync("@my-channel", "01012345678");

// 2단계 — 사람이 받은 인증번호로 발신프로필 생성
var created = await sendgo.CreateKakaoSenderAsync(new KakaoSenderCreateRequest
{
    Token = "123456",
    YellowId = "@my-channel",
    PhoneNumber = "01012345678",
    CategoryCode = "001001",   // GetKakaoSenderCategoriesAsync 로 조회
});

await sendgo.GetKakaoSendersAsync();
await sendgo.SyncKakaoSendersAsync();                   // 전체 상태 동기화 (하루 한 번 권장)
await sendgo.SyncKakaoSendersAsync(kakaoSenderKey);     // 단건
```

채널이 카카오 쪽에서 차단되면 발송이 조용히 실패하기 시작합니다.
`SyncKakaoSendersAsync()` 를 주기적으로 돌리고 `block: true` 인 채널을 감시하세요.

### 알림톡 템플릿 등록과 검수

```csharp
var created = await sendgo.CreateNoticeTemplateAsync(new NoticeTemplateRequest
{
    KakaoSenderKey = kakaoSenderKey,
    TemplateName = "주문 접수 안내",
    TemplateContent = "#{name}님, 주문 #{orderNo}이 접수되었습니다.",
    TemplateMessageType = "BA",       // BA 기본형 / EX 부가정보형 / AD 채널추가형 / MI 복합형
    TemplateEmphasizeType = "NONE",   // NONE / TEXT / ITEM_LIST / IMAGE
    CategoryCode = "001001",

    // sendgo 자체 정책 게이트 — 카카오 심사와 별개다
    MessagePurpose = "order_delivery",
    LegalBasis = "transaction",
    BenefitOrigin = "none",
    ExpiryType = "none",
});

// 검수 요청 — 증빙이 필요하면 파일도 붙인다 (첨부가 있으면 comment 필수)
await sendgo.RequestNoticeTemplateInspectionAsync(templateCode);
await sendgo.RequestNoticeTemplateInspectionAsync(templateCode, "주문 확인 화면 첨부",
    [MultipartFile.FromPath("attachment", "proof.png", "image/png")]);

// 결과는 비동기다. 웹훅이 없으므로 폴링한다
var synced = await sendgo.SyncNoticeTemplateAsync(templateCode);
// data.template.inspectionStatus — REG → REQ → APR / REJ
```

`OptInReviewConfirmed` · `CtaClearConfirmed` · `PolicyConfirmed` 는 기본값이
`true` 지만, **내용을 실제로 검토한 뒤에** 그대로 두어야 합니다 — 이 값은 법적
확인의 기록입니다.

정책 필드 조합이 본문과 어긋나면 저장 단계에서 `POLICY_VALIDATION_FAILED` 로
막힙니다. 여기서 걸리는 문안은 **카카오 심사에서도 거의 반려**되므로,
며칠 기다렸다 반려당하는 것보다 즉시 아는 편이 낫습니다.

```csharp
await sendgo.GetNoticeTemplatesAsync(kakaoSenderKey, inspectionStatus: "APR");
await sendgo.GetNoticeTemplateAsync(templateCode);
await sendgo.UpdateNoticeTemplateAsync(templateCode, request);   // 본문이 바뀌면 재검수 필요
await sendgo.CancelNoticeTemplateInspectionAsync(templateCode);
await sendgo.CancelNoticeTemplateApprovalAsync(templateCode);
await sendgo.ReleaseNoticeTemplateAsync(templateCode);           // 휴면 해제
await sendgo.DeleteNoticeTemplateAsync(templateCode);            // sendgo 목록에서만 삭제된다
await sendgo.GetNoticeTemplateCategoriesAsync();
```

이미지 템플릿은 multipart 로 나갑니다.

```csharp
await sendgo.CreateNoticeTemplateWithImageAsync(
    request with { TemplateEmphasizeType = "IMAGE" },
    MultipartFile.FromPath("image", "banner.jpg", "image/jpeg"));
```

> **삭제 동작이 채널마다 다릅니다.** 알림톡 템플릿은 카카오에 삭제 API 가 없어
> sendgo 목록에서만 빠지고 동기화하면 되살아납니다. 브랜드메시지 템플릿은
> 카카오 쪽에서도 실제로 삭제됩니다.

### 브랜드메시지 템플릿

```csharp
var created = await sendgo.CreateBrandTemplateAsync(new BrandTemplateRequest
{
    KakaoSenderKey = kakaoSenderKey,
    TemplateName = "여름 세일 안내",
    TemplateType = "FI",   // FT/FI/FW/FL/FC/FM/FP/FA — 서버가 chatBubbleType 으로 변환
    TemplateContent = "여름 세일이 시작되었습니다.",
    ImageUrl = "https://mud-kage.kakao.com/....jpg",
});

await sendgo.GetBrandTemplatesAsync(kakaoSenderKey);
await sendgo.SyncBrandTemplateAsync(templateCode);
await sendgo.ImportBrandTemplatesAsync(kakaoSenderKey);   // 카카오에 있는 템플릿 가져오기
await sendgo.DeleteBrandTemplateAsync(templateCode);      // 카카오에서도 삭제된다
```

응답의 `containsVariables` 가 true 면 동보 발송(`Targeting = "F"`)에는 쓸 수 없습니다.

### 발신번호 등록 신청

```csharp
// 계정 종류에 맞는 유형과 유형별 필수 서류
await sendgo.GetSenderNumberTypesAsync();

// 형식·중복 미리 확인
var check = await sendgo.ValidateSenderNumberAsync("02-1234-5678", "team_main");

var created = await sendgo.RegisterSenderAsync(
    new SenderRegistrationRequest
    {
        SenderAlias = "고객센터 대표번호",
        SenderNumberType = "team_main",   // personal_other / team_main / team_other_company
        PhoneE164 = "02-1234-5678",
        // check 의 duplicationReasonRequired 가 true 면 필수
        // DuplicationReason = "부서별 분리 운영",
    },
    [MultipartFile.FromPath("csuCertificate", "csu.pdf", "application/pdf")]);

// data.sender.status == "PENDING" — 운영자 승인 후 SUCCESS

await sendgo.GetSendersAsync();
await sendgo.UpdateSenderAsync(senderKey, "새 이름");
await sendgo.DeleteSenderAsync(senderKey);
```

**휴대폰 유형도 API 로 접수할 수 있습니다.** 콘솔의 PASS 본인인증 대신
신분증 사본(`identityDocument`)을 첨부하면 sendgo 운영자가 직접 확인합니다.
이 경로로 접수된 건은 응답의 `identityVerificationMethod` 가 `document` 이고
**자동 승인되지 않습니다** — 운영자 확인 전까지 `PENDING` 입니다.

유형별 필수 서류는 `numberTypes()` 응답의 `requiredDocuments` 로 확인하세요.
반려되면 `rejectionReason` 에 사유가 담깁니다.

### 문자 템플릿

```csharp
await sendgo.CreateMessageTemplateAsync(new MessageTemplateRequest
{
    MessageTranType = "LMS",
    MessageTranSubject = "주문 안내",   // LMS·MMS 는 필수
    MessageTranMsg = "주문이 접수되었습니다.",
});

await sendgo.GetMessageTemplatesAsync("LMS");
await sendgo.UpdateMessageTemplateAsync(templateKey, request);
await sendgo.DeleteMessageTemplateAsync(templateKey);
```

### 이벤트 웹훅 — 심사 결과를 밀어 받기

```csharp
var created = await sendgo.SubscribeWebhookAsync(new WebhookSubscriptionRequest
{
    Url = "https://reseller.example.com/hooks/sendgo",
});

// 시크릿은 이 응답에서 한 번만 나온다. 즉시 저장한다.

await sendgo.GetWebhookAsync();        // 구독 설정 + 마지막 전송 결과
await sendgo.TestWebhookAsync();       // 배선 확인
await sendgo.UnsubscribeWebhookAsync();
```

받는 쪽에서는 **원본 바이트**로 서명을 검증합니다.

```csharp
app.MapPost("/hooks/sendgo", async (HttpRequest request) =>
{
    using var buffer = new MemoryStream();
    await request.Body.CopyToAsync(buffer);
    var raw = buffer.ToArray();

    var signature = request.Headers["X-Sendgo-Signature"].ToString();

    if (!SendgoClient.VerifyWebhookSignature(raw, signature, secret))
        return Results.Unauthorized();

    // 처리는 큐로. 여기서 오래 끌면 재시도가 쌓인다.
    await queue.EnqueueAsync(raw);

    return Results.NoContent();
});
```

이벤트 목록은 `WebhookSubscriptionRequest.AllEvents` 로 확인할 수 있습니다.

### 카카오 이미지 업로드

브랜드메시지 템플릿의 `imageUrl` 은 **카카오가 호스팅하는 URL** 이어야 합니다.

```csharp
var uploaded = await sendgo.UploadKakaoImageAsync(
    "default", MultipartFile.FromPath("image", "banner.jpg", "image/jpeg"));

await sendgo.UploadKakaoImagesAsync("carousel_feed", slides);
await sendgo.GetKakaoImageTypesAsync();   // 유형별 필드·최대 개수
```

### 수신거부(080) 동기화

```csharp
// 증분만 가져간다. 하루 한 번이면 충분하다.
await sendgo.GetRejectedNumbersAsync(since: "2026-09-01", count: 500);
```

---

## 변경 사항

### 1.3.0 (2026-09-11)

- **관리 API 추가** — 콘솔에서만 되던 등록·심사를 코드로 처리합니다.
  카카오 채널 등록(`RequestKakaoChannelCodeAsync`, `CreateKakaoSenderAsync`),
  알림톡 템플릿 CRUD·검수(`CreateNoticeTemplateAsync`,
  `RequestNoticeTemplateInspectionAsync`, `SyncNoticeTemplateAsync` 외),
  브랜드메시지 템플릿, 발신번호 등록 신청(`RegisterSenderAsync`),
  문자 상용구 템플릿.
- 요청 모델 추가 — `KakaoSenderCreateRequest`, `NoticeTemplateRequest`,
  `BrandTemplateRequest`, `SenderRegistrationRequest`, `MessageTemplateRequest`,
  그리고 첨부용 `MultipartFile`.
- `SendgoClient` 를 `partial` 로 바꾸고 관리 API 를
  `SendgoClient.Management.cs` 로 분리했습니다. 공개 표면은 그대로입니다.
- multipart 업로드는 60초 타임아웃을 씁니다 — 기본 15초로는 몇 MB 짜리 서류가
  자주 끊깁니다.
- **휴대폰 발신번호도 API 로 접수됩니다.** 콘솔의 PASS 본인인증 대신
  `identityDocument`(신분증 사본)를 첨부하면 sendgo 운영자가 확인합니다.
  이 경로는 자동 승인되지 않고 항상 `PENDING` 으로 시작합니다.
- **이벤트 웹훅** 추가 — 발신번호 승인, 알림톡 검수 결과, 채널 차단,
  브랜드메시지 타겟팅 결과를 구독해 받습니다. 서명은 받은 원본 바이트로
  검증합니다(SDK 에 검증 헬퍼 포함).
- **카카오 이미지 업로드** 추가 — 브랜드메시지 템플릿의 `imageUrl` 은 카카오가
  호스팅하는 URL 이어야 하는데, 그 URL 을 얻는 길이 콘솔에만 있었습니다.
- **수신거부(080) 조회** 추가 — 자기 DB 의 수신 상태를 맞출 수 있습니다.

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
