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
        // DeleteDynamicTemplateResponse DeleteDynamicTemplate(string templateId);
        CreateDynamicTemplateResponse CreateDynamicTemplate(string templateName, string templatePath);
        // UpdateDynamicTemplateResponse UpdateDynamicTemplate(string templateId, string templateName, string templatePath);
    }
}
