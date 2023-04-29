using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paubox
{
    public class WebhookLibrary
    {
        private static string APIKey = ConfigurationManager.AppSettings["APIKey"];
        private static string APIBaseURL = "https://api.paubox.net/v1/" + ConfigurationManager.AppSettings["APIUser"] + "/";

        public async Task<WebhookEndpointResponse> CreateWebhookEndpoint(WebhookEndpointRequest endpointRequest)
        {
            string requestURI = string.Format("webhook_endpoints");
            var req = JsonConvert.SerializeObject(endpointRequest);
            string Response = await APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "POST", req);

            return JsonConvert.DeserializeObject<WebhookEndpointResponse>(Response);
        }

        public async Task<List<WebhookEndpoint>> GetAllWebhookEndpoints()
        {
            string requestURI = string.Format("webhook_endpoints");
            string Response = await APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "GET");

            return JsonConvert.DeserializeObject<List<WebhookEndpoint>>(Response);
        }

        public async Task<WebhookEndpoint> GetWebhookEndpoint(int ID)
        {
            string requestURI = string.Format("webhook_endpoints/" + ID);
            string Response = await APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "GET");

            return JsonConvert.DeserializeObject<WebhookEndpoint>(Response);
        }

        public async Task<WebhookEndpointResponse> DeleteWebhookEndpoint(int ID)
        {
            string requestURI = string.Format("webhook_endpoints/" + ID);
            string Response = await APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "DELETE");

            return JsonConvert.DeserializeObject<WebhookEndpointResponse>(Response);
        }
        public async Task<WebhookEndpointResponse> UpdateWebhookEndpoint(int ID, WebhookEndpointRequest endpointRequest)
        {
            string requestURI = string.Format("webhook_endpoints/" + ID);
            var req = JsonConvert.SerializeObject(endpointRequest);
            string Response = await APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "PATCH", req);

            return JsonConvert.DeserializeObject<WebhookEndpointResponse>(Response);
        }


        /// <summary>
        /// Gets Authorization Header
        /// </summary>
        /// <returns></returns>
        private static string GetAuthorizationHeader()
        {
            return string.Format("Token token={0}", APIKey);
        }
    }
}
