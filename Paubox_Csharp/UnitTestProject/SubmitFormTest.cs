using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SubmitFormTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    private const string FormId = "550e8400-e29b-41d4-a716-446655440000";

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object);
    }

    [Test]
    public void TestSubmitFormHappyCaseNoException()
    {
        MockApiResponse("");

        Assert.DoesNotThrow(() => _formsLibrary.SubmitForm(FormId, new Dictionary<string, object>
        {
            ["first_name"] = "Jane",
            ["last_name"] = "Smith",
            ["email"] = "jane@example.com"
        }));
    }

    [Test]
    public void TestSubmitFormPayloadVerification()
    {
        string capturedBody = null;
        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "POST",
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => { capturedBody = body; })
            .Returns("");

        _formsLibrary.SubmitForm(FormId, new Dictionary<string, object>
        {
            ["first_name"] = "Jane",
            ["email"] = "jane@example.com"
        });

        Assert.IsNotNull(capturedBody);
        var json = JObject.Parse(capturedBody);
        Assert.IsNotNull(json["form_data"]);
        Assert.AreEqual("Jane", json["form_data"]["first_name"].ToString());
        Assert.AreEqual("jane@example.com", json["form_data"]["email"].ToString());
        Assert.IsNull(json["attachments"]);
    }

    [Test]
    public void TestSubmitFormWithAttachments()
    {
        string capturedBody = null;
        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "POST",
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => { capturedBody = body; })
            .Returns("");

        var attachments = new[]
        {
            new FormAttachment { Name = "consent.pdf", Content = "JVBERi0xLjQ..." }
        };

        _formsLibrary.SubmitForm(FormId, new Dictionary<string, object>
        {
            ["first_name"] = "Jane"
        }, attachments);

        Assert.IsNotNull(capturedBody);
        var json = JObject.Parse(capturedBody);
        Assert.IsNotNull(json["attachments"]);
        Assert.AreEqual(1, json["attachments"].ToObject<JArray>().Count);
        Assert.AreEqual("consent.pdf", json["attachments"][0]["name"].ToString());
        Assert.AreEqual("JVBERi0xLjQ...", json["attachments"][0]["content"].ToString());
    }

    [Test]
    public void TestSubmitFormCallsCorrectUrl()
    {
        string capturedBaseUrl = null;
        string capturedRequestUri = null;

        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) =>
                {
                    capturedBaseUrl = baseUrl;
                    capturedRequestUri = uri;
                })
            .Returns("");

        _formsLibrary.SubmitForm(FormId, new Dictionary<string, object> { ["key"] = "value" });

        Assert.AreEqual("https://next.paubox.com/", capturedBaseUrl);
        Assert.AreEqual($"api/forms/{FormId}/submissions", capturedRequestUri);
    }

    [Test]
    public void TestSubmitFormNullFormDataThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _formsLibrary.SubmitForm(FormId, null));
    }

    [Test]
    public void TestSubmitFormNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.SubmitForm(null,
            new Dictionary<string, object> { ["key"] = "value" }));
    }

    [Test]
    public void TestSubmitFormEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.SubmitForm("",
            new Dictionary<string, object> { ["key"] = "value" }));
    }

    [Test]
    public void TestSubmitFormErrorResponseThrowsSystemException()
    {
        MockApiResponse("{\"error\": \"Missing required form_data field\"}");

        var exception = Assert.Throws<SystemException>(() => _formsLibrary.SubmitForm(FormId,
            new Dictionary<string, object> { ["key"] = "value" }));

        Assert.IsNotNull(exception.Message);
        StringAssert.Contains("Missing required form_data field", exception.Message);
    }

    [Test]
    public void TestSubmitFormPassesNoAuthorizationHeader()
    {
        string capturedAuth = "sentinel";

        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => { capturedAuth = auth; })
            .Returns("");

        _formsLibrary.SubmitForm(FormId, new Dictionary<string, object> { ["key"] = "value" });

        Assert.IsNull(capturedAuth);
    }

    // ------------------------------------------------------------
    // Helper methods

    private void MockApiResponse(string response)
    {
        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "POST",
                It.IsAny<string>()))
            .Returns(response);
    }
}
