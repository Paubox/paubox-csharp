using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace EmailLib
{
    public class EmailLibrary
    {
        public static string APIBaseURL = "https://api.paubox.net/v1/undefeatedgames/";

        public static void SendMessage(Message msg)
        {        
            try
            {                
                //Prepare JSON request for passing it to Send Message API
                JObject requestObject = JObject.FromObject(new
                {
                    data = ConvertMessageObjectToJSON(msg) // Convert i/p Message object to JSON , as per the API
                });
                
                APIHelper.CallToAPI(APIBaseURL, "messages", "Token token=6f7c0110a47f82e7bff933f68cc8d7ec", "POST",JsonConvert.SerializeObject(requestObject));
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Convert Message object to JSON , as required for the Send Message API
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>JObject</returns>
        private static JObject ConvertMessageObjectToJSON(Message msg)
        {
            JObject attachmentJSON = new JObject();
            JArray attachmentJSONArray = new JArray();

            JObject headerJSON = JObject.FromObject(new
                Dictionary<string, string>() {
                     { "subject" , msg.Header.Subject},
                     { "from" , msg.Header.From},
                     { "reply-to" , msg.Header.ReplyTo}
                });

            JObject contentJSON = JObject.FromObject(new
            Dictionary<string, string>() {
                     { "text/plain" , msg.Content.PlainText},
                     { "text/html" , msg.Content.HtmlText}
                });


            //If there are attachments, then prepare attachment array JSON
            if (msg.Attachments != null && msg.Attachments.Count > 0)
            {
                foreach (var attachment in msg.Attachments)
                {
                    attachmentJSON = JObject.FromObject(new
                    {
                        fileName = attachment.FileName,
                        contentType = attachment.ContentType,
                        content = attachment.Content
                    });
                    attachmentJSONArray.Add(attachmentJSON);
                }
            }
            else
            {
                attachmentJSONArray = null;
            }

            JObject MessageJSON = JObject.FromObject(new
            {
                recipients = msg.Recipients,
                bcc = msg.Bcc,
                headers = headerJSON,
                allowNonTLS = msg.AllowNonTLS,
                content = contentJSON,
                attachments = attachmentJSONArray
            });

            JObject requestJSON = JObject.FromObject(new
            {
                message = MessageJSON
            });
            return requestJSON;
        }

        public static void GetEmailDisposition(string sourceTrackingId)
        {
            try
            {                
                string requestURI = string.Format("message_receipt?sourceTrackingId={0}", sourceTrackingId);
                APIHelper.CallToAPI(APIBaseURL, requestURI, "Token token=6f7c0110a47f82e7bff933f68cc8d7ec", "GET");
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
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

}

