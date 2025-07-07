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
        _emailLibrary = new EmailLibrary("testApiKey", "testApiUser", _mockApiHelper.Object);
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
        return JsonConvert.SerializeObject(new List<DynamicTemplateSummary>
        {
            new DynamicTemplateSummary { Id = 1, Name = "Template 1", ApiCustomerId = 56789 },
            new DynamicTemplateSummary { Id = 2, Name = "Template 2", ApiCustomerId = 56789 },
            new DynamicTemplateSummary { Id = 3, Name = "Template 3", ApiCustomerId = 56789 }
        });
    }

    private string EmptyResponse()
    {
        return JsonConvert.SerializeObject(new List<DynamicTemplateSummary>{});
    }
}
