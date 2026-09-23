using Sendgo;
using Sendgo.Models;
using Sendgo.Exceptions;
using var c = new SendgoClient(new SendgoOptions { AccessKey = "test-access", SecretKey = "test-secret", ApiVersion = "v2", BaseUrl = Environment.GetEnvironmentVariable("SENDGO_TEST_URL")! });
var f = "11111111-1111-4111-8111-111111111111";
var key = "채널 /?";
await c.GetTemplateFoldersAsync();
await c.GetTemplateFoldersAsync("brand", key);
await c.CreateTemplateFolderAsync("주문");
await c.CreateTemplateFolderAsync("하위", f);
foreach (var kind in new[] { "notice", "brand" }) {
    await c.AssignTemplateFolderAsync(kind, key, new[] { "코드 1", "code/2" }, f);
    await c.AssignTemplateFolderAsync(kind, key, new[] { "코드 1" }, null);
}
await c.GetNoticeTemplatesByFolderAsync("none");
await c.GetBrandTemplatesByFolderAsync(f);
await c.CreateNoticeTemplateAsync(new NoticeTemplateRequest { TemplateName="테스트", FolderUuid=f, TemplateContent="본문", CategoryCode="001001", MessagePurpose="order_delivery", LegalBasis="transaction", BenefitOrigin="none", ExpiryType="none" });
await c.CreateBrandTemplateAsync(new BrandTemplateRequest { TemplateName="테스트", FolderUuid=f });
foreach (var (kind, status, code) in new[] { ("forbidden",403,"ACCESS_KEY_NOT_APPROVED"),("invalid",422,"VALIDATION_FAILED"),("missing",404,"TEMPLATE_FOLDER_NOT_FOUND") }) {
    try { await c.GetTemplateFoldersAsync(kind); throw new Exception("오류가 발생하지 않음"); }
    catch (SendgoException e) { if (e.StatusCode!=status || e.ErrorCode!=code) throw; }
}
