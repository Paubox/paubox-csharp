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
    internal class APIHelper
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
        public async static Task<string> CallToAPI(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string requestBody = "")
        {
            string apiResponse = string.Empty;

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
                    response = await client.GetAsync(requestURI);
                }
                else if (APIVerb == "POST")
                {
                    response = await client.PostAsync(requestURI, new StringContent(requestBody, Encoding.UTF8, "application/json"));
                }
                else if (APIVerb == "PATCH")
                {
                    response = await client.PutAsync(requestURI, new StringContent(requestBody, Encoding.UTF8, "application/json"));
                }
                else if (APIVerb == "DELETE")
                {
                    response = await client.DeleteAsync(requestURI);
                }

                apiResponse = await response.Content.ReadAsStringAsync();
            }

            return apiResponse;
        }

        public async static Task<string> UploadTemplateAsync(string BaseAPIUrl, string requestURI, string authHeader, DynamicTemplateRequest templateData, bool isUpdateRequest = false)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAPIUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(authHeader))
                {
                    client.DefaultRequestHeaders.Add("Authorization", authHeader);  // Adds Authorization Header
                }

                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(templateData.TemplatePath));
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "data[body]",
                        FileName = Path.GetFileName(templateData.TemplatePath)
                    };
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypes.MimeTypeMap.GetMimeType(Path.GetFileName(templateData.TemplatePath)));
                    content.Add(fileContent);

                    content.Add(new StringContent(templateData.TemplateName), "data[name]");

                    HttpResponseMessage response = null;

                    if (isUpdateRequest)
                    {
                        response = await client.PutAsync(requestURI, content);
                    }
                    else
                    {
                        response = await client.PostAsync(requestURI, content);
                    }

                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
