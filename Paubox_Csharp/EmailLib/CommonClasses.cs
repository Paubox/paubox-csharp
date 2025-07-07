using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paubox
{
    public class Header
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string ReplyTo { get; set; }
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>();
    }

    public class Content
    {
        public string PlainText { get; set; }
        public string HtmlText { get; set; }
    }

    public class Attachment
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
    }

    public class SendMessageResponse
    {
        [JsonProperty("sourceTrackingId")] // SIC: This property is not snake_cased in the API response
        public string SourceTrackingId { get; set; }

        public string Data { get; set; }

        [JsonProperty("customHeaders")] // SIC: This property is not snake_cased in the API response
        public Dictionary<string, string> CustomHeaders { get; set; }

        public List<Error> Errors { get; set; }
    }

    public class SendBulkMessagesResponse
    {
        public List<BulkMessageResponse> Messages { get; set; }
    }

    public class BulkMessageResponse
    {
        [JsonProperty("sourceTrackingId")] // SIC: This property is not snake_cased in the API response
        public string SourceTrackingId { get; set; }

        public string Data { get; set; }

        [JsonProperty("customHeaders")] // SIC: This property is not snake_cased in the API response
        public Dictionary<string, string> CustomHeaders { get; set; }
        public List<Error> Errors { get; set; }
    }

    public class GetEmailDispositionResponse
    {
        [JsonProperty("sourceTrackingId")] // SIC: This property is not snake_cased in the API response
        public string SourceTrackingId { get; set; }

        public MessageData Data { get; set; }
        public List<Error> Errors { get; set; }
    }

    public class MessageData
    {
        public MessageDetails Message { get; set; }
    }

    public class MessageDetails
    {
        public string Id { get; set; }
        public List<MessageDeliveries> Message_Deliveries { get; set; }
    }

    public class MessageDeliveries
    {
        public string Recipient { get; set; }
        public MessageStatus Status { get; set; }
    }

    public class MessageStatus
    {
        [JsonProperty("deliveryStatus")] // SIC: This property is not snake_cased in the API response
        public string DeliveryStatus { get; set; }

        [JsonProperty("deliveryTime")] // SIC: This property is not snake_cased in the API response
        public DateTime? DeliveryTime { get; set; }

        [JsonProperty("openedStatus")] // SIC: This property is not snake_cased in the API response
        public string OpenedStatus { get; set; }

        [JsonProperty("openedTime")] // SIC: This property is not snake_cased in the API response
        public DateTime? OpenedTime { get; set; }
    }

    public class GetDynamicTemplateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonProperty("api_customer_id")]
        public int ApiCustomerId { get; set; }

        public string Body { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public Dictionary<string, string> Metadata { get; set; }
        public string Error { get; set; }
    }

    public class DynamicTemplateSummary
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonProperty("api_customer_id")]
        public int ApiCustomerId { get; set; }
    }

    public class DynamicTemplateResponse
    {
        public string Message { get; set; }
        public DynamicTemplateParams Params { get; set; }
        public string Error { get; set; }
    }

    public class DynamicTemplateParams
    {
        public string Name { get; set; }
        public DynamicTemplateParamsBody Body { get; set; }
    }

    public class DynamicTemplateParamsBody
    {
        public string Tempfile { get; set; }
        public string Headers { get; set; }

        [JsonProperty("original_filename")]
        public string OriginalFilename { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }
    }

    public class DeleteDynamicTemplateResponse
    {
        public string Message { get; set; }
        public string Error { get; set; }
    }

    public class Error
    {
        public int Code { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
    }
}
