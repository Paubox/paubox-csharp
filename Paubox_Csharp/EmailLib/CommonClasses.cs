using System;
using System.Collections.Generic;

namespace Paubox
{

    #region  Classes for Send Message method
    public class Header
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string ReplyTo { get; set; }
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
    public class Message
    {
        public string[] Recipients { get; set; }
        public string[] Bcc { get; set; }
        public string[] Cc { get; set; }
        public Header Header { get; set; }
        public bool AllowNonTLS { get; set; } = false;
        public string ForceSecureNotification { get; set; }
        public Content Content { get; set; }
        public List<Attachment> Attachments { get; set; }
    }

    public class SendMessageResponse
    {
        public string SourceTrackingId { get; set; }
        public string Data { get; set; }
        public List<Error> Errors { get; set; }
    }
    #endregion  Classes for Send Message method

    #region  Classes for Get Email Disposition method
    public class GetEmailDispositionResponse
    {
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
        public string DeliveryStatus { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public string OpenedStatus { get; set; }
        public DateTime? OpenedTime { get; set; }
    }



    #endregion  Classes for Get Email Disposition method

    #region Classes For Dynamic Template 

    #region Requst
    public class DynamicTemplateRequest
    {
        public string TemplatePath { get; set; }
        public string TemplateName { get; set; }
    }
    #endregion

    #region Response

    public class CreateDynamicTemplateResponse
    {
        public string Message { get; set; }
        public Params Params { get; set; }
    }

    public class Params
    {
        public string Name { get; set; }
        public Body Body { get; set; }
    }

    public class Body
    {
        public string Tempfile { get; set; }
        public string Original_filename { get; set; }
        public string Content_type { get; set; }
        public string Headers { get; set; }
    }

    public class DynamicTemplateAllResponse
    {
        public int id { get; set; }
        public string name { get; set; }
        public int api_customer_id { get; set; }
    }

    public class DynamicTemplateResponse
    {
        public int id { get; set; }
        public string name { get; set; }
        public int api_customer_id { get; set; }
        public string body { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class DeleteDynamicTemplateResponse
    {
        public string message { get; set; }
    }


    #endregion

    #endregion

    #region Classes For Webhook_Endpoints

    #region Request

    public class WebhookEndpointRequest
    {
        public string target_url { get; set; }
        public string[] events { get; set; }
        public bool active { get; set; }
        public string signing_key { get; set; }
        public string api_key { get; set; }
    }

    #endregion

    #region Response

    public class WebhookEndpoint
    {
        public int id { get; set; }
        public string target_url { get; set; }
        public int api_customer_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string[] events { get; set; }
        public bool active { get; set; }
        public object signing_key { get; set; }
        public object api_key { get; set; }
    }


    public class WebhookEndpointResponse
    {
        public string message { get; set; }
        public WebhookEndpoint data { get; set; }
    }

    #endregion

    #endregion

    #region Common Classes
    public class Error
    {
        public int Code { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
    }
    #endregion Common Classes
}
