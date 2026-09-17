using System.Collections.Generic;

namespace Paubox
{
    public interface IWebhookLibrary
    {
        List<WebhookEndpoint> ListWebhookEndpoints();
        WebhookEndpointResponse CreateWebhookEndpoint(CreateWebhookEndpointRequest request);
        WebhookEndpointResponse GetWebhookEndpoint(int id);
        WebhookEndpointResponse UpdateWebhookEndpoint(int id, UpdateWebhookEndpointRequest request);
        WebhookEndpointDeleteResponse DeleteWebhookEndpoint(int id);
    }
}
