using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Moq;
using Paubox;
using Newtonsoft.Json;

[TestFixture]
public class GetDynamicTemplateTest
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
    public void TestCanSuccessfullyGetADynamicTemplate()
    {
        string apiResponse = SuccessResponse();
        MockApiResponse(apiResponse);

        int templateId = 123;
        GetDynamicTemplateResponse result = _emailLibrary.GetDynamicTemplate(templateId);

        Assert.IsNotNull(result);
        Assert.AreEqual(123, result.Id);
        Assert.AreEqual("Test Template", result.Name);
        Assert.AreEqual(456, result.ApiCustomerId);
        Assert.AreEqual("<html>\n    <body>\n        <p>Hello {{first_name}} {{last_name}}!</p>\n    </body>\n</html>\n\n", result.Body);
        Assert.AreEqual(new DateTime(2025, 7, 7, 8, 13, 50, 807), result.CreatedAt);
        Assert.AreEqual(new DateTime(2025, 7, 7, 8, 13, 50, 807), result.UpdatedAt);
        Assert.AreEqual("value", result.Metadata["key"]);
        Assert.IsNull(result.Error);
    }

    [Test]
    public void TestThrowsExceptionIfTemplateIsNotFound()
    {
        string apiResponse = NotFoundResponse();
        MockApiResponse(apiResponse);

        int templateId = -15;
        var exception = Assert.Throws<SystemException>(() => _emailLibrary.GetDynamicTemplate(templateId));

        Assert.IsNotNull(exception.Message);
        Assert.IsTrue(exception.Message.Length > 0);
        StringAssert.Contains("Couldn't find DynamicTemplate", exception.Message);
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
            ["id"] = 123,
            ["name"] = "Test Template",
            ["api_customer_id"] = 456,
            ["body"] = "<html>\n    <body>\n        <p>Hello {{first_name}} {{last_name}}!</p>\n    </body>\n</html>\n\n",
            ["created_at"] = new DateTime(2025, 7, 7, 8, 13, 50, 807),
            ["updated_at"] = new DateTime(2025, 7, 7, 8, 13, 50, 807),
            ["metadata"] = new Dictionary<string, object>
            {
                ["key"] = "value"
            }
        });
    }

    private string NotFoundResponse()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["Error"] = "Couldn't find DynamicTemplate with 'id'=-15 [WHERE \"dynamic_templates\".\"api_customer_id\" = $1]"
        });
    }
}
