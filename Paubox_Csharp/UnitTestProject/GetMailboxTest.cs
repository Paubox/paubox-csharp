using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class GetMailboxTest
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
    public void TestGetMailboxReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        MailboxResponse result = _receivingLibrary.GetMailbox(1, 10);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(10, result.Data.Id);
        Assert.AreEqual("info", result.Data.Name);
        Assert.AreEqual("info@acme.paubox.net", result.Data.Email);
        Assert.AreEqual(1073741824, result.Data.QuotaBytes);
    }

    [Test]
    public void TestGetMailboxSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _receivingLibrary.GetMailbox(42, 99);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/domains/42/mailboxes/99"),
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
                ["id"] = 10,
                ["name"] = "info",
                ["email"] = "info@acme.paubox.net",
                ["quota_bytes"] = 1073741824,
                ["created_at"] = "2026-01-15T10:00:00Z",
                ["updated_at"] = "2026-01-15T10:00:00Z"
            }
        });
    }
}
