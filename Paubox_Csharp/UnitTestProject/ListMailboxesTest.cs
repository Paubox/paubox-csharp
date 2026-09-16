using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class ListMailboxesTest
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
    public void TestListMailboxesReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        MailboxListResponse result = _receivingLibrary.ListMailboxes(1);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Count);
        Assert.AreEqual(10, result.Data[0].Id);
        Assert.AreEqual("info", result.Data[0].Name);
        Assert.AreEqual("info@acme.paubox.net", result.Data[0].Email);
    }

    [Test]
    public void TestListMailboxesSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _receivingLibrary.ListMailboxes(42);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/domains/42/mailboxes"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestListMailboxesEmptyListReturnsEmptyData()
    {
        string apiResponse = EmptyResponse();
        MockApiResponse(apiResponse);

        MailboxListResponse result = _receivingLibrary.ListMailboxes(1);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(0, result.Data.Count);
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
            ["data"] = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["id"] = 10,
                    ["name"] = "info",
                    ["email"] = "info@acme.paubox.net",
                    ["quota_bytes"] = 1073741824,
                    ["created_at"] = "2026-01-15T10:00:00Z",
                    ["updated_at"] = "2026-01-15T10:00:00Z"
                },
                new Dictionary<string, object>
                {
                    ["id"] = 11,
                    ["name"] = "support",
                    ["email"] = "support@acme.paubox.net",
                    ["quota_bytes"] = 2147483648,
                    ["created_at"] = "2026-02-20T14:30:00Z",
                    ["updated_at"] = "2026-02-20T14:30:00Z"
                }
            }
        });
    }

    private string EmptyResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["data"] = new List<object>()
        });
    }
}
