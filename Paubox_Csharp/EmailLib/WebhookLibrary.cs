using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace Paubox
{
    public class WebhookLibrary : IWebhookLibrary
    {
        private readonly string _apiKey;
        private readonly string _apiBaseURL;
        private readonly IAPIHelper _apiHelper;

        public WebhookLibrary(string apiKey) : this(apiKey, new APIHelper())
        {
        }

        public WebhookLibrary(string apiKey, IAPIHelper apiHelper)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

            _apiKey = apiKey;
            _apiBaseURL = "https://api.paubox.com/v1/email/";
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
        }

        public WebhookLibrary(IConfiguration configuration) : this(configuration, new APIHelper())
        {
        }

        public WebhookLibrary(IConfiguration configuration, IAPIHelper apiHelper)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var apiKey = configuration["APIKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("APIKey not found in configuration", nameof(configuration));

            _apiKey = apiKey;
            _apiBaseURL = "https://api.paubox.com/v1/email/";
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
        }

        public List<WebhookEndpoint> ListWebhookEndpoints()
        {
            string response = _apiHelper.CallToAPI(_apiBaseURL, "webhook_endpoints", GetAuthorizationHeader(), "GET");
            return JsonConvert.DeserializeObject<List<WebhookEndpoint>>(response);
        }

        public WebhookEndpointResponse CreateWebhookEndpoint(CreateWebhookEndpointRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.TargetUrl))
                throw new ArgumentException("TargetUrl cannot be null or empty", nameof(request));
            if (request.Events == null || request.Events.Count == 0)
                throw new ArgumentException("Events cannot be null or empty", nameof(request));

            string requestBody = JsonConvert.SerializeObject(request);
            string response = _apiHelper.CallToAPI(_apiBaseURL, "webhook_endpoints", GetAuthorizationHeader(), "POST", requestBody);
            return JsonConvert.DeserializeObject<WebhookEndpointResponse>(response);
        }

        public WebhookEndpointResponse GetWebhookEndpoint(int id)
        {
            string requestURI = string.Format("webhook_endpoints/{0}", id);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "GET");
            return JsonConvert.DeserializeObject<WebhookEndpointResponse>(response);
        }

        public WebhookEndpointResponse UpdateWebhookEndpoint(int id, UpdateWebhookEndpointRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string requestURI = string.Format("webhook_endpoints/{0}", id);
            string requestBody = JsonConvert.SerializeObject(request);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "PATCH", requestBody);
            return JsonConvert.DeserializeObject<WebhookEndpointResponse>(response);
        }

        public WebhookEndpointDeleteResponse DeleteWebhookEndpoint(int id)
        {
            string requestURI = string.Format("webhook_endpoints/{0}", id);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "DELETE");
            return JsonConvert.DeserializeObject<WebhookEndpointDeleteResponse>(response);
        }

        private string GetAuthorizationHeader()
        {
            return string.Format("Token token={0}", _apiKey);
        }
    }
}
