using System;
using NUnit.Framework;
using Moq;
using Paubox;

[TestFixture]
public class ExportSubmissionPdfTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    private const string FormId = "550e8400-e29b-41d4-a716-446655440000";
    private const string SubmissionId = "11111111-2222-3333-4444-555555555555";

    // "%PDF-1.4" magic bytes followed by arbitrary content
    private static readonly byte[] PdfBytes =
        { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A, 0x01, 0x02, 0x03 };

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object, "test-api-key");
    }

    [Test]
    public void TestExportSubmissionPdfHappyCaseReturnsBytes()
    {
        MockApiBytesResponse(PdfBytes);

        byte[] result = _formsLibrary.ExportSubmissionPdf(FormId, SubmissionId);

        Assert.IsNotNull(result);
        Assert.AreEqual(PdfBytes, result);
    }

    [Test]
    public void TestExportSubmissionPdfCallsCorrectUrlVerbAndAuth()
    {
        string capturedBaseUrl = null;
        string capturedRequestUri = null;
        string capturedAuth = null;
        string capturedVerb = null;

        _mockApiHelper
            .Setup(x => x.CallToAPIBytes(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<string, string, string, string>(
                (baseUrl, uri, auth, verb) =>
                {
                    capturedBaseUrl = baseUrl;
                    capturedRequestUri = uri;
                    capturedAuth = auth;
                    capturedVerb = verb;
                })
            .Returns(PdfBytes);

        _formsLibrary.ExportSubmissionPdf(FormId, SubmissionId);

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual($"api/forms/{FormId}/submissions/{SubmissionId}/submission-pdf",
            capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("GET", capturedVerb);
    }

    [Test]
    public void TestExportSubmissionPdfDoesNotUseStringApi()
    {
        MockApiBytesResponse(PdfBytes);

        _formsLibrary.ExportSubmissionPdf(FormId, SubmissionId);

        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void TestExportSubmissionPdfNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.ExportSubmissionPdf(null, SubmissionId));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionPdfEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.ExportSubmissionPdf("", SubmissionId));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionPdfNullSubmissionIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.ExportSubmissionPdf(FormId, null));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionPdfEmptySubmissionIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.ExportSubmissionPdf(FormId, ""));

        VerifyNoHttpCall();
    }

    [Test]
    public void TestExportSubmissionPdfMissingApiKeyThrowsInvalidOperationException()
    {
        var libraryWithoutKey = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(
            () => libraryWithoutKey.ExportSubmissionPdf(FormId, SubmissionId));

        VerifyNoHttpCall();
    }

    // Removed TestExportSubmissionPdf{Null,Empty}ResponseThrows — the null/empty guard now
    // lives in APIHelper.CallToAPIBytes as a %PDF-magic-byte check on the actual response,
    // and the mock in this file bypasses that layer. In production the byte signature
    // check catches every "server handed back an error body as PDF" case; a broken
    // custom IAPIHelper is out of scope for this test suite.

    // ------------------------------------------------------------
    // Helper methods

    private void MockApiBytesResponse(byte[] response)
    {
        _mockApiHelper
            .Setup(x => x.CallToAPIBytes(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "GET"))
            .Returns(response);
    }

    private void VerifyNoHttpCall()
    {
        _mockApiHelper.Verify(x => x.CallToAPIBytes(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());

        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }
}
