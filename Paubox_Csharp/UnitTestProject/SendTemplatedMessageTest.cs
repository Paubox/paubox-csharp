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
public class SendTemplatedMessageTest
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
    public void TestSendTemplatedMessageHappyCaseReturnsTheSourceTrackingIdAndData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        TemplatedMessage message = CreateTestMessage();
        SendMessageResponse result = _emailLibrary.SendTemplatedMessage(message);

        Assert.IsNotNull(result);
        Assert.AreEqual("3d38ab13-0af8-4028-bd45-52e882e0d584", result.SourceTrackingId);
        Assert.AreEqual("Service OK", result.Data);
        Assert.IsNull(result.Errors);
    }

    [Test]
    public void TestSendTemplatedMessageSendsTheCorrectPayloadToThePauboxAPI()
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
        ).Callback<string, string, string, string, string>((url, uri, auth, verb, body) => {
            capturedRequestBody = body;
        }).Returns(apiResponse);

        TemplatedMessage message = CreateTestMessage();
        SendMessageResponse result = _emailLibrary.SendTemplatedMessage(message);

        string expectedPayload = (new JObject
            {
                ["data"] = new JObject
                {
                    ["template_name"] = "Example Template",
                    ["template_values"] = "{\"first_name\":\"John\",\"last_name\":\"Doe\"}",
                    ["message"] = new JObject
                    {
                        ["recipients"] = new JArray { "someone@domain.com", "someoneelse@domain.com" },
                        ["bcc"] = new JArray { "bcc-recipient@domain.com" },
                        ["cc"] = new JArray { "cc-recipient@domain.com" },
                        ["headers"] = new JObject
                        {
                            ["subject"] = "Testing!",
                            ["from"] = "you@yourdomain.com",
                            ["reply-to"] = "reply-to@yourdomain.com"
                        },
                        ["allowNonTLS"] = false,
                        ["attachments"] = null
                    }
                }
            }
        ).ToString(Formatting.None);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.net/v1/testApiUser/"),
                It.Is<string>(uri => uri == "templated_messages"),
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
    private TemplatedMessage CreateTestMessage()
    {
        return new TemplatedMessage() {
            TemplateName = "Example Template",
            TemplateValues = new Dictionary<string, string> {
                { "first_name", "John" },
                { "last_name", "Doe" }
            },
            Recipients = new string[] { "someone@domain.com", "someoneelse@domain.com" },
            Cc = new string[] { "cc-recipient@domain.com" },
            Bcc = new string[] { "bcc-recipient@domain.com" },
            Header = new Header() {
                Subject = "Testing!",
                From = "you@yourdomain.com",
                ReplyTo = "reply-to@yourdomain.com",
            },
            Attachments = null
        };
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
}
