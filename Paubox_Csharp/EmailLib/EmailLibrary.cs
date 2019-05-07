using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace EmailLib
{
    public class EmailLibrary
    {
        private static string APIKey = ConfigurationManager.AppSettings["APIKey"];
        private static string APIBaseURL = "https://api.paubox.net/v1/" + ConfigurationManager.AppSettings["APIUser"] + "/";

        /// <summary>
        /// Gets Email Delivery Information
        /// </summary>
        /// <param name="sourceTrackingId"></param>
        /// <returns>GetEmailDispositionResponse</returns>
        public static GetEmailDispositionResponse GetEmailDisposition(string sourceTrackingId)
        {
            GetEmailDispositionResponse apiResponse = new GetEmailDispositionResponse();
            try
            {
                string requestURI = string.Format("message_receipt?sourceTrackingId={0}", sourceTrackingId);
                string Response = APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "GET");
                apiResponse = JsonConvert.DeserializeObject<GetEmailDispositionResponse>(Response);
                if (apiResponse.Data == null && apiResponse.SourceTrackingId == null && apiResponse.Errors == null)
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
        public static SendMessageResponse SendMessage(Message message)
        {
            SendMessageResponse apiResponse = new SendMessageResponse();
            try
            {
                //Prepare JSON request for passing it to Send Message API
                JObject requestObject = JObject.FromObject(new
                {
                    data = ConvertMessageObjectToJSON(message) // Convert i/p Message object to JSON , as per the API
                });

                string Response = APIHelper.CallToAPI(APIBaseURL, "messages", GetAuthorizationHeader(), "POST", JsonConvert.SerializeObject(requestObject));
                apiResponse = JsonConvert.DeserializeObject<SendMessageResponse>(Response);
                if (apiResponse.Data == null && apiResponse.SourceTrackingId==null && apiResponse.Errors == null)
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
        private static string GetAuthorizationHeader()
        {
            return string.Format("Token token={0}", APIKey);
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

                if (message.Content != null)
                {
                    contentJSON = JObject.FromObject(new
                    Dictionary<string, string>() {
                         { "text/plain" , message.Content.PlainText},
                         { "text/html" , message.Content.HtmlText}
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

                JObject MessageJSON = JObject.FromObject(new
                {
                    recipients = message.Recipients,
                    bcc = message.Bcc,
                    headers = headerJSON,
                    allowNonTLS = message.AllowNonTLS,
                    forceSecureNotification = message.forceSecureNotification,
                    content = contentJSON,
                    attachments = attachmentJSONArray
                });

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
    }


}

