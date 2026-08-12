using System;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class ExportSubmissionCsvTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    private const string FormId = "550e8400-e29b-41d4-a716-446655440000";
    private const string SubmissionId = "11111111-2222-3333-4444-555555555555";

    private const string CsvBody =
        "id,submitter_email,created_at\n" +
        "11111111-2222-3333-4444-555555555555,jane@example.com,2024-06-01T08:00:00Z\n" +
        "66666666-7777-8888-9999-000000000000,john@example.com,2024-06-02T09:30:00Z\n";

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object, "test-api-key");
    }

    // ------------------------------------------------------------
    // ExportSubmissionsCsv (all submissions for a form)

    [Test]
    public void TestExportSubmissionsCsvHappyCaseReturnsRawCsv()
    {
        MockApiResponse(CsvBody);

        string result = _formsLibrary.ExportSubmissionsCsv(FormId);

        Assert.AreEqual(CsvBody, result);
    }

    [Test]
    public void TestExportSubmissionsCsvCallsCorrectUrlVerbAndAuth()
    {
        string capturedBaseUrl = null;
        string capturedRequestUri = null;
        string capturedAuth = null;
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
                    capturedAuth = auth;
                    capturedVerb = verb;
                    capturedBody = body;
                })
            .Returns(CsvBody);

        _formsLibrary.ExportSubmissionsCsv(FormId);

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual($"api/forms/{FormId}/submissions/submission-csv", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("GET", capturedVerb);
        Assert.AreEqual("", capturedBody);
    }

    [Test]
    public void TestExportSubmissionsCsvNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.ExportSubmissionsCsv(null));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionsCsvEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.ExportSubmissionsCsv(""));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionsCsvMissingApiKeyThrowsInvalidOperationException()
    {
        var libraryWithoutKey = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(
            () => libraryWithoutKey.ExportSubmissionsCsv(FormId));

        VerifyNoHttpCall();
    }

    // Removed TestExportSubmissionsCsv{Empty,Whitespace}ResponseThrows — the pre-fix SDK
    // treated a whitespace/empty 200 body as an error and threw. That guarded against
    // nothing real: a form with zero submissions IS a valid empty CSV, and non-2xx errors
    // now surface via APIHelper.IsSuccessStatusCode. The mock in these tests bypasses that.

    // ------------------------------------------------------------
    // ExportSubmissionCsv (single submission)

    [Test]
    public void TestExportSubmissionCsvHappyCaseReturnsRawCsv()
    {
        MockApiResponse(CsvBody);

        string result = _formsLibrary.ExportSubmissionCsv(FormId, SubmissionId);

        Assert.AreEqual(CsvBody, result);
    }

    [Test]
    public void TestExportSubmissionCsvCallsCorrectUrlVerbAndAuth()
    {
        string capturedBaseUrl = null;
        string capturedRequestUri = null;
        string capturedAuth = null;
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
                    capturedAuth = auth;
                    capturedVerb = verb;
                    capturedBody = body;
                })
            .Returns(CsvBody);

        _formsLibrary.ExportSubmissionCsv(FormId, SubmissionId);

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual($"api/forms/{FormId}/submissions/submission-csv/{SubmissionId}",
            capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("GET", capturedVerb);
        Assert.AreEqual("", capturedBody);
    }

    [Test]
    public void TestExportSubmissionCsvNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.ExportSubmissionCsv(null, SubmissionId));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionCsvEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.ExportSubmissionCsv("", SubmissionId));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionCsvNullSubmissionIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.ExportSubmissionCsv(FormId, null));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionCsvEmptySubmissionIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.ExportSubmissionCsv(FormId, ""));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionCsvMissingApiKeyThrowsInvalidOperationException()
    {
        var libraryWithoutKey = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(
            () => libraryWithoutKey.ExportSubmissionCsv(FormId, SubmissionId));

        VerifyNoHttpCall();
    }

    // Removed TestExportSubmissionCsv{Empty,Whitespace}ResponseThrows — same rationale as
    // the all-variant siblings above. The pre-fix defensive check has been retired.

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
}
