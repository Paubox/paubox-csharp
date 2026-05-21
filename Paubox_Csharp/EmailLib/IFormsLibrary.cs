using System.Collections.Generic;

namespace Paubox
{
    public interface IFormsLibrary
    {
        Form GetForm(string formId);
        void SubmitForm(string formId, Dictionary<string, object> formData, FormAttachment[] attachments = null);
    }
}
