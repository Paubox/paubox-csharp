using System;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class ListWebhookEndpointsTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private WebhookLibrary _webhookLibrary;

    private const string BaseUrl = "https://api.paubox.com/v1/webhooks/";
    private const string EndpointId = "2ec66c21-bf48-48eb-8d28-f80b2d6b77c7";

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _webhookLibrary = new WebhookLibrary(_mockApiHelper.Object, "testApiKey");
    }

    private static string ListResponse(int count = 1, int items = 50)
    {
        return @"{""data"":[{""id"":""" + EndpointId + @""",
            ""target_url"":""https://hooks.example.com/paubox"",
            ""status"":""active"",
            ""events"":[""forms.submission.created""],
            ""created_at"":""2026-10-09T01:43:41.639968+00:00"",
            ""updated_at"":""2026-10-09T01:43:41.639968+00:00""}],
            ""page_info"":{""count"":" + count + @",""items"":" + items + "}}";
    }

    private void MockApiResponse(string response)
    {
        _mockApiHelper
            .Setup(x => x.CallToAPI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                    It.IsAny<string>(), It.IsAny<string>()))
            .Returns(response);
    }

    [Test]
    public void TestListReturnsData()
    {
        MockApiResponse(ListResponse());

        WebhookEndpointList result = _webhookLibrary.ListWebhookEndpoints();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual(EndpointId, result.Data[0].Id);
        Assert.AreEqual("active", result.Data[0].Status);
    }

    /// <summary>
    /// count is the total, not the length of the page — a client that assumes
    /// otherwise paginates wrongly.
    /// </summary>
    [Test]
    public void TestCountIsTheTotalNotThePageLength()
    {
        MockApiResponse(ListResponse(count: 2, items: 1));

        WebhookEndpointList result = _webhookLibrary.ListWebhookEndpoints();

        Assert.AreEqual(2, result.PageInfo.Count);
        Assert.AreEqual(1, result.Data.Count);
    }

    [Test]
    public void TestSendsBearerAuthToTheEndpointsResource()
    {
        MockApiResponse(ListResponse());

        _webhookLibrary.ListWebhookEndpoints();

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == BaseUrl),
                It.Is<string>(uri => uri == "endpoints"),
                // Bearer, not the Email API's "Token token=".
                It.Is<string>(auth => auth == "Bearer testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public void TestSendsPaginationWhenGiven()
    {
        MockApiResponse(ListResponse());

        _webhookLibrary.ListWebhookEndpoints(new ListWebhookEndpointsParams { Page = 2, Items = 1 });

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.Is<string>(uri => uri == "endpoints?page=2&items=1"),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public void TestRejectsAnEmptyApiKey()
    {
        Assert.Throws<ArgumentException>(() => new WebhookLibrary(_mockApiHelper.Object, ""));
        Assert.Throws<ArgumentException>(() => new WebhookLibrary(_mockApiHelper.Object, "   "));
    }

    [Test]
    public void TestBaseUrlIsOverridableAndNormalized()
    {
        var library = new WebhookLibrary(
            _mockApiHelper.Object, "k", "https://api.staging.paubox.net/v1/webhooks");
        MockApiResponse(ListResponse());

        library.ListWebhookEndpoints();

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.staging.paubox.net/v1/webhooks/"),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }
}
