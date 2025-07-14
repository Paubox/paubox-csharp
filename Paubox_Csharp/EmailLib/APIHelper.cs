using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
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
                else if (APIVerb == "DELETE")
                {
                    response = client.DeleteAsync(requestURI).Result;
                }
                else if (APIVerb == "PATCH")
                {
                    response = client.PatchAsync(requestURI, new StringContent(requestBody, Encoding.UTF8, "application/json")).Result;
                }
                else
                {
                    throw new ArgumentException("Invalid API verb: " + APIVerb);
                }

                apiResponse = response.Content.ReadAsStringAsync();
            }

            return apiResponse.Result.ToString();
        }

        public string UploadTemplate(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string templateName, string templatePath)
        {
            Task<string> apiResponse = null;
            string originalFilename = Path.GetFileName(templatePath);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAPIUrl);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(authHeader))
                {
                    client.DefaultRequestHeaders.Add("Authorization", authHeader);  // Adds Authorization Header
                }

                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(templatePath));
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "data[body]",
                        FileName = originalFilename
                    };

                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/x-handlebars-template");
                    content.Add(fileContent);
                    content.Add(new StringContent(templateName), "data[name]");

                    HttpResponseMessage response = null;

                    if (APIVerb == "PATCH")
                    {
                        response = client.PatchAsync(requestURI, content).Result;
                    }
                    else if (APIVerb == "POST")
                    {
                        response = client.PostAsync(requestURI, content).Result;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid API verb: " + APIVerb);
                    }

                    apiResponse = response.Content.ReadAsStringAsync();
                }

                return apiResponse.Result.ToString();
            }
        }
    }
}
