using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace EmailLib
{
    public class EmailLibrary
    {
        public static string APIURL = "https://api.paubox.net/v1/";
        public static string APIKey = ConfigurationManager.AppSettings["APIKey"].ToString();
        public static string APIUser = ConfigurationManager.AppSettings["APIUser"].ToString();
        public static string APIBaseURL = APIURL + APIUser + "/";

        public static GetEmailDispositionResponse GetEmailDisposition(string sourceTrackingId)
        {
            GetEmailDispositionResponse apiResponse = new GetEmailDispositionResponse();
            try
            {
                string requestURI = string.Format("message_receipt?sourceTrackingId={0}", sourceTrackingId);
                string Response = APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "GET");
                apiResponse = JsonConvert.DeserializeObject<GetEmailDispositionResponse>(Response);

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

                //throw;
            }
            return apiResponse;
        }

        public static SendMessageResponse SendMessage(Message msg)
        {
            SendMessageResponse apiResponse = new SendMessageResponse();
            try
            {
                //Prepare JSON request for passing it to Send Message API
                JObject requestObject = JObject.FromObject(new
                {
                    data = ConvertMessageObjectToJSON(msg) // Convert i/p Message object to JSON , as per the API
                });

                string Response = APIHelper.CallToAPI(APIBaseURL, "messages", GetAuthorizationHeader(), "POST", JsonConvert.SerializeObject(requestObject));
                apiResponse = JsonConvert.DeserializeObject<SendMessageResponse>(Response);
            }
            catch (Exception ex)
            {

                //throw;
            }
            return apiResponse;

        }

        private static string GetAuthorizationHeader()
        {
            return string.Format("Token token={0}", APIKey);
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


    }


}

