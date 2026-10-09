namespace Paubox
{
    /// <summary>
    /// Manages webhook subscriptions on the Paubox webhooks service: which URL
    /// Paubox notifies, and for which events.
    /// </summary>
    /// <remarks>
    /// Authenticates with a scoped API key sent as
    /// <c>Authorization: Bearer &lt;key&gt;</c> — the same scheme as
    /// <see cref="IFormsLibrary"/>, and a different one from the Email API's
    /// <c>Token token=</c>. Which events a key may subscribe to is decided by
    /// its scopes: one it is not scoped for is refused with 403, and an
    /// unrecognised event with 422.
    /// </remarks>
    public interface IWebhookLibrary
    {
        /// <summary>
        /// Lists the endpoints this key can act on. Endpoints carrying an event
        /// the key is not scoped for are filtered out by the service.
        /// </summary>
        /// <param name="parameters">Optional pagination. Pass null for defaults.</param>
        WebhookEndpointList ListWebhookEndpoints(ListWebhookEndpointsParams parameters = null);

        /// <summary>
        /// Subscribes a URL to one or more events. The result carries
        /// <see cref="CreatedWebhookEndpoint.SigningSecret"/>, which the
        /// service returns only here.
        /// </summary>
        CreatedWebhookEndpoint CreateWebhookEndpoint(CreateWebhookEndpointRequest request);

        /// <summary>
        /// Retrieves a single endpoint by its UUID. The response does not
        /// include the signing secret.
        /// </summary>
        WebhookEndpoint GetWebhookEndpoint(string id);

        /// <summary>
        /// Updates an endpoint. Only the non-null fields on
        /// <paramref name="request"/> are sent.
        /// </summary>
        WebhookEndpoint UpdateWebhookEndpoint(string id, UpdateWebhookEndpointRequest request);

        /// <summary>
        /// Deletes an endpoint, stopping every event on it. The service answers
        /// 204 with no body.
        /// </summary>
        void DeleteWebhookEndpoint(string id);
    }
}
