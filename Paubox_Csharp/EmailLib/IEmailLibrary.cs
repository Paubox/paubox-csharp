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
        GetDynamicTemplateResponse GetDynamicTemplate(int templateId);
        DynamicTemplateResponse CreateDynamicTemplate(string templateName, string templatePath);
        DynamicTemplateResponse UpdateDynamicTemplate(int templateId, string templateName, string templatePath);
        DeleteDynamicTemplateResponse DeleteDynamicTemplate(int templateId);
    }
}
