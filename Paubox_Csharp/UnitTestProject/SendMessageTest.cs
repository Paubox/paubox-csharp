using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class SendMessageTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private EmailLibrary _emailLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _emailLibrary = new EmailLibrary("testApiKey", "testApiUser", _mockApiHelper.Object);
    }

    [Test]
    public void TestSendMessageHappyCaseReturnsTheSourceTrackingIdAndData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        Message message = CreateTestMessage();
        SendMessageResponse result = _emailLibrary.SendMessage(message);

        Assert.IsNotNull(result);
        Assert.AreEqual("3d38ab13-0af8-4028-bd45-52e882e0d584", result.SourceTrackingId);
        Assert.AreEqual("Service OK", result.Data);
        Assert.IsNull(result.Errors);
    }

    [Test]
    public void TestSendMessageWithEmptyResponseThrowsSystemException()
    {
        string apiResponse = EmptyResponse();
        MockApiResponse(apiResponse);

        Message message = CreateTestMessage();
        var exception = Assert.Throws<SystemException>(() => _emailLibrary.SendMessage(message));

        Assert.IsNotNull(exception);
    }

    [Test]
    public void TestSendMessageForBadRequestThrowsSystemException()
    {
        string apiResponse = BadRequestResponse();
        MockApiResponse(apiResponse);

        Message message = CreateTestMessage();
        var exception = Assert.Throws<SystemException>(() => _emailLibrary.SendMessage(message));

        Assert.IsNotNull(exception);
    }

    // ------------------------------------------------------------
    // Helper methods
    //
    private Message CreateTestMessage()
    {
        Message message = new Message();
        message.Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" };
        message.Cc = new string[] { "cc-recipient@domain.com" };
        message.Bcc = new string[] { "bcc-recipient@domain.com" };

        Header header = new Header();
        header.From = "you@yourdomain.com";
        header.ReplyTo = "reply-to@yourdomain.com";
        header.Subject = "Test email";
        message.Header = header;

        Content content = new Content();
        content.PlainText = "This is a test email.";
        message.Content = content;

        return message;
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
            ["SourceTrackingId"] = "3d38ab13-0af8-4028-bd45-52e882e0d584",
            ["Data"] = "Service OK",
            ["Errors"] = null
        });
    }

    private string BadRequestResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["SourceTrackingId"] = null,
            ["Data"] = null,
            ["Errors"] = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["code"] = 400,
                    ["title"] = "Error Title",
                    ["details"] = "Description of error"
                }
            }
        });
    }

    private string EmptyResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["SourceTrackingId"] = null,
            ["Data"] = null,
            ["Errors"] = null
        });
    }
}
