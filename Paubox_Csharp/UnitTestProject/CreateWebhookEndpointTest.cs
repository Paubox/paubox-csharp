using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class CreateWebhookEndpointTest
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

    private static string CreatedResponse()
    {
        return @"{""data"":{""id"":""" + EndpointId + @""",
            ""target_url"":""https://hooks.example.com/paubox"",
            ""status"":""active"",
            ""events"":[""forms.submission.created""],
            ""created_at"":""2026-10-09T01:43:41.639968+00:00"",
            ""updated_at"":""2026-10-09T01:43:41.639968+00:00"",
            ""signing_secret"":""whsec_abc123""},
            ""message"":""Store this signing_secret now""}";
    }

    private void MockApiResponse(string response)
    {
        _mockApiHelper
            .Setup(x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                    It.IsAny<string>(), It.IsAny<string>()))
            .Returns(response);
    }

    private static CreateWebhookEndpointRequest ValidRequest()
    {
        return new CreateWebhookEndpointRequest
        {
            TargetUrl = "https://hooks.example.com/paubox",
            Events = new List<string> { "forms.submission.created" }
        };
    }

    /// <summary>
    /// The service returns the signing secret only on create; losing it means
    /// replacing the endpoint, so the client must surface it.
    /// </summary>
    [Test]
    public void TestReturnsTheSigningSecret()
    {
        MockApiResponse(CreatedResponse());

        CreatedWebhookEndpoint result = _webhookLibrary.CreateWebhookEndpoint(ValidRequest());

        Assert.AreEqual("whsec_abc123", result.SigningSecret);
        Assert.AreEqual(EndpointId, result.Id);
        Assert.AreEqual("active", result.Status);
    }

    [Test]
    public void TestPostsTargetUrlAndEvents()
    {
        MockApiResponse(CreatedResponse());

        _webhookLibrary.CreateWebhookEndpoint(ValidRequest());

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.Is<string>(uri => uri == "endpoints"),
                It.Is<string>(auth => auth == "Bearer testApiKey"),
                It.Is<string>(verb => verb == "POST"),
                It.Is<string>(body =>
                    body.Contains("\"target_url\":\"https://hooks.example.com/paubox\"") &&
                    body.Contains("\"events\":[\"forms.submission.created\"]"))),
            Times.Once);
    }

    [Test]
    public void TestValidatesLocallyBeforeSending()
    {
        Assert.Throws<ArgumentNullException>(() => _webhookLibrary.CreateWebhookEndpoint(null));

        Assert.Throws<ArgumentException>(() => _webhookLibrary.CreateWebhookEndpoint(
            new CreateWebhookEndpointRequest { Events = new List<string> { "x" } }));

        Assert.Throws<ArgumentException>(() => _webhookLibrary.CreateWebhookEndpoint(
            new CreateWebhookEndpointRequest { TargetUrl = "https://e.test/h" }));

        Assert.Throws<ArgumentException>(() => _webhookLibrary.CreateWebhookEndpoint(
            new CreateWebhookEndpointRequest
            {
                TargetUrl = "https://e.test/h",
                Events = new List<string>()
            }));

        _mockApiHelper.Verify(
            x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                             It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// The catalog belongs to the service and grows without an SDK release, so
    /// an unrecognised name must reach the server rather than be rejected here.
    /// </summary>
    [Test]
    public void TestDoesNotValidateEventNames()
    {
        MockApiResponse(CreatedResponse());

        _webhookLibrary.CreateWebhookEndpoint(new CreateWebhookEndpointRequest
        {
            TargetUrl = "https://e.test/h",
            Events = new List<string> { "some.future.event" }
        });

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<string>(body => body.Contains("some.future.event"))),
            Times.Once);
    }

    [Test]
    public void TestUpdateRequestOmitsUnsetFields()
    {
        string body = JsonConvert.SerializeObject(
            new UpdateWebhookEndpointRequest { Status = "disabled" });

        Assert.IsTrue(body.Contains("status"));
        Assert.IsFalse(body.Contains("target_url"));
        Assert.IsFalse(body.Contains("events"));
    }
}
