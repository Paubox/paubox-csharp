using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class ListReceivedEmailsTest
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
    public void TestListReceivedEmailsReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        ReceivedEmailListResponse result = _receivingLibrary.ListReceivedEmails();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Count);
        Assert.AreEqual("msg-001", result.Data[0].Id);
        Assert.AreEqual("sender@example.com", result.Data[0].From);
        Assert.AreEqual("Test email", result.Data[0].Subject);
    }

    [Test]
    public void TestListReceivedEmailsWithoutParamsSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _receivingLibrary.ListReceivedEmails();

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestListReceivedEmailsWithParamsSendsQueryString()
    {
        MockApiResponse(SuccessResponse());
        string capturedUri = null;

        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "GET",
                It.IsAny<string>()
            )
        ).Callback<string, string, string, string, string>((url, uri, auth, verb, body) =>
        {
            capturedUri = uri;
        }).Returns(SuccessResponse());

        var parameters = new ListReceivedEmailsParams
        {
            Limit = 10,
            After = "cursor-abc",
            Before = "cursor-xyz"
        };
        _receivingLibrary.ListReceivedEmails(parameters);

        Assert.IsTrue(capturedUri.StartsWith("receiving?"));
        StringAssert.Contains("limit=10", capturedUri);
        StringAssert.Contains("after=cursor-abc", capturedUri);
        StringAssert.Contains("before=cursor-xyz", capturedUri);
    }

    [Test]
    public void TestListReceivedEmailsEmptyListReturnsEmptyData()
    {
        string apiResponse = EmptyResponse();
        MockApiResponse(apiResponse);

        ReceivedEmailListResponse result = _receivingLibrary.ListReceivedEmails();

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
                    ["id"] = "msg-001",
                    ["from"] = "sender@example.com",
                    ["to"] = new List<string> { "info@acme.paubox.net" },
                    ["subject"] = "Test email",
                    ["body_text"] = "Hello world",
                    ["received_at"] = "2026-03-15T10:00:00Z"
                },
                new Dictionary<string, object>
                {
                    ["id"] = "msg-002",
                    ["from"] = "another@example.com",
                    ["to"] = new List<string> { "support@acme.paubox.net" },
                    ["subject"] = "Another email",
                    ["body_text"] = "More content",
                    ["received_at"] = "2026-03-16T12:00:00Z"
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
