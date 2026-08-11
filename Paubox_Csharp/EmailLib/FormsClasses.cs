using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paubox
{
    public class Form
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("form_html")]
        public string FormHtml { get; set; }

        [JsonProperty("form_json")]
        public object FormJson { get; set; }

        [JsonProperty("form_css")]
        public string FormCss { get; set; }

        [JsonProperty("vanity_url")]
        public string VanityUrl { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        [JsonProperty("signable")]
        public bool Signable { get; set; }

        [JsonProperty("signature_confirmation_label")]
        public string SignatureConfirmationLabel { get; set; }

        [JsonProperty("submission_count")]
        public int SubmissionCount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [JsonProperty("old_form_id")]
        public int? OldFormId { get; set; }

        [JsonProperty("subscription_list_id")]
        public string SubscriptionListId { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class FormAttachment
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class PageInfo
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("items")]
        public int Items { get; set; }
    }

    public class FormsListResponse
    {
        [JsonProperty("results")]
        public List<Form> Results { get; set; }

        [JsonProperty("page_info")]
        public PageInfo PageInfo { get; set; }
    }

    /// <summary>
    /// Query-string parameters for listing forms. No JSON attributes — these are
    /// used to build the request query string, not a JSON body.
    /// </summary>
    public class FormsListParams
    {
        public int? CustomerId { get; set; }
        public string FormId { get; set; }
        public string Search { get; set; }
        public string Order { get; set; }
        public string OrderBy { get; set; }
        public bool? Archived { get; set; }
        public bool? Active { get; set; }
        public int? Page { get; set; }
        public int? Items { get; set; }
    }

    public class CreateFormRequest
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("form_html")]
        public string FormHtml { get; set; }

        [JsonProperty("form_json")]
        public object FormJson { get; set; }

        [JsonProperty("form_css")]
        public string FormCss { get; set; }

        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [JsonProperty("signable")]
        public bool Signable { get; set; }

        [JsonProperty("signature_confirmation_label")]
        public string SignatureConfirmationLabel { get; set; }

        [JsonProperty("subscription_list_id")]
        public string SubscriptionListId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("submission_count")]
        public int SubmissionCount { get; set; }
    }

    public class CreateFormResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    /// <summary>
    /// PATCH-semantics update request — every field is optional, and null fields are
    /// omitted from the serialized JSON (the backend treats absent = leave unchanged).
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UpdateFormRequest
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("form_json")]
        public object FormJson { get; set; }

        [JsonProperty("vanity_url")]
        public string VanityUrl { get; set; }

        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [JsonProperty("active")]
        public bool? Active { get; set; }

        [JsonProperty("subscription_list_id")]
        public string SubscriptionListId { get; set; }
    }

    public class UpdateFormResponse
    {
        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("form_id")]
        public string FormId { get; set; }
    }

    public class FormStats
    {
        [JsonProperty("active_form_count")]
        public long ActiveFormCount { get; set; }

        [JsonProperty("total_submission_count")]
        public long TotalSubmissionCount { get; set; }

        [JsonProperty("submissions_last_7_days")]
        public long SubmissionsLast7Days { get; set; }
    }

    public class FormSubmission
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("form_id")]
        public string FormId { get; set; }

        [JsonProperty("form_data")]
        public string FormData { get; set; }

        [JsonProperty("storage_type")]
        public string StorageType { get; set; }

        [JsonProperty("storage_url")]
        public string StorageUrl { get; set; }

        [JsonProperty("submitter_email")]
        public string SubmitterEmail { get; set; }

        [JsonProperty("recipients")]
        public string Recipients { get; set; }

        [JsonProperty("attachment")]
        public string Attachment { get; set; }

        [JsonProperty("attachment_name")]
        public string AttachmentName { get; set; }

        [JsonProperty("attachment_url")]
        public string AttachmentUrl { get; set; }

        [JsonProperty("attachment_type")]
        public string AttachmentType { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class FormSubmissionListResponse
    {
        [JsonProperty("data")]
        public List<FormSubmission> Data { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("items")]
        public int Items { get; set; }
    }

    /// <summary>
    /// Query-string parameters for listing form submissions.
    /// </summary>
    public class SubmissionListParams
    {
        public string SubmissionId { get; set; }
        public string OrderBy { get; set; }
        public string Order { get; set; }
        public int? Page { get; set; }
        public int? Items { get; set; }
    }
}
