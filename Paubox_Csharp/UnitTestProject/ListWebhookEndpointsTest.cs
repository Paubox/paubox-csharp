using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class ListWebhookEndpointsTest
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
    public void TestListWebhookEndpointsReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        List<WebhookEndpoint> result = _webhookLibrary.ListWebhookEndpoints();

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(1, result[0].Id);
        Assert.AreEqual("https://example.com/webhook1", result[0].TargetUrl);
        Assert.IsTrue(result[0].Active);
        Assert.AreEqual(2, result[0].Events.Count);
    }

    [Test]
    public void TestListWebhookEndpointsSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _webhookLibrary.ListWebhookEndpoints();

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "webhook_endpoints"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestListWebhookEndpointsEmptyReturnsEmptyList()
    {
        MockApiResponse("[]");

        List<WebhookEndpoint> result = _webhookLibrary.ListWebhookEndpoints();

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    private void MockApiResponse(string response)
    {
        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "GET",
                It.IsAny<string>()
            )
        ).Returns(response);
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["id"] = 1,
                ["target_url"] = "https://example.com/webhook1",
                ["events"] = new List<string> { "api_mail_log_delivered", "api_mail_log_opened" },
                ["active"] = true,
                ["signing_key"] = "sk_test_123",
                ["created_at"] = "2026-01-15T10:00:00Z",
                ["updated_at"] = "2026-01-15T10:00:00Z"
            },
            new Dictionary<string, object>
            {
                ["id"] = 2,
                ["target_url"] = "https://example.com/webhook2",
                ["events"] = new List<string> { "inbound_mail_received" },
                ["active"] = false,
                ["created_at"] = "2026-02-20T14:30:00Z",
                ["updated_at"] = "2026-02-20T14:30:00Z"
            }
        });
    }
}
