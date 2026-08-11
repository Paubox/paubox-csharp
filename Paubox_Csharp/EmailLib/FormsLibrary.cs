using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paubox
{
    public class FormsLibrary : IFormsLibrary
    {
        private const string FormsBaseUrl = "https://apx.paubox.com/forms/";
        private readonly IAPIHelper _apiHelper;
        private readonly string _apiKey;

        public FormsLibrary() : this(new APIHelper())
        {
        }

        public FormsLibrary(string apiKey) : this(new APIHelper(), apiKey)
        {
        }

        public FormsLibrary(IAPIHelper apiHelper)
        {
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
        }

        public FormsLibrary(IAPIHelper apiHelper, string apiKey) : this(apiHelper)
        {
            _apiKey = apiKey;
        }

        /// <summary>
        /// Builds the Authorization header for authenticated Forms API endpoints,
        /// or throws when no API key was provided to the constructor.
        /// </summary>
        private string RequireAuthHeader()
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException(
                    "This endpoint requires authentication. Construct FormsLibrary with a scoped API key " +
                    "that has the 'forms' scope, e.g. new FormsLibrary(apiKey).");

            return "Bearer " + _apiKey;
        }

        private static string BuildQueryString(List<KeyValuePair<string, string>> queryParams)
        {
            if (queryParams == null || queryParams.Count == 0)
                return string.Empty;

            var parts = new List<string>();
            foreach (var pair in queryParams)
            {
                parts.Add(pair.Key + "=" + Uri.EscapeDataString(pair.Value));
            }
            return "?" + string.Join("&", parts);
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

        public FormsListResponse ListForms(FormsListParams parameters = null)
        {
            string authHeader = RequireAuthHeader();

            var queryParams = new List<KeyValuePair<string, string>>();
            if (parameters != null)
            {
                if (parameters.CustomerId.HasValue)
                    queryParams.Add(new KeyValuePair<string, string>("customer_id", parameters.CustomerId.Value.ToString()));
                if (!string.IsNullOrWhiteSpace(parameters.FormId))
                    queryParams.Add(new KeyValuePair<string, string>("form_id", parameters.FormId));
                if (!string.IsNullOrWhiteSpace(parameters.Search))
                    queryParams.Add(new KeyValuePair<string, string>("search", parameters.Search));
                if (!string.IsNullOrWhiteSpace(parameters.Order))
                    queryParams.Add(new KeyValuePair<string, string>("order", parameters.Order));
                if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
                    queryParams.Add(new KeyValuePair<string, string>("order_by", parameters.OrderBy));
                if (parameters.Archived.HasValue)
                    queryParams.Add(new KeyValuePair<string, string>("archived", parameters.Archived.Value ? "true" : "false"));
                if (parameters.Active.HasValue)
                    queryParams.Add(new KeyValuePair<string, string>("active", parameters.Active.Value ? "true" : "false"));
                if (parameters.Page.HasValue)
                    queryParams.Add(new KeyValuePair<string, string>("page", parameters.Page.Value.ToString()));
                if (parameters.Items.HasValue)
                    queryParams.Add(new KeyValuePair<string, string>("items", parameters.Items.Value.ToString()));
            }

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                "api/forms" + BuildQueryString(queryParams), authHeader, "GET");

            FormsListResponse result = JsonConvert.DeserializeObject<FormsListResponse>(response);
            if (result == null || result.Results == null)
                throw new SystemException(response);

            return result;
        }

        public CreateFormResponse CreateForm(CreateFormRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Title cannot be null or empty", nameof(request));
            if (request.FormJson == null)
                throw new ArgumentException("FormJson cannot be null", nameof(request));

            string authHeader = RequireAuthHeader();

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                "api/forms", authHeader, "POST",
                JsonConvert.SerializeObject(request));

            CreateFormResponse result = JsonConvert.DeserializeObject<CreateFormResponse>(response);
            if (result == null || result.Id == null)
                throw new SystemException(response);

            return result;
        }

        public Form GetFormById(string formId)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));

            string authHeader = RequireAuthHeader();

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                $"api/forms/{formId}", authHeader, "GET");

            JObject envelope = null;
            try
            {
                envelope = JsonConvert.DeserializeObject<JObject>(response);
            }
            catch (JsonException)
            {
                throw new SystemException(response);
            }

            JToken data = envelope?["data"];
            Form form = data?.ToObject<Form>();
            if (form == null || form.Id == null)
                throw new SystemException(response);

            return form;
        }

        public UpdateFormResponse UpdateForm(string formId, UpdateFormRequest request)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string authHeader = RequireAuthHeader();

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                $"api/forms/{formId}", authHeader, "PUT",
                JsonConvert.SerializeObject(request));

            UpdateFormResponse result = JsonConvert.DeserializeObject<UpdateFormResponse>(response);
            if (result == null || result.Detail == null)
                throw new SystemException(response);

            return result;
        }

        public string ArchiveForm(string formId)
        {
            return ArchiveAction(formId, "archive");
        }

        public string UnarchiveForm(string formId)
        {
            return ArchiveAction(formId, "unarchive");
        }

        private string ArchiveAction(string formId, string action)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));

            string authHeader = RequireAuthHeader();

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                $"api/forms/{formId}/{action}", authHeader, "POST");

            UpdateFormResponse result = JsonConvert.DeserializeObject<UpdateFormResponse>(response);
            if (result == null || result.Detail == null)
                throw new SystemException(response);

            return result.Detail;
        }

        public Form CopyForm(string formId, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title cannot be null or empty", nameof(newTitle));

            string authHeader = RequireAuthHeader();

            var body = new Dictionary<string, object>
            {
                ["form_id"] = formId,
                ["title"] = newTitle
            };

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                "api/forms/copy", authHeader, "POST",
                JsonConvert.SerializeObject(body));

            Form form = JsonConvert.DeserializeObject<Form>(response);
            if (form == null || form.Id == null)
                throw new SystemException(response);

            return form;
        }

        public FormStats GetFormStats(int? customerId = null)
        {
            string authHeader = RequireAuthHeader();

            string requestUri = "api/forms/stats";
            if (customerId.HasValue)
                requestUri += "?customer_id=" + Uri.EscapeDataString(customerId.Value.ToString());

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                requestUri, authHeader, "GET");

            FormStats stats = JsonConvert.DeserializeObject<FormStats>(response);
            if (stats == null)
                throw new SystemException(response);

            return stats;
        }

        public FormSubmissionListResponse ListFormSubmissions(string formId, SubmissionListParams parameters = null)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));

            string authHeader = RequireAuthHeader();

            var queryParams = new List<KeyValuePair<string, string>>();
            if (parameters != null)
            {
                if (!string.IsNullOrWhiteSpace(parameters.SubmissionId))
                    queryParams.Add(new KeyValuePair<string, string>("submission_id", parameters.SubmissionId));
                if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
                    queryParams.Add(new KeyValuePair<string, string>("order_by", parameters.OrderBy));
                if (!string.IsNullOrWhiteSpace(parameters.Order))
                    queryParams.Add(new KeyValuePair<string, string>("order", parameters.Order));
                if (parameters.Page.HasValue)
                    queryParams.Add(new KeyValuePair<string, string>("page", parameters.Page.Value.ToString()));
                if (parameters.Items.HasValue)
                    queryParams.Add(new KeyValuePair<string, string>("items", parameters.Items.Value.ToString()));
            }

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                $"api/forms/{formId}/submissions" + BuildQueryString(queryParams), authHeader, "GET");

            FormSubmissionListResponse result = JsonConvert.DeserializeObject<FormSubmissionListResponse>(response);
            if (result == null || result.Data == null)
                throw new SystemException(response);

            return result;
        }

        public string ExportSubmissionsCsv(string formId)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));

            string authHeader = RequireAuthHeader();

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                $"api/forms/{formId}/submissions/submission-csv", authHeader, "GET");

            if (string.IsNullOrWhiteSpace(response))
                throw new SystemException(response);

            return response;
        }

        public string ExportSubmissionCsv(string formId, string submissionId)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));
            if (string.IsNullOrWhiteSpace(submissionId))
                throw new ArgumentException("Submission ID cannot be null or empty", nameof(submissionId));

            string authHeader = RequireAuthHeader();

            string response = _apiHelper.CallToAPI(FormsBaseUrl,
                $"api/forms/{formId}/submissions/submission-csv/{submissionId}", authHeader, "GET");

            if (string.IsNullOrWhiteSpace(response))
                throw new SystemException(response);

            return response;
        }

        public byte[] ExportSubmissionPdf(string formId, string submissionId)
        {
            if (string.IsNullOrWhiteSpace(formId))
                throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));
            if (string.IsNullOrWhiteSpace(submissionId))
                throw new ArgumentException("Submission ID cannot be null or empty", nameof(submissionId));

            string authHeader = RequireAuthHeader();

            byte[] response = _apiHelper.CallToAPIBytes(FormsBaseUrl,
                $"api/forms/{formId}/submissions/{submissionId}/submission-pdf", authHeader, "GET");

            if (response == null || response.Length == 0)
                throw new SystemException("Empty response from PDF export");

            return response;
        }
    }
}
