using System;
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
}
