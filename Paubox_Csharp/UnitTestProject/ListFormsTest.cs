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

        FormsListResponse result = _formsLibrary.ListForms(new FormsListParams { CustomerId = 123 });

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(2, result.Results.Count);
        Assert.AreEqual("Patient Intake Form", result.Results[0].Title);
        Assert.AreEqual(123, result.Results[0].CustomerId);

        Assert.IsNotNull(result.PageInfo);
        Assert.AreEqual(2, result.PageInfo.Count);
        Assert.AreEqual(1, result.PageInfo.Pages);
        Assert.AreEqual(1, result.PageInfo.Page);
        Assert.AreEqual(25, result.PageInfo.Items);
    }

    [Test]
    public void TestListFormsMinimalParametersSendsCustomerIdOnly()
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

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 123 });

        Assert.AreEqual("https://apx.paubox.com/forms/", capturedBaseUrl);
        Assert.AreEqual("api/forms?customer_id=123", capturedRequestUri);
        Assert.AreEqual("Bearer test-api-key", capturedAuth);
        Assert.AreEqual("GET", capturedVerb);
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
    public void TestListFormsCustomerIdOnly()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7 });

        Assert.AreEqual("api/forms?customer_id=7", capturedRequestUri);
    }

    [Test]
    public void TestListFormsWithFormId()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7, FormId = "abc-123" });

        Assert.AreEqual("api/forms?customer_id=7&form_id=abc-123", capturedRequestUri);
    }

    [Test]
    public void TestListFormsSearchIsUrlEncoded()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7, Search = "patient intake & consent" });

        Assert.AreEqual("api/forms?customer_id=7&search=patient%20intake%20%26%20consent", capturedRequestUri);
    }

    [Test]
    public void TestListFormsOrder()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7, Order = "asc" });

        Assert.AreEqual("api/forms?customer_id=7&order=asc", capturedRequestUri);
    }

    [Test]
    public void TestListFormsOrderBy()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7, OrderBy = "title" });

        Assert.AreEqual("api/forms?customer_id=7&order_by=title", capturedRequestUri);
    }

    [Test]
    public void TestListFormsArchivedTrueIsLowercase()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7, Archived = true });

        Assert.AreEqual("api/forms?customer_id=7&archived=true", capturedRequestUri);
    }

    [Test]
    public void TestListFormsActiveFalseIsLowercase()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7, Active = false });

        Assert.AreEqual("api/forms?customer_id=7&active=false", capturedRequestUri);
    }

    [Test]
    public void TestListFormsPage()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7, Page = 3 });

        Assert.AreEqual("api/forms?customer_id=7&page=3", capturedRequestUri);
    }

    [Test]
    public void TestListFormsItems()
    {
        string capturedRequestUri = null;
        CaptureRequestUri(uri => capturedRequestUri = uri);

        _formsLibrary.ListForms(new FormsListParams { CustomerId = 7, Items = 100 });

        Assert.AreEqual("api/forms?customer_id=7&items=100", capturedRequestUri);
    }

    [Test]
    public void TestListFormsMissingApiKeyThrowsInvalidOperationException()
    {
        var library = new FormsLibrary(_mockApiHelper.Object);

        Assert.Throws<InvalidOperationException>(
            () => library.ListForms(new FormsListParams { CustomerId = 123 }));

        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void TestListFormsNullParametersThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _formsLibrary.ListForms(null));

        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void TestListFormsMissingCustomerIdThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _formsLibrary.ListForms(new FormsListParams { Search = "anything" }));

        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void TestListFormsPageZeroThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _formsLibrary.ListForms(new FormsListParams { CustomerId = 7, Page = 0 }));

        _mockApiHelper.Verify(x => x.CallToAPI(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void TestListFormsEmptyObjectResponseThrowsPauboxApiException()
    {
        MockApiResponse("{}");

        Assert.Throws<PauboxApiException>(
            () => _formsLibrary.ListForms(new FormsListParams { CustomerId = 123 }));
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

    // I removed the old TestListForms{Error,ErrorResponse}ThrowsPauboxApiExceptionWithRawBody tests
    // because they asserted `exception.Message == errorBody`, which no longer holds — the raw body
    // now lives on `exception.Body` (Message is verb + endpoint + status). Coverage for the new
    // shape lives in PauboxApiExceptionTest.cs, which exercises the helper directly.
}
