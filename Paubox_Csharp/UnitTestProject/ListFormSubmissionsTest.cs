using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class ListFormSubmissionsTest
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
    public void TestListFormSubmissionsHappyCaseReturnsSubmissions()
    {
        MockApiResponse(SuccessResponse());

        FormSubmissionListResponse result = _formsLibrary.ListFormSubmissions(FormId);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Data.Count);
        Assert.AreEqual(57, result.Total);
        Assert.AreEqual(1, result.Page);
        Assert.AreEqual(25, result.Items);

        FormSubmission first = result.Data[0];
        Assert.AreEqual("11111111-2222-3333-4444-555555555555", first.Id);
        Assert.AreEqual(FormId, first.FormId);
        Assert.AreEqual("{\"first_name\":\"Jane\"}", first.FormData);
        Assert.AreEqual("s3", first.StorageType);
        Assert.AreEqual("https://storage.example.com/sub-1", first.StorageUrl);
        Assert.AreEqual("jane@example.com", first.SubmitterEmail);
        Assert.AreEqual("intake@clinic.example.com", first.Recipients);
        Assert.AreEqual("consent.pdf", first.AttachmentName);
        Assert.AreEqual("https://storage.example.com/consent.pdf", first.AttachmentUrl);
        Assert.AreEqual("application/pdf", first.AttachmentType);
        Assert.AreEqual(new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc),
            first.CreatedAt.ToUniversalTime());

        Assert.AreEqual("66666666-7777-8888-9999-000000000000", result.Data[1].Id);
    }

    [Test]
    public void TestListFormSubmissionsNoParamsCallsCorrectUrlAndVerb()
    {
        string capturedBaseUrl = null;
        string capturedRequestUri = null;
        string capturedVerb = null;
        string capturedBody = "sentinel";

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
                    capturedVerb = verb;
                    capturedBody = body;
                })
            .Returns(SuccessResponse());

        _formsLibrary.ListFormSubmissions(FormId);

        Assert.AreEqual("https://api.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual($"api/forms/{FormId}/submissions", capturedRequestUri);
        Assert.AreEqual("GET", capturedVerb);
        Assert.AreEqual("", capturedBody);
    }

    [Test]
    public void TestListFormSubmissionsBuildsFullQueryString()
    {
        string capturedRequestUri = null;

        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => { capturedRequestUri = uri; })
            .Returns(SuccessResponse());

        _formsLibrary.ListFormSubmissions(FormId, new SubmissionListParams
        {
            SubmissionId = "11111111-2222-3333-4444-555555555555",
            OrderBy = "created_at",
            Order = "desc",
            Page = 2,
            Items = 50
        });

        Assert.AreEqual(
            $"api/forms/{FormId}/submissions" +
            "?submission_id=11111111-2222-3333-4444-555555555555" +
            "&order_by=created_at&order=desc&page=2&items=50",
            capturedRequestUri);
    }

    [Test]
    public void TestListFormSubmissionsOmitsUnsetParams()
    {
        string capturedRequestUri = null;

        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => { capturedRequestUri = uri; })
            .Returns(SuccessResponse());

        _formsLibrary.ListFormSubmissions(FormId, new SubmissionListParams
        {
            Page = 3
        });

        Assert.AreEqual($"api/forms/{FormId}/submissions?page=3", capturedRequestUri);
    }

    [Test]
    public void TestListFormSubmissionsUrlEncodesQueryValues()
    {
        string capturedRequestUri = null;

        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => { capturedRequestUri = uri; })
            .Returns(SuccessResponse());

        _formsLibrary.ListFormSubmissions(FormId, new SubmissionListParams
        {
            OrderBy = "created at&x"
        });

        Assert.AreEqual($"api/forms/{FormId}/submissions?order_by=created%20at%26x",
            capturedRequestUri);
    }

    [Test]
    public void TestListFormSubmissionsPassesBearerAuthHeader()
    {
        string capturedAuth = null;

        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => { capturedAuth = auth; })
            .Returns(SuccessResponse());

        _formsLibrary.ListFormSubmissions(FormId);

        Assert.AreEqual("Bearer test-api-key", capturedAuth);
    }

    [Test]
    public void TestListFormSubmissionsNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.ListFormSubmissions(null));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestListFormSubmissionsEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.ListFormSubmissions(""));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestListFormSubmissionsWhitespaceFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.ListFormSubmissions("   "));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestListFormSubmissionsMissingApiKeyThrowsInvalidOperationException()
    {
        var libraryWithoutKey = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(
            () => libraryWithoutKey.ListFormSubmissions(FormId));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestListFormSubmissionsErrorResponseThrowsPauboxApiExceptionWithRawBody()
    {
        string errorBody = "{\"error\":\"Form not found\"}";
        MockApiResponse(errorBody);

        var exception = Assert.Throws<PauboxApiException>(
            () => _formsLibrary.ListFormSubmissions(FormId));

        Assert.AreEqual(errorBody, exception.Body);
    }

    [Test]
    public void TestListFormSubmissionsEmptyResponseThrowsPauboxApiException()
    {
        MockApiResponse("{}");

        Assert.Throws<PauboxApiException>(() => _formsLibrary.ListFormSubmissions(FormId));
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
            ["data"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["id"] = "11111111-2222-3333-4444-555555555555",
                    ["form_id"] = FormId,
                    ["form_data"] = "{\"first_name\":\"Jane\"}",
                    ["storage_type"] = "s3",
                    ["storage_url"] = "https://storage.example.com/sub-1",
                    ["submitter_email"] = "jane@example.com",
                    ["recipients"] = "intake@clinic.example.com",
                    ["attachment"] = null,
                    ["attachment_name"] = "consent.pdf",
                    ["attachment_url"] = "https://storage.example.com/consent.pdf",
                    ["attachment_type"] = "application/pdf",
                    ["created_at"] = "2024-06-01T08:00:00Z"
                },
                new Dictionary<string, object>
                {
                    ["id"] = "66666666-7777-8888-9999-000000000000",
                    ["form_id"] = FormId,
                    ["form_data"] = "{\"first_name\":\"John\"}",
                    ["storage_type"] = "s3",
                    ["created_at"] = "2024-06-02T09:30:00Z"
                }
            },
            ["total"] = 57,
            ["page"] = 1,
            ["items"] = 25
        });
    }
}
