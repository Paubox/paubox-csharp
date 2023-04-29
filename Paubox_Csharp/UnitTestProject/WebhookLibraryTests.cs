using Paubox;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestFixture]
    public class WebhookLibraryTests
    {
        private readonly WebhookLibrary _library;

        public WebhookLibraryTests()
        {
            _library = new WebhookLibrary();
        }

        [Test]
        public async Task CreateWebhookEndpoint_ValidTemplateRequest_ReturnsCreateWebhookEndpointResponse()
        {
            // Arrange
            var webhookEndpointRequest = new WebhookEndpointRequest()
            {
                target_url = "https://example.com",
                events = new[] { "api_mail_log_delivered" },
                active = true,
                signing_key = "Abc",
                api_key = "abc"
            };

            // Act
            var result = await _library.CreateWebhookEndpoint(webhookEndpointRequest);

            // Assert
            Assert.IsAssignableFrom<WebhookEndpointResponse>(result);
            Assert.AreEqual("Webhook created!", result.message);
        }

        [Test]
        public async Task GetAllWebhookEndpoint_ValidCall_ReturnsListOfWebhookEndpointAllResponse()
        {
            // Act
            var result = await _library.GetAllWebhookEndpoints();

            // Assert
            Assert.IsAssignableFrom<List<WebhookEndpoint>>(result);
            Assert.GreaterOrEqual(result.Count, 0);
        }

        [Test]
        public async Task GetWebhookEndpoint_ValidID_ReturnsWebhookEndpointResponse()
        {
            // Arrange
            var ID = 2962;

            // Act
            var result = await _library.GetWebhookEndpoint(ID);

            // Assert
            Assert.IsAssignableFrom<WebhookEndpointResponse>(result);
            Assert.AreEqual(ID, result.id);
        }

        [Test]
        public async Task DeleteWebhookEndpoint_ValidID_ReturnsDeleteWebhookEndpointResponse()
        {
            // Arrange
            var ID = 2962;

            // Act
            var result = await _library.DeleteWebhookEndpoint(ID);

            // Assert
            Assert.IsAssignableFrom<WebhookEndpointResponse>(result);
            Assert.AreEqual("Webhook deleted!", result.message);
        }

        [Test]
        public async Task UpdateDynamicTemplate_ValidIDAndTemplateRequest_ReturnsCreateDynamicTemplateResponse()
        {
            // Arrange
            var ID = 2962;
            var webhookEndpointRequest = new WebhookEndpointRequest()
            {
                target_url = "https://example2.com",
                events = new[] { "api_mail_log_delivered" },
                active = true,
                signing_key = "updated_signing_key",
                api_key = "updated_api_key"
            };

            // Act
            var result = await _library.UpdateWebhookEndpoint(ID, webhookEndpointRequest);

            // Assert
            Assert.IsAssignableFrom<WebhookEndpointResponse>(result);
            Assert.AreEqual("Webhook updated!", result.message);
        }
    }
}
