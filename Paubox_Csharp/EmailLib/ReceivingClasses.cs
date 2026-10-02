using System;
using System.Collections.Generic;
using System.Linq;
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
        [JsonProperty("email_id")]
        public string Id { get; set; }

        [JsonProperty("from")]
        public List<ReceivedEmailAddress> FromAddresses { get; set; }

        [JsonProperty("to")]
        public List<ReceivedEmailAddress> ToAddresses { get; set; }

        [JsonProperty("cc")]
        public List<ReceivedEmailAddress> CcAddresses { get; set; }

        [Obsolete("The API returns sender address objects; use FromAddresses. This returns only the first sender's address.")]
        [JsonIgnore]
        public string From
        {
            get { return FromAddresses?.FirstOrDefault()?.Address; }
            set { FromAddresses = value == null ? null : new List<ReceivedEmailAddress> { new ReceivedEmailAddress { Address = value } }; }
        }

        [Obsolete("The API returns recipient address objects; use ToAddresses.")]
        [JsonIgnore]
        public List<string> To
        {
            get { return ToAddresses?.Select(a => a.Address).ToList(); }
            set { ToAddresses = value?.Select(a => new ReceivedEmailAddress { Address = a }).ToList(); }
        }

        [Obsolete("The API returns recipient address objects; use CcAddresses.")]
        [JsonIgnore]
        public List<string> Cc
        {
            get { return CcAddresses?.Select(a => a.Address).ToList(); }
            set { CcAddresses = value?.Select(a => new ReceivedEmailAddress { Address = a }).ToList(); }
        }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("received_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ReceivedAt { get; set; }

        [JsonProperty("message_id")]
        public List<string> MessageId { get; set; }

        [JsonProperty("in_reply_to")]
        public List<string> InReplyTo { get; set; }

        [JsonProperty("references")]
        public List<string> References { get; set; }

        [JsonProperty("has_attachment")]
        public bool? HasAttachment { get; set; }

        [JsonProperty("spam")]
        public bool Spam { get; set; }

        [JsonProperty("spam_score")]
        public double? SpamScore { get; set; }

        [JsonProperty("text_body")]
        public string BodyText { get; set; }

        [JsonProperty("html_body")]
        public string BodyHtml { get; set; }

        [JsonProperty("attachments")]
        public List<ReceivedAttachment> Attachments { get; set; }

        [JsonProperty("size")]
        public long? Size { get; set; }

        [JsonProperty("authentication")]
        public ReceivedEmailAuthentication Authentication { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("headers")]
        public List<ReceivedEmailHeader> Headers { get; set; }
    }

    public class ReceivedEmailAddress
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }

    public class ReceivedEmailAuthentication
    {
        [JsonProperty("spf")]
        public string Spf { get; set; }

        [JsonProperty("dkim")]
        public string Dkim { get; set; }

        [JsonProperty("dmarc")]
        public string Dmarc { get; set; }
    }

    public class ReceivedEmailHeader
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class ReceivedAttachment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long Size { get; set; }

        [JsonProperty("content_id")]
        public string ContentId { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }
    }

    public class ReceivedEmailListResponse
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("data")]
        public List<ReceivedEmail> Data { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }
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
