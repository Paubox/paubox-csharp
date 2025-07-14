![Paubox Logo](https://avatars.githubusercontent.com/u/22528478?s=200&v=4)

# Paubox C# SDK <!-- omit in toc -->

This is the official C# wrapper/SDK for the Paubox Email API.

The Paubox Email API allows your application to send secure, HIPAA compliant email via Paubox and track deliveries and
opens.

The API wrapper allows you to construct and send messages.

- [Installation](#installation)
  - [Getting Paubox API Credentials](#getting-paubox-api-credentials)
  - [Configuring API Credentials](#configuring-api-credentials)
    - [For .NET Core/.NET 5+ Projects (Recommended)](#for-net-corenet-5-projects-recommended)
    - [For Legacy .NET Framework Projects](#for-legacy-net-framework-projects)
  - [Supported .NET Versions](#supported-net-versions)
- [Usage](#usage)
  - [Adding Paubox namespace](#adding-paubox-namespace)
  - [Initializing the EmailLibrary](#initializing-the-emaillibrary)
    - [Option 1: Initialize with direct parameters](#option-1-initialize-with-direct-parameters)
    - [Option 2: Initialize with configuration (recommended for .NET Core/.NET 5+)](#option-2-initialize-with-configuration-recommended-for-net-corenet-5)
  - [Send Message](#send-message)
    - [Allowing non-TLS message delivery](#allowing-non-tls-message-delivery)
    - [Forcing Secure Notifications](#forcing-secure-notifications)
    - [Custom Headers](#custom-headers)
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

Add the class library [Paubox.Email.API.dll](lib/Paubox.Email.API.dll) in your C# project by using 'Add Reference'
option within the Project – References node.

### Getting Paubox API Credentials

You will need to have a Paubox account. You can [sign up here](https://www.paubox.com/join/see-pricing?unit=messages).

Once you have an account, follow the instructions on the Rest API dashboard to verify domain ownership and generate API
credentials.

### Configuring API Credentials

The EmailLibrary SDK requires initialization with your API credentials before use. You can provide these credentials in
several ways:

#### For .NET Core/.NET 5+ Projects (Recommended)

1. Copy `appsettings.example.json` to `appsettings.json` in your project
2. Update the values in `appsettings.json` with your actual credentials:

    ```json
    {
        "APIKey": "Your-API-Key-Here",
        "APIUser": "Your-Username-Here"
    }
    ```

3. In your own application, load the configuration and initialize the EmailLibrary:

    ```csharp
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    EmailLibrary.Initialize(configuration);
    ```

**Important**: The `appsettings.json` file is ignored by git to protect your credentials. Always use the example file as
a template.

**Configuration Fields:**

- `APIKey`: Your Paubox API key (required)
- `APIUser`: Your Paubox username/domain (required)

#### For Legacy .NET Framework Projects

Add two app settings keys with their values in App.Config (For Desktop App,
Windows Service) or Web.Config (For ASP.NET projects):

```xml
<add key="APIKey" value="Your-API-Key-Here"/>
<add key="APIUser" value="Your-Username-Here"/>
```

### Supported .NET Versions

This library supports the following .NET versions (see
[official support dates](https://dotnet.microsoft.com/en-us/download/dotnet?cid=getdotnetcorecli)):

| .NET Version         | Support Type          | End of Support    | Paubox SDK Support  |
| -------------------- | --------------------- | ----------------- | ------------------- |
| .NET v10.0 (preview) | Long Term Support     | TBA               | ❌ Not yet supported |
| .NET v9.0 (latest)   | Standard Term Support | May 12, 2026      | ✅ Supported         |
| .NET v8.0            | Long Term Support     | November 10, 2026 | ✅ Supported         |

To use this library, you must use a supported .NET version.

To add the .NET version to your project, add the following to your config file:

```xml
<startup>
  <supportedRuntime version="v8.0" sku=".NETFramework,Version=v8.0"/>
</startup>
```

## Usage

### Adding Paubox namespace

Add the Paubox namespace in the using section as shown below:

```csharp
using Paubox;
```

### Initializing the EmailLibrary

Before using the EmailLibrary, you must create an instance with your API credentials. You have two options:

#### Option 1: Initialize with direct parameters

```csharp
var paubox = new EmailLibrary("your-api-key", "your-username");
```

#### Option 2: Initialize with configuration (recommended for .NET Core/.NET 5+)

```csharp
// Load your configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Create the EmailLibrary instance with the configuration
var paubox = new EmailLibrary(configuration);
```

### Send Message

Please also see the [API Documentation](https://docs.paubox.com/docs/paubox_email_api/messages#send-message).

To send an email, prepare a `Message` object with `Header` and `Content` and call `EmailLibrary.SendMessage`:

```csharp
Message message = new Message();
message.Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" };
message.Cc = new string[] { "cc-recipient@domain.com" };
message.Bcc = new string[] { "bcc-recipient@domain.com" };

Header header = new Header();
header.From = "you@yourdomain.com";
header.ReplyTo = "reply-to@yourdomain.com";
header.Subject = "Testing!";
message.Header = header;

Content content = new Content();
content.PlainText = "Hello World!";
message.Content = content;

SendMessageResponse response = paubox.SendMessage(message);
```

Alternatively, you can use an object initializer to create the message:

```csharp
Message message = new Message() {
    Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" },
    Cc = new string[] { "cc-recipient@domain.com" },
    Bcc = new string[] { "bcc-recipient@domain.com" },
    Header = new Header() {
        Subject = "Testing!",
        From = "you@yourdomain.com",
        ReplyTo = "reply-to@yourdomain.com",
        CustomHeaders = new Dictionary<string, string> {
            { "X-Custom-Header", "Custom Value" },
            { "X-Another-Header", "Another Value" }
        }
    },
    Content = new Content() {
        PlainText = "Hello World!",
        HtmlText = "<html><body><h1>Hello World!</h1></body></html>"
    },
    Attachments = new List<Attachment>() {
        new Attachment() {
            FileName = "hello_world.txt",
            ContentType = "text/plain",
            Content = "SGVsbG8gV29ybGQh\n"
        }
    }
};

SendMessageResponse response = paubox.SendMessage(message);
```

#### Allowing non-TLS message delivery

If you want to send non-PHI mail that does not need to be HIPAA-compliant, you can
allow the message delivery to take place even if a TLS connection is unavailable. This
means a message will not be converted into a Secure Notification message when a unencrypted
connection is encountered. For this, just set message.AllowNonTLS to true, as
shown below:

```csharp
Message message = new Message();
message.Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" };
message.Cc = new string[] { "cc-recipient@domain.com" };
message.Bcc = new string[] { "bcc-recipient@domain.com" };
message.AllowNonTLS = true;

Header header = new Header();
header.From = "you@yourdomain.com";
header.ReplyTo = "reply-to@yourdomain.com";
header.Subject = "Testing!";
message.Header = header;

Content content = new Content();
content.PlainText = "Hello World!";
message.Content = content;

SendMessageResponse response = paubox.SendMessage(message);
```

#### Forcing Secure Notifications

Paubox Secure Notifications allow an extra layer of security, especially when coupled with an organization's requirement for message recipients to use 2-factor authentication to read messages (this setting is available to org administrators in the Paubox Admin Panel).

Instead of receiving an email with the message contents, the recipient will receive a notification email that they have a new message in Paubox.

```csharp
Message message = new Message();
message.Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" };
header.From = "you@yourdomain.com";
message.Cc = new string[] { "cc-recipient@domain.com" };
message.Bcc = new string[] { "bcc-recipient@domain.com" };
message.ForceSecureNotification = "true";

Header header = new Header();
header.Subject = "Testing!";
header.ReplyTo = "reply-to@yourdomain.com";
message.Header = header;

Content content = new Content();
content.PlainText = "Hello World!";
message.Content = content;

SendMessageResponse response = paubox.SendMessage(message);
```

#### Custom Headers

Please see the [API Documentation](https://docs.paubox.com/docs/paubox_email_api/messages#data-parameters) for more
information on custom headers. You can add custom headers to your message by adding them as a Dictionary to the `Header`
object:

```csharp
Message message = new Message();
// ...

Header header = new Header();
header.Subject = "Testing!";
header.ReplyTo = "reply-to@yourdomain.com";
header.CustomHeaders = new Dictionary<string, string> {
  { "X-Custom-Header", "Custom Value" },
  { "X-Another-Header", "Another Value" }
};
message.Header = header;

// ...

SendMessageResponse response = paubox.SendMessage(message);
```

### Get Email Disposition

Please also see the [API Documentation](https://docs.paubox.com/docs/paubox_email_api/messages#get-email-disposition).

To get email status for any source tracking id, call the `EmailLibrary.GetEmailDisposition` method with the source
tracking id of the message:

```csharp
GetEmailDispositionResponse response = paubox.GetEmailDisposition("2a3c048485aa4cf6");
```

### Send Bulk Messages

Please see the [API Documentation](https://docs.paubox.com/docs/paubox_email_api/messages#send-bulk-messages) for more
details. Specifically:

> We recommend batches of 50 (fifty) or less.

Simply construct an array of `Message` objects and call `EmailLibrary.SendBulkMessages`:

```csharp
Message message1 = new Message(...);
Message message2 = new Message(...);
Message message3 = new Message(...);

Message[] messages = new Message[] {
    message1,
    message2,
    message3
};

SendBulkMessagesResponse response = paubox.SendBulkMessages(messages);
```

### Dynamic Templates

Please refer to the [related API documentation](https://docs.paubox.com/docs/paubox_email_api/dynamic_templates) for
more details.

#### Create Dynamic Template

To create a dynamic template, call the `EmailLibrary.CreateDynamicTemplate` method with the template name and the path
to the Handlebars template file:

```csharp
var paubox = new EmailLibrary(configuration);

string templateName = "Example Template";
string templatePath = "path/to/ExampleTemplate.hbs";

DynamicTemplateResponse result = paubox.CreateDynamicTemplate(templateName, templatePath);
```

#### Update Dynamic Template

To update a dynamic template, call the `EmailLibrary.UpdateDynamicTemplate` method with the template id (integer), the template
name, and the path to the Handlebars template file:

```csharp
var paubox = new EmailLibrary(configuration);

int templateId = 123;
string templateName = "Updated Example Template";
string templatePath = "path/to/UpdatedExampleTemplate.hbs";

DynamicTemplateResponse result = paubox.UpdateDynamicTemplate(templateId, templateName, templatePath);
```

It is also possible to update only the template name by passing `null` as the template path:

```csharp
var paubox = new EmailLibrary(configuration);

int templateId = 123;
string templateName = "Updated Example Template";

DynamicTemplateResponse result = paubox.UpdateDynamicTemplate(templateId, templateName, null);
```

#### Delete Dynamic Template

To delete a dynamic template, call the `EmailLibrary.DeleteDynamicTemplate` method with the template id (integer):

```csharp
var paubox = new EmailLibrary(configuration);

int templateId = 123;

DeleteDynamicTemplateResponse result = paubox.DeleteDynamicTemplate(templateId);
```

#### Get Dynamic Template

To get a single dynamic template, call the `EmailLibrary.GetDynamicTemplate` method with the template id (integer):

```csharp
var paubox = new EmailLibrary(configuration);

int templateId = 123;

DynamicTemplateResponse result = paubox.GetDynamicTemplate(templateId);
```

#### List Dynamic Templates

To list all dynamic templates, call the `EmailLibrary.ListDynamicTemplates` method:

```csharp
var paubox = new EmailLibrary(configuration);

List<DynamicTemplateSummary> result = paubox.ListDynamicTemplates();
```

#### Send a Dynamically Templated Message

To [send a dynamically templated message](https://docs.paubox.com/docs/paubox_email_api/dynamic_templates#send-a-dynamically-templated-message),
firstly construct a new `TemplatedMessage` object:

```csharp
var paubox = new EmailLibrary(configuration);

TemplatedMessage message = new TemplatedMessage();

// Note that instead of setting the `Content` property as with a non-templated message,
// we set the `TemplateName` and `TemplateData` properties:
message.TemplateName = "Example Template";
message.TemplateData = new Dictionary<string, string> {
    { "first_name", "John" },
    { "last_name", "Doe" }
};

// Set the other properties as above:
message.Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" };
message.Cc = new string[] { "cc-recipient@domain.com" };
message.Bcc = new string[] { "bcc-recipient@domain.com" };

Header header = new Header();
header.From = "you@yourdomain.com";
header.ReplyTo = "reply-to@yourdomain.com";
header.Subject = "Testing!";
header.CustomHeaders = new Dictionary<string, string> {
    { "X-Custom-Header", "Custom Value" },
    { "X-Another-Header", "Another Value" }
};
message.Header = header;
```

Alternatively, you can use an object initializer to create the message:

```csharp
TemplatedMessage message = new TemplatedMessage() {
    TemplateName = "Example Template",
    TemplateData = new Dictionary<string, string> {
        { "first_name", "John" },
        { "last_name", "Doe" }
    },
    Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" },
    Cc = new string[] { "cc-recipient@domain.com" },
    Bcc = new string[] { "bcc-recipient@domain.com" },
    Header = new Header() {
        Subject = "Testing!",
        From = "you@yourdomain.com",
        ReplyTo = "reply-to@yourdomain.com",
        CustomHeaders = new Dictionary<string, string> {
            { "X-Custom-Header", "Custom Value" },
            { "X-Another-Header", "Another Value" }
        }
    },
    Attachments = new List<Attachment>() {
        new Attachment() {
            FileName = "hello_world.txt",
            ContentType = "text/plain",
            Content = "SGVsbG8gV29ybGQh\n"
        }
    }
};
```

Then, call the `EmailLibrary.SendTemplatedMessage` method to send the message:

```csharp
SendTemplatedMessageResponse response = paubox.SendTemplatedMessage(message);
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md)

## License

See [LICENSE](LICENSE)

## Copyright

Copyright &copy; 2025, Paubox, Inc.
