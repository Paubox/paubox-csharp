using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Paubox
{
    public class Message : BaseMessage
    {
        public Content Content { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (this.Content == null) {
                throw new ArgumentNullException("Content cannot be null.");
            }
        }

        /// <summary>
        /// Convert the Message object to a JSON object, including the Content
        /// </summary>
        /// <returns>JObject</returns>
        public override JObject ToJson()
        {
            JObject messageJSON = base.ToJson();

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

            messageJSON.Add("content", contentJSON);

            return messageJSON;
        }
    }
}
