using System.Net.Http.Headers;

namespace Sendgo.Models;

/// <summary>
/// multipart 업로드에 붙일 파일.
/// </summary>
/// <remarks>
/// 서류·이미지 첨부가 있는 관리 API(발신번호 등록, 이미지 템플릿, 검수 첨부)는
/// JSON 으로 보낼 수 없습니다. 서버 검증이 확장자를 보므로 <see cref="FileName"/> 은
/// 반드시 실제 확장자를 포함해야 합니다.
/// </remarks>
/// <example>
/// var csu = MultipartFile.FromPath("csuCertificate", "csu.pdf", "application/pdf");
/// </example>
public sealed record MultipartFile
{
    /// <summary>폼 필드 이름 (예: "csuCertificate").</summary>
    public required string FieldName { get; init; }

    /// <summary>서버에 알릴 파일명 (예: "csu.pdf").</summary>
    public required string FileName { get; init; }

    /// <summary>MIME 타입. 비우면 application/octet-stream.</summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>파일 내용.</summary>
    public required byte[] Content { get; init; }

    /// <summary>파일 경로에서 읽어 만듭니다.</summary>
    public static MultipartFile FromPath(string fieldName, string path, string? contentType = null) => new()
    {
        FieldName = fieldName,
        FileName = Path.GetFileName(path),
        ContentType = contentType ?? "application/octet-stream",
        Content = File.ReadAllBytes(path),
    };

    /// <summary>필드 이름만 바꾼 복사본. 서비스가 필드명을 정할 때 씁니다.</summary>
    public MultipartFile WithFieldName(string fieldName) => this with { FieldName = fieldName };

    internal ByteArrayContent ToContent()
    {
        var content = new ByteArrayContent(Content);
        content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
        return content;
    }
}
