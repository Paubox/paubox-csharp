using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Paubox
{
    public interface IAPIHelper
    {
        string CallToAPI(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string requestBody = "");
        string UploadTemplate(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string templateName, string templatePath);
    }
}
