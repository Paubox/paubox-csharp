![Paubox Logo](https://avatars.githubusercontent.com/u/22528478?s=200&v=4)

# Paubox C# SDK <!-- omit in toc -->

This is the official C# wrapper/SDK for the Paubox Email API.

The Paubox Email API allows your application to send secure, HIPAA compliant email via Paubox and track deliveries and
opens.

The API wrapper allows you to construct and send messages.

- [Installation](#installation)
  - [Getting Paubox API Credentials](#getting-paubox-api-credentials)
  - [Configuring API Credentials](#configuring-api-credentials)
  - [Supported .NET Versions](#supported-net-versions)
- [Usage](#usage)
  - [Adding Paubox namespace](#adding-paubox-namespace)
  - [Send Message](#send-message)
    - [Allowing non-TLS message delivery](#allowing-non-tls-message-delivery)
    - [Forcing Secure Notifications](#forcing-secure-notifications)
  - [Get Email Disposition](#get-email-disposition)
  - [Send Bulk Messages](#send-bulk-messages)
  - [Dynamic Templates](#dynamic-templates)
    - [Create Dynamic Template](#create-dynamic-template)
    - [Update Dynamic Template](#update-dynamic-template)
    - [Delete Dynamic Template](#delete-dynamic-template)
    - [Get Dynamic Template](#get-dynamic-template)
    - [List Dynamic Templates](#list-dynamic-templates)
    - [Send a Dynamically Templated Message](#send-a-dynamically-templated-message)
- [Contributing](#contributing)
- [License](#license)
- [Copyright](#copyright)

## Installation

Add the class library [Paubox.Email.API.dll](lib/Paubox.Email.API.dll) in your C# project by using ‘Add Reference’ option within the Project – References node.

### Getting Paubox API Credentials

You will need to have a Paubox account. You can [sign up here](https://www.paubox.com/join/see-pricing?unit=messages).

Once you have an account, follow the instructions on the Rest API dashboard to verify domain ownership and generate API credentials.

### Configuring API Credentials

Include your API credentials in a config file.

Add two app settings keys with their values in App.Config (For Desktop App,
Windows Service) or Web.Config (For ASP.NET projects):

```xml
<add key="APIKey" value="Your-API-Key-Here"/>
<add key="APIUser" value="Your-Username-Here"/>
```

### Supported .NET Versions

This library supports the following .NET versions (see [here](https://dotnet.microsoft.com/en-us/download/dotnet?cid=getdotnetcorecli) for official support dates):

| .NET Version         | Support Type          | End of Support    | Paubox SDK Support  |
| -------------------- | --------------------- | ----------------- | ------------------- |
| .NET v10.0 (preview) | Long Term Support     | TBA               | ❌ Not yet supported |
| .NET v9.0 (latest)   | Standard Term Support | May 12, 2026      | ✅ Supported         |
| .NET v8.0            | Long Term Support     | November 10, 2026 | ✅ Supported         |

To use this library, you must use a supported .NET version.

To add the .NET version to your project, add the following to your config file:

```xml
<startup>
  <supportedRuntime version="v9.0" sku=".NETFramework,Version=v9.0"/>
</startup>
```

## Usage

### Adding Paubox namespace

Please add the Paubox namespace in the using section as shown below:

```csharp
using Paubox;
```

### Send Message

Please also see the [API Documentation](https://docs.paubox.com/docs/paubox_email_api/messages#send-message).

To send an email, prepare a `Message` object and call EmailLibrary.SendMessage` method:

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

#### Allowing non-TLS message delivery

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

#### Forcing Secure Notifications

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

### Get Email Disposition

Please also see the [API Documentation](https://docs.paubox.com/docs/paubox_email_api/messages#get-email-disposition).

To get email status for any source tracking id, call the `EmailLibrary.GetEmailDisposition` method:

```csharp
static void GetEmailDisposition()
{
    GetEmailDispositionResponse response =
        EmailLibrary.GetEmailDisposition("2a3c048485aa4cf6");
}
```

### Send Bulk Messages

Coming soon.

### Dynamic Templates

#### Create Dynamic Template

Coming soon.

#### Update Dynamic Template

Coming soon.

#### Delete Dynamic Template

Coming soon.

#### Get Dynamic Template

Coming soon.

#### List Dynamic Templates

Coming soon.

#### Send a Dynamically Templated Message

Coming soon.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md)

## License

See [LICENSE](LICENSE)

## Copyright

Copyright &copy; 2025, Paubox, Inc.
