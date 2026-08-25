using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paubox
{
    public class FormsLibrary : IFormsLibrary
    {
        internal const string DefaultFormsBaseUrl = "https://api.paubox.com/v1/forms/";

        /// <summary>
        /// Serializer settings scoped to this library — the parameterless
        /// <see cref="JsonConvert"/> overloads inherit whatever the consuming
        /// application has configured on <see cref="JsonConvert.DefaultSettings"/>,
        /// which is not something a library should trust.
        /// </summary>
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings();

        private readonly string _baseUrl;
        private readonly IAPIHelper _apiHelper;
        private readonly string _apiKey;

        public FormsLibrary() : this(new APIHelper(), null, DefaultFormsBaseUrl)
        {
        }

        public FormsLibrary(string apiKey) : this(new APIHelper(), apiKey, DefaultFormsBaseUrl)
        {
        }

        public FormsLibrary(IAPIHelper apiHelper) : this(apiHelper, null, DefaultFormsBaseUrl)
        {
        }

        public FormsLibrary(IAPIHelper apiHelper, string apiKey) : this(apiHelper, apiKey, DefaultFormsBaseUrl)
        {
        }

        /// <summary>
        /// Full constructor with base-URL override — for staging / regional endpoints
        /// or dependency-injected tests. Callers that need a non-production base URL
        /// should use this overload or the <see cref="IConfiguration"/>-based ones
        /// rather than editing the SDK source.
        /// </summary>
        public FormsLibrary(IAPIHelper apiHelper, string apiKey, string baseUrl)
        {
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
            _apiKey = apiKey;
            _baseUrl = NormalizeBaseUrl(baseUrl);
        }

        /// <summary>
        /// Reads <c>FormsAPIKey</c> and (optionally) <c>FormsBaseURL</c> from configuration.
        /// Mirrors <see cref="EmailLibrary"/>'s <c>IConfiguration</c> pattern so both
        /// libraries can be wired up the same way in ASP.NET Core apps.
        /// </summary>
        public FormsLibrary(IConfiguration configuration) : this(configuration, new APIHelper())
        {
        }

        public FormsLibrary(IConfiguration configuration, IAPIHelper apiHelper)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
            _apiKey = configuration["FormsAPIKey"];
            _baseUrl = NormalizeBaseUrl(configuration["FormsBaseURL"]);
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return DefaultFormsBaseUrl;
            return baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        }

        /// <summary>
        /// Builds the Authorization header value for authenticated Forms API endpoints,
        /// or throws when no API key was provided. Note that the SDK does not — and
        /// cannot — validate the key's scope; the server enforces the 'forms' scope
        /// and returns 401/403 for keys without it.
        /// </summary>
        private string RequireAuthHeader()
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException(
                    "This endpoint requires authentication. Construct FormsLibrary with a scoped API key " +
                    "that has the 'forms' scope, e.g. new FormsLibrary(apiKey).");

            return "Bearer " + _apiKey;
        }

        /// <summary>
        /// Rejects <paramref name="value"/> if it isn't a plausible form/submission id
        /// (must parse as a <see cref="Guid"/>). Every method that interpolates an id
        /// into a URL segment must guard here; the server accepts both dashed and
        /// dashless UUID shapes, and <see cref="Guid.TryParse"/> accepts both.
        /// </summary>
        private static void ValidateUuid(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(paramName + " cannot be null or empty", paramName);
            if (!Guid.TryParse(value, out _))
                throw new ArgumentException(paramName + " must be a valid UUID; got: " + value, paramName);
        }

        private static void ValidatePage(int? page)
        {
            if (page.HasValue && page.Value < 1)
                throw new ArgumentOutOfRangeException(nameof(page), page.Value,
                    "page must be >= 1 (server treats page as 1-indexed).");
        }

        private static string BuildQueryString(List<KeyValuePair<string, string>> queryParams)
        {
            if (queryParams == null || queryParams.Count == 0)
                return string.Empty;

            var parts = new List<string>();
            foreach (var pair in queryParams)
                parts.Add(pair.Key + "=" + Uri.EscapeDataString(pair.Value));
            return "?" + string.Join("&", parts);
        }

        public Form GetForm(string formId)
        {
            ValidateUuid(formId, nameof(formId));

            string response = _apiHelper.CallToAPI(_baseUrl,
                "public/form_data/" + formId, null, "GET");

            Form form = JsonConvert.DeserializeObject<Form>(response, JsonSettings);
            if (form == null || form.Id == null)
                throw new PauboxApiException(200, "GET", "public/form_data/" + formId, response);

            return form;
        }

        public void SubmitForm(string formId, Dictionary<string, object> formData,
                               FormAttachment[] attachments = null)
        {
            ValidateUuid(formId, nameof(formId));
            if (formData == null)
                throw new ArgumentNullException(nameof(formData));

            var body = new Dictionary<string, object> { ["form_data"] = formData };
            if (attachments != null && attachments.Length > 0)
                body["attachments"] = attachments;

            _apiHelper.CallToAPI(_baseUrl,
                "api/forms/" + formId + "/submissions", null, "POST",
                JsonConvert.SerializeObject(body, JsonSettings));
        }

        public FormsListResponse ListForms(FormsListParams parameters = null)
        {
            if (parameters == null || !parameters.CustomerId.HasValue)
                throw new ArgumentException(
                    "customer_id is required — the server compares it against the API key's owning customer " +
                    "and returns 403 if it's missing. Pass FormsListParams { CustomerId = <int> }.",
                    nameof(parameters));

            ValidatePage(parameters.Page);
            string authHeader = RequireAuthHeader();

            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("customer_id", parameters.CustomerId.Value.ToString())
            };
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

            string requestUri = "api/forms" + BuildQueryString(queryParams);
            string response = _apiHelper.CallToAPI(_baseUrl, requestUri, authHeader, "GET");

            FormsListResponse result = JsonConvert.DeserializeObject<FormsListResponse>(response, JsonSettings);
            if (result == null || result.Results == null)
                throw new PauboxApiException(200, "GET", requestUri, response);

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
            if (!request.CustomerId.HasValue)
                throw new ArgumentException(
                    "CustomerId is required — the server rejects create with 403 when it's missing.",
                    nameof(request));

            string authHeader = RequireAuthHeader();

            string response = _apiHelper.CallToAPI(_baseUrl,
                "api/forms", authHeader, "POST",
                JsonConvert.SerializeObject(request, JsonSettings));

            CreateFormResponse result = JsonConvert.DeserializeObject<CreateFormResponse>(response, JsonSettings);
            if (result == null || result.Id == null)
                throw new PauboxApiException(200, "POST", "api/forms", response);

            return result;
        }

        public Form GetFormById(string formId)
        {
            ValidateUuid(formId, nameof(formId));
            string authHeader = RequireAuthHeader();

            string requestUri = "api/forms/" + formId;
            string response = _apiHelper.CallToAPI(_baseUrl, requestUri, authHeader, "GET");

            JObject envelope;
            try
            {
                envelope = JsonConvert.DeserializeObject<JObject>(response, JsonSettings);
            }
            catch (JsonException)
            {
                throw new PauboxApiException(200, "GET", requestUri, response);
            }

            JToken data = envelope?["data"];
            Form form = data?.ToObject<Form>();
            if (form == null || form.Id == null)
                throw new PauboxApiException(200, "GET", requestUri, response);

            return form;
        }

        public UpdateFormResponse UpdateForm(string formId, UpdateFormRequest request)
        {
            ValidateUuid(formId, nameof(formId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string authHeader = RequireAuthHeader();
            string requestUri = "api/forms/" + formId;
            string response = _apiHelper.CallToAPI(_baseUrl, requestUri, authHeader, "PUT",
                JsonConvert.SerializeObject(request, JsonSettings));

            UpdateFormResponse result = JsonConvert.DeserializeObject<UpdateFormResponse>(response, JsonSettings);
            if (result == null || result.Detail == null)
                throw new PauboxApiException(200, "PUT", requestUri, response);

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
            ValidateUuid(formId, nameof(formId));
            string authHeader = RequireAuthHeader();

            string requestUri = "api/forms/" + formId + "/" + action;
            string response = _apiHelper.CallToAPI(_baseUrl, requestUri, authHeader, "POST");

            UpdateFormResponse result = JsonConvert.DeserializeObject<UpdateFormResponse>(response, JsonSettings);
            if (result == null || result.Detail == null)
                throw new PauboxApiException(200, "POST", requestUri, response);

            return result.Detail;
        }

        public Form CopyForm(string formId, string newTitle)
        {
            // formId goes in the JSON body here — not a URL segment — so URL-injection isn't
            // a concern the way it is for the other methods. Still validate for consistency.
            ValidateUuid(formId, nameof(formId));
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title cannot be null or empty", nameof(newTitle));

            string authHeader = RequireAuthHeader();

            var body = new Dictionary<string, object>
            {
                ["form_id"] = formId,
                ["title"] = newTitle
            };

            string response = _apiHelper.CallToAPI(_baseUrl,
                "api/forms/copy", authHeader, "POST",
                JsonConvert.SerializeObject(body, JsonSettings));

            Form form = JsonConvert.DeserializeObject<Form>(response, JsonSettings);
            if (form == null || form.Id == null)
                throw new PauboxApiException(200, "POST", "api/forms/copy", response);

            return form;
        }

        public FormStats GetFormStats(int? customerId = null)
        {
            string authHeader = RequireAuthHeader();

            string requestUri = "api/forms/stats";
            if (customerId.HasValue)
                requestUri += "?customer_id=" + Uri.EscapeDataString(customerId.Value.ToString());

            string response = _apiHelper.CallToAPI(_baseUrl, requestUri, authHeader, "GET");

            FormStats stats = JsonConvert.DeserializeObject<FormStats>(response, JsonSettings);
            if (stats == null)
                throw new PauboxApiException(200, "GET", requestUri, response);

            return stats;
        }

        public FormSubmissionListResponse ListFormSubmissions(string formId, SubmissionListParams parameters = null)
        {
            ValidateUuid(formId, nameof(formId));
            ValidatePage(parameters?.Page);
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

            string requestUri = "api/forms/" + formId + "/submissions" + BuildQueryString(queryParams);
            string response = _apiHelper.CallToAPI(_baseUrl, requestUri, authHeader, "GET");

            FormSubmissionListResponse result = JsonConvert.DeserializeObject<FormSubmissionListResponse>(response, JsonSettings);
            if (result == null || result.Data == null)
                throw new PauboxApiException(200, "GET", requestUri, response);

            return result;
        }

        public string ExportSubmissionsCsv(string formId)
        {
            ValidateUuid(formId, nameof(formId));
            string authHeader = RequireAuthHeader();
            string requestUri = "api/forms/" + formId + "/submissions/submission-csv";
            return _apiHelper.CallToAPI(_baseUrl, requestUri, authHeader, "GET");
        }

        public string ExportSubmissionCsv(string formId, string submissionId)
        {
            ValidateUuid(formId, nameof(formId));
            ValidateUuid(submissionId, nameof(submissionId));
            string authHeader = RequireAuthHeader();
            string requestUri = "api/forms/" + formId + "/submissions/submission-csv/" + submissionId;
            return _apiHelper.CallToAPI(_baseUrl, requestUri, authHeader, "GET");
        }

        public byte[] ExportSubmissionPdf(string formId, string submissionId)
        {
            ValidateUuid(formId, nameof(formId));
            ValidateUuid(submissionId, nameof(submissionId));
            string authHeader = RequireAuthHeader();
            string requestUri = "api/forms/" + formId + "/submissions/" + submissionId + "/submission-pdf";
            return _apiHelper.CallToAPIBytes(_baseUrl, requestUri, authHeader, "GET");
        }
    }
}
