using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SendBulkMessagesTest
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
    public void TestSendBulkMessagesHappyCaseReturnsTheSourceTrackingIdAndData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        Message[] messages = CreateTestMessages();
        SendBulkMessagesResponse result = _emailLibrary.SendBulkMessages(messages);

        // Further assertions here
    }

    // ------------------------------------------------------------
    // Helper methods
    //
    private Message[] CreateTestMessages()
    {
        Message message1 = new Message()
        {
            Recipients = new string[] { "recipient1@domain.com" },
            Cc = new string[] { "cc1@domain.com" },
            Bcc = new string[] { "bcc1@domain.com" },
            Header = new Header()
            {
                From = "from1@yourdomain.com",
                ReplyTo = "reply-to1@yourdomain.com",
                Subject = "Email 1"
            },
            Content = new Content()
            {
                PlainText = "Email 1 Content"
            }
        };

        Message message2 = new Message()
        {
            Recipients = new string[] { "recipient2@domain.com" },
            Cc = new string[] { "cc2@domain.com" },
            Bcc = new string[] { "bcc2@domain.com" },
            Header = new Header()
            {
                From = "from2@yourdomain.com",
                ReplyTo = "reply-to2@yourdomain.com",
                Subject = "Email 2"
            },
            Content = new Content()
            {
                PlainText = "Email 2 Content"
            }
        };

        return new Message[] { message1, message2 };
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
            ["Messages"] = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["SourceTrackingId"] = "11111111-1111-1111-1111-111111111111",
                    ["Data"] = "Service OK",
                },
                new Dictionary<string, object>
                {
                    ["SourceTrackingId"] = "22222222-2222-2222-2222-222222222222",
                    ["Data"] = "Service OK",
                }
            },
        });
    }
}
