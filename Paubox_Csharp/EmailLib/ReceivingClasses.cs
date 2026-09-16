using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paubox
{
    public class ReceivingDomain
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("dns_records")]
        public List<DnsRecord> DnsRecords { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class DnsRecord
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("priority")]
        public int? Priority { get; set; }
    }

    public class ReceivingDomainListResponse
    {
        [JsonProperty("data")]
        public List<ReceivingDomain> Data { get; set; }
    }

    public class ReceivingDomainResponse
    {
        [JsonProperty("data")]
        public ReceivingDomain Data { get; set; }
    }

    public class DeleteReceivingDomainResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreateReceivingDomainRequest
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }
    }

    public class Mailbox
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("quota_bytes")]
        public long? QuotaBytes { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class MailboxListResponse
    {
        [JsonProperty("data")]
        public List<Mailbox> Data { get; set; }
    }

    public class MailboxResponse
    {
        [JsonProperty("data")]
        public Mailbox Data { get; set; }
    }

    public class DeleteMailboxResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreateMailboxRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("quota_bytes")]
        public long? QuotaBytes { get; set; }
    }

    public class ReceivedEmail
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public List<string> To { get; set; }

        [JsonProperty("cc")]
        public List<string> Cc { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("body_text")]
        public string BodyText { get; set; }

        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }

        [JsonProperty("received_at")]
        public DateTime ReceivedAt { get; set; }

        [JsonProperty("attachments")]
        public List<ReceivedAttachment> Attachments { get; set; }
    }

    public class ReceivedAttachment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }

    public class ReceivedEmailListResponse
    {
        [JsonProperty("data")]
        public List<ReceivedEmail> Data { get; set; }
    }

    public class ReceivedEmailResponse
    {
        [JsonProperty("data")]
        public ReceivedEmail Data { get; set; }
    }

    public class ListReceivedEmailsParams
    {
        public int? Limit { get; set; }
        public string After { get; set; }
        public string Before { get; set; }
    }
}
