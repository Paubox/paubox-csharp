using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Paubox
{
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

        /// <summary>
        /// Convert the Message object to a JSON object
        /// </summary>
        /// <returns>JObject</returns>
        public JObject ToJson()
        {
            if (this.Header == null) {
                throw new ArgumentNullException("Message Header cannot be null.");
            }
            if (this.Content == null) {
                throw new ArgumentNullException("Message Content cannot be null.");
            }

            JObject headerJSON = JObject.FromObject(new Dictionary<string, string>() {
                { "subject" , this.Header.Subject},
                { "from" , this.Header.From},
                { "reply-to" , this.Header.ReplyTo}
            });

            if (this.Header.CustomHeaders != null && this.Header.CustomHeaders.Count > 0)
            {
                foreach (var header in this.Header.CustomHeaders)
                {
                    headerJSON.Add(header.Key, header.Value);
                }
            }

            // Converting the html text to a base 64 string
            string base64EncodedHtmlText = null;
            if (!string.IsNullOrWhiteSpace(this.Content.HtmlText))
            {
                byte[] htmlTextByteArray = System.Text.Encoding.UTF8.GetBytes(this.Content.HtmlText);
                base64EncodedHtmlText = Convert.ToBase64String(htmlTextByteArray);
            }

            JObject contentJSON = JObject.FromObject(new
            Dictionary<string, string>() {
                { "text/plain" , this.Content.PlainText},
                { "text/html" , base64EncodedHtmlText}
            });

            //If there are attachments, then prepare attachment array JSON
            JArray attachmentJSONArray = null;
            if (this.Attachments != null && this.Attachments.Count > 0)
            {
                foreach (var attachment in this.Attachments)
                {
                    JObject attachmentJSON = JObject.FromObject(new
                    {
                        fileName = attachment.FileName,
                        contentType = attachment.ContentType,
                        content = attachment.Content
                    });
                    attachmentJSONArray.Add(attachmentJSON);
                }
            }

            JObject MessageJSON = JObject.FromObject(new
            {
                recipients = this.Recipients,
                bcc = this.Bcc,
                cc = this.Cc,
                headers = headerJSON,
                allowNonTLS = this.AllowNonTLS,
                content = contentJSON,
                attachments = attachmentJSONArray
            });

            bool? forceSecureNotificationValue = ReturnValidForceSecureNotificationValue(this.ForceSecureNotification);
            if (forceSecureNotificationValue != null) // Add forceSecureNotificationValue to Request, only if it is not null
            {
                MessageJSON.Add("forceSecureNotification", forceSecureNotificationValue);
            }

            return MessageJSON;
        }

        /// <summary>
        /// Returns valid nullable bool ForceSecureNotification value
        /// </summary>
        /// <param name="forceSecureNotification"></param>
        /// <returns></returns>
        private static bool? ReturnValidForceSecureNotificationValue(string forceSecureNotification)
        {
            string forceSecureNotificationValue = null;
            if (string.IsNullOrWhiteSpace(forceSecureNotification))
            {
                return null;
            }
            else
            {
                forceSecureNotificationValue = forceSecureNotification.Trim().ToLower();
                if (forceSecureNotificationValue.Equals("true"))
                {
                    return true;
                }
                else if (forceSecureNotificationValue.Equals("false"))
                {
                    return false;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
