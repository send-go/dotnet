using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Sendgo.Services;
using Sendgo.Exceptions;
namespace Sendgo;

/// <summary>서버 전용 이메일 API. JSON 응답은 JsonElement, 204는 null, EML은 byte[]입니다.</summary>
public sealed class EmailService : IDisposable
{
    private readonly TokenManager? _tokens;
    private readonly string _baseUrl, _version;
    private readonly string? _credential;
    private readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(60) };
    internal EmailService(TokenManager? tokens, string baseUrl, string version, string? credential = null)
    { _tokens=tokens; _baseUrl=baseUrl.TrimEnd('/'); _version=version; _credential=credential; }
    /// <summary>앱 키가 아닌 이메일 전용 ID/password로 생성합니다.</summary>
    public static EmailService WithCredentials(string id, string password, string baseUrl = "https://sendgo.io") =>
        new(null,baseUrl,"v2",Convert.ToBase64String(Encoding.UTF8.GetBytes(id+":"+password)));
    public void Dispose() => _http.Dispose();
    private async Task<object?> RequestAsync(string method,string path,Dictionary<string,object?>? body,Dictionary<string,string>? query,bool raw,CancellationToken ct,bool retry=false)
    {
        if (_version!="v2") throw new InvalidOperationException("이메일 API는 ApiVersion=v2가 필요합니다.");
        var prefix=_credential is null ? "email" : "email-service";
        var url=_baseUrl+"/api/v2/"+prefix+"/"+path;
        if(query is {Count: >0}) url+="?"+string.Join("&",query.Select(p=>Uri.EscapeDataString(p.Key)+"="+Uri.EscapeDataString(p.Value)));
        using var req=new HttpRequestMessage(new HttpMethod(method),url);
        req.Headers.Authorization=_credential is null ? new AuthenticationHeaderValue("Bearer",await _tokens!.GetTokenAsync(ct)) : new AuthenticationHeaderValue("Basic",_credential);
        req.Headers.Accept.ParseAdd("application/json");
        if(body is not null) req.Content=new StringContent(JsonSerializer.Serialize(body),Encoding.UTF8,"application/json");
        using var resp=await _http.SendAsync(req,ct);
        var bytes=await resp.Content.ReadAsByteArrayAsync(ct);
        if(resp.IsSuccessStatusCode && raw) return bytes;
        JsonElement? result=null;
        if(bytes.Length>0) {try {result=JsonSerializer.Deserialize<JsonElement>(bytes);} catch(JsonException) {if(resp.IsSuccessStatusCode) throw;} }
        if(!resp.IsSuccessStatusCode) {
            var fields=result is {ValueKind:JsonValueKind.Object} ? JsonSerializer.Deserialize<Dictionary<string,object?>>(result.Value.GetRawText())! : new Dictionary<string,object?>();
            var code=SendgoException.ReadString(fields,"code");
            if(!retry && _credential is null && (int)resp.StatusCode==401 && _tokens!.ShouldRefresh(401,code)) {
                _tokens.Invalidate();return await RequestAsync(method,path,body,query,raw,ct,true);
            }
            throw SendgoException.FromResponse((int)resp.StatusCode,fields,path,"v2");
        }
        return result;
    }
    /// <summary>GET /email/account</summary>
    public Task<object?> AccountAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"account",null,query,false,ct);
    /// <summary>POST /email/request</summary>
    public Task<object?> RequestAccessAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"request",body ?? new(),null,false,ct);
    /// <summary>POST /email/credentials</summary>
    public Task<object?> CreateCredentialAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"credentials",body ?? new(),null,false,ct);
    /// <summary>GET /email/credentials</summary>
    public Task<object?> CredentialsAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"credentials",null,query,false,ct);
    /// <summary>DELETE /email/credentials/{id}</summary>
    public Task<object?> RevokeCredentialAsync(string id, CancellationToken ct = default) =>
        RequestAsync("DELETE",$"credentials/{Uri.EscapeDataString(id)}",null,null,false,ct);
    /// <summary>GET /email/domains</summary>
    public Task<object?> DomainsAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"domains",null,query,false,ct);
    /// <summary>POST /email/domains</summary>
    public Task<object?> RegisterDomainAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"domains",body ?? new(),null,false,ct);
    /// <summary>POST /email/domains/{id}/verify</summary>
    public Task<object?> VerifyDomainAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"domains/{Uri.EscapeDataString(id)}/verify",body ?? new(),null,false,ct);
    /// <summary>GET /email/senders</summary>
    public Task<object?> SendersAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"senders",null,query,false,ct);
    /// <summary>POST /email/senders</summary>
    public Task<object?> RequestSenderAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"senders",body ?? new(),null,false,ct);
    /// <summary>POST /email/senders/{id}/verify</summary>
    public Task<object?> VerifySenderAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"senders/{Uri.EscapeDataString(id)}/verify",body ?? new(),null,false,ct);
    /// <summary>POST /email/recipients/verification</summary>
    public Task<object?> RequestRecipientVerificationAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"recipients/verification",body ?? new(),null,false,ct);
    /// <summary>POST /email/recipients/check</summary>
    public Task<object?> CheckRecipientsAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"recipients/check",body ?? new(),null,false,ct);
    /// <summary>GET /email/address-book</summary>
    public Task<object?> AddressBookAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"address-book",null,query,false,ct);
    /// <summary>GET /email/sender-profiles</summary>
    public Task<object?> SenderProfilesAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"sender-profiles",null,query,false,ct);
    /// <summary>POST /email/sender-profiles</summary>
    public Task<object?> CreateSenderProfileAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"sender-profiles",body ?? new(),null,false,ct);
    /// <summary>PATCH /email/sender-profiles/{id}</summary>
    public Task<object?> UpdateSenderProfileAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("PATCH",$"sender-profiles/{Uri.EscapeDataString(id)}",body ?? new(),null,false,ct);
    /// <summary>DELETE /email/sender-profiles/{id}</summary>
    public Task<object?> DeleteSenderProfileAsync(string id, CancellationToken ct = default) =>
        RequestAsync("DELETE",$"sender-profiles/{Uri.EscapeDataString(id)}",null,null,false,ct);
    /// <summary>POST /email/address-book/import</summary>
    public Task<object?> ImportAddressBookAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"address-book/import",body ?? new(),null,false,ct);
    /// <summary>POST /email/address-book/preferences</summary>
    public Task<object?> UpdateAddressBookPreferencesAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"address-book/preferences",body ?? new(),null,false,ct);
    /// <summary>POST /email/send</summary>
    public Task<object?> SendAsync(Dictionary<string,object?> body, CancellationToken ct = default) =>
        RequestAsync("POST",$"send",body ?? new(),null,false,ct);
    /// <summary>POST /email/quote</summary>
    public Task<object?> QuoteAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"quote",body ?? new(),null,false,ct);
    /// <summary>GET /email/messages</summary>
    public Task<object?> MessagesAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"messages",null,query,false,ct);
    /// <summary>GET /email/messages/{id}</summary>
    public Task<object?> MessageAsync(string id, Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"messages/{Uri.EscapeDataString(id)}",null,query,false,ct);
    /// <summary>POST /email/messages/{id}/cancel</summary>
    public Task<object?> CancelMessageAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"messages/{Uri.EscapeDataString(id)}/cancel",body ?? new(),null,false,ct);
    /// <summary>GET /email/inboxes</summary>
    public Task<object?> InboxesAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"inboxes",null,query,false,ct);
    /// <summary>POST /email/inboxes</summary>
    public Task<object?> CreateInboxAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"inboxes",body ?? new(),null,false,ct);
    /// <summary>PATCH /email/inboxes/{id}</summary>
    public Task<object?> UpdateInboxAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("PATCH",$"inboxes/{Uri.EscapeDataString(id)}",body ?? new(),null,false,ct);
    /// <summary>GET /email/inboxes/{id}/messages</summary>
    public Task<object?> InboxMessagesAsync(string id, Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"inboxes/{Uri.EscapeDataString(id)}/messages",null,query,false,ct);
    /// <summary>GET /email/inboxes/{id}/messages/{messageId}</summary>
    public Task<object?> InboxMessageAsync(string id, string messageId, Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"inboxes/{Uri.EscapeDataString(id)}/messages/{Uri.EscapeDataString(messageId)}",null,query,false,ct);
    /// <summary>GET /email/inboxes/{id}/messages/{messageId}/raw</summary>
    public Task<object?> RawMessageAsync(string id, string messageId, Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"inboxes/{Uri.EscapeDataString(id)}/messages/{Uri.EscapeDataString(messageId)}/raw",null,query,true,ct);
    /// <summary>DELETE /email/inboxes/{id}/messages/{messageId}</summary>
    public Task<object?> DeleteInboxMessageAsync(string id, string messageId, CancellationToken ct = default) =>
        RequestAsync("DELETE",$"inboxes/{Uri.EscapeDataString(id)}/messages/{Uri.EscapeDataString(messageId)}",null,null,false,ct);
    /// <summary>GET /email/templates</summary>
    public Task<object?> TemplatesAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"templates",null,query,false,ct);
    /// <summary>GET /email/templates/{id}</summary>
    public Task<object?> TemplateAsync(string id, Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"templates/{Uri.EscapeDataString(id)}",null,query,false,ct);
    /// <summary>POST /email/templates</summary>
    public Task<object?> CreateTemplateAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"templates",body ?? new(),null,false,ct);
    /// <summary>PATCH /email/templates/{id}</summary>
    public Task<object?> UpdateTemplateAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("PATCH",$"templates/{Uri.EscapeDataString(id)}",body ?? new(),null,false,ct);
    /// <summary>DELETE /email/templates/{id}</summary>
    public Task<object?> DeleteTemplateAsync(string id, CancellationToken ct = default) =>
        RequestAsync("DELETE",$"templates/{Uri.EscapeDataString(id)}",null,null,false,ct);
    /// <summary>GET /email/contacts</summary>
    public Task<object?> ContactsAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"contacts",null,query,false,ct);
    /// <summary>POST /email/contacts</summary>
    public Task<object?> SaveContactAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"contacts",body ?? new(),null,false,ct);
    /// <summary>POST /email/contacts/import</summary>
    public Task<object?> ImportContactsAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"contacts/import",body ?? new(),null,false,ct);
    /// <summary>POST /email/contacts/{id}/unsubscribe</summary>
    public Task<object?> UnsubscribeContactAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"contacts/{Uri.EscapeDataString(id)}/unsubscribe",body ?? new(),null,false,ct);
    /// <summary>GET /email/campaigns</summary>
    public Task<object?> CampaignsAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"campaigns",null,query,false,ct);
    /// <summary>POST /email/campaigns</summary>
    public Task<object?> CreateCampaignAsync(Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"campaigns",body ?? new(),null,false,ct);
    /// <summary>GET /email/campaigns/{id}</summary>
    public Task<object?> CampaignAsync(string id, Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"campaigns/{Uri.EscapeDataString(id)}",null,query,false,ct);
    /// <summary>POST /email/campaigns/{id}/quote</summary>
    public Task<object?> QuoteCampaignAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"campaigns/{Uri.EscapeDataString(id)}/quote",body ?? new(),null,false,ct);
    /// <summary>POST /email/campaigns/{id}/send</summary>
    public Task<object?> SendCampaignAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"campaigns/{Uri.EscapeDataString(id)}/send",body ?? new(),null,false,ct);
    /// <summary>POST /email/campaigns/{id}/cancel</summary>
    public Task<object?> CancelCampaignAsync(string id, Dictionary<string,object?>? body = null, CancellationToken ct = default) =>
        RequestAsync("POST",$"campaigns/{Uri.EscapeDataString(id)}/cancel",body ?? new(),null,false,ct);
    /// <summary>GET /email/auth</summary>
    public Task<object?> AuthAsync(Dictionary<string,string>? query = null, CancellationToken ct = default) =>
        RequestAsync("GET",$"auth",null,query,false,ct);
}
