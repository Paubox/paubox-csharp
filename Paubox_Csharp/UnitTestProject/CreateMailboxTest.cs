using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CreateMailboxTest
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
    public void TestCreateMailboxReturnsData()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        var request = new CreateMailboxRequest { Name = "info", Password = "secret123" };
        MailboxResponse result = _receivingLibrary.CreateMailbox(1, request);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(10, result.Data.Id);
        Assert.AreEqual("info", result.Data.Name);
        Assert.AreEqual("info@acme.paubox.net", result.Data.Email);
    }

    [Test]
    public void TestCreateMailboxSendsCorrectRequest()
    {
        MockApiResponse(SuccessResponse());

        var request = new CreateMailboxRequest { Name = "info", Password = "secret123" };
        _receivingLibrary.CreateMailbox(42, request);

        _mockApiHelper.Verify(
            x => x.CallToAPI(
                It.Is<string>(url => url == "https://api.paubox.com/v1/email/"),
                It.Is<string>(uri => uri == "receiving/domains/42/mailboxes"),
                It.Is<string>(auth => auth == "Token token=testApiKey"),
                It.Is<string>(verb => verb == "POST"),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Test]
    public void TestCreateMailboxSendsCorrectPayload()
    {
        string capturedBody = null;

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
            capturedBody = body;
        }).Returns(SuccessResponse());

        var request = new CreateMailboxRequest
        {
            Name = "info",
            Password = "secret123",
            QuotaBytes = 1073741824
        };
        _receivingLibrary.CreateMailbox(1, request);

        JObject parsed = JObject.Parse(capturedBody);
        Assert.AreEqual("info", parsed["name"].ToString());
        Assert.AreEqual("secret123", parsed["password"].ToString());
        Assert.AreEqual(1073741824, parsed["quota_bytes"].Value<long>());
    }

    [Test]
    public void TestCreateMailboxThrowsWhenRequestIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _receivingLibrary.CreateMailbox(1, null));
    }

    [Test]
    public void TestCreateMailboxThrowsWhenNameIsEmpty()
    {
        var request = new CreateMailboxRequest { Password = "secret123" };
        Assert.Throws<ArgumentException>(() => _receivingLibrary.CreateMailbox(1, request));
    }

    [Test]
    public void TestCreateMailboxThrowsWhenPasswordIsEmpty()
    {
        var request = new CreateMailboxRequest { Name = "info" };
        Assert.Throws<ArgumentException>(() => _receivingLibrary.CreateMailbox(1, request));
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
