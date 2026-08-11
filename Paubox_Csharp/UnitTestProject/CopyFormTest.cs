using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CopyFormTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    private const string FormId = "550e8400-e29b-41d4-a716-446655440000";
    private const string CopiedFormId = "7c9e6679-7425-40de-944b-e07fc1f90ae7";
    private const string NewTitle = "Patient Intake Form (Copy)";

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object, "test-api-key");
    }

    [Test]
    public void TestCopyFormHappyCaseReturnsNewForm()
    {
        MockApiResponse(SuccessResponse());

        Form result = _formsLibrary.CopyForm(FormId, NewTitle);

        Assert.IsNotNull(result);
        Assert.AreEqual(CopiedFormId, result.Id);
        Assert.AreEqual(NewTitle, result.Title);
        Assert.AreEqual("Please complete before your appointment.", result.Description);
        Assert.AreEqual(123, result.CustomerId);
        Assert.IsTrue(result.Active);
        Assert.AreEqual(0, result.SubmissionCount);
    }

    [Test]
    public void TestCopyFormRequestVerification()
    {
        string capturedBaseUrl = null;
        string capturedRequestUri = null;
        string capturedAuth = null;
        string capturedVerb = null;
        string capturedBody = null;

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
                    capturedBody = body;
                })
            .Returns(SuccessResponse());

        _formsLibrary.CopyForm(FormId, NewTitle);

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual("api/forms/copy", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("POST", capturedVerb);

        Assert.IsNotNull(capturedBody);
        var json = JObject.Parse(capturedBody);
        Assert.AreEqual(FormId, json["form_id"].ToString());
        Assert.AreEqual(NewTitle, json["title"].ToString());
        Assert.AreEqual(2, CountProperties(json), "Body should contain exactly form_id and title");
    }

    [Test]
    public void TestCopyFormNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.CopyForm(null, NewTitle));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestCopyFormEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.CopyForm("", NewTitle));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestCopyFormNullTitleThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.CopyForm(FormId, null));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestCopyFormEmptyTitleThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.CopyForm(FormId, ""));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestCopyFormMissingApiKeyThrowsInvalidOperationException()
    {
        var libraryWithoutKey = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(() => libraryWithoutKey.CopyForm(FormId, NewTitle));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestCopyFormErrorResponseThrowsSystemExceptionWithRawBody()
    {
        string errorResponse = "{\"error\": \"Form not found\"}";
        MockApiResponse(errorResponse);

        var exception = Assert.Throws<SystemException>(() => _formsLibrary.CopyForm(FormId, NewTitle));

        Assert.AreEqual(errorResponse, exception.Message);
    }

    [Test]
    public void TestCopyFormEmptyObjectResponseThrowsSystemException()
    {
        MockApiResponse("{}");

        Assert.Throws<SystemException>(() => _formsLibrary.CopyForm(FormId, NewTitle));
    }

    // ------------------------------------------------------------
    // Helper methods

    private static int CountProperties(JObject json)
    {
        int count = 0;
        foreach (var _ in json.Properties())
            count++;
        return count;
    }

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

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["id"] = CopiedFormId,
            ["title"] = NewTitle,
            ["description"] = "Please complete before your appointment.",
            ["form_html"] = "<form>...</form>",
            ["form_json"] = new { },
            ["form_css"] = "form { font-family: sans-serif; }",
            ["active"] = true,
            ["customer_id"] = 123,
            ["signable"] = false,
            ["submission_count"] = 0,
            ["version"] = 1,
            ["deleted"] = false,
            ["archived"] = false,
            ["created_at"] = "2024-06-01T08:00:00Z",
            ["updated_at"] = "2024-06-01T08:00:00Z"
        });
    }
}
