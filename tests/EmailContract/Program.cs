using Sendgo;
using Sendgo.Exceptions;
using System.Text.Json;
var url=Environment.GetEnvironmentVariable("SENDGO_TEST_URL") ?? throw new Exception();
using var client=new SendgoClient(new SendgoOptions{AccessKey="ak",SecretKey="sk",ApiVersion="v2",BaseUrl=url});
var email=client.Email;using var basic=EmailService.WithCredentials("credential","password",url);
var body=JsonSerializer.Deserialize<Dictionary<string,object?>>("""{"subject":"한글","enabled":false,"optional":null,"idempotency_key":"fixed-key","attachments":[{"name":"a.txt","type":"text/plain","content":"aGk="}]}""")!;
var query=new Dictionary<string,string>{{"search","한글 +&"},{"page","2"}};
void Check(object? v,string n,string verb){
 if(n=="rawMessage"){if(!((byte[])v!).SequenceEqual(new byte[]{69,77,76,13,10,0,255}))throw new Exception();return;}
 if(verb=="DELETE"){if(v is not null)throw new Exception();return;}
 var j=(JsonElement)v!;if(n=="domains")j=j[0];if(j.GetProperty("marker").GetString()!="한글")throw new Exception();
}
Check(await email.AccountAsync(query),"account","GET");
Check(await email.RequestAccessAsync(body),"requestAccess","POST");
Check(await email.CreateCredentialAsync(body),"createCredential","POST");
Check(await email.CredentialsAsync(query),"credentials","GET");
Check(await email.RevokeCredentialAsync("id 한글+"),"revokeCredential","DELETE");
Check(await email.DomainsAsync(query),"domains","GET");
Check(await email.RegisterDomainAsync(body),"registerDomain","POST");
Check(await email.VerifyDomainAsync("id 한글+", body),"verifyDomain","POST");
Check(await email.SendersAsync(query),"senders","GET");
Check(await email.RequestSenderAsync(body),"requestSender","POST");
Check(await email.VerifySenderAsync("id 한글+", body),"verifySender","POST");
Check(await email.RequestRecipientVerificationAsync(body),"requestRecipientVerification","POST");
Check(await email.CheckRecipientsAsync(body),"checkRecipients","POST");
Check(await email.AddressBookAsync(query),"addressBook","GET");
Check(await email.SenderProfilesAsync(query),"senderProfiles","GET");
Check(await email.CreateSenderProfileAsync(body),"createSenderProfile","POST");
Check(await email.UpdateSenderProfileAsync("id 한글+", body),"updateSenderProfile","PATCH");
Check(await email.DeleteSenderProfileAsync("id 한글+"),"deleteSenderProfile","DELETE");
Check(await email.ImportAddressBookAsync(body),"importAddressBook","POST");
Check(await email.UpdateAddressBookPreferencesAsync(body),"updateAddressBookPreferences","POST");
Check(await email.SendAsync(body),"send","POST");
Check(await email.QuoteAsync(body),"quote","POST");
Check(await email.MessagesAsync(query),"messages","GET");
Check(await email.MessageAsync("id 한글+", query),"message","GET");
Check(await email.CancelMessageAsync("id 한글+", body),"cancelMessage","POST");
Check(await email.InboxesAsync(query),"inboxes","GET");
Check(await email.CreateInboxAsync(body),"createInbox","POST");
Check(await email.UpdateInboxAsync("id 한글+", body),"updateInbox","PATCH");
Check(await email.InboxMessagesAsync("id 한글+", query),"inboxMessages","GET");
Check(await email.InboxMessageAsync("id 한글+", "id 한글+", query),"inboxMessage","GET");
Check(await email.RawMessageAsync("id 한글+", "id 한글+", query),"rawMessage","GET");
Check(await email.DeleteInboxMessageAsync("id 한글+", "id 한글+"),"deleteInboxMessage","DELETE");
Check(await email.TemplatesAsync(query),"templates","GET");
Check(await email.TemplateAsync("id 한글+", query),"template","GET");
Check(await email.CreateTemplateAsync(body),"createTemplate","POST");
Check(await email.UpdateTemplateAsync("id 한글+", body),"updateTemplate","PATCH");
Check(await email.DeleteTemplateAsync("id 한글+"),"deleteTemplate","DELETE");
Check(await email.ContactsAsync(query),"contacts","GET");
Check(await email.SaveContactAsync(body),"saveContact","POST");
Check(await email.ImportContactsAsync(body),"importContacts","POST");
Check(await email.UnsubscribeContactAsync("id 한글+", body),"unsubscribeContact","POST");
Check(await email.CampaignsAsync(query),"campaigns","GET");
Check(await email.CreateCampaignAsync(body),"createCampaign","POST");
Check(await email.CampaignAsync("id 한글+", query),"campaign","GET");
Check(await email.QuoteCampaignAsync("id 한글+", body),"quoteCampaign","POST");
Check(await email.SendCampaignAsync("id 한글+", body),"sendCampaign","POST");
Check(await email.CancelCampaignAsync("id 한글+", body),"cancelCampaign","POST");
Check(await basic.DomainsAsync(query),"domains","GET");
Check(await basic.RegisterDomainAsync(body),"registerDomain","POST");
Check(await basic.VerifyDomainAsync("id 한글+", body),"verifyDomain","POST");
Check(await basic.SendAsync(body),"send","POST");
Check(await basic.QuoteAsync(body),"quote","POST");
Check(await basic.MessagesAsync(query),"messages","GET");
Check(await basic.MessageAsync("id 한글+", query),"message","GET");
Check(await basic.CancelMessageAsync("id 한글+", body),"cancelMessage","POST");
Check(await basic.AuthAsync(query),"auth","GET");
Check(await email.MessagesAsync(new(){{"mode","refresh"}}),"messages","GET");
try {await email.MessagesAsync(new(){{"mode","403"}});throw new Exception();}catch(SendgoException e){if(e.StatusCode!=403)throw;}
try {await email.MessagesAsync(new(){{"mode","422"}});throw new Exception();}catch(SendgoException e){if(e.StatusCode!=422)throw;}
try {await email.MessagesAsync(new(){{"mode","429"}});throw new Exception();}catch(SendgoException e){if(e.StatusCode!=429)throw;}
try {await email.MessagesAsync(new(){{"mode","500"}});throw new Exception();}catch(SendgoException e){if(e.StatusCode!=500)throw;}
try {await basic.MessagesAsync(new(){{"mode","401"}});throw new Exception();}catch(SendgoException e){if(e.StatusCode!=401)throw;}
using var other=new SendgoClient(new SendgoOptions{AccessKey="ak",SecretKey="sk",ApiVersion="v2",BaseUrl=url});
await other.SendBrandMessageAsync(new Sendgo.Models.BrandMessageRequest{FriendTemplateUuid="template",Targeting="O",Contacts=new[]{new Sendgo.Models.Contact{PhoneNumber="01000000000"}}});
Console.WriteLine("PASS email");
