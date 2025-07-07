using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Paubox
{
    public interface IEmailLibrary
    {
        SendMessageResponse SendMessage(Message message);
        GetEmailDispositionResponse GetEmailDisposition(string sourceTrackingId);
        SendBulkMessagesResponse SendBulkMessages(Message[] messages);
        List<DynamicTemplateSummary> ListDynamicTemplates();
        GetDynamicTemplateResponse GetDynamicTemplate(string templateId);
        DynamicTemplateResponse CreateDynamicTemplate(string templateName, string templatePath);
        DynamicTemplateResponse UpdateDynamicTemplate(string templateId, string templateName, string templatePath);
        DeleteDynamicTemplateResponse DeleteDynamicTemplate(string templateId);
    }
}
