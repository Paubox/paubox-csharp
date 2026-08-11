using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class ListFormsTest
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
    public void TestListFormsHappyCaseReturnsResultsAndPageInfo()
    {
        MockApiResponse(SuccessResponse());

        FormsListResponse result = _formsLibrary.ListForms();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(2, result.Results.Count);
        Assert.AreEqual("550e8400-e29b-41d4-a716-446655440000", result.Results[0].Id);
        Assert.AreEqual("Patient Intake Form", result.Results[0].Title);
        Assert.AreEqual(123, result.Results[0].CustomerId);
        Assert.IsTrue(result.Results[0].Active);
        Assert.AreEqual("660e8400-e29b-41d4-a716-446655440111", result.Results[1].Id);
        Assert.AreEqual("Consent Form", result.Results[1].Title);
        Assert.IsNotNull(result.PageInfo);
        Assert.AreEqual(2, result.PageInfo.Count);
        Assert.AreEqual(1, result.PageInfo.Pages);
        Assert.AreEqual(1, result.PageInfo.Page);
        Assert.AreEqual(25, result.PageInfo.Items);
    }

    [Test]
    public void TestListFormsNoParametersCallsBareUri()
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

        _formsLibrary.ListForms();

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual("api/forms", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("GET", capturedVerb);
    }

    [Test]
    public void TestListFormsNullParametersObjectCallsBareUri()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(null);

        Assert.AreEqual("api/forms", capturedRequestUri);
    }

    [Test]
    public void TestListFormsEmptyParametersObjectCallsBareUri()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams());

        Assert.AreEqual("api/forms", capturedRequestUri);
    }

    [Test]
    public void TestListFormsAllParametersBuildsFullQueryString()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams
        {
            CustomerId = 123,
            FormId = "550e8400-e29b-41d4-a716-446655440000",
            Search = "intake",
            Order = "desc",
            OrderBy = "created_at",
            Archived = false,
            Active = true,
            Page = 2,
            Items = 50
        });

        Assert.AreEqual(
            "api/forms?customer_id=123" +
            "&form_id=550e8400-e29b-41d4-a716-446655440000" +
            "&search=intake" +
            "&order=desc" +
            "&order_by=created_at" +
            "&archived=false" +
            "&active=true" +
            "&page=2" +
            "&items=50",
            capturedRequestUri);
    }

    [Test]
    public void TestListFormsSingleParameterCustomerId()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7 });

        Assert.AreEqual("api/forms?customer_id=7", capturedRequestUri);
    }

    [Test]
    public void TestListFormsSingleParameterFormId()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { FormId = "abc-123" });

        Assert.AreEqual("api/forms?form_id=abc-123", capturedRequestUri);
    }

    [Test]
    public void TestListFormsSingleParameterSearchIsUrlEncoded()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { Search = "patient intake & consent" });

        Assert.AreEqual("api/forms?search=patient%20intake%20%26%20consent", capturedRequestUri);
    }

    [Test]
    public void TestListFormsSingleParameterOrder()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { Order = "asc" });

        Assert.AreEqual("api/forms?order=asc", capturedRequestUri);
    }

    [Test]
    public void TestListFormsSingleParameterOrderBy()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { OrderBy = "title" });

        Assert.AreEqual("api/forms?order_by=title", capturedRequestUri);
    }

    [Test]
    public void TestListFormsSingleParameterArchivedTrueIsLowercase()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { Archived = true });

        Assert.AreEqual("api/forms?archived=true", capturedRequestUri);
    }

    [Test]
    public void TestListFormsSingleParameterActiveFalseIsLowercase()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { Active = false });

        Assert.AreEqual("api/forms?active=false", capturedRequestUri);
    }

    [Test]
    public void TestListFormsSingleParameterPage()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { Page = 3 });

        Assert.AreEqual("api/forms?page=3", capturedRequestUri);
    }

    [Test]
    public void TestListFormsSingleParameterItems()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { Items = 100 });

        Assert.AreEqual("api/forms?items=100", capturedRequestUri);
    }

    [Test]
    public void TestListFormsMissingApiKeyThrowsInvalidOperationException()
    {
        var library = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(() => library.ListForms());

        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void TestListFormsErrorResponseThrowsSystemExceptionWithRawBody()
    {
        string errorBody = "{\"error\": \"Invalid or expired API key\"}";
        MockApiResponse(errorBody);

        var exception = Assert.Throws<SystemException>(() => _formsLibrary.ListForms());

        Assert.AreEqual(errorBody, exception.Message);
    }

    [Test]
    public void TestListFormsEmptyObjectResponseThrowsSystemException()
    {
        MockApiResponse("{}");

        Assert.Throws<SystemException>(() => _formsLibrary.ListForms());
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

    private void CaptureRequestUri(Action<string> capture)
    {
        _mockApiHelper
            .Setup(x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<string, string, string, string, string>(
                (baseUrl, uri, auth, verb, body) => capture(uri))
            .Returns(SuccessResponse());
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["results"] = new object[]
            {
                new Dictionary<string, object>
                {
                    ["id"] = "550e8400-e29b-41d4-a716-446655440000",
                    ["title"] = "Patient Intake Form",
                    ["customer_id"] = 123,
                    ["active"] = true,
                    ["version"] = 1
                },
                new Dictionary<string, object>
                {
                    ["id"] = "660e8400-e29b-41d4-a716-446655440111",
                    ["title"] = "Consent Form",
                    ["customer_id"] = 123,
                    ["active"] = false,
                    ["version"] = 2
                }
            },
            ["page_info"] = new Dictionary<string, object>
            {
                ["count"] = 2,
                ["pages"] = 1,
                ["page"] = 1,
                ["items"] = 25
            }
        });
    }
}
