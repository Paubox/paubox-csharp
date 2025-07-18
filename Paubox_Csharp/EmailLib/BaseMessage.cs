using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Paubox
{
    /// <summary>
    /// Base class for all message types
    /// </summary>
    /// <remarks>
    /// Inherited by:
    ///     1. Message
    ///     2. TemplatedMessage
    /// </remarks>
    public class BaseMessage
    {
        public string[] Recipients { get; set; }
        public string[] Bcc { get; set; }
        public string[] Cc { get; set; }
        public Header Header { get; set; }
        public bool AllowNonTLS { get; set; } = false;

        private bool? _forceSecureNotificationValue;
        private string _forceSecureNotification;
        public string ForceSecureNotification
        {
            get => _forceSecureNotification;
            set
            {
                _forceSecureNotification = value;
                _forceSecureNotificationValue = string.IsNullOrWhiteSpace(value)
                    ? null
                    : value.Trim().ToLower() switch
                    {
                        "true" => true,
                        "false" => false,
                        _ => null
                    };
            }
        }

        public List<Attachment> Attachments { get; set; }

        /// <summary>
        /// Validate the message object
        /// </summary>
        /// <remarks>
        /// This method throws an exception if the message object is invalid. This method can be overridden by the child
        /// classes to add additional validation specific to that message type.
        /// </remarks>
        public virtual void Validate()
        {
            if (this.Header == null) {
                throw new ArgumentNullException("Header cannot be null.");
            }
        }

        /// <summary>
        /// Convert the message object to a JSON object
        /// </summary>
        /// <returns>JObject</returns>
        public virtual JObject ToJson()
        {
            this.Validate();

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

            // If there are attachments, then prepare attachment array JSON
            JArray attachmentJSONArray = null;
            if (this.Attachments != null && this.Attachments.Count > 0)
            {
                attachmentJSONArray = new JArray();
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
                attachments = attachmentJSONArray
            });

            if (_forceSecureNotificationValue.HasValue) // Add forceSecureNotificationValue to Request, only if it is not null
            {
                MessageJSON.Add("forceSecureNotification", _forceSecureNotificationValue.Value);
            }

            return MessageJSON;
        }
    }
}
