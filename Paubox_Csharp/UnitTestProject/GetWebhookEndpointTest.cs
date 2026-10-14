using System;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class GetWebhookEndpointTest
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

    private static string EndpointResponse()
    {
        return @"{""data"":{""id"":""" + EndpointId + @""",
            ""target_url"":""https://hooks.example.com/paubox"",
            ""status"":""active"",
            ""events"":[""forms.submission.created""],
            ""created_at"":""2026-10-09T01:43:41.639968+00:00"",
            ""updated_at"":""2026-10-09T01:43:41.639968+00:00""}}";
    }

    private void MockApiResponse(string response)
    {
        _mockApiHelper
            .Setup(x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                    It.IsAny<string>(), It.IsAny<string>()))
            .Returns(response);
    }

    [Test]
    public void TestUnwrapsTheDataEnvelope()
    {
        MockApiResponse(EndpointResponse());

        WebhookEndpoint result = _webhookLibrary.GetWebhookEndpoint(EndpointId);

        Assert.AreEqual(EndpointId, result.Id);
        Assert.AreEqual("https://hooks.example.com/paubox", result.TargetUrl);
        Assert.AreEqual(1, result.Events.Count);
    }

    [Test]
    public void TestRequestsTheEndpointById()
    {
        MockApiResponse(EndpointResponse());

        _webhookLibrary.GetWebhookEndpoint(EndpointId);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.Is<string>(uri => uri == "endpoints/" + EndpointId),
                It.Is<string>(auth => auth == "Bearer testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()),
            Times.Once);
    }

    /// <summary>
    /// A non-UUID id must be refused before a request. Without the guard a
    /// value containing ".." or "/" would change which endpoint is called, and
    /// the Authorization header goes on the same host, so the key would ride
    /// along on the retargeted request.
    /// </summary>
    [Test]
    public void TestRejectsBadIdsBeforeAnyRequest()
    {
        foreach (string bad in new[] { "not-a-uuid", "../endpoints", "", "   " })
        {
            Assert.Throws<ArgumentException>(() => _webhookLibrary.GetWebhookEndpoint(bad));
        }

        _mockApiHelper.Verify(
            x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                             It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public void TestThrowsWhenTheEnvelopeIsMissing()
    {
        MockApiResponse("{}");

        Assert.Throws<PauboxApiException>(() => _webhookLibrary.GetWebhookEndpoint(EndpointId));
    }
}
