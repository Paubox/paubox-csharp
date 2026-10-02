using System;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class ListReceivedEmailsTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private ReceivingLibrary _receivingLibrary;

    private const string FirstEmailId = "0192f0c4-0000-7000-8000-000000000001";
    private const string SecondEmailId = "0192f0c4-0000-7000-8000-000000000002";

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _receivingLibrary = new ReceivingLibrary("testApiKey", _mockApiHelper.Object);
    }

    [Test]
    public void TestListReceivedEmailsReturnsData()
    {
        MockApiResponse(SuccessResponse());

        ReceivedEmailListResponse result = _receivingLibrary.ListReceivedEmails();

        Assert.IsNotNull(result);
        Assert.AreEqual("list", result.Object);
        Assert.IsTrue(result.HasMore);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Count);

        ReceivedEmail first = result.Data[0];
        Assert.AreEqual(FirstEmailId, first.Id);
        Assert.AreEqual("Sender Name", first.FromAddresses[0].Name);
        Assert.AreEqual("sender@example.com", first.FromAddresses[0].Address);
        Assert.AreEqual("info@acme.paubox.net", first.ToAddresses[0].Address);
        Assert.AreEqual("Test email", first.Subject);
        Assert.AreEqual(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc), first.ReceivedAt.ToUniversalTime());
        Assert.AreEqual(true, first.HasAttachment);
        Assert.IsFalse(first.Spam);
        Assert.AreEqual(2048, first.Size);
        Assert.AreEqual("acme.paubox.net", first.Domain);

        ReceivedEmail second = result.Data[1];
        Assert.AreEqual(SecondEmailId, second.Id);
        Assert.IsNull(second.Subject);
        Assert.AreEqual(default(DateTime), second.ReceivedAt);
        Assert.IsNull(second.HasAttachment);
        Assert.IsTrue(second.Spam);
        Assert.IsNull(second.Size);
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
            After = FirstEmailId,
            Before = SecondEmailId
        };
        _receivingLibrary.ListReceivedEmails(parameters);

        Assert.IsTrue(capturedUri.StartsWith("receiving?"));
        StringAssert.Contains("limit=10", capturedUri);
        StringAssert.Contains("after=" + FirstEmailId, capturedUri);
        StringAssert.Contains("before=" + SecondEmailId, capturedUri);
    }

    [Test]
    public void TestListReceivedEmailsEmptyListReturnsEmptyData()
    {
        MockApiResponse("""{"object":"list","data":[],"has_more":false}""");

        ReceivedEmailListResponse result = _receivingLibrary.ListReceivedEmails();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(0, result.Data.Count);
        Assert.IsFalse(result.HasMore);
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

    private static string SuccessResponse()
    {
        return $$"""
        {
          "object": "list",
          "data": [
            {
              "email_id": "{{FirstEmailId}}",
              "from": [{ "name": "Sender Name", "address": "sender@example.com" }],
              "to": [{ "name": null, "address": "info@acme.paubox.net" }],
              "subject": "Test email",
              "received_at": "2026-03-15T10:00:00Z",
              "has_attachment": true,
              "spam": false,
              "size": 2048,
              "domain": "acme.paubox.net"
            },
            {
              "email_id": "{{SecondEmailId}}",
              "from": [{ "name": null, "address": "another@example.com" }],
              "to": [{ "name": null, "address": "support@acme.paubox.net" }],
              "subject": null,
              "received_at": null,
              "has_attachment": null,
              "spam": true,
              "size": null,
              "domain": "acme.paubox.net"
            }
          ],
          "has_more": true
        }
        """;
    }
}
