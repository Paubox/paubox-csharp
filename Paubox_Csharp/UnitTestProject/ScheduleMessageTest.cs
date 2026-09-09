using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class ScheduleMessageTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private EmailLibrary _emailLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _emailLibrary = new EmailLibrary("testApiKey", _mockApiHelper.Object);
    }

    [Test]
    public void TestScheduleMessageReturnsSourceTrackingIdAndState()
    {
        string apiResponse = JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["sourceTrackingId"] = "sched-001",
            ["scheduledAt"] = "2025-12-25T15:00:00Z",
            ["state"] = "pending",
            ["data"] = "Service OK"
        });

        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.Is<string>(uri => uri == "schedule"),
                It.IsAny<string>(),
                "POST",
                It.IsAny<string>()
            )
        ).Returns(apiResponse);

        Message message = CreateTestMessage();
        ScheduleMessageResponse result = _emailLibrary.ScheduleMessage(message, "2025-12-25T15:00:00Z");

        Assert.IsNotNull(result);
        Assert.AreEqual("sched-001", result.SourceTrackingId);
        Assert.AreEqual("pending", result.State);
        Assert.AreEqual("2025-12-25T15:00:00Z", result.ScheduledAt);
    }

    [Test]
    public void TestScheduleMessageSendsCorrectVerb()
    {
        string apiResponse = JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["sourceTrackingId"] = "sched-001",
            ["scheduledAt"] = "2025-12-25T15:00:00Z",
            ["state"] = "pending",
            ["data"] = "Service OK"
        });

        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )
        ).Returns(apiResponse);

        _emailLibrary.ScheduleMessage(CreateTestMessage(), "2025-12-25T15:00:00Z");

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "schedule"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "POST"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestGetScheduledMessageReturnsStatus()
    {
        string apiResponse = JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["sourceTrackingId"] = "sched-001",
            ["scheduledAt"] = "2025-12-25T15:00:00Z",
            ["state"] = "pending",
            ["messageId"] = 12345
        });

        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.Is<string>(uri => uri == "schedule/sched-001"),
                It.IsAny<string>(),
                "GET",
                It.IsAny<string>()
            )
        ).Returns(apiResponse);

        ScheduledMessageStatusResponse result = _emailLibrary.GetScheduledMessage("sched-001");

        Assert.IsNotNull(result);
        Assert.AreEqual("sched-001", result.SourceTrackingId);
        Assert.AreEqual("pending", result.State);
    }

    [Test]
    public void TestRescheduleMessageSendsPatch()
    {
        string apiResponse = JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["sourceTrackingId"] = "sched-001",
            ["scheduledAt"] = "2025-12-26T10:00:00Z",
            ["data"] = "Rescheduled"
        });

        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )
        ).Returns(apiResponse);

        RescheduleResponse result = _emailLibrary.RescheduleMessage("sched-001", "2025-12-26T10:00:00Z");

        Assert.IsNotNull(result);
        Assert.AreEqual("2025-12-26T10:00:00Z", result.ScheduledAt);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.Is<string>(uri => uri == "schedule/sched-001"),
                It.IsAny<string>(),
                It.Is<string>(verb => verb == "PATCH"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestCancelScheduledMessagePostsToCancel()
    {
        string apiResponse = JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["sourceTrackingId"] = "sched-001",
            ["state"] = "cancelled",
            ["data"] = "Cancelled"
        });

        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )
        ).Returns(apiResponse);

        CancelScheduledResponse result = _emailLibrary.CancelScheduledMessage("sched-001");

        Assert.IsNotNull(result);
        Assert.AreEqual("cancelled", result.State);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.Is<string>(uri => uri == "schedule/sched-001/cancel"),
                It.IsAny<string>(),
                It.Is<string>(verb => verb == "POST"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    private Message CreateTestMessage()
    {
        Message message = new Message();
        message.Recipients = new string[] { "someone@domain.com" };

        Header header = new Header();
        header.From = "you@yourdomain.com";
        header.Subject = "Test scheduled email";
        message.Header = header;

        Content content = new Content();
        content.PlainText = "This is a scheduled test.";
        message.Content = content;

        return message;
    }
}
