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

        string templateId = "123";
        GetDynamicTemplateResponse result = _emailLibrary.GetDynamicTemplate(templateId);

        Assert.IsNotNull(result);
        Assert.AreEqual(123, result.Id);
        Assert.AreEqual("Test Template", result.Name);
        Assert.AreEqual(456, result.ApiCustomerId);
        Assert.AreEqual("<html>\n    <body>\n        <p>Hello {{first_name}} {{last_name}}!</p>\n    </body>\n</html>\n\n", result.Body);
        Assert.AreEqual(new DateTime(2025, 7, 7, 8, 13, 50, 807), result.CreatedAt);
        Assert.AreEqual(new DateTime(2025, 7, 7, 8, 13, 50, 807), result.UpdatedAt);
        Assert.AreEqual("MetadataValue", result.Metadata["MetadataKey"]);

        Assert.IsNull(result.Error);
        Assert.IsNull(result.Errors);
    }

    [Test]
    public void TestThrowsExceptionIfTemplateIsNotFound()
    {
        string apiResponse = NotFoundResponse();
        MockApiResponse(apiResponse);

        string templateId = "-15";
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
            ["Id"] = 123,
            ["Name"] = "Test Template",
            ["ApiCustomerId"] = 456,
            ["Body"] = "<html>\n    <body>\n        <p>Hello {{first_name}} {{last_name}}!</p>\n    </body>\n</html>\n\n",
            ["CreatedAt"] = "2025-07-06T23:13:50.807-07:00",
            ["UpdatedAt"] = "2025-07-06T23:13:50.807-07:00",
            ["Metadata"] = new Dictionary<string, object>
            {
                ["MetadataKey"] = "MetadataValue"
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
