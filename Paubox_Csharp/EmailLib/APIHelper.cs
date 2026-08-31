using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Paubox
{
    internal class APIHelper : IAPIHelper
    {
        /// <summary>
        /// A single shared HttpClient — creating one per call exhausts sockets under load.
        /// The Authorization header is set per-request on <see cref="HttpRequestMessage"/> below;
        /// keeping it off <c>DefaultRequestHeaders</c> means multiple library instances configured
        /// with different keys can share this client without cross-contamination.
        /// </summary>
        private static readonly HttpClient _httpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(100)
            };
            return client;
        }

        private static readonly MediaTypeWithQualityHeaderValue JsonAccept =
            new MediaTypeWithQualityHeaderValue("application/json");
        private static readonly MediaTypeWithQualityHeaderValue PdfAccept =
            new MediaTypeWithQualityHeaderValue("application/pdf");

        /// <summary>
        /// Executes an HTTP request against a Paubox API and returns the response body.
        /// Throws <see cref="PauboxApiException"/> on any non-2xx status, so callers never
        /// see an error body returned as data — that class of bug is what makes CSV/PDF
        /// exports produce garbage files and stats endpoints return zeros on 403.
        /// </summary>
        public string CallToAPI(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string requestBody = "")
        {
            HttpMethod method = ResolveMethod(APIVerb);
            Uri absoluteUri = BuildUri(BaseAPIUrl, requestURI);

            using (var request = new HttpRequestMessage(method, absoluteUri))
            {
                request.Headers.Accept.Add(JsonAccept);
                if (!string.IsNullOrEmpty(authHeader))
                    request.Headers.TryAddWithoutValidation("Authorization", authHeader);
                if (MethodTakesBody(method))
                    request.Content = new StringContent(requestBody ?? string.Empty, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = _httpClient.SendAsync(request).GetAwaiter().GetResult())
                {
                    string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                        throw new PauboxApiException((int)response.StatusCode, APIVerb, requestURI, body);

                    return body ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// GET-only path for binary bodies (e.g. PDF exports). Sends
        /// <c>Accept: application/pdf</c>, requires the response Content-Type to match
        /// and the first four bytes to be <c>%PDF</c> — otherwise the SDK would happily
        /// hand back a JSON error body as "PDF bytes" and callers would write it to disk
        /// via <c>File.WriteAllBytes</c> as a broken file.
        /// </summary>
        public byte[] CallToAPIBytes(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb)
        {
            if (APIVerb != "GET")
                throw new ArgumentException("Invalid API verb: " + APIVerb);

            Uri absoluteUri = BuildUri(BaseAPIUrl, requestURI);

            using (var request = new HttpRequestMessage(HttpMethod.Get, absoluteUri))
            {
                request.Headers.Accept.Add(PdfAccept);
                if (!string.IsNullOrEmpty(authHeader))
                    request.Headers.TryAddWithoutValidation("Authorization", authHeader);

                using (HttpResponseMessage response = _httpClient.SendAsync(request).GetAwaiter().GetResult())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        throw new PauboxApiException((int)response.StatusCode, APIVerb, requestURI, errorBody);
                    }

                    byte[] bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

                    string contentType = response.Content.Headers.ContentType?.MediaType;
                    if (contentType != null &&
                        !contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) &&
                        !contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new PauboxApiException((int)response.StatusCode, APIVerb, requestURI,
                            "Unexpected Content-Type: " + contentType);
                    }
                    if (bytes == null || bytes.Length < 4 ||
                        bytes[0] != 0x25 || bytes[1] != 0x50 || bytes[2] != 0x44 || bytes[3] != 0x46)
                    {
                        throw new PauboxApiException((int)response.StatusCode, APIVerb, requestURI,
                            "Response does not start with %PDF magic bytes");
                    }

                    return bytes;
                }
            }
        }

        public string UploadTemplate(string BaseAPIUrl, string requestURI, string authHeader, string APIVerb, string templateName, string templatePath)
        {
            HttpMethod method;
            if (APIVerb == "PATCH") method = new HttpMethod("PATCH");
            else if (APIVerb == "POST") method = HttpMethod.Post;
            else throw new ArgumentException("Invalid API verb: " + APIVerb);

            string originalFilename = Path.GetFileName(templatePath);
            Uri absoluteUri = BuildUri(BaseAPIUrl, requestURI);

            using (var request = new HttpRequestMessage(method, absoluteUri))
            using (var content = new MultipartFormDataContent())
            {
                request.Headers.Accept.Add(JsonAccept);
                if (!string.IsNullOrEmpty(authHeader))
                    request.Headers.TryAddWithoutValidation("Authorization", authHeader);

                var fileContent = new ByteArrayContent(File.ReadAllBytes(templatePath));
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "data[body]",
                    FileName = originalFilename
                };
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/x-handlebars-template");
                content.Add(fileContent);
                content.Add(new StringContent(templateName), "data[name]");
                request.Content = content;

                using (HttpResponseMessage response = _httpClient.SendAsync(request).GetAwaiter().GetResult())
                {
                    string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                        throw new PauboxApiException((int)response.StatusCode, APIVerb, requestURI, body);
                    return body ?? string.Empty;
                }
            }
        }

        private static HttpMethod ResolveMethod(string verb)
        {
            switch (verb)
            {
                case "GET": return HttpMethod.Get;
                case "POST": return HttpMethod.Post;
                case "PUT": return HttpMethod.Put;
                case "DELETE": return HttpMethod.Delete;
                case "PATCH": return new HttpMethod("PATCH");
                default: throw new ArgumentException("Invalid API verb: " + verb);
            }
        }

        private static bool MethodTakesBody(HttpMethod method)
        {
            return method == HttpMethod.Post ||
                   method == HttpMethod.Put ||
                   method.Method == "PATCH";
        }

        private static Uri BuildUri(string baseUrl, string requestUri)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("BaseAPIUrl cannot be null or empty", nameof(baseUrl));
            var baseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/", UriKind.Absolute);
            return new Uri(baseUri, requestUri ?? string.Empty);
        }
    }
}
