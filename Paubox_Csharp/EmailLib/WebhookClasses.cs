using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paubox
{
    public class WebhookEndpoint
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("target_url")]
        public string TargetUrl { get; set; }

        [JsonProperty("events")]
        public List<string> Events { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("signing_key")]
        public string SigningKey { get; set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }

    public class WebhookEndpointResponse
    {
        [JsonProperty("data")]
        public WebhookEndpoint Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class WebhookEndpointDeleteResponse
    {
        [JsonProperty("data")]
        public WebhookEndpoint Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreateWebhookEndpointRequest
    {
        [JsonProperty("target_url")]
        public string TargetUrl { get; set; }

        [JsonProperty("events")]
        public List<string> Events { get; set; }

        [JsonProperty("signing_key")]
        public string SigningKey { get; set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("active")]
        public bool? Active { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UpdateWebhookEndpointRequest
    {
        [JsonProperty("target_url")]
        public string TargetUrl { get; set; }

        [JsonProperty("events")]
        public List<string> Events { get; set; }

        [JsonProperty("signing_key")]
        public string SigningKey { get; set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("active")]
        public bool? Active { get; set; }
    }
}
