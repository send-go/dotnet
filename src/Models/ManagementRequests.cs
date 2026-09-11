using System.Text.Json.Serialization;

namespace Sendgo.Models;

/// <summary>
/// 카카오 발신프로필 등록 요청 (2단계).
/// </summary>
/// <remarks>
/// <see cref="Token"/> 은 <c>RequestKakaoChannelCodeAsync</c> 가 발송을 트리거한
/// 인증번호로, 카카오가 채널 관리자 휴대폰에 SMS 로 보냅니다. SDK 는 그 값을
/// 볼 수 없습니다 — 사람이 문자를 읽어 넣어야 합니다.
/// </remarks>
public record KakaoSenderCreateRequest
{
    /// <summary>1단계에서 관리자 휴대폰으로 받은 인증번호.</summary>
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    /// <summary>채널 검색용 아이디. "@" 는 있어도 없어도 됩니다.</summary>
    [JsonPropertyName("yellowId")]
    public required string YellowId { get; init; }

    /// <summary>채널 관리자 휴대폰 번호.</summary>
    [JsonPropertyName("phoneNumber")]
    public required string PhoneNumber { get; init; }

    /// <summary><c>GetKakaoSenderCategoriesAsync</c> 로 조회한 코드.</summary>
    [JsonPropertyName("categoryCode")]
    public required string CategoryCode { get; init; }
}

/// <summary>
/// 알림톡 템플릿 등록·수정 요청.
/// </summary>
/// <remarks>
/// 뒤쪽 정책 속성 일곱 개는 sendgo 자체 게이트입니다. 카카오 심사와 별개이며,
/// 조합이 본문과 어긋나면 POLICY_VALIDATION_FAILED 로 거절됩니다. 확인 플래그
/// 셋은 기본값이 true 지만, 내용을 실제로 검토한 뒤에 그대로 두어야 합니다 —
/// 이 값은 법적 확인의 기록입니다.
/// </remarks>
public record NoticeTemplateRequest
{
    /// <summary>발신프로필 키. 수정 시에는 무시됩니다 (변경 불가).</summary>
    [JsonPropertyName("kakaoSenderKey")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KakaoSenderKey { get; init; }

    /// <summary>템플릿명 (150자).</summary>
    [JsonPropertyName("templateName")]
    public required string TemplateName { get; init; }

    /// <summary>본문. 변수는 #{name} 형식으로 씁니다.</summary>
    [JsonPropertyName("templateContent")]
    public required string TemplateContent { get; init; }

    /// <summary>BA 기본형 / EX 부가정보형 / AD 채널추가형 / MI 복합형.</summary>
    [JsonPropertyName("templateMessageType")]
    public string TemplateMessageType { get; init; } = "BA";

    /// <summary>NONE / TEXT 강조표기 / ITEM_LIST / IMAGE.</summary>
    [JsonPropertyName("templateEmphasizeType")]
    public string TemplateEmphasizeType { get; init; } = "NONE";

    /// <summary>6자리 숫자. 카테고리 조회로 확인합니다.</summary>
    [JsonPropertyName("categoryCode")]
    public required string CategoryCode { get; init; }

    /// <summary>강조표기형(TEXT)의 핵심 정보. TEXT 유형은 필수입니다.</summary>
    [JsonPropertyName("templateTitle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TemplateTitle { get; init; }

    /// <summary>강조표기형(TEXT)의 보조 문구. TEXT 유형은 필수입니다.</summary>
    [JsonPropertyName("templateSubtitle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TemplateSubtitle { get; init; }

    /// <summary>아이템 리스트 헤더 (16자).</summary>
    [JsonPropertyName("templateHeader")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TemplateHeader { get; init; }

    /// <summary>부가 정보 (500자). EX·MI 유형은 필수입니다.</summary>
    [JsonPropertyName("templateExtra")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TemplateExtra { get; init; }

    /// <summary>아이템 리스트. ITEM_LIST 유형은 list 키가 필수입니다.</summary>
    [JsonPropertyName("templateItem")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? TemplateItem { get; init; }

    /// <summary>아이템 하이라이트.</summary>
    [JsonPropertyName("templateItemHighlight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? TemplateItemHighlight { get; init; }

    /// <summary>대표링크 정보.</summary>
    [JsonPropertyName("templateRepresentLink")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? TemplateRepresentLink { get; init; }

    /// <summary>버튼. AD·MI 유형은 linkType "AC" 버튼이 필수입니다.</summary>
    [JsonPropertyName("buttons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object?>>? Buttons { get; init; }

    /// <summary>바로연결.</summary>
    [JsonPropertyName("quickReplies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object?>>? QuickReplies { get; init; }

    /// <summary>보안 템플릿 여부. 켜면 수신자 기기에서 내용이 가려집니다.</summary>
    [JsonPropertyName("securityFlag")]
    public bool SecurityFlag { get; init; }

    /// <summary>성인용 메시지 여부.</summary>
    [JsonPropertyName("adultFlag")]
    public bool AdultFlag { get; init; }

    /// <summary>
    /// 메시지 목적. order_delivery / reservation_booking / payment_billing /
    /// account_auth / service_ops / policy_notice / benefit_notice /
    /// customer_support / other.
    /// </summary>
    [JsonPropertyName("messagePurpose")]
    public required string MessagePurpose { get; init; }

    /// <summary>발송 근거. transaction / paid_purchase / event_entry / contract / policy_notice.</summary>
    [JsonPropertyName("legalBasis")]
    public required string LegalBasis { get; init; }

    /// <summary>혜택 발생 경위. none / paid / event / contract / promo / free.</summary>
    [JsonPropertyName("benefitOrigin")]
    public required string BenefitOrigin { get; init; }

    /// <summary>소멸 유형. none / rights_based / promo.</summary>
    [JsonPropertyName("expiryType")]
    public required string ExpiryType { get; init; }

    /// <summary>사전동의 검토 확인. 실제로 검토한 뒤에 true 로 둡니다.</summary>
    [JsonPropertyName("optInReviewConfirmed")]
    public bool OptInReviewConfirmed { get; init; } = true;

    /// <summary>구매·혜택 유도 문구가 없음을 확인.</summary>
    [JsonPropertyName("ctaClearConfirmed")]
    public bool CtaClearConfirmed { get; init; } = true;

    /// <summary>발신자 정책 확인. false 면 검수를 요청할 수 없습니다.</summary>
    [JsonPropertyName("policyConfirmed")]
    public bool PolicyConfirmed { get; init; } = true;
}

/// <summary>
/// 브랜드메시지(구 친구톡) 템플릿 등록·수정 요청.
/// </summary>
/// <remarks>
/// <see cref="TemplateType"/> 은 친구톡 표기(FT/FI/FW/FL/FC/FM/FP/FA)를 그대로
/// 씁니다 — 서버가 chatBubbleType 으로 변환합니다.
/// </remarks>
public record BrandTemplateRequest
{
    /// <summary>발신프로필 키. 수정 시에는 무시됩니다 (변경 불가).</summary>
    [JsonPropertyName("kakaoSenderKey")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KakaoSenderKey { get; init; }

    /// <summary>템플릿명 (200자).</summary>
    [JsonPropertyName("templateName")]
    public required string TemplateName { get; init; }

    /// <summary>FT 텍스트 / FI 이미지 / FW 와이드 / FL 리스트 / FC 캐러셀 / FM 커머스 / FP 동영상 / FA 캐러셀커머스.</summary>
    [JsonPropertyName("templateType")]
    public string TemplateType { get; init; } = "FT";

    /// <summary>본문. FW·FP 는 76자, FI 는 400자, FT 는 1000자 제한.</summary>
    [JsonPropertyName("templateContent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TemplateContent { get; init; }

    /// <summary>성인용 메시지 여부.</summary>
    [JsonPropertyName("adult")]
    public bool Adult { get; init; }

    /// <summary>헤더 (100자).</summary>
    [JsonPropertyName("header")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Header { get; init; }

    /// <summary>서버는 이 필드만 snake_case 로 받습니다.</summary>
    [JsonPropertyName("additional_content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AdditionalContent { get; init; }

    /// <summary>FI·FW 는 필수.</summary>
    [JsonPropertyName("imageUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageUrl { get; init; }

    /// <summary>이미지 클릭 시 이동할 URL.</summary>
    [JsonPropertyName("imageLink")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageLink { get; init; }

    /// <summary>버튼 (최대 5개). FM 커머스는 최소 1개 필요합니다.</summary>
    [JsonPropertyName("buttons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object?>>? Buttons { get; init; }

    /// <summary>쿠폰.</summary>
    [JsonPropertyName("coupon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Coupon { get; init; }

    /// <summary>아이템.</summary>
    [JsonPropertyName("item")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Item { get; init; }

    /// <summary>커머스 정보. FM·FA 유형에서 씁니다.</summary>
    [JsonPropertyName("commerce")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Commerce { get; init; }

    /// <summary>리스트 항목.</summary>
    [JsonPropertyName("list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object?>>? List { get; init; }

    /// <summary>리스트 헤드.</summary>
    [JsonPropertyName("head")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Head { get; init; }

    /// <summary>리스트 테일.</summary>
    [JsonPropertyName("tail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Tail { get; init; }

    /// <summary>동영상. FP 프리미엄 동영상 유형에서 씁니다.</summary>
    [JsonPropertyName("video")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Video { get; init; }

    /// <summary>대표 와이드 아이템. FL 유형에서 씁니다.</summary>
    [JsonPropertyName("mainWideItem")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? MainWideItem { get; init; }

    /// <summary>하위 와이드 아이템 목록. FL 유형에서 씁니다.</summary>
    [JsonPropertyName("subWideItemList")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object?>>? SubWideItemList { get; init; }
}

/// <summary>
/// 발신번호 등록 신청의 텍스트 필드. 서류는 <see cref="MultipartFile"/> 목록으로
/// 따로 넘깁니다.
/// </summary>
public record SenderRegistrationRequest
{
    /// <summary>API로 접수할 수 있는 발신번호 유형 — 전부입니다.</summary>
    public static readonly string[] RegistrableTypes =
    [
        "personal_mobile",
        "personal_other",
        "team_main",
        "team_representative_mobile",
        "team_emp_mobile",
        "team_other_company",
    ];

    /// <summary>
    /// 신분증 사본(<c>identityDocument</c>)이 필요한 유형.
    /// </summary>
    /// <remarks>
    /// 콘솔은 PASS 본인인증을 쓰지만 API는 신분증 사본을 받아 sendgo 운영자가
    /// 직접 확인합니다. 이 경로로 접수된 건은 자동 승인되지 않습니다.
    /// </remarks>
    public static readonly string[] IdentityDocumentTypes =
        ["personal_mobile", "team_representative_mobile", "team_emp_mobile"];

    /// <summary>계정 안에서 중복될 수 없는 관리용 이름 (20자).</summary>
    public required string SenderAlias { get; init; }

    /// <summary>
    /// 여섯 유형 전부 API로 접수할 수 있습니다. 휴대폰 계열은 PASS 대신
    /// <c>identityDocument</c>(신분증 사본)를 함께 올립니다.
    /// </summary>
    public required string SenderNumberType { get; init; }

    /// <summary>숫자와 하이픈만. 서버가 E.164 로 정규화합니다.</summary>
    public required string PhoneE164 { get; init; }

    /// <summary>중복 확인 결과 duplicationReasonRequired 가 true 면 필수.</summary>
    public string? DuplicationReason { get; init; }

    /// <summary>team_other_company 필수. acceptance_representative / acceptance_employee.</summary>
    public string? AcceptanceName { get; init; }

    /// <summary>team_other_company 필수. delegation_representative / delegation_employee.</summary>
    public string? DelegationName { get; init; }

    /// <summary>team_other_company 필수.</summary>
    public string? DelegationReason { get; init; }

    /// <summary>
    /// multipart 필드 맵. 빈 값은 넣지 않습니다 — 서버가 "빈 값으로 저장"으로 읽습니다.
    /// </summary>
    internal Dictionary<string, object?> ToFields()
    {
        var fields = new Dictionary<string, object?>
        {
            ["senderAlias"] = SenderAlias,
            ["senderNumberType"] = SenderNumberType,
            ["phoneE164"] = PhoneE164,
        };

        void Put(string key, string? value)
        {
            if (!string.IsNullOrEmpty(value)) fields[key] = value;
        }

        Put("duplicationReason", DuplicationReason);
        Put("acceptanceName", AcceptanceName);
        Put("delegationName", DelegationName);
        Put("delegationReason", DelegationReason);

        return fields;
    }
}

/// <summary>문자(SMS/LMS/MMS) 상용구 템플릿 등록·수정 요청.</summary>
public record MessageTemplateRequest
{
    /// <summary>SMS / LMS / MMS.</summary>
    [JsonPropertyName("messageTranType")]
    public string MessageTranType { get; init; } = "SMS";

    /// <summary>본문 (2,000자).</summary>
    [JsonPropertyName("messageTranMsg")]
    public required string MessageTranMsg { get; init; }

    /// <summary>제목 (40자). LMS·MMS 는 필수. SMS 에 넣으면 발송 시 버려집니다.</summary>
    [JsonPropertyName("messageTranSubject")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MessageTranSubject { get; init; }

    /// <summary>즐겨찾기.</summary>
    [JsonPropertyName("isFavorite")]
    public bool IsFavorite { get; init; }
}

/// <summary>
/// 이벤트 웹훅 구독 생성·수정 요청.
/// </summary>
/// <remarks>
/// 등록·심사는 비동기입니다. 구독해 두면 상태가 바뀔 때마다 밀어 줍니다.
/// </remarks>
public record WebhookSubscriptionRequest
{
    /// <summary>발신번호 심사 상태가 바뀜.</summary>
    public const string EventSenderStatus = "sender.status_changed";

    /// <summary>알림톡 검수 상태가 바뀜.</summary>
    public const string EventNoticeTemplateInspection = "notice_template.inspection_status_changed";

    /// <summary>카카오 채널의 차단·휴면·프로필 상태가 바뀜.</summary>
    public const string EventKakaoSenderStatus = "kakao_sender.status_changed";

    /// <summary>브랜드메시지 M/N 신청 상태가 바뀜.</summary>
    public const string EventBrandMessageTargeting = "kakao_sender.brand_message_status_changed";

    /// <summary>구독할 수 있는 이벤트 전체.</summary>
    public static readonly string[] AllEvents =
    [
        EventSenderStatus,
        EventNoticeTemplateInspection,
        EventKakaoSenderStatus,
        EventBrandMessageTargeting,
    ];

    /// <summary>이벤트를 받을 주소. <b>https만 허용</b>됩니다.</summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>
    /// 서명 키 (16~128자). 비우면 서버가 만들어 응답에서 한 번만 돌려줍니다.
    /// 이미 있는 상태에서 비우면 기존 값을 유지합니다.
    /// </summary>
    [JsonPropertyName("secret")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Secret { get; init; }

    /// <summary>구독할 이벤트. 비우면 전체 구독.</summary>
    [JsonPropertyName("events")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Events { get; init; }

    /// <summary>구독 활성 여부.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; } = true;
}
