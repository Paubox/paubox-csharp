# Paubox C# SDK — API Reference

## Table of Contents

- [Email API](#email-api)
  - [Authentication](#authentication)
  - [EmailLibrary Methods](#emaillibrary-methods)
- [Forms API](#forms-api)
  - [Authentication (Forms)](#authentication-forms)
  - [FormsLibrary Methods (Public)](#formslibrary-methods-public)
  - [FormsLibrary Methods (Management)](#formslibrary-methods-management)
- [Error Handling](#error-handling)

---

## Email API

**Base URL:** `https://api.paubox.com/v1/`

### Authentication

All Email API requests require an `Authorization` header:

```
Authorization: Token token={apiKey}
```

Pass your `apiKey` when constructing `EmailLibrary` — a Paubox username is no longer required:

```csharp
var paubox = new EmailLibrary(apiKey);
```

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

**Base URL:** `https://api.paubox.com/forms/`

### Authentication (Forms)

The Forms API has two tiers of endpoints:

- **Public endpoints** (`GetForm`, `SubmitForm`) require no authentication — they are intended
  for form embed usage.
- **Management endpoints** (all other methods) require a **scoped API key** sent as a Bearer
  token. The key must carry the `forms` scope:

```
Authorization: Bearer {apiKey}
```

Pass the scoped API key when constructing `FormsLibrary`:

```csharp
var forms = new FormsLibrary();          // public endpoints only
var forms = new FormsLibrary(apiKey);    // public + management endpoints
```

Or load it from `IConfiguration` under the `FormsAPIKey` key (optionally with `FormsBaseURL`
for staging/regional endpoints):

```csharp
var forms = new FormsLibrary(config);
```

Calling a management method on an instance constructed without an API key throws
`InvalidOperationException` before any HTTP request is made. Scope enforcement is server-side —
the SDK only checks that a non-empty key was supplied; a key without the `forms` scope
returns 401/403 from the API.

### FormsLibrary Methods (Public)

These methods require no authentication.

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
| `Recipient` | `string` | Address notified on submission |
| `OldFormId` | `int?` | Legacy form ID, if migrated |
| `SubscriptionListId` | `string` | Associated subscription list ID |
| `Deleted` | `bool` | Soft-delete flag |
| `Archived` | `bool` | Archive flag |
| `CreatedAt` | `DateTime` | Creation timestamp |
| `UpdatedAt` | `DateTime` | Last update timestamp |

**Errors:** Throws `PauboxApiException` if the form is not found or the response is invalid.

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
- Throws `PauboxApiException` if the API returns an error response (400 form not found, 404 invalid form data, etc.)

---

### FormsLibrary Methods (Management)

All methods below require a `FormsLibrary` constructed with a scoped API key carrying the
`forms` scope (see [Authentication (Forms)](#authentication-forms)). Each throws
`InvalidOperationException` if no API key was provided, and `PauboxApiException` with the raw
response body if the API returns an unexpected response.

#### `ListForms(FormsListParams parameters = null) → FormsListResponse`

Lists forms, with pagination and optional filtering.

**Path:** `GET /api/forms`

**Query parameters (via `FormsListParams`):**

| Property | Type | Query param | Required | Description |
|---|---|---|---|---|
| `CustomerId` | `int?` | `customer_id` | **Yes** | Owning customer — the server compares this against the API key's owner and returns 403 if it's missing or wrong. |
| `FormId` | `string` | `form_id` | No | Filter to a specific form UUID |
| `Search` | `string` | `search` | No | Free-text search on title/description |
| `Order` | `string` | `order` | No | Sort direction (`asc` / `desc`) |
| `OrderBy` | `string` | `order_by` | No | Field to sort by; server silently falls back to a default when given an unrecognized column |
| `Archived` | `bool?` | `archived` | No | Filter by archive flag |
| `Active` | `bool?` | `active` | No | Filter by active flag |
| `Page` | `int?` | `page` | No | Page number, 1-indexed; the SDK rejects `Page < 1` with `ArgumentOutOfRangeException` |
| `Items` | `int?` | `items` | No | Items per page; server caps at 100 |

**Response:** `FormsListResponse`

| Field | Type | Description |
|---|---|---|
| `Results` | `List<Form>` | Forms on this page |
| `PageInfo` | `PageInfo` | Pagination metadata |

`PageInfo` fields:

| Field | Type | Description |
|---|---|---|
| `Count` | `long` | Total matching forms |
| `Pages` | `int` | Total pages |
| `Page` | `int` | Current page |
| `Items` | `int` | Items per page |

---

#### `CreateForm(CreateFormRequest request) → CreateFormResponse`

Creates a new form.

**Path:** `POST /api/forms`

**Request body fields (via `CreateFormRequest`):**

| Field | Type | Required | Description |
|---|---|---|---|
| `Title` | `string` | Yes | Form display title |
| `FormJson` | `object` | Yes | JSON schema of form fields |
| `Description` | `string` | No | Optional description |
| `FormHtml` | `string` | No | Rendered form HTML |
| `FormCss` | `string` | No | Associated CSS |
| `CustomerId` | `int?` | **Yes** | Owning customer ID — server rejects create with 403 when this is missing |
| `Recipient` | `string` | No | Address notified on submission |
| `Signable` | `bool` | No | Whether the form supports e-signatures |
| `SignatureConfirmationLabel` | `string` | No | Label shown on signature confirmation |
| `SubscriptionListId` | `string` | No | Associated subscription list ID |
| `Type` | `string` | No | Form type |
| `Active` | `bool` | No | Whether the form accepts submissions |
| `Version` | `int` | No | Schema version |
| `SubmissionCount` | `int` | No | Initial submission count |

**Response:** `CreateFormResponse`

| Field | Type | Description |
|---|---|---|
| `Id` | `string` | UUID of the created form |

**Errors:**
- Throws `ArgumentNullException` if `request` is null
- Throws `ArgumentException` if `Title` is null/empty or `FormJson` is null

---

#### `GetFormById(string formId) → Form`

Retrieves a form by UUID via the authenticated management endpoint. Distinct from the public
`GetForm` — this endpoint returns forms regardless of embed visibility.

**Path:** `GET /api/forms/{form_id}`

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formId` | `string` (UUID) | Yes | The form's unique identifier |

**Response:** `Form` (same fields as [`GetForm`](#getformstring-formid--form); unwrapped from
the API's `{"data": {...}}` envelope).

---

#### `UpdateForm(string formId, UpdateFormRequest request) → UpdateFormResponse`

Updates a form. Update semantics are partial: every field of `UpdateFormRequest` is optional,
and fields left `null` are omitted from the request body and left unchanged by the backend.

**Path:** `PUT /api/forms/{form_id}`

**Request body fields (via `UpdateFormRequest`, all optional):**

| Field | Type | Description |
|---|---|---|
| `Title` | `string` | Form display title |
| `Description` | `string` | Description |
| `FormJson` | `object` | JSON schema of form fields |
| `VanityUrl` | `string` | Custom URL slug |
| `Recipient` | `string` | Address notified on submission |
| `Active` | `bool?` | Whether the form accepts submissions |
| `SubscriptionListId` | `string` | Associated subscription list ID |

**Response:** `UpdateFormResponse`

| Field | Type | Description |
|---|---|---|
| `Detail` | `string` | Human-readable result message |
| `FormId` | `string` | UUID of the updated form |

**Errors:**
- Throws `ArgumentException` if `formId` is null or empty
- Throws `ArgumentNullException` if `request` is null

---

#### `ArchiveForm(string formId) → string`

Archives a form. Returns the API's detail message (e.g. `"Form archived."`).

**Path:** `POST /api/forms/{form_id}/archive`

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formId` | `string` (UUID) | Yes | The form's unique identifier |

---

#### `UnarchiveForm(string formId) → string`

Restores an archived form. Returns the API's detail message.

**Path:** `POST /api/forms/{form_id}/unarchive`

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formId` | `string` (UUID) | Yes | The form's unique identifier |

---

#### `CopyForm(string formId, string newTitle) → Form`

Duplicates an existing form under a new title and returns the newly created `Form`.

**Path:** `POST /api/forms/copy`

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formId` | `string` (UUID) | Yes | UUID of the form to copy |
| `newTitle` | `string` | Yes | Title for the copy |

**Request body:**

| Field | Type | Description |
|---|---|---|
| `form_id` | `string` | UUID of the form to copy |
| `title` | `string` | Title for the copy |

**Response:** the new `Form` object (same fields as [`GetForm`](#getformstring-formid--form)).

---

#### `GetFormStats(int? customerId = null) → FormStats`

Returns aggregate form statistics, optionally scoped to a customer.

**Path:** `GET /api/forms/stats` (with `?customer_id={customerId}` when provided)

**Response:** `FormStats`

| Field | Type | Description |
|---|---|---|
| `ActiveFormCount` | `long` | Number of active forms |
| `TotalSubmissionCount` | `long` | Total submissions across forms |
| `SubmissionsLast7Days` | `long` | Submissions received in the last 7 days |

---

#### `ListFormSubmissions(string formId, SubmissionListParams parameters = null) → FormSubmissionListResponse`

Lists submissions for a form, with optional filtering and pagination.

**Path:** `GET /api/forms/{form_id}/submissions`

**Query parameters (via `SubmissionListParams`, all optional):**

| Property | Type | Query param | Description |
|---|---|---|---|
| `SubmissionId` | `string` | `submission_id` | Filter to a specific submission |
| `OrderBy` | `string` | `order_by` | Field to sort by |
| `Order` | `string` | `order` | Sort direction (`asc` / `desc`) |
| `Page` | `int?` | `page` | Page number |
| `Items` | `int?` | `items` | Items per page |

**Response:** `FormSubmissionListResponse`

| Field | Type | Description |
|---|---|---|
| `Data` | `List<FormSubmission>` | Submissions on this page |
| `Total` | `long` | Total matching submissions |
| `Page` | `int` | Current page |
| `Items` | `int` | Items per page |

`FormSubmission` fields:

| Field | Type | Description |
|---|---|---|
| `Id` | `string` | Submission UUID |
| `FormId` | `string` | Parent form UUID |
| `FormData` | `string` | Submitted field values (JSON string) |
| `StorageType` | `string` | Where the submission payload is stored |
| `StorageUrl` | `string` | Storage location URL |
| `SubmitterEmail` | `string` | Respondent's email address |
| `Recipients` | `string` | Notification recipients |
| `Attachment` | `string` | Attachment payload reference |
| `AttachmentName` | `string` | Attachment filename |
| `AttachmentUrl` | `string` | Attachment download URL |
| `AttachmentType` | `string` | Attachment MIME type |
| `CreatedAt` | `DateTime` | Submission timestamp |

---

#### `ExportSubmissionsCsv(string formId) → string`

Exports all submissions for a form as CSV text.

**Path:** `GET /api/forms/{form_id}/submissions/submission-csv`

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formId` | `string` (UUID) | Yes | The form's unique identifier |

**Response:** raw CSV as a `string`. An empty CSV is a valid response for a form with no
submissions. On a non-2xx response, throws `PauboxApiException`.

---

#### `ExportSubmissionCsv(string formId, string submissionId) → string`

Exports a single submission as CSV text.

**Path:** `GET /api/forms/{form_id}/submissions/submission-csv/{submission_id}`

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formId` | `string` (UUID) | Yes | The form's unique identifier |
| `submissionId` | `string` (UUID) | Yes | The submission's unique identifier |

**Response:** raw CSV as a `string`. Throws `PauboxApiException` on a non-2xx response.

---

#### `ExportSubmissionPdf(string formId, string submissionId) → byte[]`

Exports a single submission as a PDF document.

**Path:** `GET /api/forms/{form_id}/submissions/{submission_id}/submission-pdf`

The SDK sends `Accept: application/pdf` and validates that the response Content-Type matches
and the first four bytes are the `%PDF` magic sequence — otherwise it throws
`PauboxApiException` so callers never write a broken "PDF" file to disk.

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formId` | `string` (UUID) | Yes | The form's unique identifier |
| `submissionId` | `string` (UUID) | Yes | The submission's unique identifier |

**Response:** raw PDF bytes as `byte[]`.

---

## Error Handling

Every management method throws `PauboxApiException` on a non-2xx HTTP response. The exception's
`Message` property carries only `{verb} {endpoint} -> {status}` — the raw response body is on
the `Body` property so structured loggers (Sentry, Application Insights, most .NET logging
sinks) that record `Exception.Message` by default don't pick up submitter-supplied content.

```csharp
try
{
    forms.GetFormById(formId);
}
catch (PauboxApiException ex)
{
    // ex.StatusCode, ex.Verb, ex.Endpoint are always safe to log
    logger.LogWarning("Forms API {Status}: {Verb} {Endpoint}", ex.StatusCode, ex.Verb, ex.Endpoint);

    // ex.Body carries the raw response — opt in when you know the endpoint's contract
    if (ex.StatusCode == 404) { /* handle */ }
}
```

Client-side validation errors surface as `ArgumentException` (or `ArgumentNullException`,
`ArgumentOutOfRangeException`) before any HTTP request is made:

- `ArgumentException` on missing / non-UUID / hostile-shape form or submission ids
- `ArgumentException` when required parameters (e.g. `CustomerId` on list/create) are unset
- `ArgumentOutOfRangeException` on `Page < 1`
- `InvalidOperationException` when a management method is called without an API key

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
