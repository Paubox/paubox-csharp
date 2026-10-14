using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paubox
{
    /// <summary>
    /// A configured webhook endpoint.
    /// </summary>
    /// <remarks>
    /// <see cref="Id"/> is a UUID. Timestamps are RFC 3339 with microsecond
    /// precision and an explicit <c>+00:00</c> offset, kept as strings so no
    /// precision is lost round-tripping through <see cref="System.DateTime"/>.
    /// </remarks>
    public class WebhookEndpoint
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("target_url")]
        public string TargetUrl { get; set; }

        /// <summary>"active" or "disabled".</summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("events")]
        public List<string> Events { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }

    /// <summary>
    /// The result of <see cref="IWebhookLibrary.CreateWebhookEndpoint"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="SigningSecret"/> is returned once, at creation, and never
    /// again — not by <c>GetWebhookEndpoint</c> and not by
    /// <c>ListWebhookEndpoints</c>. Store it when you receive it; recovering
    /// from a lost secret means replacing the endpoint.
    /// </remarks>
    public class CreatedWebhookEndpoint : WebhookEndpoint
    {
        [JsonProperty("signing_secret")]
        public string SigningSecret { get; set; }
    }

    /// <summary>
    /// Request body for <see cref="IWebhookLibrary.CreateWebhookEndpoint"/>.
    /// </summary>
    /// <remarks>
    /// Event names are not validated client-side on purpose: the catalog is
    /// owned by the service and grows without an SDK release. An event this key
    /// is not scoped for is refused with 403, an unrecognised one with 422.
    /// </remarks>
    public class CreateWebhookEndpointRequest
    {
        [JsonProperty("target_url")]
        public string TargetUrl { get; set; }

        [JsonProperty("events")]
        public List<string> Events { get; set; }
    }

    /// <summary>
    /// Fields to change on an existing endpoint. Only non-null properties are
    /// serialized, so an update that moves <see cref="TargetUrl"/> leaves
    /// <see cref="Events"/> and <see cref="Status"/> untouched.
    /// </summary>
    public class UpdateWebhookEndpointRequest
    {
        [JsonProperty("target_url", NullValueHandling = NullValueHandling.Ignore)]
        public string TargetUrl { get; set; }

        /// <summary>"active" or "disabled".</summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("events", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Events { get; set; }
    }

    /// <summary>
    /// Pagination metadata. <see cref="Count"/> is the total number of
    /// endpoints matching the request, not the length of the returned page.
    /// </summary>
    public class WebhookPageInfo
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items")]
        public int Items { get; set; }
    }

    /// <summary>
    /// The result of <see cref="IWebhookLibrary.ListWebhookEndpoints"/>.
    /// </summary>
    public class WebhookEndpointList
    {
        [JsonProperty("data")]
        public List<WebhookEndpoint> Data { get; set; }

        [JsonProperty("page_info")]
        public WebhookPageInfo PageInfo { get; set; }
    }

    /// <summary>
    /// Optional query parameters for listing endpoints.
    /// </summary>
    public class ListWebhookEndpointsParams
    {
        public int? Page { get; set; }
        public int? Items { get; set; }
    }

    /// <summary>
    /// Single resources arrive wrapped in a <c>data</c> envelope.
    /// </summary>
    internal class WebhookDataEnvelope<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
