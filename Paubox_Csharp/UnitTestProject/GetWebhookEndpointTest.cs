using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class GetWebhookEndpointTest
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
    public void TestGetWebhookEndpointReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        WebhookEndpointResponse result = _webhookLibrary.GetWebhookEndpoint(1);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Id);
        Assert.AreEqual("https://example.com/webhook", result.Data.TargetUrl);
        Assert.IsTrue(result.Data.Active);
    }

    [Test]
    public void TestGetWebhookEndpointSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _webhookLibrary.GetWebhookEndpoint(42);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "webhook_endpoints/42"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
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
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["data"] = new Dictionary<string, object>
            {
                ["id"] = 1,
                ["target_url"] = "https://example.com/webhook",
                ["events"] = new List<string> { "api_mail_log_delivered" },
                ["active"] = true,
                ["signing_key"] = "sk_test_123",
                ["created_at"] = "2026-01-15T10:00:00Z",
                ["updated_at"] = "2026-01-15T10:00:00Z"
            }
        });
    }
}
