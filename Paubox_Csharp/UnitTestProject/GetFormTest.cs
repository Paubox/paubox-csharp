using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class GetFormTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private FormsLibrary _formsLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _formsLibrary = new FormsLibrary(_mockApiHelper.Object);
    }

    [Test]
    public void TestGetFormHappyCaseReturnsFormFields()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        Form result = _formsLibrary.GetForm("550e8400-e29b-41d4-a716-446655440000");

        Assert.IsNotNull(result);
        Assert.AreEqual("550e8400-e29b-41d4-a716-446655440000", result.Id);
        Assert.AreEqual("Patient Intake Form", result.Title);
        Assert.AreEqual("Please complete before your appointment.", result.Description);
        Assert.AreEqual("<form>...</form>", result.FormHtml);
        Assert.AreEqual("form { font-family: sans-serif; }", result.FormCss);
        Assert.IsTrue(result.Active);
        Assert.AreEqual(123, result.CustomerId);
        Assert.IsFalse(result.Signable);
        Assert.AreEqual(42, result.SubmissionCount);
    }

    [Test]
    public void TestGetFormNotFoundThrowsSystemException()
    {
        MockApiResponse(NotFoundResponse());

        var exception = Assert.Throws<SystemException>(
            () => _formsLibrary.GetForm("550e8400-e29b-41d4-a716-446655440000"));

        Assert.IsNotNull(exception.Message);
    }

    [Test]
    public void TestGetFormEmptyResponseThrowsSystemException()
    {
        MockApiResponse("{}");

        Assert.Throws<SystemException>(
            () => _formsLibrary.GetForm("550e8400-e29b-41d4-a716-446655440000"));
    }

    [Test]
    public void TestGetFormNullFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.GetForm(null));
    }

    [Test]
    public void TestGetFormEmptyFormIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.GetForm(""));
    }

    [Test]
    public void TestGetFormCallsCorrectUrl()
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
            .Returns(SuccessResponse());

        _formsLibrary.GetForm("550e8400-e29b-41d4-a716-446655440000");

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual("public/form_data/550e8400-e29b-41d4-a716-446655440000", capturedRequestUri);
    }

    [Test]
    public void TestGetFormPassesNoAuthorizationHeader()
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
            .Returns(SuccessResponse());

        _formsLibrary.GetForm("550e8400-e29b-41d4-a716-446655440000");

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
                "GET",
                It.IsAny<string>()))
            .Returns(response);
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["id"] = "550e8400-e29b-41d4-a716-446655440000",
            ["title"] = "Patient Intake Form",
            ["description"] = "Please complete before your appointment.",
            ["form_html"] = "<form>...</form>",
            ["form_json"] = new { },
            ["form_css"] = "form { font-family: sans-serif; }",
            ["active"] = true,
            ["customer_id"] = 123,
            ["signable"] = false,
            ["submission_count"] = 42,
            ["version"] = 1,
            ["deleted"] = false,
            ["archived"] = false,
            ["created_at"] = "2024-01-15T10:30:00Z",
            ["updated_at"] = "2024-06-01T08:00:00Z"
        });
    }

    private string NotFoundResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["error"] = "Form not found"
        });
    }
}
