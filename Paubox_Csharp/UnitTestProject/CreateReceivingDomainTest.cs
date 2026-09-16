using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CreateReceivingDomainTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private ReceivingLibrary _receivingLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _receivingLibrary = new ReceivingLibrary("testApiKey", _mockApiHelper.Object);
    }

    [Test]
    public void TestCreateReceivingDomainWithSlugReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        ReceivingDomainResponse result = _receivingLibrary.CreateReceivingDomain("acme");

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Id);
        Assert.AreEqual("acme", result.Data.Slug);
        Assert.AreEqual("acme.paubox.net", result.Data.Domain);
    }

    [Test]
    public void TestCreateReceivingDomainWithoutSlugSendsEmptyBody()
    {
        MockApiResponse(SuccessResponse());
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

        _receivingLibrary.CreateReceivingDomain();

        Assert.AreEqual("{}", capturedBody);
    }

    [Test]
    public void TestCreateReceivingDomainWithSlugSendsSlugInBody()
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

        _receivingLibrary.CreateReceivingDomain("acme");

        JObject parsed = JObject.Parse(capturedBody);
        Assert.AreEqual("acme", parsed["slug"].ToString());
    }

    [Test]
    public void TestCreateReceivingDomainSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _receivingLibrary.CreateReceivingDomain("acme");

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/domains"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "POST"),
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
                "POST",
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
                ["slug"] = "acme",
                ["domain"] = "acme.paubox.net",
                ["verified"] = false,
                ["created_at"] = "2026-01-15T10:00:00Z",
                ["updated_at"] = "2026-01-15T10:00:00Z"
            }
        });
    }
}
