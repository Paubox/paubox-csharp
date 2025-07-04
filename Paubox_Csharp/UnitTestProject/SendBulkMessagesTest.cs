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
    public void TestSendBulkMessagesHappyCaseReturnsTheSourceTrackingIdsAndData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        Message[] messages = CreateTestMessages();
        SendBulkMessagesResponse result = _emailLibrary.SendBulkMessages(messages);

        Assert.IsNotNull(result);
        Assert.AreEqual("11111111-1111-1111-1111-111111111111", result.Messages[0].SourceTrackingId);
        Assert.AreEqual("22222222-2222-2222-2222-222222222222", result.Messages[1].SourceTrackingId);
        Assert.AreEqual("Service OK", result.Messages[0].Data);
        Assert.AreEqual("Service OK", result.Messages[1].Data);
        Assert.IsNull(result.Messages[0].Errors);
        Assert.IsNull(result.Messages[1].Errors);
    }

    [Test]
    public void TestSendBulkMessagesSendsTheCorrectPayloadToThePauboxAPI()
    {
        string apiResponse = SuccessResponse();
        string capturedRequestBody = null;

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
            capturedRequestBody = body;
        }).Returns(apiResponse);

        Message[] messages = CreateTestMessages();
        SendBulkMessagesResponse result = _emailLibrary.SendBulkMessages(messages);

        string expectedPayload = (new JObject
        {
            ["data"] = new JObject
            {
                ["messages"] = new JArray
                    {
                        new JObject
                        {
                            ["recipients"] = new JArray { "recipient1@domain.com" },
                            ["bcc"] = new JArray { "bcc1@domain.com" },
                            ["cc"] = new JArray { "cc1@domain.com" },
                            ["headers"] = new JObject
                            {
                                ["subject"] = "Email 1",
                                ["from"] = "from1@yourdomain.com",
                                ["reply-to"] = "reply-to1@yourdomain.com",
                                ["X-Custom-Header"] = "Custom Value",
                                ["X-Another-Header"] = "Another Value"
                            },
                            ["allowNonTLS"] = false,
                            ["content"] = new JObject
                            {
                                ["text/plain"] = "Email 1 Content",
                                ["text/html"] = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("<html><body><h1>Email 1 Content</h1></body></html>"))
                            },
                            ["attachments"] = null
                        },
                        new JObject
                        {
                            ["recipients"] = new JArray { "recipient2@domain.com" },
                            ["bcc"] = new JArray { "bcc2@domain.com" },
                            ["cc"] = new JArray { "cc2@domain.com" },
                            ["headers"] = new JObject
                            {
                                ["subject"] = "Email 2",
                                ["from"] = "from2@yourdomain.com",
                                ["reply-to"] = "reply-to2@yourdomain.com"
                            },
                            ["allowNonTLS"] = false,
                            ["content"] = new JObject
                            {
                                ["text/plain"] = "Email 2 Content",
                                ["text/html"] = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("<html><body><h1>Email 2 Content</h1></body></html>"))
                            },
                            ["attachments"] = new JArray
                            {
                                new JObject
                                {
                                    ["fileName"] = "attachment1.txt",
                                    ["contentType"] = "text/plain",
                                    ["content"] = "Attachment 1 Content"
                                }
                            }
                        }
                    }
            }
        }
        ).ToString(Formatting.None);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.net/v1/testApiUser/"),
                It.Is<string>(uri => uri == "bulk_messages"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "POST"),
                It.Is<string>(body => body == expectedPayload)
            ),
            Times.Once
        );
    }

    // ------------------------------------------------------------
    // Helper methods
    //
    private Message[] CreateTestMessages()
    {
        // Construct the first message with custom header
        Message message1 = new Message()
        {
            Recipients = new string[] { "recipient1@domain.com" },
            Cc = new string[] { "cc1@domain.com" },
            Bcc = new string[] { "bcc1@domain.com" },
            Header = new Header()
            {
                From = "from1@yourdomain.com",
                ReplyTo = "reply-to1@yourdomain.com",
                Subject = "Email 1",
                CustomHeaders = new Dictionary<string, string> {
                    { "X-Custom-Header", "Custom Value" },
                    { "X-Another-Header", "Another Value" }
                }
            },
            Content = new Content()
            {
                PlainText = "Email 1 Content",
                HtmlText = "<html><body><h1>Email 1 Content</h1></body></html>"
            }
        };

        // Construct the first message with attachments
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
                PlainText = "Email 2 Content",
                HtmlText = "<html><body><h1>Email 2 Content</h1></body></html>"
            },
            Attachments = new List<Attachment>()
            {
                new Attachment()
                {
                    FileName = "attachment1.txt",
                    ContentType = "text/plain",
                    Content = "Attachment 1 Content"
                }
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
