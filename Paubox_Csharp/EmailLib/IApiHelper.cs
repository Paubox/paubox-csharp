
namespace Paubox
{
    public interface IAPIHelper
    {
        string CallToAPI(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string requestBody = "");
        byte[] CallToAPIBytes(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb);
        string UploadTemplate(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string templateName, string templatePath);
    }
}
