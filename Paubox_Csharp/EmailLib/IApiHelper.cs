using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Paubox
{
    public interface IAPIHelper
    {
        string CallToAPI(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string requestBody = "");
    }

    public interface IEmailLibrary
    {
        // Instance methods for the new SDK design
        SendMessageResponse SendMessage(Message message);
        GetEmailDispositionResponse GetEmailDisposition(string sourceTrackingId);
        SendBulkMessagesResponse SendBulkMessages(Message[] messages);
    }
}
