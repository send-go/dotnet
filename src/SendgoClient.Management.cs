using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sendgo.Exceptions;
using Sendgo.Models;

namespace Sendgo;

/// <summary>
/// 관리 API (v2 전용) — 콘솔에서만 되던 등록·심사.
/// </summary>
/// <remarks>
/// 발송과 달리 대부분 <b>즉시 완료되지 않습니다.</b> 등록 호출이 성공했다는 건
/// "접수됐다"는 뜻이지 "쓸 수 있다"는 뜻이 아닙니다.
///
/// <b>sendgo.io 콘솔에 들어올 일이 없습니다.</b> 휴대폰 발신번호는 PASS 대신
/// 신분증 사본을 받아 sendgo 운영자가 대신 심사합니다. 사람이 개입하는 지점은
/// 카카오 채널 인증번호 하나뿐이고, 그것도 여러분 화면에서 입력받으면 됩니다.
///
/// 카카오 관련 메서드는 기업(Team) 소유 애플리케이션 전용입니다.
/// </remarks>
public sealed partial class SendgoClient
{
    // -------------------------------------------------- 카카오 발신프로필(채널)

    /// <summary>
    /// 1단계 — 카카오 채널 인증번호 발송.
    /// </summary>
    /// <remarks>
    /// 응답에 인증번호는 들어있지 않습니다. 카카오가 <paramref name="phoneNumber"/> 로
    /// SMS 를 보내고, 사람이 그 값을 <see cref="CreateKakaoSenderAsync"/> 에 넣어야 합니다.
    /// </remarks>
    public Task<Dictionary<string, object?>> RequestKakaoChannelCodeAsync(
        string yellowId, string phoneNumber, CancellationToken ct = default) =>
        PostAsync(KakaoSenderUrl("token"),
            new Dictionary<string, object?> { ["yellowId"] = yellowId, ["phoneNumber"] = phoneNumber }, ct);

    /// <summary>
    /// 2단계 — 발신프로필 등록.
    /// </summary>
    /// <remarks>
    /// 이미 등록된 채널을 다시 등록해도 오류가 아닙니다. 카카오가 같은 senderKey 를
    /// 돌려주고 서버가 기존 행을 갱신합니다.
    /// </remarks>
    public Task<Dictionary<string, object?>> CreateKakaoSenderAsync(
        KakaoSenderCreateRequest request, CancellationToken ct = default) =>
        PostAsync(KakaoSenderUrl(), ToPayload(request), ct);

    /// <summary>발신프로필 목록 조회.</summary>
    public Task<Dictionary<string, object?>> GetKakaoSendersAsync(CancellationToken ct = default) =>
        GetAsync(KakaoSenderUrl(), ct);

    /// <summary>발신프로필 상세 조회.</summary>
    public Task<Dictionary<string, object?>> GetKakaoSenderAsync(
        string kakaoSenderKey, CancellationToken ct = default) =>
        GetAsync(KakaoSenderUrl(kakaoSenderKey), ct);

    /// <summary>발신프로필 카테고리 조회. 등록 시 CategoryCode 로 넣을 값입니다.</summary>
    public Task<Dictionary<string, object?>> GetKakaoSenderCategoriesAsync(
        string? categoryCode = null, CancellationToken ct = default)
    {
        var url = KakaoSenderUrl("categories");
        if (categoryCode is not null) url += $"?categoryCode={Uri.EscapeDataString(categoryCode)}";

        return GetAsync(url, ct);
    }

    /// <summary>
    /// 발신프로필 상태 동기화. 키를 주면 단건, 없으면 팀 전체.
    /// </summary>
    /// <remarks>
    /// 채널이 카카오 쪽에서 차단·휴면되면 발송이 조용히 실패하기 시작합니다.
    /// 그 사실을 먼저 알 방법은 이 호출뿐이므로 하루 한 번 정도 돌리는 게 좋습니다.
    /// </remarks>
    public Task<Dictionary<string, object?>> SyncKakaoSendersAsync(
        string? kakaoSenderKey = null, CancellationToken ct = default)
    {
        var url = kakaoSenderKey is null
            ? KakaoSenderUrl("sync")
            : $"{KakaoSenderUrl(kakaoSenderKey)}/sync";

        return PostAsync(url, new Dictionary<string, object?>(), ct);
    }

    /// <summary>
    /// 브랜드메시지 M 신청에 필요한 광고성 정보 수신동의 증적자료 업로드.
    /// jpg/png, 5MB 이하.
    /// </summary>
    public Task<Dictionary<string, object?>> UploadBrandMessageEvidenceAsync(
        string kakaoSenderKey, MultipartFile evidence, CancellationToken ct = default) =>
        PostMultipartAsync(
            $"{KakaoSenderUrl(kakaoSenderKey)}/brand-message/evidence",
            new Dictionary<string, object?>(),
            [evidence.WithFieldName("evidence")],
            ct);

    /// <summary>
    /// 브랜드메시지 M(마케팅) / N(정보성) 사용 신청.
    /// </summary>
    /// <remarks>
    /// 결과는 즉시 확정되지 않습니다. 발신프로필의 brandMessageStatus 로 확인합니다.
    /// </remarks>
    public Task<Dictionary<string, object?>> ApplyBrandMessageTargetingAsync(
        string kakaoSenderKey, string targetType, CancellationToken ct = default) =>
        PostAsync($"{KakaoSenderUrl(kakaoSenderKey)}/brand-message/apply",
            new Dictionary<string, object?> { ["targetType"] = targetType }, ct);

    // ------------------------------------------------------------ 알림톡 템플릿

    /// <summary>알림톡 템플릿 목록 조회. null 인 조건은 적용하지 않습니다.</summary>
    public Task<Dictionary<string, object?>> GetNoticeTemplatesAsync(
        string? kakaoSenderKey = null, string? inspectionStatus = null,
        string? search = null, int? count = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (kakaoSenderKey is not null) query.Add($"kakaoSenderKey={Uri.EscapeDataString(kakaoSenderKey)}");
        if (inspectionStatus is not null) query.Add($"inspectionStatus={Uri.EscapeDataString(inspectionStatus)}");
        if (search is not null) query.Add($"search={Uri.EscapeDataString(search)}");
        if (count is not null) query.Add($"count={count}");

        var url = NoticeTemplateUrl();
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return GetAsync(url, ct);
    }

    /// <summary>알림톡 템플릿 상세 조회. data.template.policy 에 정책 검토 상태가 들어 있습니다.</summary>
    public Task<Dictionary<string, object?>> GetNoticeTemplateAsync(
        string templateCode, CancellationToken ct = default) =>
        GetAsync(NoticeTemplateUrl(templateCode), ct);

    /// <summary>
    /// 알림톡 템플릿 등록. 등록만으로는 발송할 수 없습니다 — 검수를 요청해야 합니다.
    /// </summary>
    public Task<Dictionary<string, object?>> CreateNoticeTemplateAsync(
        NoticeTemplateRequest request, CancellationToken ct = default) =>
        PostAsync(NoticeTemplateUrl(), ToPayload(request), ct);

    /// <summary>
    /// 이미지 템플릿 등록 (TemplateEmphasizeType 이 "IMAGE" 인 경우).
    /// </summary>
    /// <remarks>
    /// multipart 로 나가므로 Buttons 같은 필드는 JSON 문자열로 직렬화해 보냅니다.
    /// </remarks>
    public Task<Dictionary<string, object?>> CreateNoticeTemplateWithImageAsync(
        NoticeTemplateRequest request, MultipartFile image, CancellationToken ct = default) =>
        PostMultipartAsync(NoticeTemplateUrl(), ToPayload(request), [image.WithFieldName("image")], ct);

    /// <summary>
    /// 알림톡 템플릿 수정.
    /// </summary>
    /// <remarks>
    /// 발신프로필과 템플릿 코드는 바꿀 수 없습니다. 본문·버튼처럼 카카오에 등록된
    /// 내용이 바뀌면 검수 상태가 되돌아가므로 재검수를 요청해야 합니다.
    /// </remarks>
    public Task<Dictionary<string, object?>> UpdateNoticeTemplateAsync(
        string templateCode, NoticeTemplateRequest request, CancellationToken ct = default) =>
        SendRequestAsync(HttpMethod.Put, NoticeTemplateUrl(templateCode), ToPayload(request), ct);

    /// <summary>
    /// 알림톡 템플릿 삭제.
    /// </summary>
    /// <remarks>
    /// 카카오는 템플릿 삭제 API 를 제공하지 않습니다. sendgo 목록에서만 지워지고
    /// 비즈니스 채널 쪽 템플릿은 남습니다. 동기화하면 다시 나타납니다.
    /// </remarks>
    public Task<Dictionary<string, object?>> DeleteNoticeTemplateAsync(
        string templateCode, CancellationToken ct = default) =>
        DeleteAsync(NoticeTemplateUrl(templateCode), ct);

    /// <summary>카카오에서 검수 상태와 반려 사유를 다시 읽어 옵니다.</summary>
    public Task<Dictionary<string, object?>> SyncNoticeTemplateAsync(
        string templateCode, CancellationToken ct = default) =>
        PostAsync($"{NoticeTemplateUrl(templateCode)}/sync", new Dictionary<string, object?>(), ct);

    /// <summary>
    /// 검수 요청.
    /// </summary>
    /// <remarks>
    /// 첨부가 있으면 <paramref name="comment"/> 는 필수입니다. 정책 검토를 통과하지
    /// 못한 템플릿은 POLICY_REVIEW_REQUIRED 로 거절되고 errors.reasons 에 사유가 담깁니다.
    /// </remarks>
    public Task<Dictionary<string, object?>> RequestNoticeTemplateInspectionAsync(
        string templateCode, string? comment = null,
        IReadOnlyList<MultipartFile>? attachments = null, CancellationToken ct = default)
    {
        var url = $"{NoticeTemplateUrl(templateCode)}/inspection";

        if (attachments is null || attachments.Count == 0)
        {
            var body = new Dictionary<string, object?>();
            if (comment is not null) body["comment"] = comment;

            return PostAsync(url, body, ct);
        }

        // 서버는 attachments[0], attachments[1] 형태를 기대합니다.
        var named = attachments
            .Select((file, index) => file.WithFieldName($"attachments[{index}]"))
            .ToList();

        var fields = new Dictionary<string, object?>();
        if (comment is not null) fields["comment"] = comment;

        return PostMultipartAsync(url, fields, named, ct);
    }

    /// <summary>검수 요청 취소. 아직 심사 중(REQ)일 때만 통합니다.</summary>
    public Task<Dictionary<string, object?>> CancelNoticeTemplateInspectionAsync(
        string templateCode, CancellationToken ct = default) =>
        DeleteAsync($"{NoticeTemplateUrl(templateCode)}/inspection", ct);

    /// <summary>승인 취소. 승인(APR)된 템플릿을 되돌립니다. 이후에는 발송할 수 없습니다.</summary>
    public Task<Dictionary<string, object?>> CancelNoticeTemplateApprovalAsync(
        string templateCode, CancellationToken ct = default) =>
        DeleteAsync($"{NoticeTemplateUrl(templateCode)}/approval", ct);

    /// <summary>휴면 해제. 오래 안 쓴 템플릿이 dormant 로 잠기면 이걸로 깨웁니다.</summary>
    public Task<Dictionary<string, object?>> ReleaseNoticeTemplateAsync(
        string templateCode, CancellationToken ct = default) =>
        PostAsync($"{NoticeTemplateUrl(templateCode)}/release", new Dictionary<string, object?>(), ct);

    /// <summary>템플릿 카테고리 코드 조회.</summary>
    public Task<Dictionary<string, object?>> GetNoticeTemplateCategoriesAsync(
        string? categoryCode = null, CancellationToken ct = default)
    {
        var url = NoticeTemplateUrl("categories");
        if (categoryCode is not null) url += $"?categoryCode={Uri.EscapeDataString(categoryCode)}";

        return GetAsync(url, ct);
    }

    // ------------------------------------------------------ 브랜드메시지 템플릿

    /// <summary>브랜드메시지 템플릿 목록 조회.</summary>
    public Task<Dictionary<string, object?>> GetBrandTemplatesAsync(
        string? kakaoSenderKey = null, string? search = null, int? count = null,
        CancellationToken ct = default)
    {
        var query = new List<string>();
        if (kakaoSenderKey is not null) query.Add($"kakaoSenderKey={Uri.EscapeDataString(kakaoSenderKey)}");
        if (search is not null) query.Add($"search={Uri.EscapeDataString(search)}");
        if (count is not null) query.Add($"count={count}");

        var url = BrandTemplateUrl();
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return GetAsync(url, ct);
    }

    /// <summary>브랜드메시지 템플릿 상세 조회. sendgo 코드와 카카오 코드 둘 다 받습니다.</summary>
    public Task<Dictionary<string, object?>> GetBrandTemplateAsync(
        string templateCode, CancellationToken ct = default) =>
        GetAsync(BrandTemplateUrl(templateCode), ct);

    /// <summary>브랜드메시지 템플릿 등록. 알림톡과 달리 검수 요청 단계가 없습니다.</summary>
    public Task<Dictionary<string, object?>> CreateBrandTemplateAsync(
        BrandTemplateRequest request, CancellationToken ct = default) =>
        PostAsync(BrandTemplateUrl(), ToPayload(request), ct);

    /// <summary>브랜드메시지 템플릿 수정. 발신프로필은 바꿀 수 없습니다.</summary>
    public Task<Dictionary<string, object?>> UpdateBrandTemplateAsync(
        string templateCode, BrandTemplateRequest request, CancellationToken ct = default) =>
        SendRequestAsync(HttpMethod.Put, BrandTemplateUrl(templateCode), ToPayload(request), ct);

    /// <summary>브랜드메시지 템플릿 삭제. 알림톡과 달리 카카오 쪽에서도 실제로 삭제됩니다.</summary>
    public Task<Dictionary<string, object?>> DeleteBrandTemplateAsync(
        string templateCode, CancellationToken ct = default) =>
        DeleteAsync(BrandTemplateUrl(templateCode), ct);

    /// <summary>
    /// 브랜드메시지 템플릿 동기화. 카카오 쪽에서 이미 삭제됐으면 로컬에서도
    /// 제거하고 data.deleted: true 를 반환합니다.
    /// </summary>
    public Task<Dictionary<string, object?>> SyncBrandTemplateAsync(
        string templateCode, CancellationToken ct = default) =>
        PostAsync($"{BrandTemplateUrl(templateCode)}/sync", new Dictionary<string, object?>(), ct);

    /// <summary>발신프로필 단위 가져오기 — 카카오 쪽에 이미 있는 템플릿을 들여옵니다.</summary>
    public Task<Dictionary<string, object?>> ImportBrandTemplatesAsync(
        string kakaoSenderKey, CancellationToken ct = default) =>
        PostAsync(BrandTemplateUrl("import"),
            new Dictionary<string, object?> { ["kakaoSenderKey"] = kakaoSenderKey }, ct);

    // ---------------------------------------------------------------- 발신번호

    /// <summary>발신번호 목록 조회. 심사 상태(status)를 여기서 확인합니다.</summary>
    public Task<Dictionary<string, object?>> GetSendersAsync(CancellationToken ct = default) =>
        GetAsync(SenderUrl(), ct);

    /// <summary>발신번호 상세 조회.</summary>
    public Task<Dictionary<string, object?>> GetSenderAsync(
        string senderKey, CancellationToken ct = default) =>
        GetAsync(SenderUrl(senderKey), ct);

    /// <summary>
    /// 계정 종류에 맞는 발신번호 유형과 유형별 필수 서류.
    /// 유형별 identityVerification(none/document)과 필요한 서류 목록을 줍니다.
    /// </summary>
    public Task<Dictionary<string, object?>> GetSenderNumberTypesAsync(CancellationToken ct = default) =>
        GetAsync(SenderUrl("number-types"), ct);

    /// <summary>
    /// 발신번호 등록 전 형식·중복 확인. 응답의 duplicationReasonRequired 가 true 면
    /// 등록 시 DuplicationReason 을 함께 넣어야 합니다.
    /// </summary>
    public Task<Dictionary<string, object?>> ValidateSenderNumberAsync(
        string phoneE164, string senderNumberType, CancellationToken ct = default) =>
        PostAsync(SenderUrl("validate"), new Dictionary<string, object?>
        {
            ["phoneE164"] = phoneE164,
            ["senderNumberType"] = senderNumberType,
        }, ct);

    /// <summary>
    /// 발신번호 등록 신청. 서류가 붙으므로 multipart 로 나갑니다.
    /// </summary>
    /// <remarks>
    /// files 에는 최소한 csuCertificate(통신서비스 이용증명원)가 있어야 합니다.
    /// 휴대폰 계열은 identityDocument(신분증 사본)가, team_other_company 는
    /// 수임·위임 서류가 더 필요합니다 — GetSenderNumberTypesAsync 로 확인하세요.
    /// </remarks>
    public Task<Dictionary<string, object?>> RegisterSenderAsync(
        SenderRegistrationRequest request, IReadOnlyList<MultipartFile> files,
        CancellationToken ct = default) =>
        PostMultipartAsync(SenderUrl(), request.ToFields(), files, ct);

    /// <summary>별칭 변경 / 기본 발신 지정. 번호와 심사 상태는 바꿀 수 없습니다.</summary>
    public Task<Dictionary<string, object?>> UpdateSenderAsync(
        string senderKey, string senderAlias, string? primaryType = null,
        CancellationToken ct = default)
    {
        var body = new Dictionary<string, object?> { ["senderAlias"] = senderAlias };
        if (primaryType is not null) body["primaryType"] = primaryType;

        return SendRequestAsync(HttpMethod.Patch, SenderUrl(senderKey), body, ct);
    }

    /// <summary>발신번호 삭제. 기본 발신번호를 지우면 남은 번호 중 하나가 승계됩니다.</summary>
    public Task<Dictionary<string, object?>> DeleteSenderAsync(
        string senderKey, CancellationToken ct = default) =>
        DeleteAsync(SenderUrl(senderKey), ct);

    // ------------------------------------------------------------- 문자 템플릿

    /// <summary>문자 상용구 템플릿 목록 조회.</summary>
    public Task<Dictionary<string, object?>> GetMessageTemplatesAsync(
        string? messageType = null, string? search = null, int? count = null,
        CancellationToken ct = default)
    {
        var query = new List<string>();
        if (messageType is not null) query.Add($"messageType={Uri.EscapeDataString(messageType)}");
        if (search is not null) query.Add($"search={Uri.EscapeDataString(search)}");
        if (count is not null) query.Add($"count={count}");

        var url = MessageTemplateUrl();
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return GetAsync(url, ct);
    }

    /// <summary>문자 템플릿 상세 조회.</summary>
    public Task<Dictionary<string, object?>> GetMessageTemplateAsync(
        string templateKey, CancellationToken ct = default) =>
        GetAsync(MessageTemplateUrl(templateKey), ct);

    /// <summary>문자 템플릿 등록. LMS·MMS 는 MessageTranSubject 가 필수입니다.</summary>
    public Task<Dictionary<string, object?>> CreateMessageTemplateAsync(
        MessageTemplateRequest request, CancellationToken ct = default) =>
        PostAsync(MessageTemplateUrl(), ToPayload(request), ct);

    /// <summary>문자 템플릿 수정.</summary>
    public Task<Dictionary<string, object?>> UpdateMessageTemplateAsync(
        string templateKey, MessageTemplateRequest request, CancellationToken ct = default) =>
        SendRequestAsync(HttpMethod.Put, MessageTemplateUrl(templateKey), ToPayload(request), ct);

    /// <summary>문자 템플릿 삭제 (소프트 삭제 — 목록에서만 사라집니다).</summary>
    public Task<Dictionary<string, object?>> DeleteMessageTemplateAsync(
        string templateKey, CancellationToken ct = default) =>
        DeleteAsync(MessageTemplateUrl(templateKey), ct);

    // ------------------------------------------------------------ 카카오 이미지

    /// <summary>업로드 가능한 이미지 유형과 제약.</summary>
    public Task<Dictionary<string, object?>> GetKakaoImageTypesAsync(CancellationToken ct = default) =>
        GetAsync(ResourceUrl("kakao-images", "types"), ct);

    /// <summary>
    /// 단일 이미지 업로드. jpg/png, 2MB 이하. <c>data.imageUrl</c> 을 받습니다.
    /// </summary>
    /// <remarks>
    /// 브랜드메시지 템플릿의 <c>imageUrl</c> 은 <b>카카오가 호스팅하는 URL</b>
    /// 이어야 하고, 그 URL 을 얻는 방법이 이 업로드뿐입니다.
    /// </remarks>
    public Task<Dictionary<string, object?>> UploadKakaoImageAsync(
        string type, MultipartFile image, CancellationToken ct = default) =>
        PostMultipartAsync(
            ResourceUrl("kakao-images", type),
            new Dictionary<string, object?>(),
            [image.WithFieldName("image")],
            ct);

    /// <summary>다중 이미지 업로드. 유형별 최대 개수가 다릅니다.</summary>
    public Task<Dictionary<string, object?>> UploadKakaoImagesAsync(
        string type, IReadOnlyList<MultipartFile> images, CancellationToken ct = default)
    {
        // 서버는 images[0], images[1] 형태를 기대합니다.
        var named = images
            .Select((file, index) => file.WithFieldName($"images[{index}]"))
            .ToList();

        return PostMultipartAsync(
            ResourceUrl("kakao-images", type), new Dictionary<string, object?>(), named, ct);
    }

    // ---------------------------------------------------------------- 수신거부

    /// <summary>
    /// 수신거부(080) 번호 목록. 조회 전용입니다.
    /// </summary>
    /// <remarks>
    /// 발송 API가 알아서 제외하지만 <b>자기 DB의 수신 상태도 맞춰야</b> 합니다.
    /// <paramref name="since"/> 로 증분만 가져가세요.
    /// </remarks>
    public Task<Dictionary<string, object?>> GetRejectedNumbersAsync(
        string? since = null, string? search = null, int? count = null,
        CancellationToken ct = default)
    {
        var query = new List<string>();
        if (since is not null) query.Add($"since={Uri.EscapeDataString(since)}");
        if (search is not null) query.Add($"search={Uri.EscapeDataString(search)}");
        if (count is not null) query.Add($"count={count}");

        var url = ResourceUrl("rejected-numbers", null);
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return GetAsync(url, ct);
    }

    // ------------------------------------------------------------------- 웹훅

    /// <summary>
    /// 현재 웹훅 구독 설정. 마지막 전송 결과도 함께 옵니다 — 내 엔드포인트가
    /// 실제로 받고 있는지 확인할 수 있어야 합니다.
    /// </summary>
    public Task<Dictionary<string, object?>> GetWebhookAsync(CancellationToken ct = default) =>
        GetAsync(ResourceUrl("webhook", null), ct);

    /// <summary>웹훅 구독 생성·수정.</summary>
    public Task<Dictionary<string, object?>> SubscribeWebhookAsync(
        WebhookSubscriptionRequest request, CancellationToken ct = default) =>
        SendRequestAsync(HttpMethod.Put, ResourceUrl("webhook", null), ToPayload(request), ct);

    /// <summary>테스트 이벤트 발송. 구독 목록과 무관하게 도착합니다.</summary>
    public Task<Dictionary<string, object?>> TestWebhookAsync(CancellationToken ct = default) =>
        PostAsync(ResourceUrl("webhook", "test"), new Dictionary<string, object?>(), ct);

    /// <summary>웹훅 구독 해지.</summary>
    public Task<Dictionary<string, object?>> UnsubscribeWebhookAsync(CancellationToken ct = default) =>
        DeleteAsync(ResourceUrl("webhook", null), ct);

    /// <summary>
    /// 수신한 웹훅의 서명을 검증합니다.
    /// </summary>
    /// <remarks>
    /// <paramref name="rawBody"/> 는 <b>받은 바이트 그대로</b>여야 합니다.
    /// 파싱한 뒤 다시 인코딩한 값으로 계산하면 키 순서나 이스케이프 차이로
    /// 검증이 깨집니다. ASP.NET Core 라면
    /// <c>Request.EnableBuffering()</c> 후 본문을 직접 읽으세요.
    /// </remarks>
    public static bool VerifyWebhookSignature(ReadOnlySpan<byte> rawBody, string? signature, string secret)
    {
        if (signature is null) return false;

        Span<byte> computed = stackalloc byte[32];
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), rawBody, computed);

        var expected = Convert.ToHexString(computed).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }

    // ------------------------------------------------------------------- 내부

    /// <summary>
    /// multipart/form-data POST — 서류·이미지 첨부가 있는 관리 API 전용.
    /// </summary>
    /// <remarks>
    /// multipart 에는 배열도 불리언도 없으므로, 컬렉션·객체 값은 JSON 문자열로
    /// 눌러 보냅니다 — 서버가 그렇게 받아 읽습니다.
    /// </remarks>
    private async Task<Dictionary<string, object?>> PostMultipartAsync(
        string url, Dictionary<string, object?> fields, IReadOnlyList<MultipartFile> files,
        CancellationToken ct, bool isRetry = false)
    {
        var token = await _tokenManager.GetTokenAsync(ct);

        using var content = new MultipartFormDataContent();

        foreach (var (key, value) in fields)
        {
            if (value is null) continue;

            var encoded = value switch
            {
                bool flag => flag ? "1" : "0",
                string text => text,
                JsonElement { ValueKind: JsonValueKind.String } element => element.GetString() ?? string.Empty,
                JsonElement { ValueKind: JsonValueKind.True } => "1",
                JsonElement { ValueKind: JsonValueKind.False } => "0",
                JsonElement element => element.GetRawText(),
                _ => JsonSerializer.Serialize(value, _jsonOptions),
            };

            content.Add(new StringContent(encoded, Encoding.UTF8), key);
        }

        foreach (var file in files)
        {
            content.Add(file.ToContent(), file.FieldName, file.FileName);
        }

        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", MakeBearerToken(token));
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // 파일 업로드는 JSON 요청보다 오래 걸린다. 기본 15초 타임아웃으로는
        // 몇 MB 짜리 서류가 자주 끊긴다.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));

        var resp = await _http.SendAsync(req, timeout.Token);
        var bodyText = await resp.Content.ReadAsStringAsync(timeout.Token);
        var responseBody = JsonSerializer.Deserialize<Dictionary<string, object?>>(bodyText) ?? new();

        if (!resp.IsSuccessStatusCode)
        {
            var errorCode = SendgoException.ReadString(responseBody, "code");
            var endpoint = url.Split('/').Last();

            if (!isRetry && _tokenManager.ShouldRefresh((int)resp.StatusCode, errorCode))
            {
                _tokenManager.Invalidate();
                return await PostMultipartAsync(url, fields, files, ct, isRetry: true);
            }

            throw SendgoException.FromResponse((int)resp.StatusCode,
                responseBody.ToDictionary(k => k.Key, v => v.Value), endpoint, _options.ApiVersion);
        }

        return responseBody;
    }

    private string KakaoSenderUrl(string? segment = null) => ResourceUrl("kakao-senders", segment);
    private string NoticeTemplateUrl(string? segment = null) => ResourceUrl("notice-templates", segment);
    private string BrandTemplateUrl(string? segment = null) => ResourceUrl("brand-templates", segment);
    private string SenderUrl(string? segment = null) => ResourceUrl("senders", segment);
    private string MessageTemplateUrl(string? segment = null) => ResourceUrl("message-templates", segment);

    private string ResourceUrl(string resource, string? segment)
    {
        var baseUrl = $"{_options.BaseUrl}/api/{_options.ApiVersion}/{resource}";

        return segment is null ? baseUrl : $"{baseUrl}/{Uri.EscapeDataString(segment)}";
    }

    /// <summary>
    /// 요청 레코드를 페이로드 사전으로 바꿉니다.
    /// </summary>
    /// <remarks>
    /// JsonSerializer 를 한 번 거치면 JsonPropertyName 과 null 무시 규칙을 손으로
    /// 다시 적지 않아도 됩니다 — JSON 경로와 multipart 경로가 같은 필드명을 씁니다.
    /// </remarks>
    private static Dictionary<string, object?> ToPayload<T>(T request)
    {
        var element = JsonSerializer.SerializeToElement(request, _jsonOptions);

        return element.Deserialize<Dictionary<string, object?>>(_jsonOptions) ?? [];
    }
}
