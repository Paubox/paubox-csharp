using System.Collections.Generic;

namespace Paubox
{
    public interface IFormsLibrary
    {
        Form GetForm(string formId);
        void SubmitForm(string formId, Dictionary<string, object> formData, FormAttachment[] attachments = null);
        FormsListResponse ListForms(FormsListParams parameters = null);
        CreateFormResponse CreateForm(CreateFormRequest request);
        Form GetFormById(string formId);
        UpdateFormResponse UpdateForm(string formId, UpdateFormRequest request);
        string ArchiveForm(string formId);
        string UnarchiveForm(string formId);
        Form CopyForm(string formId, string newTitle);
        FormStats GetFormStats(int? customerId = null);
        FormSubmissionListResponse ListFormSubmissions(string formId, SubmissionListParams parameters = null);
        string ExportSubmissionsCsv(string formId);
        string ExportSubmissionCsv(string formId, string submissionId);
        byte[] ExportSubmissionPdf(string formId, string submissionId);
    }
}
