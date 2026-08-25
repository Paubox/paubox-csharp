using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class ArchiveFormTest
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
    public void TestArchiveFormHappyCaseReturnsDetail()
    {
        MockApiResponse(SuccessResponse());

        string result = _formsLibrary.ArchiveForm(FormId);

        Assert.AreEqual("Form archived.", result);
    }

    [Test]
    public void TestArchiveFormRequestVerification()
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
            .Returns(SuccessResponse());

        _formsLibrary.ArchiveForm(FormId);

        Assert.AreEqual("https://api.paubox.com/v1/forms/", capturedBaseUrl);
        Assert.AreEqual($"api/forms/{FormId}/archive", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("POST", capturedVerb);
        Assert.IsTrue(string.IsNullOrEmpty(capturedBody), "Archive should send an empty body");
    }

    [Test]
    public void TestArchiveFormNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.ArchiveForm(null));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestArchiveFormEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.ArchiveForm(""));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestArchiveFormWhitespaceFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.ArchiveForm("   "));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestArchiveFormMissingApiKeyThrowsInvalidOperationException()
    {
        var libraryWithoutKey = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(() => libraryWithoutKey.ArchiveForm(FormId));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestArchiveFormErrorResponseThrowsPauboxApiExceptionWithRawBody()
    {
        string errorResponse = "{\"error\": \"Form not found\"}";
        MockApiResponse(errorResponse);

        var exception = Assert.Throws<PauboxApiException>(() => _formsLibrary.ArchiveForm(FormId));

        Assert.AreEqual(errorResponse, exception.Body);
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

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["detail"] = "Form archived."
        });
    }
}
