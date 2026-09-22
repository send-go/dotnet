using Sendgo;
using Sendgo.Exceptions;
var url = Environment.GetEnvironmentVariable("SENDGO_TEST_URL") + "/";
try { using var invalid = new AccountClient(""); throw new Exception("빈 토큰 허용"); } catch (ArgumentException) {}
using var c = new AccountClient("test-agent", url);
void Check(Dictionary<string, object?> data) { if (data["message"]?.ToString() != "Success") throw new Exception("응답 오류"); }
Check(await c.MeAsync());
Check(await c.OrganizationsAsync());
Check(await c.SelectOrganizationAsync(null));
Check(await c.SelectOrganizationAsync("team-id"));
Check(await c.ApiKeysAsync());
Check(await c.CreateApiKeyAsync(new Dictionary<string, object?> { ["name"] = "한글 이름", ["ipAddresses"] = new object[] {new Dictionary<string, object?> { ["ip"] = "192.0.2.1", ["description"] = "서버"}}}));
Check(await c.ApiKeyAsync("key/id ?"));
Check(await c.UpdateApiKeyAsync("key/id ?", "새 이름"));
Check(await c.DeleteApiKeyAsync("key/id ?"));
Check(await c.IssueTokenAsync("key/id ?"));
Check(await c.AllowedIpsAsync("key/id ?"));
Check(await c.AddAllowedIpAsync("key/id ?", new Dictionary<string, object?> { ["ip"] = "192.0.2.1", ["description"] = "서버"}));
Check(await c.DeleteAllowedIpAsync("key/id ?", "ip/id ?"));
foreach (var (token, status, code) in new[] { ("expired", 401, "AGENT_TOKEN_EXPIRED"), ("forbidden", 403, "AGENT_ABILITY_MISSING") }) {
 using var invalid = new AccountClient(token, url);
 try { await invalid.MeAsync(); throw new Exception("오류가 발생하지 않음"); }
 catch (SendgoException e) { if (e.StatusCode != status || e.ErrorCode != code) throw; }
}

// JsonElement 오류 코드를 읽고 갱신 불가 오류를 그대로 반환해야 합니다.
using var sending = new SendgoClient(new SendgoOptions { AccessKey = "test-access", SecretKey = "test-secret", ApiVersion = "v2", BaseUrl = url.TrimEnd('/') });
try { await sending.GetShortUrlsAsync(); throw new Exception("오류가 발생하지 않음"); }
catch (SendgoException e) { if (e.StatusCode != 403 || e.ErrorCode != "IP_NOT_ALLOWED") throw; }
