using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace Paubox
{
    public class ReceivingLibrary : IReceivingLibrary
    {
        private readonly string _apiKey;
        private readonly string _apiBaseURL;
        private readonly IAPIHelper _apiHelper;

        public ReceivingLibrary(string apiKey) : this(apiKey, new APIHelper())
        {
        }

        public ReceivingLibrary(string apiKey, IAPIHelper apiHelper)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

            _apiKey = apiKey;
            _apiBaseURL = "https://api.paubox.com/v1/email/";
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
        }

        public ReceivingLibrary(IConfiguration configuration) : this(configuration, new APIHelper())
        {
        }

        public ReceivingLibrary(IConfiguration configuration, IAPIHelper apiHelper)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var apiKey = configuration["APIKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("APIKey not found in configuration", nameof(configuration));

            _apiKey = apiKey;
            _apiBaseURL = "https://api.paubox.com/v1/email/";
            _apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
        }

        public ReceivingDomainListResponse ListReceivingDomains()
        {
            string response = _apiHelper.CallToAPI(_apiBaseURL, "receiving/domains", GetAuthorizationHeader(), "GET");
            return JsonConvert.DeserializeObject<ReceivingDomainListResponse>(response);
        }

        public ReceivingDomainResponse CreateReceivingDomain(string slug = null)
        {
            string requestBody = "{}";
            if (!string.IsNullOrWhiteSpace(slug))
            {
                var request = new CreateReceivingDomainRequest { Slug = slug };
                requestBody = JsonConvert.SerializeObject(request);
            }

            string response = _apiHelper.CallToAPI(_apiBaseURL, "receiving/domains", GetAuthorizationHeader(), "POST", requestBody);
            return JsonConvert.DeserializeObject<ReceivingDomainResponse>(response);
        }

        public ReceivingDomainResponse GetReceivingDomain(int domainId)
        {
            string requestURI = string.Format("receiving/domains/{0}", domainId);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "GET");
            return JsonConvert.DeserializeObject<ReceivingDomainResponse>(response);
        }

        public DeleteReceivingDomainResponse DeleteReceivingDomain(int domainId)
        {
            string requestURI = string.Format("receiving/domains/{0}", domainId);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "DELETE");
            return JsonConvert.DeserializeObject<DeleteReceivingDomainResponse>(response);
        }

        public MailboxListResponse ListMailboxes(int domainId)
        {
            string requestURI = string.Format("receiving/domains/{0}/mailboxes", domainId);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "GET");
            return JsonConvert.DeserializeObject<MailboxListResponse>(response);
        }

        public MailboxResponse CreateMailbox(int domainId, CreateMailboxRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name cannot be null or empty", nameof(request));
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password cannot be null or empty", nameof(request));

            string requestURI = string.Format("receiving/domains/{0}/mailboxes", domainId);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "POST", JsonConvert.SerializeObject(request));
            return JsonConvert.DeserializeObject<MailboxResponse>(response);
        }

        public MailboxResponse GetMailbox(int domainId, int mailboxId)
        {
            string requestURI = string.Format("receiving/domains/{0}/mailboxes/{1}", domainId, mailboxId);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "GET");
            return JsonConvert.DeserializeObject<MailboxResponse>(response);
        }

        public DeleteMailboxResponse DeleteMailbox(int domainId, int mailboxId)
        {
            string requestURI = string.Format("receiving/domains/{0}/mailboxes/{1}", domainId, mailboxId);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "DELETE");
            return JsonConvert.DeserializeObject<DeleteMailboxResponse>(response);
        }

        public ReceivedEmailListResponse ListReceivedEmails(ListReceivedEmailsParams parameters = null)
        {
            string requestURI = "receiving";
            if (parameters != null)
            {
                var queryParams = new List<KeyValuePair<string, string>>();
                if (parameters.Limit.HasValue)
                    queryParams.Add(new KeyValuePair<string, string>("limit", parameters.Limit.Value.ToString()));
                if (!string.IsNullOrWhiteSpace(parameters.After))
                    queryParams.Add(new KeyValuePair<string, string>("after", parameters.After));
                if (!string.IsNullOrWhiteSpace(parameters.Before))
                    queryParams.Add(new KeyValuePair<string, string>("before", parameters.Before));

                if (queryParams.Count > 0)
                {
                    var parts = new List<string>();
                    foreach (var pair in queryParams)
                        parts.Add(pair.Key + "=" + Uri.EscapeDataString(pair.Value));
                    requestURI += "?" + string.Join("&", parts);
                }
            }

            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "GET");
            return JsonConvert.DeserializeObject<ReceivedEmailListResponse>(response);
        }

        public ReceivedEmailResponse GetReceivedEmail(string emailId)
        {
            if (string.IsNullOrWhiteSpace(emailId))
                throw new ArgumentException("Email ID cannot be null or empty", nameof(emailId));

            string requestURI = string.Format("receiving/{0}", emailId);
            string response = _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "GET");
            return JsonConvert.DeserializeObject<ReceivedEmailResponse>(response);
        }

        public string DownloadAttachment(string emailId, string blobId)
        {
            if (string.IsNullOrWhiteSpace(emailId))
                throw new ArgumentException("Email ID cannot be null or empty", nameof(emailId));
            if (string.IsNullOrWhiteSpace(blobId))
                throw new ArgumentException("Blob ID cannot be null or empty", nameof(blobId));

            string requestURI = string.Format("receiving/{0}/attachments/{1}", emailId, blobId);
            return _apiHelper.CallToAPI(_apiBaseURL, requestURI, GetAuthorizationHeader(), "GET");
        }

        private string GetAuthorizationHeader()
        {
            return string.Format("Token token={0}", _apiKey);
        }
    }
}
