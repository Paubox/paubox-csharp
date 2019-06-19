<img src="https://github.com/Paubox/paubox-csharp/raw/master/paubox_logo.png" alt="Paubox" width="150px">

# Paubox C#
This is the official C# wrapper for the Paubox Transactional Email HTTP API. It is currently in alpha development.

The Paubox Transactional Email API allows your application to send secure, HIPAA-compliant email via Paubox and track deliveries and opens.

The API wrapper allows you to construct and send messages.

# Table of Contents
* [Installation](#installation)
*  [Usage](#usage)
*  [Contributing](#contributing)
*  [License](#license)


<a name="#installation"></a>
## Installation
Add the provided class library (Paubox.Email.API.dll and Newtonsoft.Json.dll) in your C#
project by using ‘Add Reference’ option within the Project – References node.

### Getting Paubox API Credentials
You will need to have a Paubox account. You can [sign up here](https://www.paubox.com/join/see-pricing?unit=messages).

Once you have an account, follow the instructions on the Rest API dashboard to verify domain ownership and generate API credentials.

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
<a name="#usage"></a>
## Usage
### Sending Messages using the Paubox C# Library

To send an email, prepare a Message object and call EmailLibrary.SendMessage method:

```csharp
static SendMessageResponse SendMessage()
{
 Message message = new Message();
 Content content = new Content();
 Header header = new Header();
 message.Recipients = new string[] { "someone@domain.com",
 "someoneelse@domain.com" };
 header.From = "you@yourdomain.com";
 message.Cc = new string[] { "cc-recipient@domain.com" };
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

### Allowing non-TLS message delivery
If you want to send non-PHI mail that does not need to be HIPAA-compliant, you can
allow the message delivery to take place even if a TLS connection is unavailable. This
means a message will not be converted into a Secure Notification message when a unencrypted
connection is encountered. For this, just set message.AllowNonTLS to true, as
shown below:

```csharp
static SendMessageResponse SendNonTLSMessage()
{
 Message message = new Message();
 Content content = new Content();
 Header header = new Header();
 message.Recipients = new string[] { "someone@domain.com",
 "someoneelse@domain.com" };
 header.From = "you@yourdomain.com";
 message.Cc = new string[] { "cc-recipient@domain.com" };
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

### Forcing Secure Notifications
Paubox Secure Notifications allow an extra layer of security, especially when coupled with an organization's requirement for message recipients to use 2-factor authentication to read messages (this setting is available to org administrators in the Paubox Admin Panel).

Instead of receiving an email with the message contents, the recipient will receive a notification email that they have a new message in Paubox.


```csharp
static SendMessageResponse SendMessage()
{
 Message message = new Message();
 Content content = new Content();
 Header header = new Header();
 message.Recipients = new string[] { "someone@domain.com",
 "someoneelse@domain.com" };
 header.From = "you@yourdomain.com";
 message.Cc = new string[] { "cc-recipient@domain.com" };
 message.Bcc = new string[] { "bcc-recipient@domain.com" };
 header.Subject = "Testing!";
 header.ReplyTo = "reply-to@yourdomain.com";
 content.PlainText = "Hello World!";
 message.ForceSecureNotification = "true";
 message.Header = header;
 message.Content = content;
 SendMessageResponse response = EmailLibrary.SendMessage(message);
 return response;
}
```

### Checking Email Dispositions
To get email status for any source tracking id, call the
EmailLibrary.GetEmailDisposition method:

```csharp
static void GetEmailDisposition()
{
 GetEmailDispositionResponse response =
 EmailLibrary.GetEmailDisposition("2a3c048485aa4cf6");
}
```
<a name="#contributing"></a>
## Contributing

Bug reports and pull requests are welcome on GitHub at https://github.com/paubox/paubox-csharp.

<a name="#license"></a>
## License

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

## Copyright
Copyright &copy; 2019, Paubox Inc.
