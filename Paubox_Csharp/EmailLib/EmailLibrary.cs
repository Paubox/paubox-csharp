﻿using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Linq;

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
        /// (used for dependency injection in unit tests)
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
                    data = new Dictionary<string, object> {
                        ["message"] = message.ToJson()
                    }
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
        /// Send Bulk Messages
        /// </summary>
        /// <param name="messages">Array of Message objects</param>
        /// <returns>SendBulkMessagesResponse</returns>
        public SendBulkMessagesResponse SendBulkMessages(Message[] messages)
        {
            SendBulkMessagesResponse apiResponse = new SendBulkMessagesResponse();
            try
            {
                // Convert each message to JSON using LINQ Select (map function)
                JObject requestObject = JObject.FromObject(new
                {
                    data = new Dictionary<string, object>
                    {
                        { "messages", messages.Select(message => message.ToJson()).ToList() }
                    }
                });

                string Response = _apiHelper.CallToAPI(_apiBaseURL, "bulk_messages", GetAuthorizationHeader(), "POST", JsonConvert.SerializeObject(requestObject));
                apiResponse = JsonConvert.DeserializeObject<SendBulkMessagesResponse>(Response);

                if (apiResponse.Messages == null)
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
    }
}

