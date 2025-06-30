using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace Paubox
{
    public class EmailLibrary : IEmailLibrary
    {
        private readonly string _apiKey;
        private readonly string _apiBaseURL;
        private readonly IAPIHelper _apiHelper;

        /// <summary>
        /// Constructor for EmailLibrary with API credentials
        /// </summary>
        /// <param name="apiKey">Your Paubox API key</param>
        /// <param name="apiUser">Your Paubox username/domain</param>
        public EmailLibrary(string apiKey, string apiUser) : this(apiKey, apiUser, new APIHelper())
        {
        }

        /// <summary>
        /// Constructor for EmailLibrary with API credentials and custom API helper (useful for testing)
        /// </summary>
        /// <param name="apiKey">Your Paubox API key</param>
        /// <param name="apiUser">Your Paubox username/domain</param>
        /// <param name="apiHelper">Custom API helper implementation</param>
        public EmailLibrary(string apiKey, string apiUser, IAPIHelper apiHelper)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
            if (string.IsNullOrWhiteSpace(apiUser))
                throw new ArgumentException("API user cannot be null or empty", nameof(apiUser));

            _apiKey = apiKey;
            _apiBaseURL = $"https://api.paubox.net/v1/{apiUser}/";
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
        }

        /// <summary>
        /// Constructor for EmailLibrary with Configuration
        /// </summary>
        /// <param name="configuration">IConfiguration instance containing APIKey and APIUser</param>
        public EmailLibrary(IConfiguration configuration) : this(configuration, new APIHelper())
        {
        }

        /// <summary>
        /// Constructor for EmailLibrary with Configuration and custom API helper
        /// </summary>
        /// <param name="configuration">IConfiguration instance containing APIKey and APIUser</param>
        /// <param name="apiHelper">Custom API helper implementation</param>
        public EmailLibrary(IConfiguration configuration, IAPIHelper apiHelper)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var apiKey = configuration["APIKey"];
            var apiUser = configuration["APIUser"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("APIKey not found in configuration", nameof(configuration));
            if (string.IsNullOrWhiteSpace(apiUser))
                throw new ArgumentException("APIUser not found in configuration", nameof(configuration));

            _apiKey = apiKey;
            _apiBaseURL = $"https://api.paubox.net/v1/{apiUser}/";
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
        }

        /// <summary>
        /// Gets Email Delivery Information
        /// </summary>
        /// <param name="sourceTrackingId"></param>
        /// <returns>GetEmailDispositionResponse</returns>
        public GetEmailDispositionResponse GetEmailDisposition(string sourceTrackingId)
        {
            GetEmailDispositionResponse apiResponse = new GetEmailDispositionResponse();
            try
            {
                string requestURI = string.Format("message_receipt?sourceTrackingId={0}", sourceTrackingId);
                string Response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "GET");
                apiResponse = JsonConvert.DeserializeObject<GetEmailDispositionResponse>(Response);
                if (apiResponse.Data == null && apiResponse.SourceTrackingId == null && apiResponse.Errors == null)
                {
                    throw new SystemException(Response);
                }

                if (apiResponse.Errors != null && apiResponse.Errors.Count > 0)
                {
                    throw new SystemException(Response);
                }

                if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Message != null
                    && apiResponse.Data.Message.Message_Deliveries != null && apiResponse.Data.Message.Message_Deliveries.Count > 0)
                {
                    foreach (var message_deliveries in apiResponse.Data.Message.Message_Deliveries)
                    {
                        if (string.IsNullOrWhiteSpace(message_deliveries.Status.OpenedStatus))
                        {
                            message_deliveries.Status.OpenedStatus = "unopened";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return apiResponse;
        }

        /// <summary>
        /// Send Message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>SendMessageResponse</returns>
        public SendMessageResponse SendMessage(Message message)
        {
            SendMessageResponse apiResponse = new SendMessageResponse();
            try
            {
                //Prepare JSON request for passing it to Send Message API
                JObject requestObject = JObject.FromObject(new
                {
                    data = ConvertMessageObjectToJSON(message) // Convert i/p Message object to JSON , as per the API
                });

                string Response = _apiHelper.CallToAPI(_apiBaseURL, "messages", GetAuthorizationHeader(), "POST", JsonConvert.SerializeObject(requestObject));
                apiResponse = JsonConvert.DeserializeObject<SendMessageResponse>(Response);

                if (apiResponse.Data == null && apiResponse.SourceTrackingId == null && apiResponse.Errors == null)
                {
                    throw new SystemException(Response);
                }

                if (apiResponse.Errors != null && apiResponse.Errors.Count > 0)
                {
                    throw new SystemException(Response);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return apiResponse;
        }

        /// <summary>
        /// Gets Authorization Header
        /// </summary>
        /// <returns></returns>
        private string GetAuthorizationHeader()
        {
            return string.Format("Token token={0}", _apiKey);
        }

        /// <summary>
        /// Convert Message object to JSON , as required for the Send Message API
        /// </summary>
        /// <param name="message"></param>
        /// <returns>JObject</returns>
        private static JObject ConvertMessageObjectToJSON(Message message)
        {
            JObject attachmentJSON = new JObject();
            JArray attachmentJSONArray = null;
            JObject contentJSON = null;
            JObject headerJSON = null;
            JObject requestJSON = null;
            JObject MessageJSON = null;
            string base64EncodedHtmlText = null;

            if (message != null)
            {

                if (message.Header != null)
                {
                    headerJSON = JObject.FromObject(new
                        Dictionary<string, string>() {
                     { "subject" , message.Header.Subject},
                     { "from" , message.Header.From},
                     { "reply-to" , message.Header.ReplyTo}
                     });
                }
                else
                {
                    throw new ArgumentNullException("Message Header cannot be null.");
                }

                if (!string.IsNullOrWhiteSpace(message.Content.HtmlText)) // Converting the html text to a base 64 string
                {
                    byte[] htmlTextByteArray = System.Text.Encoding.UTF8.GetBytes(message.Content.HtmlText);
                    // convert the byte array to a Base64 string
                    base64EncodedHtmlText = Convert.ToBase64String(htmlTextByteArray);
                }

                if (message.Content != null)
                {
                    contentJSON = JObject.FromObject(new
                    Dictionary<string, string>() {
                         { "text/plain" , message.Content.PlainText},
                         { "text/html" , base64EncodedHtmlText}
                    });
                }
                else
                {
                    throw new ArgumentNullException("Message Content cannot be null.");
                }

                //If there are attachments, then prepare attachment array JSON
                if (message.Attachments != null && message.Attachments.Count > 0)
                {
                    attachmentJSONArray = new JArray();

                    foreach (var attachment in message.Attachments)
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

                MessageJSON = JObject.FromObject(new
                {
                    recipients = message.Recipients,
                    bcc = message.Bcc,
                    cc = message.Cc,
                    headers = headerJSON,
                    allowNonTLS = message.AllowNonTLS,
                    content = contentJSON,
                    attachments = attachmentJSONArray
                });

                bool? forceSecureNotificationValue = ReturnValidForceSecureNotificationValue(message.ForceSecureNotification);
                if (forceSecureNotificationValue != null) // Add forceSecureNotificationValue to Request, only if it is not null
                {
                    MessageJSON.Add("forceSecureNotification", forceSecureNotificationValue);
                }

                requestJSON = JObject.FromObject(new
                {
                    message = MessageJSON
                });
            }
            else
            {
                throw new ArgumentNullException("Message argument cannot be null.");
            }

            return requestJSON;
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

