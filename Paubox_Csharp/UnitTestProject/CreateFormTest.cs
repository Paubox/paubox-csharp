using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CreateFormTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    private const string NewFormId = "770e8400-e29b-41d4-a716-446655440222";

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object, "test-api-key");
    }

    [Test]
    public void TestCreateFormHappyCaseReturnsId()
    {
        MockApiResponse(SuccessResponse());

        CreateFormResponse result = _formsLibrary.CreateForm(ValidRequest());

        Assert.IsNotNull(result);
        Assert.AreEqual(NewFormId, result.Id);
    }

    [Test]
    public void TestCreateFormCallsCorrectUrlVerbAndAuthHeader()
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

        _formsLibrary.CreateForm(ValidRequest());

        Assert.AreEqual("https://api.paubox.com/v1/forms/", capturedBaseUrl);
        Assert.AreEqual("api/forms", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("POST", capturedVerb);
    }

    [Test]
    public void TestCreateFormPayloadVerification()
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
            .Returns(SuccessResponse());

        _formsLibrary.CreateForm(new CreateFormRequest
        {
            Title = "Patient Intake Form",
            Description = "Please complete before your appointment.",
            FormHtml = "<form>...</form>",
            FormJson = new Dictionary<string, object> { ["fields"] = new object[] { } },
            FormCss = "form { font-family: sans-serif; }",
            CustomerId = 123,
            Recipient = "clinic@example.com",
            Signable = true,
            SignatureConfirmationLabel = "I agree",
            SubscriptionListId = "list-42",
            Type = "intake",
            Active = true,
            Version = 1,
            SubmissionCount = 0
        });

        Assert.IsNotNull(capturedBody);
        var json = JObject.Parse(capturedBody);
        Assert.AreEqual("Patient Intake Form", json["title"].ToString());
        Assert.AreEqual("Please complete before your appointment.", json["description"].ToString());
        Assert.AreEqual("<form>...</form>", json["form_html"].ToString());
        Assert.IsNotNull(json["form_json"]);
        Assert.IsNotNull(json["form_json"]["fields"]);
        Assert.AreEqual("form { font-family: sans-serif; }", json["form_css"].ToString());
        Assert.AreEqual(123, (int)json["customer_id"]);
        Assert.AreEqual("clinic@example.com", json["recipient"].ToString());
        Assert.IsTrue((bool)json["signable"]);
        Assert.AreEqual("I agree", json["signature_confirmation_label"].ToString());
        Assert.AreEqual("list-42", json["subscription_list_id"].ToString());
        Assert.AreEqual("intake", json["type"].ToString());
        Assert.IsTrue((bool)json["active"]);
        Assert.AreEqual(1, (int)json["version"]);
        Assert.AreEqual(0, (int)json["submission_count"]);
    }

    [Test]
    public void TestCreateFormNullRequestThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _formsLibrary.CreateForm(null));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestCreateFormNullTitleThrowsArgumentException()
    {
        var request = ValidRequest();
        request.Title = null;

        Assert.Throws<ArgumentException>(() => _formsLibrary.CreateForm(request));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestCreateFormEmptyTitleThrowsArgumentException()
    {
        var request = ValidRequest();
        request.Title = "";

        Assert.Throws<ArgumentException>(() => _formsLibrary.CreateForm(request));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestCreateFormNullFormJsonThrowsArgumentException()
    {
        var request = ValidRequest();
        request.FormJson = null;

        Assert.Throws<ArgumentException>(() => _formsLibrary.CreateForm(request));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestCreateFormMissingApiKeyThrowsInvalidOperationException()
    {
        var library = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(() => library.CreateForm(ValidRequest()));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestCreateFormErrorResponseThrowsPauboxApiExceptionWithRawBody()
    {
        string errorBody = "{\"error\": \"customer_id does not exist\"}";
        MockApiResponse(errorBody);

        var exception = Assert.Throws<PauboxApiException>(
            () => _formsLibrary.CreateForm(ValidRequest()));

        Assert.AreEqual(errorBody, exception.Body);
    }

    [Test]
    public void TestCreateFormEmptyObjectResponseThrowsPauboxApiException()
    {
        MockApiResponse("{}");

        Assert.Throws<PauboxApiException>(() => _formsLibrary.CreateForm(ValidRequest()));
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

    private void VerifyNoHttpCall()
    {
        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }

    private CreateFormRequest ValidRequest()
    {
        return new CreateFormRequest
        {
            Title = "Patient Intake Form",
            FormJson = new Dictionary<string, object> { ["fields"] = new object[] { } },
            CustomerId = 123,
            Active = true,
            Version = 1
        };
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["id"] = NewFormId
        });
    }
}
