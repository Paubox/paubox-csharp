# Paubox C# SDK — API Reference

## Table of Contents

- [Email API](#email-api)
  - [Authentication](#authentication)
  - [EmailLibrary Methods](#emaillibrary-methods)
- [Forms API](#forms-api)
  - [FormsLibrary Methods](#formslibrary-methods)
- [Error Handling](#error-handling)

---

## Email API

**Base URL:** `https://api.paubox.net/v1/{apiUser}/`

### Authentication

All Email API requests require an `Authorization` header:

```
Authorization: Token token={apiKey}
```

Pass your `apiKey` and `apiUser` (your Paubox username/domain) when constructing `EmailLibrary`.

### EmailLibrary Methods

#### `SendMessage(Message message) → SendMessageResponse`

Sends a single secure email.

**Request body fields (via `Message`):**

| Field | Type | Required | Description |
|---|---|---|---|
| `Recipients` | `string[]` | Yes | To addresses |
| `Cc` | `string[]` | No | CC addresses |
| `Bcc` | `string[]` | No | BCC addresses |
| `Header.From` | `string` | Yes | Sender address |
| `Header.Subject` | `string` | Yes | Email subject |
| `Header.ReplyTo` | `string` | No | Reply-to address |
| `Header.CustomHeaders` | `Dictionary<string,string>` | No | Additional headers |
| `Content.PlainText` | `string` | No | Plain text body |
| `Content.HtmlText` | `string` | No | HTML body (base64-encoded on the wire) |
| `Attachments` | `List<Attachment>` | No | File attachments |
| `AllowNonTLS` | `bool` | No | Allow unencrypted delivery (default: false) |
| `ForceSecureNotification` | `string` | No | Force secure notification delivery |

**Response:** `SendMessageResponse`

| Field | Type | Description |
|---|---|---|
| `SourceTrackingId` | `string` | ID for tracking via `GetEmailDisposition` |
| `Data` | `string` | Raw API data payload |
| `CustomHeaders` | `Dictionary<string,string>` | Echoed custom headers |
| `Errors` | `List<Error>` | Non-null on failure |

---

#### `SendBulkMessages(Message[] messages) → SendBulkMessagesResponse`

Sends up to 50 messages in a single request. Recommended batch size: ≤ 50.

**Response:** `SendBulkMessagesResponse`

| Field | Type | Description |
|---|---|---|
| `Messages` | `List<BulkMessageResponse>` | Per-message results |

Each `BulkMessageResponse` has the same fields as `SendMessageResponse`.

---

#### `SendTemplatedMessage(TemplatedMessage message) → SendMessageResponse`

Sends a message rendered from a stored dynamic template.

**Additional fields (via `TemplatedMessage`):**

| Field | Type | Required | Description |
|---|---|---|---|
| `TemplateName` | `string` | Yes | Name of the dynamic template |
| `TemplateValues` | `Dictionary<string,object>` | No | Variables injected into the template |

---

#### `GetEmailDisposition(string sourceTrackingId) → GetEmailDispositionResponse`

Returns delivery and open status for a previously sent message.

**Response:** `GetEmailDispositionResponse`

| Field | Type | Description |
|---|---|---|
| `SourceTrackingId` | `string` | Echoed tracking ID |
| `Data.Message.Id` | `string` | Message ID |
| `Data.Message.Message_Deliveries` | `List<MessageDeliveries>` | Per-recipient status |
| `Errors` | `List<Error>` | Non-null on failure |

Each `MessageDeliveries` entry:

| Field | Type | Description |
|---|---|---|
| `Recipient` | `string` | Recipient address |
| `Status.DeliveryStatus` | `string` | e.g. `"delivered"` |
| `Status.DeliveryTime` | `DateTime?` | When delivered |
| `Status.OpenedStatus` | `string` | `"opened"` or `"unopened"` |
| `Status.OpenedTime` | `DateTime?` | When first opened |

---

#### Dynamic Template Methods

| Method | Signature | Description |
|---|---|---|
| `CreateDynamicTemplate` | `(string name, string templatePath) → DynamicTemplateResponse` | Upload a new Handlebars template |
| `UpdateDynamicTemplate` | `(int id, string name, string templatePath) → DynamicTemplateResponse` | Replace or rename a template |
| `DeleteDynamicTemplate` | `(int id) → DeleteDynamicTemplateResponse` | Remove a template |
| `GetDynamicTemplate` | `(int id) → GetDynamicTemplateResponse` | Fetch a single template |
| `ListDynamicTemplates` | `() → List<DynamicTemplateSummary>` | List all templates |

---

## Forms API

**Base URL:** `https://next.paubox.com/`

**Authentication:** None — these are public endpoints intended for form embed usage.

### FormsLibrary Methods

#### `GetForm(string formId) → Form`

Retrieves the full definition of a form (HTML, JSON schema, CSS) by its UUID.

**Path:** `GET /public/form_data/{form_id}`

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formId` | `string` (UUID) | Yes | The form's unique identifier |

**Response:** `Form`

| Field | Type | Description |
|---|---|---|
| `Id` | `string` | Form UUID |
| `Title` | `string` | Form display title |
| `Description` | `string` | Optional description |
| `FormHtml` | `string` | Rendered form HTML |
| `FormJson` | `object` | JSON schema of form fields |
| `FormCss` | `string` | Associated CSS |
| `VanityUrl` | `string` | Custom URL slug |
| `Version` | `int` | Schema version |
| `Active` | `bool` | Whether the form accepts submissions |
| `CustomerId` | `int` | Owning customer ID |
| `Signable` | `bool` | Whether the form supports e-signatures |
| `SignatureConfirmationLabel` | `string` | Label shown on signature confirmation |
| `SubmissionCount` | `int` | Total submissions received |
| `Type` | `string` | Form type |
| `Deleted` | `bool` | Soft-delete flag |
| `Archived` | `bool` | Archive flag |
| `CreatedAt` | `DateTime` | Creation timestamp |
| `UpdatedAt` | `DateTime` | Last update timestamp |

**Errors:** Throws `SystemException` if the form is not found or the response is invalid.

---

#### `SubmitForm(string formId, Dictionary<string, object> formData, FormAttachment[] attachments = null) → void`

Submits a respondent's answers for a form. On success, the service stores the submission,
increments the form's submission count, and emails recipients if configured.

Maximum request size: **250 MB** (to support file attachments).

**Path:** `POST /api/forms/{form_id}/submissions`

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formId` | `string` (UUID) | Yes | The form's unique identifier |
| `formData` | `Dictionary<string, object>` | Yes | Key-value pairs matching the form's field schema |
| `attachments` | `FormAttachment[]` | No | File attachments |

**`FormAttachment` fields:**

| Field | Type | Description |
|---|---|---|
| `Name` | `string` | Filename (e.g. `"consent.pdf"`) |
| `Content` | `string` | Base64-encoded file bytes |

**Errors:**
- Throws `ArgumentNullException` if `formData` is null
- Throws `ArgumentException` if `formId` is null or empty
- Throws `SystemException` if the API returns an error response (400 form not found, 404 invalid form data, etc.)

---

## Error Handling

Both libraries throw `SystemException` when the API returns an error, with the raw API response
string as the exception message. Parse the message as JSON to extract structured error details.

Example error response shape from the Email API:

```json
{
  "errors": [
    {
      "code": 404,
      "title": "Message was not found",
      "details": "Message with this tracking id was not found"
    }
  ]
}
```

The `Error` class maps these fields:

```csharp
public class Error
{
    public int Code { get; set; }
    public string Title { get; set; }
    public string Details { get; set; }
}
```
