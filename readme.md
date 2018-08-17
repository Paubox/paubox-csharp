![Paubox]([[https://github.com/Paubox/paubox-csharp/octocat.png|alt=octocat]])
#Paubox C Sharp

This is the official C# wrapper for the Paubox Transactional Email HTTP API. 

The Paubox Transactional Email API allows your application to send secure, HIPAA-compliant email via Paubox and track deliveries and opens.

The API wrapper allows you to construct and send messages.

## Installation
Add the provided class library (Paubox.Email.API.dll and Newtonsoft.Json.dll) in your C#
project by using ‘Add Reference’ option within Project – References node.

### Getting Paubox API Credentials
You will need to have a Paubox account. Please contact [Paubox Customer Success](https://paubox.zendesk.com/hc/en-us) for details on gaining access to the Transactional Email API alpha testing program.

### Configuring API Credentials
Include your API credentials in a config file.

Add two app settings keys with their values in App.Config (For Desktop App,
Windows Service) or Web.Config (For ASP.NET projects):

```csharp
<add key="APIKey" value="Your-API-Key-Here"/>
<add key="APIUser" value="Your-Username-Here"/>
```

### Adding the .NET Framework Configuration
This library supports .NET v4.6.1. Add the following to your config file:

```
<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1"/></startup>
```

## Usage
### Sending Messages using the Paubox C# Library

To send email, prepare a Message object and call EmailLibrary.SendMessage method:

```csharp
static SendMessageResponse SendMessage()
{
 Message message = new Message();
 Content content = new Content();
 Header header = new Header();
 message.Recipients = new string[] { "someone@domain.com",
 "someoneelse@domain.com" };
 header.From = "you@yourdomain.com";
 message.Bcc = new string[] { "bcc-recipient@domain.com" };
 header.Subject = "Testing!";
 header.ReplyTo = "reply-to@yourdomain.com";
 content.PlainText = "Hello World!";
 message.Header = header;
 message.Content = content;
 SendMessageResponse response = EmailLibrary.SendMessage(message);
 return response;
}
```
###Allowing non-TLS message delivery
If you want to send non-PHI mail that does not need to be HIPAA-compliant, you can
allow the message delivery to take place even if a TLS connection is unavailable. This
means a message will not be converted into a secure portal message when a unencrypted
connection is encountered. For this, just set message.AllowNonTLS to true, as
shown below:

```
static SendMessageResponse SendNonTLSMessage()
{
 Message message = new Message();
 Content content = new Content();
 Header header = new Header();
 message.Recipients = new string[] { "someone@domain.com",
 "someoneelse@domain.com" };
 header.From = "you@yourdomain.com";
 message.Bcc = new string[] { "bcc-recipient@domain.com" };
 header.Subject = "Testing!";
 header.ReplyTo = "reply-to@yourdomain.com";
 content.PlainText = "Hello World!";
 message.AllowNonTLS = true;
 message.Header = header;
 message.Content = content;
 SendMessageResponse response = EmailLibrary.SendMessage(message);
 return response;
}
```

### Checking Email Dispositions
To get email status for any source tracking id, call the
EmailLibrary.GetEmailDisposition method:

```
static void GetEmailDisposition()
{
 GetEmailDispositionResponse response =
 EmailLibrary.GetEmailDisposition("2a3c048485aa4cf6");
}
```