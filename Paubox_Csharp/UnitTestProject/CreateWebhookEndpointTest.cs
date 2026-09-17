using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CreateWebhookEndpointTest
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
    public void TestCreateWebhookEndpointReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        var request = new CreateWebhookEndpointRequest
        {
            TargetUrl = "https://example.com/webhook",
            Events = new List<string> { "api_mail_log_delivered" }
        };

        WebhookEndpointResponse result = _webhookLibrary.CreateWebhookEndpoint(request);

        Assert.IsNotNull(result);
        Assert.AreEqual("Webhook created!", result.Message);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Id);
        Assert.AreEqual("https://example.com/webhook", result.Data.TargetUrl);
    }

    [Test]
    public void TestCreateWebhookEndpointSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        var request = new CreateWebhookEndpointRequest
        {
            TargetUrl = "https://example.com/webhook",
            Events = new List<string> { "api_mail_log_delivered" }
        };

        _webhookLibrary.CreateWebhookEndpoint(request);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "webhook_endpoints"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "POST"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestCreateWebhookEndpointSendsCorrectBody()
    {
        string capturedBody = null;

        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "POST",
                It.IsAny<string>()
            )
        ).Callback<string, string, string, string, string>((url, uri, auth, verb, body) =>
        {
            capturedBody = body;
        }).Returns(SuccessResponse());

        var request = new CreateWebhookEndpointRequest
        {
            TargetUrl = "https://example.com/webhook",
            Events = new List<string> { "api_mail_log_delivered", "api_mail_log_opened" },
            SigningKey = "sk_test"
        };

        _webhookLibrary.CreateWebhookEndpoint(request);

        JObject parsed = JObject.Parse(capturedBody);
        Assert.AreEqual("https://example.com/webhook", parsed["target_url"].ToString());
        Assert.AreEqual(2, ((JArray)parsed["events"]).Count);
        Assert.AreEqual("sk_test", parsed["signing_key"].ToString());
    }

    [Test]
    public void TestCreateWebhookEndpointOmitsNullOptionalFields()
    {
        string capturedBody = null;

        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "POST",
                It.IsAny<string>()
            )
        ).Callback<string, string, string, string, string>((url, uri, auth, verb, body) =>
        {
            capturedBody = body;
        }).Returns(SuccessResponse());

        var request = new CreateWebhookEndpointRequest
        {
            TargetUrl = "https://example.com/webhook",
            Events = new List<string> { "api_mail_log_delivered" }
        };

        _webhookLibrary.CreateWebhookEndpoint(request);

        JObject parsed = JObject.Parse(capturedBody);
        Assert.IsNull(parsed["signing_key"]);
        Assert.IsNull(parsed["api_key"]);
        Assert.IsNull(parsed["active"]);
    }

    [Test]
    public void TestCreateWebhookEndpointRejectsNullRequest()
    {
        Assert.Throws<ArgumentNullException>(() => _webhookLibrary.CreateWebhookEndpoint(null));
    }

    [Test]
    public void TestCreateWebhookEndpointRejectsNullTargetUrl()
    {
        var request = new CreateWebhookEndpointRequest
        {
            Events = new List<string> { "api_mail_log_delivered" }
        };

        Assert.Throws<ArgumentException>(() => _webhookLibrary.CreateWebhookEndpoint(request));
    }

    [Test]
    public void TestCreateWebhookEndpointRejectsEmptyTargetUrl()
    {
        var request = new CreateWebhookEndpointRequest
        {
            TargetUrl = "",
            Events = new List<string> { "api_mail_log_delivered" }
        };

        Assert.Throws<ArgumentException>(() => _webhookLibrary.CreateWebhookEndpoint(request));
    }

    [Test]
    public void TestCreateWebhookEndpointRejectsNullEvents()
    {
        var request = new CreateWebhookEndpointRequest
        {
            TargetUrl = "https://example.com/webhook"
        };

        Assert.Throws<ArgumentException>(() => _webhookLibrary.CreateWebhookEndpoint(request));
    }

    [Test]
    public void TestCreateWebhookEndpointRejectsEmptyEvents()
    {
        var request = new CreateWebhookEndpointRequest
        {
            TargetUrl = "https://example.com/webhook",
            Events = new List<string>()
        };

        Assert.Throws<ArgumentException>(() => _webhookLibrary.CreateWebhookEndpoint(request));
    }

    private void MockApiResponse(string response)
    {
        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "POST",
                It.IsAny<string>()
            )
        ).Returns(response);
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["message"] = "Webhook created!",
            ["data"] = new Dictionary<string, object>
            {
                ["id"] = 1,
                ["target_url"] = "https://example.com/webhook",
                ["events"] = new List<string> { "api_mail_log_delivered" },
                ["active"] = true,
                ["created_at"] = "2026-01-15T10:00:00Z",
                ["updated_at"] = "2026-01-15T10:00:00Z"
            }
        });
    }
}
