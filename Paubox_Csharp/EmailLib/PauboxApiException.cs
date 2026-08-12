using System;

namespace Paubox
{
    /// <summary>
    /// Thrown when a Paubox API call returns a non-2xx HTTP status.
    /// The exception message contains only verb + endpoint + status —
    /// the response body is on the <see cref="Body"/> property so
    /// error reporters that capture <c>Exception.Message</c> by default
    /// (Sentry, Application Insights, most .NET structured loggers)
    /// don't inadvertently record submitter-supplied content.
    /// </summary>
    public class PauboxApiException : Exception
    {
        public int StatusCode { get; }
        public string Verb { get; }
        public string Endpoint { get; }
        public string Body { get; }

        public PauboxApiException(int statusCode, string verb, string endpoint, string body)
            : base($"{verb} {endpoint} -> {statusCode}")
        {
            StatusCode = statusCode;
            Verb = verb;
            Endpoint = endpoint;
            Body = body;
        }
    }
}
