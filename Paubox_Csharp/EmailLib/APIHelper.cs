using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Paubox
{
    internal class APIHelper : IAPIHelper
    {
        /// <summary>
        /// This method calls an API and returns API response
        /// </summary>
        /// <param name="BaseAPIUrl"></param>
        /// <param name="requestURI"></param>
        /// <param name="authHeader"></param>
        /// <param name="APIVerb"></param>
        /// <param name="requestBody"></param>
        /// <returns>apiResponse</returns>
        public string CallToAPI(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string requestBody = "")
        {
            Task<string> apiResponse = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAPIUrl);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(authHeader))
                {
                    client.DefaultRequestHeaders.Add("Authorization", authHeader);  // Adds Authorization Header
                }

                HttpResponseMessage response = new HttpResponseMessage();

                if (APIVerb == "GET")
                {
                    response = client.GetAsync(requestURI).Result;
                }
                else if (APIVerb == "POST")
                {
                    response = client.PostAsync(requestURI, new StringContent(requestBody, Encoding.UTF8, "application/json")).Result;
                }

                apiResponse = response.Content.ReadAsStringAsync();
            }

            return apiResponse.Result.ToString();
        }

        public string UploadTemplate(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string templateName, string templatePath)
        {
            throw new NotImplementedException();
        }
    }
}
