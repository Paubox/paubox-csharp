using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class UpdateFormTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    private const string FormId = "550e8400-e29b-41d4-a716-446655440000";

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object, "test-api-key");
    }

    [Test]
    public void TestUpdateFormHappyCaseReturnsDetailAndFormId()
    {
        MockApiResponse(SuccessResponse());

        UpdateFormResponse result = _formsLibrary.UpdateForm(FormId,
            new UpdateFormRequest { Title = "Updated Title" });

        Assert.IsNotNull(result);
        Assert.AreEqual("Form updated.", result.Detail);
        Assert.AreEqual(FormId, result.FormId);
    }

    [Test]
    public void TestUpdateFormCallsCorrectUrlPutVerbAndAuthHeader()
    {
        string capturedBaseUrl = null;
        string capturedRequestUri = null;
        string capturedAuth = null;
        string capturedVerb = null;

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
                    capturedAuth = auth;
                    capturedVerb = verb;
                })
            .Returns(SuccessResponse());

        _formsLibrary.UpdateForm(FormId, new UpdateFormRequest { Title = "Updated Title" });

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual($"api/forms/{FormId}", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("PUT", capturedVerb);
    }

    [Test]
    public void TestUpdateFormAllFieldsPayloadVerification()
    {
        string capturedBody = null;
        CaptureBody(body => capturedBody = body);

        _formsLibrary.UpdateForm(FormId, new UpdateFormRequest
        {
            Title = "Updated Title",
            Description = "Updated description",
            FormJson = new Dictionary<string, object> { ["fields"] = new object[] { } },
            VanityUrl = "updated-intake",
            Recipient = "clinic@example.com",
            Active = false,
            SubscriptionListId = "list-42"
        });

        Assert.IsNotNull(capturedBody);
        var json = JObject.Parse(capturedBody);
        Assert.AreEqual("Updated Title", json["title"].ToString());
        Assert.AreEqual("Updated description", json["description"].ToString());
        Assert.IsNotNull(json["form_json"]);
        Assert.IsNotNull(json["form_json"]["fields"]);
        Assert.AreEqual("updated-intake", json["vanity_url"].ToString());
        Assert.AreEqual("clinic@example.com", json["recipient"].ToString());
        Assert.IsFalse((bool)json["active"]);
        Assert.AreEqual("list-42", json["subscription_list_id"].ToString());
    }

    [Test]
    public void TestUpdateFormNullFieldsAreOmittedFromBody()
    {
        string capturedBody = null;
        CaptureBody(body => capturedBody = body);

        _formsLibrary.UpdateForm(FormId, new UpdateFormRequest { Title = "Only The Title" });

        Assert.IsNotNull(capturedBody);
        var json = JObject.Parse(capturedBody);
        Assert.AreEqual("Only The Title", json["title"].ToString());
        Assert.IsNull(json["description"]);
        Assert.IsNull(json["form_json"]);
        Assert.IsNull(json["vanity_url"]);
        Assert.IsNull(json["recipient"]);
        Assert.IsNull(json["active"]);
        Assert.IsNull(json["subscription_list_id"]);
        Assert.AreEqual(1, json.Properties().Count());
    }

    [Test]
    public void TestUpdateFormEmptyRequestSerializesToEmptyObject()
    {
        string capturedBody = null;
        CaptureBody(body => capturedBody = body);

        _formsLibrary.UpdateForm(FormId, new UpdateFormRequest());

        Assert.IsNotNull(capturedBody);
        var json = JObject.Parse(capturedBody);
        Assert.AreEqual(0, json.Properties().Count());
    }

    [Test]
    public void TestUpdateFormNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.UpdateForm(null, new UpdateFormRequest { Title = "x" }));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestUpdateFormEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.UpdateForm("", new UpdateFormRequest { Title = "x" }));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestUpdateFormNullRequestThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _formsLibrary.UpdateForm(FormId, null));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestUpdateFormMissingApiKeyThrowsInvalidOperationException()
    {
        var library = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(
            () => library.UpdateForm(FormId, new UpdateFormRequest { Title = "x" }));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestUpdateFormErrorResponseThrowsPauboxApiExceptionWithRawBody()
    {
        string errorBody = "{\"error\": \"Form not found\"}";
        MockApiResponse(errorBody);

        var exception = Assert.Throws<PauboxApiException>(
            () => _formsLibrary.UpdateForm(FormId, new UpdateFormRequest { Title = "x" }));

        Assert.AreEqual(errorBody, exception.Body);
    }

    [Test]
    public void TestUpdateFormEmptyObjectResponseThrowsPauboxApiException()
    {
        MockApiResponse("{}");

        Assert.Throws<PauboxApiException>(
            () => _formsLibrary.UpdateForm(FormId, new UpdateFormRequest { Title = "x" }));
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
                "PUT",
                It.IsAny<string>()))
            .Returns(response);
    }

    private void CaptureBody(Action<string> capture)
    {
        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "PUT",
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => capture(body))
            .Returns(SuccessResponse());
    }

    private void VerifyNoHttpCall()
    {
        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["detail"] = "Form updated.",
            ["form_id"] = FormId
        });
    }
}
