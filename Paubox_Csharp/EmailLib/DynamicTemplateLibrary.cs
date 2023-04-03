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
    public class DynamicTemplateLibrary
    {
        private static string APIKey = ConfigurationManager.AppSettings["APIKey"];
        private static string APIBaseURL = "https://api.paubox.net/v1/" + ConfigurationManager.AppSettings["APIUser"] + "/";

        public async Task<CreateDynamicTemplateResponse> CreateDynamicTemplate(DynamicTemplateRequest templateRequest)
        {
            string requestURI = string.Format("dynamic_templates");
            string Response = await APIHelper.UploadTemplateAsync(APIBaseURL, requestURI, GetAuthorizationHeader(), templateRequest);

            return JsonConvert.DeserializeObject<CreateDynamicTemplateResponse>(Response);
        }

        public async Task<List<DynamicTemplateAllResponse>> GetAllDynamicTemplate()
        {
            string requestURI = string.Format("dynamic_templates");
            string Response = await APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "GET");

            return JsonConvert.DeserializeObject<List<DynamicTemplateAllResponse>>(Response);
        }

        public async Task<DynamicTemplateResponse> GetDynamicTemplate(int ID)
        {
            string requestURI = string.Format("dynamic_templates/" + ID);
            string Response = await APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "GET");

            return JsonConvert.DeserializeObject<DynamicTemplateResponse>(Response);
        }

        public async Task<DeleteDynamicTemplateResponse> DeleteDynamicTemplate(int ID)
        {
            string requestURI = string.Format("dynamic_templates/" + ID);
            string Response = await APIHelper.CallToAPI(APIBaseURL, requestURI, GetAuthorizationHeader(), "DELETE");

            return JsonConvert.DeserializeObject<DeleteDynamicTemplateResponse>(Response);
        }
        public async Task<CreateDynamicTemplateResponse> UpdateDynamicTemplate(int ID, DynamicTemplateRequest templateRequest)
        {
            string requestURI = string.Format("dynamic_templates/" + ID);
            string Response = await APIHelper.UploadTemplateAsync(APIBaseURL, requestURI, GetAuthorizationHeader(), templateRequest, true);

            return JsonConvert.DeserializeObject<CreateDynamicTemplateResponse>(Response);
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
