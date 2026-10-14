using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Paubox
{
    /// <inheritdoc cref="IWebhookLibrary"/>
    public class WebhookLibrary : IWebhookLibrary
    {
        /// <summary>
        /// The public gateway exposes only <c>/v1/webhooks/endpoints</c> and
        /// rewrites it onto the service's own <c>/v1/endpoints</c>, so the
        /// producers' event-ingest route is unreachable from here. The bare
        /// base is deliberately unrouted; every request carries the
        /// <c>endpoints</c> resource.
        /// </summary>
        internal const string DefaultWebhooksBaseUrl = "https://api.paubox.com/v1/webhooks/";

        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly IAPIHelper _apiHelper;

        /// <summary>
        /// Library-scoped so a consuming app's <c>JsonConvert.DefaultSettings</c>
        /// cannot change how the SDK serializes.
        /// </summary>
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings();

        public WebhookLibrary(string apiKey) : this(new APIHelper(), apiKey, DefaultWebhooksBaseUrl)
        {
        }

        public WebhookLibrary(IAPIHelper apiHelper, string apiKey)
            : this(apiHelper, apiKey, DefaultWebhooksBaseUrl)
        {
        }

        public WebhookLibrary(IAPIHelper apiHelper, string apiKey, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
            _apiKey = apiKey;
            _baseUrl = NormalizeBaseUrl(baseUrl);
        }

        /// <summary>
        /// Reads <c>WebhooksAPIKey</c> and (optionally) <c>WebhooksBaseURL</c>
        /// from configuration.
        /// </summary>
        public WebhookLibrary(IConfiguration configuration) : this(configuration, new APIHelper())
        {
        }

        public WebhookLibrary(IConfiguration configuration, IAPIHelper apiHelper)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var apiKey = configuration["WebhooksAPIKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException(
                    "WebhooksAPIKey not found in configuration", nameof(configuration));

            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
            _apiKey = apiKey;
            _baseUrl = NormalizeBaseUrl(configuration["WebhooksBaseURL"]);
        }

        /// <inheritdoc/>
        public WebhookEndpointList ListWebhookEndpoints(ListWebhookEndpointsParams parameters = null)
        {
            string requestURI = "endpoints" + BuildQuery(parameters);
            string response = _apiHelper.CallToAPI(_baseUrl, requestURI, AuthHeader(), "GET");
            return Deserialize<WebhookEndpointList>(response, "GET", requestURI);
        }

        /// <inheritdoc/>
        public CreatedWebhookEndpoint CreateWebhookEndpoint(CreateWebhookEndpointRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.TargetUrl))
                throw new ArgumentException("TargetUrl cannot be null or empty", nameof(request));
            if (request.Events == null || request.Events.Count == 0)
                throw new ArgumentException("Events cannot be null or empty", nameof(request));

            string requestBody = JsonConvert.SerializeObject(request, JsonSettings);
            string response = _apiHelper.CallToAPI(_baseUrl, "endpoints", AuthHeader(), "POST", requestBody);
            return Unwrap<CreatedWebhookEndpoint>(response, "POST", "endpoints");
        }

        /// <inheritdoc/>
        public WebhookEndpoint GetWebhookEndpoint(string id)
        {
            ValidateUuid(id, nameof(id));

            string requestURI = "endpoints/" + id;
            string response = _apiHelper.CallToAPI(_baseUrl, requestURI, AuthHeader(), "GET");
            return Unwrap<WebhookEndpoint>(response, "GET", requestURI);
        }

        /// <inheritdoc/>
        public WebhookEndpoint UpdateWebhookEndpoint(string id, UpdateWebhookEndpointRequest request)
        {
            ValidateUuid(id, nameof(id));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string requestURI = "endpoints/" + id;
            string requestBody = JsonConvert.SerializeObject(request, JsonSettings);
            string response = _apiHelper.CallToAPI(_baseUrl, requestURI, AuthHeader(), "PATCH", requestBody);
            return Unwrap<WebhookEndpoint>(response, "PATCH", requestURI);
        }

        /// <inheritdoc/>
        public void DeleteWebhookEndpoint(string id)
        {
            ValidateUuid(id, nameof(id));

            // 204 with no body, so there is nothing to deserialize. A non-2xx
            // has already thrown PauboxApiException inside the helper.
            _apiHelper.CallToAPI(_baseUrl, "endpoints/" + id, AuthHeader(), "DELETE");
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return DefaultWebhooksBaseUrl;
            return baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        }

        /// <summary>
        /// Bearer, not the Email API's <c>Token token=</c>. The SDK cannot
        /// validate the key's scopes; the server returns 401/403.
        /// </summary>
        private string AuthHeader()
        {
            return "Bearer " + _apiKey;
        }

        /// <summary>
        /// Rejects an id that is not a UUID before it reaches a URL. Without
        /// this a value containing <c>..</c> or <c>/</c> would change which
        /// endpoint is called, and the Authorization header goes on the same
        /// host, so the key would ride along on the retargeted request.
        /// </summary>
        private static void ValidateUuid(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(paramName + " cannot be null or empty", paramName);
            if (!Guid.TryParse(value, out _))
                throw new ArgumentException(paramName + " must be a valid UUID; got: " + value, paramName);
        }

        private static string BuildQuery(ListWebhookEndpointsParams parameters)
        {
            if (parameters == null)
                return string.Empty;

            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
            if (parameters.Page.HasValue)
                query["page"] = parameters.Page.Value.ToString();
            if (parameters.Items.HasValue)
                query["items"] = parameters.Items.Value.ToString();

            string encoded = query.ToString();
            return string.IsNullOrEmpty(encoded) ? string.Empty : "?" + encoded;
        }

        /// <summary>
        /// Single resources arrive wrapped as <c>{"data": {...}}</c>; unwrapped
        /// so callers get the endpoint rather than an envelope carrying nothing
        /// else. <c>ListWebhookEndpoints</c> deliberately keeps its envelope —
        /// the <c>page_info</c> beside it is what a caller paginating needs.
        /// </summary>
        private static T Unwrap<T>(string response, string verb, string endpoint) where T : class
        {
            var envelope = Deserialize<WebhookDataEnvelope<T>>(response, verb, endpoint);
            if (envelope.Data == null)
                throw new PauboxApiException(200, verb, endpoint, response);
            return envelope.Data;
        }

        private static T Deserialize<T>(string response, string verb, string endpoint) where T : class
        {
            T parsed = JsonConvert.DeserializeObject<T>(response, JsonSettings);
            if (parsed == null)
                throw new PauboxApiException(200, verb, endpoint, response);
            return parsed;
        }
    }
}
