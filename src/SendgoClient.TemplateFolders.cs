namespace Sendgo;

public partial class SendgoClient
{
    /// <summary>기업 계정의 템플릿 공용 폴더 조회. v2 전용.</summary>
    public Task<Dictionary<string, object?>> GetTemplateFoldersAsync(
        string? templateType = null, string? kakaoSenderKey = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (templateType is not null) query.Add($"templateType={Uri.EscapeDataString(templateType)}");
        if (kakaoSenderKey is not null) query.Add($"kakaoSenderKey={Uri.EscapeDataString(kakaoSenderKey)}");
        var url = ResourceUrl("template-folders", null);
        if (query.Count > 0) url += "?" + string.Join("&", query);
        return GetAsync(url, ct);
    }

    /// <summary>루트 또는 하위 폴더 생성. parentUuid가 null이면 루트.</summary>
    public Task<Dictionary<string, object?>> CreateTemplateFolderAsync(
        string name, string? parentUuid = null, CancellationToken ct = default) =>
        PostAsync(ResourceUrl("template-folders", null),
            new Dictionary<string, object?> { ["name"] = name, ["parentUuid"] = parentUuid }, ct);

    /// <summary>1~100개 템플릿 이동. folderUuid가 null이면 미분류로 이동.</summary>
    public Task<Dictionary<string, object?>> AssignTemplateFolderAsync(
        string templateType, string kakaoSenderKey, IReadOnlyList<string> templateCodes,
        string? folderUuid, CancellationToken ct = default) =>
        SendRequestAsync(HttpMethod.Patch, ResourceUrl("template-folders", "templates"),
            new Dictionary<string, object?> {
                ["templateType"] = templateType, ["kakaoSenderKey"] = kakaoSenderKey,
                ["templateCodes"] = templateCodes, ["folderUuid"] = folderUuid
            }, ct);
}
