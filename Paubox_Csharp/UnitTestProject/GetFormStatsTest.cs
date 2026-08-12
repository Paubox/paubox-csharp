using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class GetFormStatsTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object, "test-api-key");
    }

    [Test]
    public void TestGetFormStatsHappyCaseReturnsStats()
    {
        MockApiResponse(SuccessResponse());

        FormStats result = _formsLibrary.GetFormStats();

        Assert.IsNotNull(result);
        Assert.AreEqual(12, result.ActiveFormCount);
        Assert.AreEqual(3456, result.TotalSubmissionCount);
        Assert.AreEqual(78, result.SubmissionsLast7Days);
    }

    [Test]
    public void TestGetFormStatsWithCustomerIdHappyCaseReturnsStats()
    {
        MockApiResponse(SuccessResponse());

        FormStats result = _formsLibrary.GetFormStats(123);

        Assert.IsNotNull(result);
        Assert.AreEqual(12, result.ActiveFormCount);
        Assert.AreEqual(3456, result.TotalSubmissionCount);
        Assert.AreEqual(78, result.SubmissionsLast7Days);
    }

    [Test]
    public void TestGetFormStatsRequestVerificationWithoutCustomerId()
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

        _formsLibrary.GetFormStats();

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual("api/forms/stats", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("GET", capturedVerb);
        Assert.IsTrue(string.IsNullOrEmpty(capturedBody), "GET should send an empty body");
    }

    [Test]
    public void TestGetFormStatsRequestVerificationWithCustomerId()
    {
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
                    capturedRequestUri = uri;
                    capturedAuth = auth;
                    capturedVerb = verb;
                })
            .Returns(SuccessResponse());

        _formsLibrary.GetFormStats(456);

        Assert.AreEqual("api/forms/stats?customer_id=456", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("GET", capturedVerb);
    }

    [Test]
    public void TestGetFormStatsMissingApiKeyThrowsInvalidOperationException()
    {
        var libraryWithoutKey = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(() => libraryWithoutKey.GetFormStats());
        VerifyNoHttpCall();
    }

    [Test]
    public void TestGetFormStatsWithCustomerIdMissingApiKeyThrowsInvalidOperationException()
    {
        var libraryWithoutKey = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(() => libraryWithoutKey.GetFormStats(123));
        VerifyNoHttpCall();
    }

    [Test]
    public void TestGetFormStatsErrorResponseThrowsPauboxApiExceptionWithRawBody()
    {
        // A response that does not deserialize to a FormStats object (JSON null)
        string errorResponse = "null";
        MockApiResponse(errorResponse);

        var exception = Assert.Throws<PauboxApiException>(() => _formsLibrary.GetFormStats());

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
            ["active_form_count"] = 12,
            ["total_submission_count"] = 3456,
            ["submissions_last_7_days"] = 78
        });
    }
}
