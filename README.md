![Paubox Logo](https://avatars.githubusercontent.com/u/22528478?s=200&v=4)

# Paubox C# SDK <!-- omit in toc -->

This is the official C# wrapper/SDK for the Paubox Email API and Paubox Forms.

The Paubox Email API allows your application to send secure, HIPAA compliant email via Paubox and track deliveries and
opens.

The Paubox Forms API allows your application to retrieve form definitions and submit form responses.

The API wrapper allows you to construct and send messages, and interact with Paubox Forms.

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
- [Paubox Forms](#paubox-forms)
  - [Initializing the FormsLibrary](#initializing-the-formslibrary)
  - [Get Form](#get-form)
  - [Submit Form](#submit-form)
    - [Submitting with file attachments](#submitting-with-file-attachments)
  - [Managing forms with a scoped API key](#managing-forms-with-a-scoped-api-key)
    - [List Forms](#list-forms)
    - [Create Form](#create-form)
    - [Get Form by ID](#get-form-by-id)
    - [Update Form](#update-form)
    - [Archive and Unarchive](#archive-and-unarchive)
    - [Copy Form](#copy-form)
    - [Form Stats](#form-stats)
    - [List Form Submissions](#list-form-submissions)
    - [Export Submissions (CSV / PDF)](#export-submissions-csv--pdf)
- [Contributing](#contributing)
- [License](#license)
- [Copyright](#copyright)

## Installation

Add the class library [Paubox.Email.API.dll](lib/Paubox.Email.API.dll) in your C# project by using 'Add Reference'
option within the Project – References node.

### Getting Paubox API Credentials

You will need to have a Paubox account. You can [sign up here](https://www.paubox.com/pricing/paubox-email-api).

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

Please also see the [API Documentation](https://docs.paubox.com/email-api/messages).

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

Please see the [API Documentation](https://docs.paubox.com/email-api/messages) for more
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

Please also see the [API Documentation](https://docs.paubox.com/email-api/message-receipt).

To get email status for any source tracking id, call the `EmailLibrary.GetEmailDisposition` method with the source
tracking id of the message:

```csharp
GetEmailDispositionResponse response = paubox.GetEmailDisposition("2a3c048485aa4cf6");
```

### Send Bulk Messages

Please see the [API Documentation](https://docs.paubox.com/email-api/bulk-messages) for more
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

Please refer to the [related API documentation](https://docs.paubox.com/email-api/dynamic-templates) for
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

To [send a dynamically templated message](https://docs.paubox.com/email-api/templated-messages),
firstly construct a new `TemplatedMessage` object:

```csharp
var paubox = new EmailLibrary(configuration);

TemplatedMessage message = new TemplatedMessage();

// Note that instead of setting the `Content` property as with a non-templated message,
// we set the `TemplateName` and `TemplateValues` properties:
message.TemplateName = "Example Template";
message.TemplateValues = new Dictionary<string, string> {
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
    TemplateValues = new Dictionary<string, string> {
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

## Paubox Forms

The Paubox Forms integration provides two public endpoints for retrieving form definitions and
submitting responses (**no API key required** — these are intended for use by form respondents),
plus a set of authenticated management endpoints for listing, creating, updating, archiving,
and copying forms and for working with submissions.

Please also see the [API Documentation](https://docs.paubox.com/forms/get-form).

### Initializing the FormsLibrary

For the public endpoints (`GetForm`, `SubmitForm`), `FormsLibrary` requires no credentials:

```csharp
var forms = new FormsLibrary();
```

For the management endpoints, construct `FormsLibrary` with a **scoped API key** that has the
`forms` scope. The key is sent as `Authorization: Bearer {apiKey}`. The SDK enforces only
that a key was provided; scope enforcement is server-side — the API returns 401/403 for
keys without the `forms` scope.

```csharp
var forms = new FormsLibrary("your-scoped-api-key");
```

For dependency injection or non-production endpoints (staging, regional), the key and base
URL can be read from `IConfiguration` under the `FormsAPIKey` and `FormsBaseURL` keys:

```csharp
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();
var forms = new FormsLibrary(config);
```

Or supply the base URL explicitly:

```csharp
var forms = new FormsLibrary(new APIHelper(), "your-scoped-api-key",
    "https://apx.staging.paubox.com/forms/");
```

Store keys in a config file or environment variable rather than committing string literals.
Calling a management method without an API key throws `InvalidOperationException`.

### Get Form

Retrieves the full definition (HTML, JSON schema, CSS) for a given form UUID:

```csharp
string formId = "550e8400-e29b-41d4-a716-446655440000";
Form form = forms.GetForm(formId);

Console.WriteLine(form.Title);
Console.WriteLine(form.FormHtml);
Console.WriteLine(form.SubmissionCount);
```

The returned `Form` object includes:

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Form UUID |
| `Title` | `string` | Form display title |
| `Description` | `string` | Optional description |
| `FormHtml` | `string` | Rendered form HTML |
| `FormJson` | `object` | JSON schema of form fields |
| `FormCss` | `string` | Associated CSS |
| `Active` | `bool` | Whether the form accepts submissions |
| `Signable` | `bool` | Whether the form supports e-signatures |
| `SubmissionCount` | `int` | Total submissions received |
| `CreatedAt` | `DateTime` | Creation timestamp |
| `UpdatedAt` | `DateTime` | Last update timestamp |

### Submit Form

Submits a respondent's answers. The keys in `formData` should match the field names defined in
the form's schema (`FormJson`):

```csharp
string formId = "550e8400-e29b-41d4-a716-446655440000";

forms.SubmitForm(formId, new Dictionary<string, object>
{
    { "first_name", "Jane" },
    { "last_name", "Smith" },
    { "email", "jane@example.com" }
});
```

#### Submitting with file attachments

File attachments are passed as `FormAttachment` objects with a filename and base64-encoded content.
The maximum total request size is **250 MB**.

```csharp
string formId = "550e8400-e29b-41d4-a716-446655440000";

// Read and base64-encode the file
byte[] fileBytes = File.ReadAllBytes("path/to/consent.pdf");
string base64Content = Convert.ToBase64String(fileBytes);

forms.SubmitForm(
    formId,
    formData: new Dictionary<string, object>
    {
        { "first_name", "Jane" },
        { "signature", "{signature_field}" }
    },
    attachments: new FormAttachment[]
    {
        new FormAttachment
        {
            Name = "consent.pdf",
            Content = base64Content
        }
    }
);
```

### Managing forms with a scoped API key

All of the following methods require a `FormsLibrary` constructed with a scoped API key
carrying the `forms` scope:

```csharp
var forms = new FormsLibrary("your-scoped-api-key");
```

See [api.md](api.md) for full request/response field tables.

#### List Forms

`CustomerId` is required — the server compares its value against the customer the API key
belongs to and returns 403 if it's missing. All other filter parameters are optional. The
server caps `Items` at 100 and silently falls back to a default `OrderBy` if given an
unrecognized column name.

```csharp
FormsListResponse list = forms.ListForms(new FormsListParams
{
    CustomerId = 20147,
    Search = "intake",
    Active = true,
    Page = 1,
    Items = 25
});

foreach (Form f in list.Results)
    Console.WriteLine($"{f.Id}: {f.Title}");

Console.WriteLine($"Total: {list.PageInfo.Count} across {list.PageInfo.Pages} pages");
```

#### Create Form

`Title`, `FormJson`, and `CustomerId` are required:

```csharp
CreateFormResponse created = forms.CreateForm(new CreateFormRequest
{
    Title = "Patient Intake",
    FormJson = new { fields = new[] { new { name = "first_name", type = "text" } } },
    CustomerId = 20147,
    Recipient = "intake@yourdomain.com",
    Active = true
});

Console.WriteLine(created.Id);   // UUID of the new form
```

#### Get Form by ID

The authenticated counterpart to the public `GetForm`:

```csharp
Form form = forms.GetFormById("550e8400-e29b-41d4-a716-446655440000");
```

#### Update Form

Updates are partial — only set the properties you want to change; `null` properties are
omitted from the request and left unchanged:

```csharp
UpdateFormResponse updated = forms.UpdateForm(formId, new UpdateFormRequest
{
    Title = "Patient Intake (v2)",
    Active = false
});

Console.WriteLine(updated.Detail);
```

#### Archive and Unarchive

Both return the API's detail message:

```csharp
string detail = forms.ArchiveForm(formId);     // "Form archived."
detail = forms.UnarchiveForm(formId);
```

#### Copy Form

Duplicates a form under a new title and returns the new `Form`:

```csharp
Form copy = forms.CopyForm(formId, "Patient Intake (copy)");
Console.WriteLine(copy.Id);
```

#### Form Stats

Aggregate statistics, optionally scoped to a customer:

```csharp
FormStats stats = forms.GetFormStats();              // all forms
FormStats customerStats = forms.GetFormStats(1234);  // one customer

Console.WriteLine(stats.ActiveFormCount);
Console.WriteLine(stats.TotalSubmissionCount);
Console.WriteLine(stats.SubmissionsLast7Days);
```

#### List Form Submissions

```csharp
FormSubmissionListResponse submissions = forms.ListFormSubmissions(formId,
    new SubmissionListParams { Page = 1, Items = 50, Order = "desc" });

foreach (FormSubmission s in submissions.Data)
    Console.WriteLine($"{s.Id} from {s.SubmitterEmail} at {s.CreatedAt}");
```

#### Export Submissions (CSV / PDF)

```csharp
// All submissions for a form, as CSV text
string csv = forms.ExportSubmissionsCsv(formId);
File.WriteAllText("submissions.csv", csv);

// A single submission, as CSV text
string oneCsv = forms.ExportSubmissionCsv(formId, submissionId);

// A single submission, as a PDF document
byte[] pdf = forms.ExportSubmissionPdf(formId, submissionId);
File.WriteAllBytes("submission.pdf", pdf);
```

#### Error handling

Every management method throws `PauboxApiException` on a non-2xx response. The exception's
`Message` property carries only `{verb} {endpoint} -> {status}` — the raw response body is on
the `Body` property so structured loggers don't pick it up by default (submission responses
can carry submitter-supplied content).

```csharp
try
{
    forms.GetFormById(formId);
}
catch (PauboxApiException ex)
{
    Console.WriteLine($"HTTP {ex.StatusCode}: {ex.Verb} {ex.Endpoint}");
    // ex.Body is the raw response — opt in when you know the endpoint's contract
}
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md)

## License

See [LICENSE](LICENSE)

## Copyright

Copyright &copy; 2025, Paubox, Inc.
## 💬 Community & support

Questions, ideas, or want to share what you built? Join the **[Paubox Community](https://github.com/Paubox/community/discussions)** — the single home for discussions across every Paubox SDK and API.

🔐 Found a security issue? Email **devops@paubox.com** — please don't post it publicly.
