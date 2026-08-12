using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class GetFormByIdTest
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
    public void TestGetFormByIdHappyCaseUnwrapsDataEnvelope()
    {
        MockApiResponse(SuccessResponse());

        Form result = _formsLibrary.GetFormById(FormId);

        Assert.IsNotNull(result);
        Assert.AreEqual(FormId, result.Id);
        Assert.AreEqual("Patient Intake Form", result.Title);
        Assert.AreEqual("Please complete before your appointment.", result.Description);
        Assert.AreEqual("<form>...</form>", result.FormHtml);
        Assert.IsTrue(result.Active);
        Assert.AreEqual(123, result.CustomerId);
        Assert.AreEqual("clinic@example.com", result.Recipient);
        Assert.AreEqual(9000, result.OldFormId);
        Assert.AreEqual("list-42", result.SubscriptionListId);
        Assert.AreEqual(42, result.SubmissionCount);
    }

    [Test]
    public void TestGetFormByIdCallsCorrectUrlVerbAndAuthHeader()
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

        _formsLibrary.GetFormById(FormId);

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual($"api/forms/{FormId}", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("GET", capturedVerb);
    }

    [Test]
    public void TestGetFormByIdNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.GetFormById(null));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestGetFormByIdEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.GetFormById(""));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestGetFormByIdMissingApiKeyThrowsInvalidOperationException()
    {
        var library = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(() => library.GetFormById(FormId));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestGetFormByIdErrorResponseThrowsPauboxApiExceptionWithRawBody()
    {
        string errorBody = "{\"error\": \"Form not found\"}";
        MockApiResponse(errorBody);

        var exception = Assert.Throws<PauboxApiException>(
            () => _formsLibrary.GetFormById(FormId));

        Assert.AreEqual(errorBody, exception.Body);
    }

    [Test]
    public void TestGetFormByIdEmptyDataEnvelopeThrowsPauboxApiException()
    {
        MockApiResponse("{\"data\": {}}");

        Assert.Throws<PauboxApiException>(() => _formsLibrary.GetFormById(FormId));
    }

    [Test]
    public void TestGetFormByIdNonJsonResponseThrowsPauboxApiExceptionWithRawBody()
    {
        string rawBody = "Internal Server Error";
        MockApiResponse(rawBody);

        var exception = Assert.Throws<PauboxApiException>(
            () => _formsLibrary.GetFormById(FormId));

        Assert.AreEqual(rawBody, exception.Body);
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
                "GET",
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
            ["data"] = new Dictionary<string, object>
            {
                ["id"] = FormId,
                ["title"] = "Patient Intake Form",
                ["description"] = "Please complete before your appointment.",
                ["form_html"] = "<form>...</form>",
                ["form_json"] = new { },
                ["active"] = true,
                ["customer_id"] = 123,
                ["recipient"] = "clinic@example.com",
                ["old_form_id"] = 9000,
                ["subscription_list_id"] = "list-42",
                ["submission_count"] = 42,
                ["version"] = 1,
                ["created_at"] = "2024-01-15T10:30:00Z",
                ["updated_at"] = "2024-06-01T08:00:00Z"
            }
        });
    }
}
