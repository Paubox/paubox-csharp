using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paubox
{
    public class FormsLibrary : IFormsLibrary
    {
        private const string FormsBaseUrl = "https://apx.paubox.com/forms/";
        private readonly IAPIHelper _apiHelper;

        public FormsLibrary() : this(new APIHelper())
        {
        }

        public FormsLibrary(IAPIHelper apiHelper)
        {
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
        }

        public Form GetForm(string formId)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                $"public/form_data/{formId}", null, "GET");

            Form form = JsonConvert.DeserializeObject<Form>(response);
            if (form == null || form.Id == null)
                throw new SystemException(response);

            return form;
        }

        public void SubmitForm(string formId, Dictionary<string, object> formData,
                               FormAttachment[] attachments = null)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));
            if (formData == null)
                throw new ArgumentNullException(nameof(formData));

            var body = new Dictionary<string, object> { ["form_data"] = formData };
            if (attachments != null && attachments.Length > 0)
                body["attachments"] = attachments;

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                $"api/forms/{formId}/submissions", null, "POST",
                JsonConvert.SerializeObject(body));

            if (!string.IsNullOrWhiteSpace(response))
                throw new SystemException(response);
        }
    }
}
