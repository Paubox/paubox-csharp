using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class UpdateWebhookEndpointTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private WebhookLibrary _webhookLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _webhookLibrary = new WebhookLibrary("testApiKey", _mockApiHelper.Object);
    }

    [Test]
    public void TestUpdateWebhookEndpointReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        var request = new UpdateWebhookEndpointRequest { Active = false };
        WebhookEndpointResponse result = _webhookLibrary.UpdateWebhookEndpoint(1, request);

        Assert.IsNotNull(result);
        Assert.AreEqual("Webhook updated!", result.Message);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Id);
    }

    [Test]
    public void TestUpdateWebhookEndpointSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        var request = new UpdateWebhookEndpointRequest { Active = false };
        _webhookLibrary.UpdateWebhookEndpoint(42, request);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "webhook_endpoints/42"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "PATCH"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestUpdateWebhookEndpointSendsCorrectBody()
    {
        string capturedBody = null;

        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "PATCH",
                It.IsAny<string>()
            )
        ).Callback<string, string, string, string, string>((url, uri, auth, verb, body) =>
        {
            capturedBody = body;
        }).Returns(SuccessResponse());

        var request = new UpdateWebhookEndpointRequest
        {
            TargetUrl = "https://example.com/new-webhook",
            Active = false
        };

        _webhookLibrary.UpdateWebhookEndpoint(1, request);

        JObject parsed = JObject.Parse(capturedBody);
        Assert.AreEqual("https://example.com/new-webhook", parsed["target_url"].ToString());
        Assert.AreEqual(false, parsed["active"].Value<bool>());
    }

    [Test]
    public void TestUpdateWebhookEndpointRejectsNullRequest()
    {
        Assert.Throws<ArgumentNullException>(() => _webhookLibrary.UpdateWebhookEndpoint(1, null));
    }

    private void MockApiResponse(string response)
    {
        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "PATCH",
                It.IsAny<string>()
            )
        ).Returns(response);
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["message"] = "Webhook updated!",
            ["data"] = new Dictionary<string, object>
            {
                ["id"] = 1,
                ["target_url"] = "https://example.com/new-webhook",
                ["events"] = new List<string> { "api_mail_log_delivered" },
                ["active"] = false,
                ["created_at"] = "2026-01-15T10:00:00Z",
                ["updated_at"] = "2026-01-15T12:00:00Z"
            }
        });
    }
}
