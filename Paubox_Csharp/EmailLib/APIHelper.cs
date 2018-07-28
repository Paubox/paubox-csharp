using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EmailLib
{
    public class APIHelper
    {

        public static string CallToAPI(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string requestBody = "")
        {
            Task<string> apiResponse = null;

            using (var client = new HttpClient())
            {                                
                client.BaseAddress = new Uri(BaseAPIUrl);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(authHeader))
                {
                    client.DefaultRequestHeaders.Add("Authorization", authHeader);
                }

                HttpResponseMessage response = new HttpResponseMessage();
                
                if (APIVerb == "GET")
                {
                    response = client.GetAsync(requestURI).Result;  // Blocking call!
                }
                else if (APIVerb == "POST")
                {
                    response = client.PostAsync(requestURI, new StringContent(requestBody, Encoding.UTF8, "application/json")).Result;
                }

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    apiResponse = response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    apiResponse = null;
                    return null;
                }
            }
            return apiResponse.Result.ToString();
        }
    }
}
