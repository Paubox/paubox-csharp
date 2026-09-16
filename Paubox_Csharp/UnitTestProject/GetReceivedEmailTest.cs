using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class GetReceivedEmailTest
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
    public void TestGetReceivedEmailReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        ReceivedEmailResponse result = _receivingLibrary.GetReceivedEmail("msg-001");

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("msg-001", result.Data.Id);
        Assert.AreEqual("sender@example.com", result.Data.From);
        Assert.AreEqual("Test email", result.Data.Subject);
        Assert.AreEqual("Hello world", result.Data.BodyText);
        Assert.AreEqual("<p>Hello world</p>", result.Data.BodyHtml);
    }

    [Test]
    public void TestGetReceivedEmailSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        _receivingLibrary.GetReceivedEmail("msg-001");

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/msg-001"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "GET"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestGetReceivedEmailWithAttachments()
    {
        string apiResponse = ResponseWithAttachments();
        MockApiResponse(apiResponse);

        ReceivedEmailResponse result = _receivingLibrary.GetReceivedEmail("msg-001");

        Assert.IsNotNull(result.Data.Attachments);
        Assert.AreEqual(1, result.Data.Attachments.Count);
        Assert.AreEqual("blob-abc", result.Data.Attachments[0].Id);
        Assert.AreEqual("report.pdf", result.Data.Attachments[0].Filename);
        Assert.AreEqual("application/pdf", result.Data.Attachments[0].ContentType);
        Assert.AreEqual(204800, result.Data.Attachments[0].Size);
    }

    [Test]
    public void TestGetReceivedEmailThrowsWhenIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.GetReceivedEmail(""));
    }

    [Test]
    public void TestGetReceivedEmailThrowsWhenIdIsNull()
    {
        Assert.Throws<ArgumentException>(() => _receivingLibrary.GetReceivedEmail(null));
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
                ["id"] = "msg-001",
                ["from"] = "sender@example.com",
                ["to"] = new List<string> { "info@acme.paubox.net" },
                ["subject"] = "Test email",
                ["body_text"] = "Hello world",
                ["body_html"] = "<p>Hello world</p>",
                ["received_at"] = "2026-03-15T10:00:00Z"
            }
        });
    }

    private string ResponseWithAttachments()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["data"] = new Dictionary<string, object>
            {
                ["id"] = "msg-001",
                ["from"] = "sender@example.com",
                ["to"] = new List<string> { "info@acme.paubox.net" },
                ["subject"] = "Test email",
                ["body_text"] = "See attached",
                ["received_at"] = "2026-03-15T10:00:00Z",
                ["attachments"] = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "blob-abc",
                        ["filename"] = "report.pdf",
                        ["content_type"] = "application/pdf",
                        ["size"] = 204800
                    }
                }
            }
        });
    }
}
