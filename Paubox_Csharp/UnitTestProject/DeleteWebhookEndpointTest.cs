using System;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class DeleteWebhookEndpointTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private WebhookLibrary _webhookLibrary;

    private const string EndpointId = "2ec66c21-bf48-48eb-8d28-f80b2d6b77c7";

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _webhookLibrary = new WebhookLibrary(_mockApiHelper.Object, "testApiKey");
    }

    /// <summary>
    /// The service answers 204 with no body, so nothing is deserialized — an
    /// empty string from the helper must not throw.
    /// </summary>
    [Test]
    public void TestHandlesAnEmpty204()
    {
        _mockApiHelper
            .Setup(x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                    It.IsAny<string>(), It.IsAny<string>()))
            .Returns(string.Empty);

        Assert.DoesNotThrow(() => _webhookLibrary.DeleteWebhookEndpoint(EndpointId));

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/webhooks/"),
                It.Is<string>(uri => uri == "endpoints/" + EndpointId),
                It.Is<string>(auth => auth == "Bearer testApiKey"),
                It.Is<string>(verb => verb == "DELETE"),
                It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public void TestRejectsBadIdsBeforeAnyRequest()
    {
        foreach (string bad in new[] { "not-a-uuid", "../endpoints", "" })
        {
            Assert.Throws<ArgumentException>(() => _webhookLibrary.DeleteWebhookEndpoint(bad));
        }

        _mockApiHelper.Verify(
            x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                             It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// Error mapping lives in APIHelper, which throws PauboxApiException for
    /// any non-2xx. This pins that the library lets it through rather than
    /// swallowing it — and that a duplicate target_url arrives as 422, not 409.
    /// </summary>
    [Test]
    public void TestPropagatesApiExceptions()
    {
        _mockApiHelper
            .Setup(x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                    It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new PauboxApiException(404, "DELETE", "endpoints/" + EndpointId,
                                           "{\"message\":\"webhook endpoint not found\"}"));

        var ex = Assert.Throws<PauboxApiException>(
            () => _webhookLibrary.DeleteWebhookEndpoint(EndpointId));

        Assert.AreEqual(404, ex.StatusCode);
        // The body stays off Message so loggers capturing Exception.Message do
        // not record response content.
        Assert.IsTrue(ex.Body.Contains("webhook endpoint not found"));
        Assert.IsFalse(ex.Message.Contains("webhook endpoint not found"));
    }
}
