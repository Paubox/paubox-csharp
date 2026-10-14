using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class ListDynamicTemplatesTest
{
    private Mock<IAPIHelper> _mockApiHelper;
    private EmailLibrary _emailLibrary;

    [SetUp]
    public void Setup()
    {
        _mockApiHelper = new Mock<IAPIHelper>();
        _emailLibrary = new EmailLibrary("testApiKey", _mockApiHelper.Object);
    }

    [Test]
    public void TestCanSuccessfullyListDynamicTemplates()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        List<DynamicTemplateSummary> result = _emailLibrary.ListDynamicTemplates();

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);

        DynamicTemplateSummary template1 = result[0];
        Assert.AreEqual(1, template1.Id);
        Assert.AreEqual("Template 1", template1.Name);
        Assert.AreEqual(56789, template1.ApiCustomerId);
    }

    [Test]
    public void TestCanSuccessfullyListEmptyDynamicTemplates()
    {
        string apiResponse = EmptyResponse();
        MockApiResponse(apiResponse);

        List<DynamicTemplateSummary> result = _emailLibrary.ListDynamicTemplates();

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void TestStillAcceptsTheLegacyBareArrayResponse()
    {
        MockApiResponse(LegacyBareArrayResponse());

        List<DynamicTemplateSummary> result = _emailLibrary.ListDynamicTemplates();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Template 1", result[0].Name);
    }

    [Test]
    public void TestThrowsForAnUnexpectedResponse()
    {
        MockApiResponse("{\"error\":\"nope\"}");

        Assert.Throws<SystemException>(() => _emailLibrary.ListDynamicTemplates());
    }

    // ------------------------------------------------------------
    // Helper methods
    //
    private void MockApiResponse(string response)
    {
        _mockApiHelper.Setup(
            x => x.CallToAPI(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "GET",
                It.IsAny<string>()
            )
        ).Returns(response);
    }

    private string SuccessResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["templates"] = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { ["id"] = 1, ["api_customer_id"] = 56789, ["name"] = "Template 1", ["body"] = "<p>Hello {{name}}</p>", ["metadata"] = null, ["created_at"] = "2026-10-13T17:02:11.123Z", ["updated_at"] = "2026-10-13T17:02:11.123Z" },
                new Dictionary<string, object> { ["id"] = 2, ["api_customer_id"] = 56789, ["name"] = "Template 2", ["body"] = "<p>Hello {{name}}</p>", ["metadata"] = null, ["created_at"] = "2026-10-13T17:02:11.123Z", ["updated_at"] = "2026-10-13T17:02:11.123Z" },
                new Dictionary<string, object> { ["id"] = 3, ["api_customer_id"] = 56789, ["name"] = "Template 3", ["body"] = "<p>Hello {{name}}</p>", ["metadata"] = null, ["created_at"] = "2026-10-13T17:02:11.123Z", ["updated_at"] = "2026-10-13T17:02:11.123Z" }
            }
        });
    }

    private string LegacyBareArrayResponse()
    {
        return JsonConvert.SerializeObject(new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { ["id"] = 1, ["name"] = "Template 1", ["api_customer_id"] = 56789 }
        });
    }

    private string EmptyResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object> { ["templates"] = new List<object>() });
    }
}
