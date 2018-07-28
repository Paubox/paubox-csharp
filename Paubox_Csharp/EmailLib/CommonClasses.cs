using System;
using System.Collections.Generic;

namespace EmailLib
{
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
        public Header Header { get; set; }
        public bool AllowNonTLS { get; set; } = false;
        public Content Content { get; set; }
        public List<Attachment> Attachments { get; set; }
    }

    public class SendMessageResponse
    {
        public string SourceTrackingId { get; set; }
        public string Data { get; set; }
        public List<Error> Errors { get; set; }
    }

    //Classes for Get Email Disposition API
    public class GetEmailDispositionResponse
    {
        public string SourceTrackingId { get; set; }
        public SubData Data { get; set; }
        public List<Error> Errors { get; set; }
    }

    public class SubData
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
        public DateTime DeliveryTime { get; set; }
        public string OpenedStatus { get; set; }        
        public DateTime OpenedTime { get; set; }
    }

    //Common Classes
    public class Error
    {
        public int Code { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
    }
}
