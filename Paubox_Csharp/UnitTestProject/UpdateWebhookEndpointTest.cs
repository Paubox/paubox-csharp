using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class UpdateWebhookEndpointTest
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

    private static string UpdatedResponse(string status)
    {
        return @"{""data"":{""id"":""" + EndpointId + @""",
            ""target_url"":""https://hooks.example.com/paubox"",
            ""status"":""" + status + @""",
            ""events"":[""forms.submission.created""],
            ""created_at"":""2026-10-09T01:43:41.639968+00:00"",
            ""updated_at"":""2026-10-09T02:00:00.000000+00:00""}}";
    }

    private void MockApiResponse(string response)
    {
        _mockApiHelper
            .Setup(x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                    It.IsAny<string>(), It.IsAny<string>()))
            .Returns(response);
    }

    [Test]
    public void TestSendsOnlyTheFieldsSet()
    {
        MockApiResponse(UpdatedResponse("disabled"));

        WebhookEndpoint result = _webhookLibrary.UpdateWebhookEndpoint(
            EndpointId, new UpdateWebhookEndpointRequest { Status = "disabled" });

        Assert.AreEqual("disabled", result.Status);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.Is<string>(uri => uri == "endpoints/" + EndpointId),
                It.Is<string>(auth => auth == "Bearer testApiKey"),
                It.Is<string>(verb => verb == "PATCH"),
                // target_url and events are omitted, not sent as null.
                It.Is<string>(body =>
                    body.Contains("status") &&
                    !body.Contains("target_url") &&
                    !body.Contains("events"))),
            Times.Once);
    }

    [Test]
    public void TestSendsTargetUrlAndEventsTogether()
    {
        MockApiResponse(UpdatedResponse("active"));

        _webhookLibrary.UpdateWebhookEndpoint(EndpointId, new UpdateWebhookEndpointRequest
        {
            TargetUrl = "https://new.example.com/hook",
            Events = new List<string> { "forms.submission.created" }
        });

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<string>(body =>
                    body.Contains("https://new.example.com/hook") &&
                    body.Contains("forms.submission.created"))),
            Times.Once);
    }

    [Test]
    public void TestValidatesBeforeSending()
    {
        Assert.Throws<ArgumentException>(() => _webhookLibrary.UpdateWebhookEndpoint(
            "not-a-uuid", new UpdateWebhookEndpointRequest { Status = "disabled" }));

        Assert.Throws<ArgumentNullException>(() =>
            _webhookLibrary.UpdateWebhookEndpoint(EndpointId, null));

        _mockApiHelper.Verify(
            x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                             It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}
